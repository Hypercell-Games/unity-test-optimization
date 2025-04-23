using UnityEngine;

namespace Unpuzzle
{
    public class EditortTestLevelBuild : MonoBehaviour
    {
        [SerializeField] private TextAsset _levelData;
        [SerializeField] private LevelBuilder _levelBuilder;

        [ContextMenu("Build Level")]
        private void CreateLevel()
        {
            var levelData = JsonUtility.FromJson<PinOutLevelData>(_levelData.text);

            var level = _levelBuilder.GetLevel(levelData);

            level.transform.position = transform.position;
        }
    }
}
