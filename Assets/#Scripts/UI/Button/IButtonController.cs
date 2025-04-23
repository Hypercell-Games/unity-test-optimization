using System;

public interface IButtonController
{
    event Action onButtonClicked;
    event Action onButtonPressed;
    event Action onButtonUnPressed;

    void OnButtonPressed();
    void OnButtonUnPressed();
    void OnButtonClicked();
}
