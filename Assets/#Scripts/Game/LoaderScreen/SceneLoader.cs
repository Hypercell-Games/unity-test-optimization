using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;

    public static SceneLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SceneLoader>();
            }

            if (_instance == null)
            {
                var go = new GameObject("SceneLoader");
                _instance = go.AddComponent<SceneLoader>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }

            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadSceneAsyncWithCallback(string sceneName, Action<float> onProgressChanged, Action callback)
    {
        StartCoroutine(Co_LoadSceneAsyncWithCallback(sceneName, onProgressChanged, callback));
    }

    private IEnumerator Co_LoadSceneAsyncWithCallback(string sceneName, Action<float> onProgressChanged,
        Action callback)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);


        while (!asyncLoad.isDone)
        {
            onProgressChanged?.Invoke(asyncLoad.progress);
            yield return null;
        }

        callback?.Invoke();
    }
}
