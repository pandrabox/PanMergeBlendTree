﻿/*
 * MIT License
 *
 * Copyright (c) 2022 bd_
 * Copyright (c) 2024 pandra
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
/*
 * This program was created by pandra based on ModularAvatar(1.10.3) developed by bd_. 
 * The original code is licensed under the MIT License (see above).
 * My modifications are also licensed under the MIT License.
 * 
 * 改変内容：MergeBlendTreePassからBlendTree統合関連機能を削除し、パラメータをMergeAnimatorに渡す処理を追加
 */

#if MA_VRCSDK3_AVATARS

#region

using System;
using System.Collections.Generic;
using nadena.dev.modular_avatar.animation;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using com.github.pandrabox.panmergeblendtree.runtime;
using nadena.dev.modular_avatar.core;

#endregion


namespace com.github.pandrabox.panmergeblendtree.editor
{
    public class PanMergeBlendTreePass : Pass<PanMergeBlendTreePass>
    {
        internal const string ALWAYS_ONE = "__ModularAvatarInternal/One";

        private string _blendTreeLayerName;
        private AnimatorController _controller;
        private BlendTree _rootBlendTree;
        private GameObject _mergeHost;
        private PanMergeBlendTree _target;

        protected override void Execute(nadena.dev.ndmf.BuildContext context)
        {
            foreach (var component in
                     context.AvatarRootObject.GetComponentsInChildren<PanMergeBlendTree>(true))
            {
                ErrorReport.WithContextObject(component, () => ProcessComponent(context, component));
            }
        }

        private void ProcessComponent(nadena.dev.ndmf.BuildContext context, PanMergeBlendTree component)
        {
            _rootBlendTree = (BlendTree)component.BlendTree;
            _mergeHost = component.gameObject;
            _blendTreeLayerName = $@"PanMBT/{_rootBlendTree.name}";
            _target = component;
            _controller = new AnimatorController();
            SetParametersToController();
            SetBlendTreeToController();
            ApplyMergeAnimator();
        }

        private void SetParametersToController()
        {
            // Get Unique parameter names
            HashSet<string> parameterNames = new HashSet<string>();
            foreach (var asset in _rootBlendTree.ReferencedAssets(includeScene: false))
            {
                if (asset is BlendTree bt2)
                {
                    if (!string.IsNullOrEmpty(bt2.blendParameter) && bt2.blendType != BlendTreeType.Direct)
                    {
                        parameterNames.Add(bt2.blendParameter);
                    }

                    if (bt2.blendType != BlendTreeType.Direct && bt2.blendType != BlendTreeType.Simple1D)
                    {
                        if (!string.IsNullOrEmpty(bt2.blendParameterY))
                        {
                            parameterNames.Add(bt2.blendParameterY);
                        }
                    }

                    if (bt2.blendType == BlendTreeType.Direct)
                    {
                        foreach (var childMotion in bt2.children)
                        {
                            if (!string.IsNullOrEmpty(childMotion.directBlendParameter))
                            {
                                parameterNames.Add(childMotion.directBlendParameter);
                            }
                        }
                    }
                }
            }

            // Set Parameters
            var parameters = new List<AnimatorControllerParameter>(parameterNames.Count + 1);
            parameterNames.Remove(ALWAYS_ONE);
            parameters.Add(new AnimatorControllerParameter()
            {
                name = ALWAYS_ONE,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1
            });

            foreach (var name in parameterNames)
            {
                parameters.Add(new AnimatorControllerParameter()
                {
                    name = name,
                    type = AnimatorControllerParameterType.Float,
                    defaultFloat = 0
                });
            }

            _controller.parameters = parameters.ToArray();
        }

        private void SetBlendTreeToController()
        {
            var newController = new AnimatorController();
            var newStateMachine = new AnimatorStateMachine();
            var newState = new AnimatorState();
            _controller = newController;

            _controller.layers = new[]
            {
                new AnimatorControllerLayer
                {
                    blendingMode = AnimatorLayerBlendingMode.Override,
                    defaultWeight = 1,
                    name = _blendTreeLayerName,
                    stateMachine = newStateMachine
                }
            };
            newStateMachine.name = "BlendTree";
            newStateMachine.states = new[]
            {
                new ChildAnimatorState
                {
                    state = newState,
                    position = Vector3.zero
                }
            };
            newStateMachine.defaultState = newState;
            newState.writeDefaultValues = true;
            newState.motion = _rootBlendTree;
        }

        private void ApplyMergeAnimator()
        {
            var merger = _mergeHost.AddComponent<ModularAvatarMergeAnimator>();
            merger.animator = _controller;
            merger.layerType = _target.LayerType;
            merger.deleteAttachedAnimator = false;
            merger.pathMode = _target.PathMode;
            merger.matchAvatarWriteDefaults = false;
            merger.relativePathRoot = _target.RelativePathRoot;
            merger.layerPriority = _target.LayerPriority;
        }

    }
}


#endif