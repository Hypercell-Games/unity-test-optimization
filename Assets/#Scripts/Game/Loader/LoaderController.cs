using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;
using Unpuzzle.Game;
using Unpuzzle.Game.Data;

namespace Game.Loader
{
    public class LoaderController : MonoBehaviour, HyperKit.IExtension
    {
        private const float PublisherKitCheckInterval = 0.2f;
        private const float EstimatedWaitInterval = 5f;

        [SerializeField] private Slider loaderSlider;
        [SerializeField] private GameData _gameData;

        private bool _initialized;
        private Tween _loaderTween;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        public void Start()
        {
            loaderSlider.value = 0;

            _loaderTween = DOTween.To(() => loaderSlider.value, x => loaderSlider.value = x, 1, EstimatedWaitInterval);

            StartCoroutine(LoadPublisherKit());
        }

        private IEnumerator LoadPublisherKit()
        {
            while (!_initialized)
            {
                yield return new WaitForSeconds(PublisherKitCheckInterval);

                if (!HyperKit.Initialized)
                {
                    continue;
                }

                _initialized = true;

                Debug.Log("HyperKit initialized");

                _loaderTween.Kill();
                loaderSlider.value = 1;

                Invoke(nameof(LoadGameplayScene), 0.1f);

                yield break;
            }
        }

        private void LoadGameplayScene()
        {
            LevelData.SessionStarted();

            GameStateSaveUtils.CheckGameStart();

            if (GameLogicUtil.IsLobbyEnabled())
            {
                _gameData.GoToGameMode(new LobbyMode(_gameData));
            }
            else
            {
                _gameData.GoToGameMode(new NormalMode(_gameData));
            }
        }
    }
}
