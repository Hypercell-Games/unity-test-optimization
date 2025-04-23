public abstract class GameMode
{
    protected GameMode(GameData gameData)
    {
        GameData = gameData;
    }

    public GameData GameData { get; private set; }

    public abstract string GetSceneName();

    public abstract void OnLevelStart();
    public abstract void OnLevelRestart();
    public abstract void OnLevelComplete();
}
