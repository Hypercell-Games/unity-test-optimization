using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkyRocket.Tools
{
    public class CoroutinesController : MonoCreateSingleton<CoroutinesController>
    {
        private readonly List<Coroutine> _globalCoroutines = new();
        private readonly List<Coroutine> _sceneCoroutines = new();

        protected override void Awake()
        {
            base.Awake();

            if (IsInstance)
            {
                SetAsDontDestroy();

                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
        }

        public bool StopCoroutine(List<Coroutine> coroutines, Coroutine coroutine)
        {
            for (var i = 0; i < coroutines.Count; i++)
            {
                var sceneCoroutine = coroutines[i];
                if (sceneCoroutine == coroutine)
                {
                    StopCoroutine(sceneCoroutine);
                    coroutines.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private void StopCoroutines(List<Coroutine> coroutines)
        {
            for (var i = 0; i < coroutines.Count; i++)
            {
                var sceneCoroutine = coroutines[i];
                if (sceneCoroutine != null)
                {
                    StopCoroutine(sceneCoroutine);
                }
            }

            coroutines.Clear();
        }

        #region SCENE COROUTINES

        public Coroutine StartSceneCoroutine(IEnumerator method)
        {
            var coroutine = StartCoroutine(method);
            _sceneCoroutines.Add(coroutine);
            return coroutine;
        }

        public bool StopSceneCoroutine(Coroutine coroutine)
        {
            return StopCoroutine(_sceneCoroutines, coroutine);
        }

        private void StopSceneCoroutines()
        {
            StopCoroutines(_sceneCoroutines);
        }

        #endregion SCENE COROUTINES

        #region GLOBAL COROUTINES

        public Coroutine StartGlobalCoroutine(IEnumerator method)
        {
            var coroutine = StartCoroutine(method);
            _globalCoroutines.Add(coroutine);
            return coroutine;
        }

        public bool StopGlobalCoroutine(Coroutine coroutine)
        {
            return StopCoroutine(_globalCoroutines, coroutine);
        }

        private void StopGlobalCoroutines()
        {
            StopCoroutines(_globalCoroutines);
        }

        #endregion GLOBAL COROUTINES

        #region SCENES

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (loadSceneMode == LoadSceneMode.Single)
            {
                StopSceneCoroutines();
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
        }

        #endregion SCENES
    }
}
