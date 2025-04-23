public static class PauseUtil
{
    private static int BossLevelPausedInd { get; set; }
    public static bool IsBossLevelPaused => BossLevelPausedInd > 0;

    public static void BossLevelPaused()
    {
        BossLevelPausedInd++;
    }

    public static void BossLevelUnpaused()
    {
        BossLevelPausedInd--;
    }
}
