namespace Unpuzzle
{
    public class NormalGameController : AbstractGameController
    {
        protected override void OnInit(LevelOptions levelOptions)
        {
            base.OnInit(levelOptions);

            _gameScreen.InitNormalLevel();
        }

        protected override void OnCompleteLevel(LevelProgress levelProgress)
        {
            base.OnCompleteLevel(levelProgress);

            LevelsController.Instance.CompleteLevel(ELevelCompleteReason.WIN, levelProgress);
        }
    }
}
