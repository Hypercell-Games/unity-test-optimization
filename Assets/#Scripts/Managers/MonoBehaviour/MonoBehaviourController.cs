using System;
using System.Collections;
using UnityEngine;

namespace SkyRocket.Tools
{
    public class MonoBehaviourController : MonoCreateSingleton<MonoBehaviourController>
    {
        protected override void Awake()
        {
            base.Awake();

            if (IsInstance)
            {
                onAwake?.Invoke();
            }
        }

        protected virtual IEnumerator Start()
        {
            onStart?.Invoke();

            yield return null;

            onLateStart?.Invoke();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            onApplicationFocus?.Invoke(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            onApplicationPause?.Invoke(pauseStatus);
        }

        private void OnApplicationQuit()
        {
            onApplicationQuit?.Invoke();
        }

        public event Action<bool> onApplicationFocus;
        public event Action<bool> onApplicationPause;

        public event Action onAwake;
        public event Action onStart;
        public event Action onLateStart;

        public event Action<float> onUpdate;
        public event Action<float> onFixedUpdate;
        public event Action<float> onLateUpdate;

        public event Action onApplicationQuit;

        #region UPDATE

        private void Update()
        {
            if (onUpdate == null)
            {
                return;
            }

            var deltaTime = Time.deltaTime;

            onUpdate.Invoke(deltaTime);
        }

        private void FixedUpdate()
        {
            if (onFixedUpdate == null)
            {
                return;
            }

            var deltaTime = Time.fixedDeltaTime;

            onFixedUpdate.Invoke(deltaTime);
        }

        private void LateUpdate()
        {
            if (onLateUpdate == null)
            {
                return;
            }

            var deltaTime = Time.deltaTime;

            onLateUpdate.Invoke(deltaTime);
        }

        #endregion UPDATE
    }
}
