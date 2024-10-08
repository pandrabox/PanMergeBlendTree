/*
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
 * 改変内容：MergeAnimationEditorを元に、PanMergeBlendTreeの変更に即した修正。ロゴガイドラインに乗っ取り継承元を変更。
 */

#if MA_VRCSDK3_AVATARS

using UnityEditor;
using com.github.pandrabox.panmergeblendtree.runtime;
using static nadena.dev.modular_avatar.core.editor.Localization;
using nadena.dev.modular_avatar.core;
using nadena.dev.modular_avatar.core.editor;

namespace com.github.pandrabox.panmergeblendtree.editor
{
    //[CustomPropertyDrawer(typeof(MergeAnimatorPathMode))]
    //class PathModeDrawer : EnumDrawer<MergeAnimatorPathMode>
    //{
    //    protected override string localizationPrefix => "path_mode";
    //}

    [CustomEditor(typeof(PanMergeBlendTree))]
    class MergeAnimationEditor : UnityEditor.Editor //MAEditorBase
    {
        private SerializedProperty prop_BlendTree,
            prop_layerType,
            // prop_deleteAttachedAnimator,
            prop_pathMode,
            // prop_matchAvatarWriteDefaults,
            prop_relativePathRoot,
            prop_layerPriority;

        private void OnEnable()
        {
            prop_BlendTree = serializedObject.FindProperty(nameof(PanMergeBlendTree.BlendTree));
            prop_layerType = serializedObject.FindProperty(nameof(PanMergeBlendTree.LayerType));
            // prop_deleteAttachedAnimator = serializedObject.FindProperty(nameof(PanMergeBlendTree.deleteAttachedAnimator));
            prop_pathMode = serializedObject.FindProperty(nameof(PanMergeBlendTree.PathMode));
            // prop_matchAvatarWriteDefaults = serializedObject.FindProperty(nameof(PanMergeBlendTree.matchAvatarWriteDefaults));
            prop_relativePathRoot =
                serializedObject.FindProperty(nameof(PanMergeBlendTree.RelativePathRoot));
            prop_layerPriority = serializedObject.FindProperty(nameof(PanMergeBlendTree.LayerPriority));
        }

        public sealed override void OnInspectorGUI()
        {
            InspectorCommon.DisplayOutOfAvatarWarning(targets);

            OnInnerInspectorGUI();
        }

        protected void OnInnerInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(prop_BlendTree, G("merge_blend_tree.blend_tree"));
            EditorGUILayout.PropertyField(prop_layerType, G("merge_animator.layer_type"));
            // EditorGUILayout.PropertyField(prop_deleteAttachedAnimator, G("merge_animator.delete_attached_animator"));
            EditorGUILayout.PropertyField(prop_pathMode, G("merge_animator.path_mode"));
            if (prop_pathMode.enumValueIndex == (int)MergeAnimatorPathMode.Relative)
                EditorGUILayout.PropertyField(prop_relativePathRoot, G("merge_animator.relative_path_root"));
            EditorGUILayout.PropertyField(prop_layerPriority, G("merge_animator.layer_priority"));
            // EditorGUILayout.PropertyField(prop_matchAvatarWriteDefaults, G("merge_animator.match_avatar_write_defaults"));

            EditorGUILayout.HelpBox($@"PanMergeBlendTreeはBlendTreeをセットするとそれを動作させるのに必要なParameterを持ったAnimatorを作成し、通常のMergeAnimatorとして動作します。MAMergeBlendTree(v1.10.3)に比較してRelativeでルート指定したときの不具合がない、レイヤ順をユーザが指定できるメリットがあります。デメリットとして、BlendTree同士のマージを行いません。(1BlendTree辺り1レイヤ生成されます)", MessageType.Info);
            serializedObject.ApplyModifiedProperties();

            ShowLanguageUI();
        }
    }
}

#endif