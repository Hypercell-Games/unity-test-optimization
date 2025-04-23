using UnityEditor;
using UnityEngine;

namespace Unpuzzle
{
    [CustomEditor(typeof(HookColoringEdtior))]
    public class HookColoringEdtiorWindow : Editor
    {
        private void OnSceneGUI()
        {
            var coloringEditor = target as HookColoringEdtior;
            var hookController = coloringEditor.GetComponent<HookController>();

            if (hookController == null)
            {
                return;
            }

            Handles.BeginGUI();
            EditorGUILayout.BeginHorizontal();
            hookController.IsColorOverrided =
                EditorGUILayout.Toggle("ColorOverriding", hookController.IsColorOverrided);
            hookController.OverrideColor = (PinColorID)EditorGUILayout.EnumPopup(hookController.OverrideColor);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            hookController.IsMaterialOverrided =
                EditorGUILayout.Toggle("Material overriding", hookController.IsMaterialOverrided);
            hookController.OverridedMaterial =
                (PinMaterialId)EditorGUILayout.EnumPopup(hookController.OverridedMaterial);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            hookController.IsTarget = EditorGUILayout.Toggle("Is target", hookController.IsTarget);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            hookController.IsContainKey = EditorGUILayout.Toggle("Is has key", hookController.IsContainKey);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            if (GUILayout.Button("Spanw sliced ice"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath("Assets/#Prefabs/PinOut/IceBlocks/IceBlockLoopSliced.prefab",
                    typeof(GameObject));
                var iceBlock = SpawnIce(prefab, hookController.transform);

                if (hookController.pinType == PinType.loop)
                {
                    var scaler = hookController.GetComponent<HookScaler>();
                    if (scaler != null)
                    {
                        if (scaler.ScaleEnabled)
                        {
                            var scale = scaler.Scale;
                            scale *= 2;
                            iceBlock.SetSize(Vector3.right * scale);
                        }
                    }

                    iceBlock.transform.localPosition += Vector3.up * 0.5f;
                }
            }

            if (GUILayout.Button("Spanw ice"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath("Assets/#Prefabs/PinOut/IceBlocks/IceBlockLoop.prefab",
                    typeof(GameObject));
                SpawnIce(prefab, hookController.transform);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            Handles.EndGUI();
        }

        private IceBlockController SpawnIce(Object prefab, Transform hook)
        {
            var instance = PrefabUtility.InstantiatePrefab(prefab, hook) as GameObject;

            return instance.GetComponent<IceBlockController>();
        }
    }
}
