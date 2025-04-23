using UnityEngine;
using UnityEngine.UI;

public class KeyController : MonoBehaviour
{
    [SerializeField] private Image unfilled;

    [SerializeField] private Image filled;

    public bool IsFilled => filled.IsActive();

    public void SetFilled(bool isFilled)
    {
        unfilled.gameObject.SetActive(!isFilled);
        filled.gameObject.SetActive(isFilled);
    }
}
