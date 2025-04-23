using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalSpriteChanger : MonoBehaviour
{
    [Header("Image")] [SerializeField] private Image _smileImage;

    [Header("Smiles")] [SerializeField] private List<Sprite> _smilesSprites;

    public void SetRandomSprite()
    {
        var randomSprite = GetRandomSprite();

        if (_smileImage.sprite == randomSprite)
        {
            return;
        }

        _smileImage.sprite = randomSprite;
    }

    private Sprite GetRandomSprite()
    {
        return _smilesSprites[Random.Range(0, _smilesSprites.Count)];
    }
}
