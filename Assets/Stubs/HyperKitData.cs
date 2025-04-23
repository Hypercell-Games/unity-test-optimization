using Newtonsoft.Json;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace Services
{
    namespace Data
    {
        public class AdConfig
        {
            public int skipAfterAnyInterstitial = 35;
            public int skipAfterAnyRewarded = 45;
            public int skipAfterAppStart = 30;
        }

        public class IAPProduct
        {
            public string key;
            public string price;
        }

        public class DataService
        {
            private readonly GameConfig _gameConfig = LoadGameConfig();

            public string InitialConfigVersion => Application.version;

            public string CurrentConfigVersion => Application.version;

            public AdConfig AdConfig => _gameConfig.adConfig;

            public IAPProduct[] IapProducts => _gameConfig.iapProducts;

            private static GameConfig LoadGameConfig()
            {
                var jsonPath = $"Configs/{Application.version.Replace('.', '_')}";
                var jsonFile = Resources.Load<TextAsset>(jsonPath);

                return JsonConvert.DeserializeObject<GameConfig>(jsonFile.text);
            }

            public GameConfig GetCustomConfig<T>()
            {
                return _gameConfig;
            }
        }
    }
}
