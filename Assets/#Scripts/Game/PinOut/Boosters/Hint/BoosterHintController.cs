using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class BoosterHintController : MonoBehaviour
    {
        [SerializeField] private BoosterHintButton _button;

        private GameData _gameData;
        private bool _isEnabled;
        private LevelsController _levelsController;

        public void Start()
        {
            _levelsController = LevelsController.Instance;
            _gameData = GlobalData.Instance.GetGameData();

            HideButton();
            _gameData.SoftCurrency.onChange.action += SoftCurrencyUpdate;
            _levelsController.onLevelCreate += OnLevelCreate;
            _levelsController.onMoveDone += OnMoveDone;


            if (_levelsController.CurrentGameController != null)
            {
                OnLevelCreate();
            }
        }

        private void OnDestroy()
        {
            _gameData.SoftCurrency.onChange.action -= SoftCurrencyUpdate;
            _levelsController.onLevelCreate -= OnLevelCreate;
            _levelsController.onMoveDone -= OnMoveDone;
        }

        public void OnLevelCreate()
        {
            _button.DisableHighlight(null);

            _isEnabled = !_levelsController.CurrentGameController.IsHintBoosterNotSupport();

            _button.gameObject.SetActive(_isEnabled);
        }

        public void SoftCurrencyUpdate(int count)
        {
            _button.UpdateCount();
        }

        public void OnMoveDone()
        {
            if (_isEnabled && !_button.gameObject.activeSelf)
            {
                ShowButton();
            }
        }

        private void HideButton()
        {
            _button.gameObject.SetActive(false);
        }

        private void ShowButton()
        {
            _button.UpdateCount();

            _button.transform.localScale = Vector3.zero;
            _button.gameObject.SetActive(true);
            _button.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }
    }
}
