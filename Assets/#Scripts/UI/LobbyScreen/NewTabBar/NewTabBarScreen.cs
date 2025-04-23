using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unpuzzle.Game.Data;

namespace Unpuzzle.UI.NewTabBar
{
    public class NewTabBarScreen : MonoBehaviour
    {
        public enum Buttons
        {
            None = -1,
            Lobby = 0,
            Skins = 1,
            InAppShop = 2,
            DailyChallenges = 3
        }

        private static NewTabBarScreen _instance;

        [SerializeField] private RectTransform _panel;

        [SerializeField] private List<TabBarButtonEntry> _buttons;

        [SerializeField] public Buttons _initialButton = Buttons.Lobby;
        private int _countButtons;

        private Action<LobbyScreenType, bool> _onButtonClick;

        private Buttons _selectedButton = Buttons.None;

        public static NewTabBarScreen Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NewTabBarScreen>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            _buttons.ForEach(r => r.button.OnClick += OnButtonClick);
        }

        private void OnDisable()
        {
            _buttons.ForEach(r => r.button.OnClick -= OnButtonClick);
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public event Action<bool> OnOpen;
        public event Action<Buttons> OnScreenRequest;

        public RectTransform GetChallengesButtonIcon()
        {
            return GetButtonIcon(Buttons.DailyChallenges);
        }

        private RectTransform GetButtonIcon(Buttons button)
        {
            var btn = _buttons.FirstOrDefault(b => b.buttonType == button);
            if (btn == null)
            {
                return null;
            }

            return btn.button.GetIcon();
        }

        public void Init(Action<LobbyScreenType, bool> switchScreen)
        {
            _onButtonClick = switchScreen;
            if (GlobalData.Instance.GetGameData().IsCurrentMode<LobbyMode>(out var lobbyMode))
            {
                _initialButton = lobbyMode.LobbyScreenType switch
                {
                    LobbyScreenType.Skins => Buttons.Skins,
                    LobbyScreenType.Lobby => Buttons.Lobby,
                    LobbyScreenType.InAppShop => Buttons.InAppShop,
                    LobbyScreenType.Challenges => Buttons.DailyChallenges,
                    _ => throw new NotImplementedException()
                };
            }

            UpdateButtonsStates();
            SetButtonSelected(_initialButton, true);
        }

        public void UpdateButtonsStates()
        {
            var currentLevel = LevelData.GetLevelNumber();
            _buttons.ForEach(b =>
            {
                if (b.buttonType == Buttons.Skins)
                {
                    var skins = GlobalData.Instance.GetGameData().SkinsConfig.BgSkins;
                    var hasAnyIntoduced = skins.Any(b => !b.IsUnlocked() && b.IsIntroduced);
                    b.button.SetPin(hasAnyIntoduced ? TabBarPinType.Pin : TabBarPinType.None);
                }
            });

            _countButtons = _buttons.Count(b => b.button.gameObject.activeSelf);
        }

        public void SetButtonSelected(Buttons button, bool force)
        {
            _selectedButton = button;
            foreach (var buttonEntry in _buttons)
            {
                buttonEntry.button.SetSelected(buttonEntry.buttonType == button, force, _countButtons);
            }

            var screen = button switch
            {
                Buttons.None => LobbyScreenType.Lobby,
                Buttons.Lobby => LobbyScreenType.Lobby,
                Buttons.Skins => LobbyScreenType.Skins,
                Buttons.InAppShop => LobbyScreenType.InAppShop,
                Buttons.DailyChallenges => LobbyScreenType.Challenges,
                _ => throw new NotImplementedException()
            };
            _onButtonClick?.Invoke(screen, force);
        }

        public void SetActive(bool value)
        {
            if (value == gameObject.activeSelf)
            {
                return;
            }

            gameObject.SetActive(value);
            if (value)
            {
            }
        }

        private void OnButtonClick(NewTabBarButton button, bool isEnabled)
        {
            var buttonEntry = _buttons.FirstOrDefault(r => r.button == button);
            if (buttonEntry == null)
            {
                Debug.LogError("Button not found");
                return;
            }

            var buttonType = buttonEntry.buttonType;
            if (_selectedButton == buttonType)
            {
                return;
            }

            if (!isEnabled)
            {
                RunLockedButtonLockedAction(buttonType);
                return;
            }

            OnPageClosed(_selectedButton);

            SetButtonSelected(buttonType, false);

            OnPageOpen(buttonType, buttonEntry);

            RunButtonAction(buttonType);
        }

        private void OnPageOpen(Buttons buttonType, TabBarButtonEntry buttonEntry)
        {
            var gameData = GlobalData.Instance.GetGameData();


            switch (buttonType)
            {
                case Buttons.DailyChallenges:
                    break;
                case Buttons.Skins:

                    break;
            }
        }

        private static void OnPageClosed(Buttons previouslySelectedButton)
        {
            switch (previouslySelectedButton)
            {
                case Buttons.DailyChallenges:
                    break;
                case Buttons.Skins:
                    break;
            }
        }

        public void ShowInAppShopFromNotEnoughGemsScreen()
        {
            SetActive(true);

            var button = GetButtonForType(GetButtonTypeForScreen(GameplayScreen.InAppShop));
            var buttonEntry = _buttons.FirstOrDefault(r => r.button == button);
            if (buttonEntry == null)
            {
                Debug.LogError("Button not found");
                return;
            }

            var buttonType = buttonEntry.buttonType;
            if (_selectedButton == buttonType)
            {
                return;
            }

            SetButtonSelected(buttonType, false);

            RunButtonAction(buttonType);
        }

        public void SetButtonActive(Buttons buttonType, bool isActive)
        {
            GetButtonEntry(buttonType)!.button.gameObject.SetActive(isActive);
        }

        private void RunButtonAction(Buttons buttonType)
        {
            switch (buttonType)
            {
                case Buttons.Lobby:
                    break;
                case Buttons.Skins:
                    break;
                case Buttons.DailyChallenges:
                    break;
                case Buttons.InAppShop:
                    break;
                case Buttons.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnScreenRequest?.Invoke(buttonType);
        }

        private void RunLockedButtonLockedAction(Buttons buttonType)
        {
            switch (buttonType)
            {
                case Buttons.Skins:

                    break;
                case Buttons.None:
                case Buttons.Lobby:
                case Buttons.InAppShop:
                case Buttons.DailyChallenges:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
            }
        }

        public Buttons GetButtonTypeForScreen(GameplayScreen screen)
        {
            return screen switch
            {
                GameplayScreen.None => Buttons.None,
                GameplayScreen.Game => Buttons.Lobby,
                GameplayScreen.Skins => Buttons.Skins,
                GameplayScreen.InAppShop => Buttons.InAppShop,
                GameplayScreen.Room => Buttons.DailyChallenges,
                _ => throw new ArgumentOutOfRangeException(nameof(screen), screen, null)
            };
        }

        private NewTabBarButton GetButtonForType(Buttons buttonType)
        {
            return GetButtonEntry(buttonType)?.button;
        }

        private TabBarButtonEntry GetButtonEntry(Buttons buttonType)
        {
            return _buttons.FirstOrDefault(r => r.buttonType == buttonType);
        }

        public void UpdateButtonsState()
        {
            var currentLevel = LevelData.GetLevelNumber() + 1;

            UpdateSkinsButtonState();
        }

        private void UpdateSkinsButtonState()
        {
        }

        [Serializable]
        public class TabBarButtonEntry
        {
            public Buttons buttonType;
            public NewTabBarButton button;
        }
    }

    public enum TabBarPinType
    {
        None,
        Pin
    }

    public enum GameplayScreen
    {
        None = -2,
        Skins = -1,
        Game = 0,
        InAppShop = 1,
        Room = 2
    }
}
