using UnityEngine;

public interface IGalleryItem
{
    public string GetName();

    public Sprite GetThumb();

    public Sprite GetBigThumb();

    public bool IsUnlocked();

    public bool SetUnlocked();

    public bool IsIntroduced();

    public bool SetIntroduced();
}
