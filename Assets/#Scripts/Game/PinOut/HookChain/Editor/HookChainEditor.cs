using UnityEditor;
using UnityEngine;

namespace Unpuzzle
{
    [CustomEditor(typeof(HookChain))]
    public class HookChainEditor : Editor
    {
        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var hookChain = target as HookChain;

            if (GUILayout.Button("Add block"))
            {
                var element = PrefabUtility.InstantiatePrefab(hookChain.Block) as AbstractHookChainElement;
                hookChain.AddElement(element);
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Add wide block"))
            {
                var element = PrefabUtility.InstantiatePrefab(hookChain.WideBlock) as AbstractHookChainElement;
                hookChain.AddElement(element);
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Add Line"))
            {
                var element = PrefabUtility.InstantiatePrefab(hookChain.Line) as AbstractHookChainElement;
                hookChain.AddElement(element);
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Remove last"))
            {
                hookChain.RemoveElement();
                EditorUtility.SetDirty(target);
            }

            foreach (var element in hookChain.Elements)
            {
                element.Scale = EditorGUILayout.FloatField("Element", element.Scale);
            }
        }
    }
}
