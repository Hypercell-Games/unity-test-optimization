using UnityEngine;

namespace Unpuzzle
{
    [CreateAssetMenu(fileName = "BgSkinConfig", menuName = MenuPath, order = MenuOrder)]
    public class BgSkinConfig : ScriptableObject
    {
        private const string MenuPath = "Configs/BgSkin";
        private const int MenuOrder = int.MinValue + 106;

        [SerializeField] private string _key;
        [SerializeField] private bool _isDefaultUnlock;
        [SerializeField] private Sprite _thumb;
        [SerializeField] private Material _skyBox;
        [SerializeField] private ParticleSystem _particles;

        public Sprite Thumb => _thumb;

        public ParticleSystem ParticleSystem => _particles;

        public Material SkyBox => _skyBox;

        public bool IsIntroduced
        {
            set => PlayerPrefs.SetInt($"skins.introduced.{_key}", value ? 1 : 0);
            get => _isDefaultUnlock || PlayerPrefs.GetInt($"skins.introduced.{_key}", 0) != 0;
        }

        public bool IsUnlocked()
        {
            return _isDefaultUnlock || PlayerPrefs.GetInt($"skins.unlocked.{_key}", 0) != 0;
        }

        public void Unlock()
        {
            PlayerPrefs.SetInt($"skins.unlocked.{_key}", 1);
        }
    }
}
