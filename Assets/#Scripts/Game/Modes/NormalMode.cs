using Game;

public class NormalMode : GameMode
{
    public NormalMode(GameData gameData) : base(gameData)
    {
    }

    public bool BossLevel { get; private set; }
    public bool ChallengeLevel { get; private set; }

    public int ChallengeLevelIndex { get; private set; }

    public override string GetSceneName()
    {
        return Scenes.GAME;
    }

    public override void OnLevelRestart() { }

    public override void OnLevelStart() { }

    public override void OnLevelComplete() { }
}
