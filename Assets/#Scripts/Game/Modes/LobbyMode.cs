using Game;
using Unpuzzle;

public class LobbyMode : GameMode
{
    public LobbyMode(GameData gameData) : base(gameData)
    {
        LobbyScreenType = LobbyScreenType.Lobby;
    }

    public LobbyScreenType LobbyScreenType { get; private set; }
    public int ChallengeLevelIndex { get; private set; }

    public static LobbyMode CreateChallengesMode(GameData gameData, int levelIndex)
    {
        var lobbyMode = new LobbyMode(gameData)
        {
            LobbyScreenType = LobbyScreenType.Challenges, ChallengeLevelIndex = levelIndex
        };
        return lobbyMode;
    }

    public override string GetSceneName()
    {
        return Scenes.LOBBY;
    }

    public override void OnLevelRestart() { }

    public override void OnLevelStart() { }

    public override void OnLevelComplete() { }
}
