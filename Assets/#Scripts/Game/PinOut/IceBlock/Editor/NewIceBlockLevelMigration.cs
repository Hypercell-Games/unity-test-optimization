using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unpuzzle
{
    public static class NewIceBlockLevelMigration
    {
        [MenuItem("Edit/Levels/Migrate ice blocks")]
        private static void Migrate()
        {
            var levelsPath = Application.dataPath + "/#Prefabs/BuildLevels";
            var levelWrite = Application.dataPath + "/Resources/Levels/";
            var files = Directory.GetFiles(levelsPath);
            var levels = new List<GameObject>();
            var levelsPaths = new List<string>();
            var iceBlock = AssetDatabase.LoadMainAssetAtPath("Assets/#Prefabs/PinOut/IceBlocks/IceBlockLoop.prefab");

            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".prefab")
                {
                    var level = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets/#Prefabs/BuildLevels/" +
                                                                              Path.GetFileName(file));

                    if (level == null)
                    {
                        continue;
                    }

                    level = (GameObject)PrefabUtility.InstantiatePrefab(level);


                    var levelPinOut = level.GetComponent<PinOutLevel>();

                    if (levelPinOut == null)
                    {
                        GameObject.DestroyImmediate(level);
                        continue;
                    }

                    var pins = levelPinOut.GetComponentsInChildren<HookController>().ToList();

                    var isFrozenFinded = false;
                    var iceBlocks = new List<GameObject>();

                    foreach (var pin in pins)
                    {
                        if (pin == null)
                        {
                            continue;
                        }

                        if (pin.IsFrozen)
                        {
                            isFrozenFinded = true;
                            var prefabBlock = PrefabUtility.InstantiatePrefab(iceBlock, levelPinOut.transform);
                            Debug.Log(prefabBlock);
                            var instancedBlock = (GameObject)prefabBlock;
                            instancedBlock.transform.parent = levelPinOut.transform;
                            instancedBlock.transform.localPosition = pin.transform.localPosition;
                            instancedBlock.transform.rotation = pin.transform.rotation;

                            iceBlocks.Add(instancedBlock);

                            PrefabUtility.RecordPrefabInstancePropertyModifications(instancedBlock.transform);
                        }
                    }

                    if (isFrozenFinded)
                    {
                        Debug.Log("Migrated: " + file);
                        levels.Add(level);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(level);
                        continue;
                    }

                    if (levels.Count > 5)
                    {
                        PrefabUtility.ApplyPrefabInstances(levels.ToArray(), InteractionMode.AutomatedAction);
                        foreach (var levelPrefab in levels)
                        {
                            GameObject.DestroyImmediate(levelPrefab);
                        }

                        levels.Clear();
                    }
                }
            }


            if (levels.Count == 0)
            {
                return;
            }

            PrefabUtility.ApplyPrefabInstances(levels.ToArray(), InteractionMode.AutomatedAction);
            foreach (var level in levels)
            {
                GameObject.DestroyImmediate(level);
            }
        }

        [MenuItem("Edit/Levels/Set type")]
        private static void SetType()
        {
            SetType("Assets/#Prefabs/PinOut/IceBlocks/IceBlockLoop.prefab", IceBlockType.loop);
            SetType("Assets/#Prefabs/PinOut/IceBlocks/IceBlockLoopSliced.prefab", IceBlockType.loopSliced);
        }

        private static void SetType(string path, IceBlockType type)
        {
            var iceBlock = (GameObject)AssetDatabase.LoadMainAssetAtPath(path);

            if (iceBlock == null)
            {
                return;
            }

            iceBlock = (GameObject)PrefabUtility.InstantiatePrefab(iceBlock);

            var iceBlockController = iceBlock.GetComponent<IceBlockController>();

            iceBlockController.SetType(type);

            PrefabUtility.ApplyPrefabInstance(iceBlock, InteractionMode.AutomatedAction);


            GameObject.DestroyImmediate(iceBlock);
        }
    }
}
