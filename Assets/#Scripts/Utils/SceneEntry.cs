using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SceneEntry
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset scene;
#endif

    [HideInInspector] [SerializeField] private string sceneName;

    public string SceneName => sceneName;

#if UNITY_EDITOR
    public void UpdateSceneName()
    {
        if (scene != null)
        {
            sceneName = scene.name;
        }
    }
#endif
}
