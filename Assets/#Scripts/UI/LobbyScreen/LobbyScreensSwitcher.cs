using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class LobbyScreensSwitcher : MonoBehaviour
    {
        [SerializeField] private LobbyScreenHelper[] _lobbyScreenHelpers;

        private Sequence _moveSeq;

        public void SwitchScreen(LobbyScreenType lobbyScreenType, bool force)
        {
            var screenInd = (int)lobbyScreenType;

            if (lobbyScreenType > LobbyScreenType.InAppShop)
            {
                screenInd--;
            }

            var screenWidth = GetScreenWidth();

            _moveSeq?.Kill();
            if (force)
            {
                _moveSeq = null;
                for (var i = 0; i < _lobbyScreenHelpers.Length; i++)
                {
                    var screen = _lobbyScreenHelpers[i];
                    screen.InitActualOffsets(false);
                    var anchoredPos = screen.RectTransform.anchoredPosition;
                    anchoredPos.x = CalcScreenOffset(screen.GetOffset(screenInd), screenInd, screenWidth);
                    screen.RectTransform.anchoredPosition = anchoredPos;
                }

                return;
            }

            var animationSetting = GlobalData.Instance.GetGameData().LobbyTabBarAnimationSetting;
            _moveSeq = DOTween.Sequence().SetLink(gameObject);
            for (var i = 0; i < _lobbyScreenHelpers.Length; i++)
            {
                var screen = _lobbyScreenHelpers[i];
                screen.InitActualOffsets(false);
                var pos1 = CalcScreenOffset(screen.GetOffset(screenInd), screenInd, screenWidth);
                _moveSeq.Join(screen.RectTransform.DOAnchorPosX(pos1, animationSetting.Duration)
                    .SetEase(animationSetting.GetEaseFunction()));
            }
        }

        private float CalcScreenOffset(int offset, int screenInd, float screenWidth)
        {
            return screenWidth * (offset - screenInd);
        }

        public static float GetScreenWidth()
        {
            return 1080f * GetScreenWidthScale();
        }

        public static float GetScreenWidthScale()
        {
            return Mathf.Max(1f, (float)Screen.width / Screen.height * 1920f / 1080f);
        }
    }

    public enum LobbyScreenType
    {
        Skins = -1,
        Lobby = 0,
        InAppShop = 1,
        Challenges = 2
    }
}
