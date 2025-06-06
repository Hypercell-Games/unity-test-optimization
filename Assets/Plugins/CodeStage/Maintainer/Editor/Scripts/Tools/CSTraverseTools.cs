#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Tools
{
    internal class TraverseStats
    {
        public long gameObjectsTraversed;
        public long componentsTraversed;
        public long propertiesTraversed;

        public void Clear()
        {
            gameObjectsTraversed = componentsTraversed = propertiesTraversed = 0;
        }
    }

    internal class ObjectTraverseInfo
    {
        public bool ForcePrefabsAsRegularObjects { get; private set; }
        public bool SkipCleanPrefabInstances { get; private set; }
        public int TotalRoots { get; private set; }

        public GameObject current;
        public bool inPrefabInstance;
        public bool inMissingPrefabInstance;
        public int[] dirtyComponents;
        public int rootIndex;

        public bool skipCurrentTree;

        public ObjectTraverseInfo(bool skipCleanPrefabInstances, bool forcePrefabsAsRegularObjects, int totalRoots)
        {
            SkipCleanPrefabInstances = skipCleanPrefabInstances;
            ForcePrefabsAsRegularObjects = forcePrefabsAsRegularObjects;
            TotalRoots = totalRoots;
        }
    }

    internal class SerializedObjectTraverseInfo
    {
        public Object TraverseTarget { get; private set; }
        public bool OnlyVisibleProperties { get; private set; }

        public bool skipChildren;

        public SerializedObjectTraverseInfo(Object traverseTarget, bool onlyVisibleProperties = true)
        {
            TraverseTarget = traverseTarget;
            OnlyVisibleProperties = onlyVisibleProperties;
        }
    }

    internal static class CSTraverseTools
    {
        internal delegate bool GameObjectTraverseCallback(ObjectTraverseInfo traverseInfo);
        internal delegate void SceneRootTraverseCallback(int index, int total, out bool canceled);
        internal delegate void ComponentTraverseCallback(ObjectTraverseInfo traverseInfo, Component component, int orderIndex);
        internal delegate void SerializableObjectTraverseCallback(SerializedObjectTraverseInfo traverseInfo, SerializedProperty currentProperty);
        internal delegate void MaterialSavedPropertyTraverseCallback(SerializedObjectTraverseInfo traverseInfo, SerializedProperty property, string propertyName, SerializedProperty propertyTexture);

        private static readonly TraverseStats Stats = new TraverseStats();

        public static void ClearStats()
        {
            Stats.Clear();
        }

        public static TraverseStats GetStats()
        {
            return Stats;
        }

        public static bool TraverseSceneGameObjects(Scene scene, bool skipCleanPrefabInstances, bool forceAllPrefabsTraverse, GameObjectTraverseCallback callback, SceneRootTraverseCallback rootTraverseCallback = null)
        {
            var rootObjects = scene.GetRootGameObjects();
            return TraverseRootGameObjects(rootObjects, skipCleanPrefabInstances, forceAllPrefabsTraverse, callback, rootTraverseCallback);
        }

        public static bool TraversePrefabGameObjects(GameObject prefabAsset, bool skipCleanPrefabInstances, bool forceAllPrefabsTraverse, GameObjectTraverseCallback callback)
        {
            return TraverseRootGameObjects(new[] { prefabAsset }, skipCleanPrefabInstances, forceAllPrefabsTraverse, callback);
        }

        public static bool TraverseRootGameObjects(GameObject[] rootObjects, bool skipCleanPrefabInstances, bool forceAllPrefabsTraverse, GameObjectTraverseCallback callback, SceneRootTraverseCallback rootTraverseCallback = null)
        {
            var rootObjectsCount = rootObjects.Length;
            var objectTraverseInfo = new ObjectTraverseInfo(skipCleanPrefabInstances, forceAllPrefabsTraverse, rootObjectsCount);

            for (var i = 0; i < rootObjectsCount; i++)
            {
                var rootObject = rootObjects[i];

                if (rootTraverseCallback != null)
                {
                    bool canceled;
                    rootTraverseCallback.Invoke(i, rootObjectsCount, out canceled);
                    if (canceled)
                    {
                        return false;
                    }
                }

                objectTraverseInfo.current = rootObject;
                objectTraverseInfo.rootIndex = i;

                if (!TraverseGameObjectTree(objectTraverseInfo, callback))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TraverseGameObjectTree(ObjectTraverseInfo traverseInfo, GameObjectTraverseCallback callback)
        {
            Stats.gameObjectsTraversed++;

            var prefabInstanceRoot = false;
            if (!traverseInfo.inPrefabInstance && !traverseInfo.ForcePrefabsAsRegularObjects)
            {
                //Debug.Log("IsPartOfPrefabInstance " + PrefabUtility.IsPartOfPrefabInstance(componentOrGameObject));
                //Debug.Log(traverseInfo.current.name);
                traverseInfo.inPrefabInstance = CSPrefabTools.IsInstance(traverseInfo.current);
                if (traverseInfo.inPrefabInstance)
                {
                    if (!CSPrefabTools.IsMissingPrefabInstance(traverseInfo.current))
                    {
                        if (traverseInfo.SkipCleanPrefabInstances)
                        {
                            if (CSPrefabTools.IsWholeInstanceHasAnyOverrides(traverseInfo.current))
                            {
                                CSPrefabTools.GetOverridenObjectsFromWholePrefabInstance(traverseInfo.current, out traverseInfo.dirtyComponents);
                            }
                        }
                    }
                    else
                    {
                        traverseInfo.inMissingPrefabInstance = true;
                    }

                    prefabInstanceRoot = true;
                }
            }

            if (!callback.Invoke(traverseInfo))
            {
                return false;
            }

            if (traverseInfo.skipCurrentTree)
            {
                if (prefabInstanceRoot)
                {
                    traverseInfo.dirtyComponents = null;
                    traverseInfo.inPrefabInstance = false;
                    traverseInfo.inMissingPrefabInstance = false;
                    traverseInfo.skipCurrentTree = false;
                }

                return true;
            }

            var transform = traverseInfo.current.transform;
            var childrenCount = transform.childCount;

            for (var i = 0; i < childrenCount; i++)
            {
                var child = transform.GetChild(i);
                traverseInfo.current = child.gameObject;
                if (!TraverseGameObjectTree(traverseInfo, callback))
                {
                    return false;
                }
            }

            if (prefabInstanceRoot)
            {
                traverseInfo.dirtyComponents = null;
                traverseInfo.inPrefabInstance = false;
                traverseInfo.inMissingPrefabInstance = false;
                traverseInfo.skipCurrentTree = false;
            }

            return true;
        }

        public static void TraverseGameObjectComponents(ObjectTraverseInfo traverseInfo, ComponentTraverseCallback callback)
        {
            var components = traverseInfo.current.GetComponents<Component>();
            var checkingPrefabInstanceObject = false;
            var checkingAddedToInstanceObject = false;

            if (traverseInfo.SkipCleanPrefabInstances)
            {
                checkingPrefabInstanceObject = traverseInfo.inPrefabInstance && !traverseInfo.inMissingPrefabInstance;
                if (checkingPrefabInstanceObject)
                {
                    checkingAddedToInstanceObject = PrefabUtility.IsAddedGameObjectOverride(traverseInfo.current);
                }
            }

            var hasDirtyComponents = traverseInfo.dirtyComponents != null;

            var visibleIndex = -1;
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (CSComponentTools.IsComponentVisibleInInspector(component))
                {
                    visibleIndex++;
                }

                if (component == null)
                {
                    // to register missing script
                    callback(traverseInfo, null, visibleIndex);
                    continue;
                }

                // transforms are checked at the GameObject level
                if (component is Transform) continue;

                if (!checkingPrefabInstanceObject)
                {
                    Stats.componentsTraversed++;
                    callback(traverseInfo, component, visibleIndex);
                }
                else
                {
                    var hasOverridenProperties = checkingAddedToInstanceObject;
                    if (!hasOverridenProperties && hasDirtyComponents)
                    {
                        if (Array.IndexOf(traverseInfo.dirtyComponents, component.GetInstanceID()) != -1)
                        {
                            hasOverridenProperties = true;
                        }
                    }

                    if (hasOverridenProperties)
                    {
                        Stats.componentsTraversed++;
                        callback(traverseInfo, component, visibleIndex);
                    }
                }
            }
        }

        public static void TraverseObjectProperties(SerializedObjectTraverseInfo traverseInfo, SerializableObjectTraverseCallback callback)
        {
            var so = new SerializedObject(traverseInfo.TraverseTarget);
            var iterator = so.GetIterator();

            if (traverseInfo.TraverseTarget.name.Contains("IAPSettings"))
                Debug.Log("IAPSettings");

#if UNITY_2021_2_OR_NEWER
            ManagedDuplicatesDetector managedDuplicates = null;
#endif
            const int maxDepth = 1000;
            while (traverseInfo.OnlyVisibleProperties ?
                iterator.NextVisible(!traverseInfo.skipChildren) :
                iterator.Next(!traverseInfo.skipChildren))
            {
#if UNITY_2021_2_OR_NEWER
                // skipping already traversed managed reference
                // avoids recursion in case of circular [SerializeReference] references
                if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                {
                    if (managedDuplicates == null)
                        managedDuplicates = ManagedDuplicatesDetector.Spawn(traverseInfo, iterator);
                    else if (managedDuplicates.IsDuplicate(iterator))
                        continue;
                }
#endif
                traverseInfo.skipChildren = false;
                Stats.propertiesTraversed++;
                callback(traverseInfo, iterator);

                // in case recursion wasn't managed
                if (iterator.depth > maxDepth)
                {
                    Debug.LogWarning(Maintainer.ConstructLog("Possible infinite recursion found at " + traverseInfo.TraverseTarget + " with depth over " + maxDepth + "\n" +
                                                             "Property path:\n" + iterator.propertyPath + "\n" +
                                                             "Skipping rest of the object."));
                    break;
                }
            }
#if UNITY_2021_2_OR_NEWER
            managedDuplicates?.Dispose();
#endif
        }

        public static void TraverseMaterialTexEnvs(SerializedObjectTraverseInfo traverseInfo, MaterialSavedPropertyTraverseCallback callback)
        {
            var so = new SerializedObject(traverseInfo.TraverseTarget);
            var texEnvs = so.FindProperty("m_SavedProperties.m_TexEnvs.Array");
            if (texEnvs != null)
            {
                for (var k = 0; k < texEnvs.arraySize; k++)
                {
                    var property = texEnvs.GetArrayElementAtIndex(k);
                    var propertyName = property.displayName;
                    var propertyTexture = property.FindPropertyRelative("second.m_Texture");
                    if (propertyTexture == null)
                    {
                        Debug.LogError(Maintainer.ErrorForSupport("Can't get second.m_Texture from texEnvs at " + traverseInfo.TraverseTarget.name));
                    }

                    callback(traverseInfo, property, propertyName, propertyTexture);
                }
            }
            else
            {
                Debug.LogError(Maintainer.ErrorForSupport("Can't get m_SavedProperties.m_TexEnvs.Array from material instance at " + traverseInfo.TraverseTarget.name));
            }
        }

        public static string TryGetMonoScriptDefaultPropertyName(SerializedProperty property)
        {
            if (!property.isArray)
                return null;

            if (property.type != "string")
                return null;

            if (property.propertyPath.IndexOf("m_DefaultReferences.Array.data[", StringComparison.OrdinalIgnoreCase) !=
                0)
                return null;

            if (property.stringValue == null)
                return null;

            var result = property.stringValue;

            // skipping first pair item of the m_DefaultReferences array item
            property.Next(false);

            return result;
        }
    }
}
