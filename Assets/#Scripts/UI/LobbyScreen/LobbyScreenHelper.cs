using UnityEngine;

namespace Unpuzzle
{
    public class LobbyScreenHelper : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private LobbyScreenType _offset0 = 0;
        [SerializeField] private LobbyScreenType _offset1 = 0;

        private int _actualOffset0;
        private int _actualOffset1;

        public RectTransform RectTransform => _rectTransform;

        public void InitActualOffsets(bool inAppShopUnlocked)
        {
            _actualOffset0 = (int)_offset0;
            _actualOffset1 = (int)_offset1;
            if (!inAppShopUnlocked)
            {
                if (_offset0 > LobbyScreenType.InAppShop)
                {
                    _actualOffset0--;
                }

                if (_offset1 > LobbyScreenType.InAppShop)
                {
                    _actualOffset1--;
                }
            }
        }

        public int GetOffset(int screenInd)
        {
            var offset0 = Mathf.Min(_actualOffset0, _actualOffset1);
            var offset1 = Mathf.Max(_actualOffset0, _actualOffset1);
            return Mathf.Clamp(screenInd, offset0, offset1);
        }
    }
}
