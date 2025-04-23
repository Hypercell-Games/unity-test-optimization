using System;
using UnityEngine;

namespace SkyRocket.Tools
{
    [CreateAssetMenu(fileName = "RemoteConfigConfigs", menuName = MenuPath, order = MenuOrder)]
    public class RemoteConfigConfigs : ScriptableObject
    {
        [Serializable]
        public enum EEnvironment
        {
            NONE = 0,

            PRODUCT = 10,

            DEV = 20
        }

        private const string MenuPath = "SkyRocket/Configs/RemoteConfigConfigs";
        private const int MenuOrder = int.MinValue + 1200;

        [Header("ENVIRONMENTS")] [SerializeField]
        private string _productionEnvironmentId;

        [SerializeField] private string _devEnvironmentId;

        [Header("CONFIGS")] [SerializeField] private EEnvironment _buildEnvironment = EEnvironment.PRODUCT;

        [SerializeField] private EEnvironment _editorEnvironment = EEnvironment.DEV;

        [Header("SAVES")] [SerializeField] private bool _saveDefaultValuesInEditor = true;

        public string GetProductionEnvironmentId => GetEnvironmentId(EEnvironment.PRODUCT);
        public string GetDevEnvironmentId => GetEnvironmentId(EEnvironment.DEV);

        public bool CacheDefaultValuesInEditor => _saveDefaultValuesInEditor;

        public string GetEnvironmentId()
        {
            var environment = !Application.isEditor
                ? _buildEnvironment
                : _editorEnvironment;

            return GetEnvironmentId(environment);
        }

        public string GetEnvironmentId(EEnvironment environment)
        {
            switch (environment)
            {
                case EEnvironment.PRODUCT:
                    return _productionEnvironmentId;
                case EEnvironment.DEV:
                    return _devEnvironmentId;
            }

            return null;
        }
    }
}
