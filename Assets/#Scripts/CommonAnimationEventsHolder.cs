using System;
using UnityEngine;

public class CommonAnimationEventsHolder : MonoBehaviour
{
    public event Action Event0;

    public void SetOnEvent0(Action action)
    {
        Event0 = action;
    }

    public void OnEvent0()
    {
        if (Event0 != null)
        {
            Event0();
        }
    }
}
