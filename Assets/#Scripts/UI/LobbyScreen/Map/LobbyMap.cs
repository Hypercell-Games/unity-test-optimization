using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle.Game;
using Unpuzzle.Game.Data;
using Unpuzzle.UI.NewTabBar;

namespace Unpuzzle
{
    public class LobbyMap : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private MapItemLevel _mapItemLevelPrefab;
        [SerializeField] private MapItemDot _mapItemDotPrefab;
        [SerializeField] private MapItemBoss _mapItemBossPrefab;
        [SerializeField] private RectTransform _contentRectTransform;
        [SerializeField] private RectTransform _flyCups;

        public void Init(LobbyScreen lobbyScreen,
            NewTabBarScreen tabBarScreen)
        {
            var startPos1 = 289f;
            var startPos0 = startPos1 + 300f + 30f;
            var startPos = startPos0 + 1000f;
            var currentLevelConfig = LevelsConfigs.GetRemoteLevelNameSimple(LevelData.GetLevelNumber());
            if (currentLevelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
            {
                startPos1 += 60f;
                startPos0 += 60f;
            }

            var currentLevel = LevelData.GetLevelNumber() + 1;

            var mapItemPrev = (MapItemLevel)null;
            var mapItemCurrent = (MapItemLevel)null;
            var dots = new List<IDotAnimation>();


            var posY = startPos;
            var upperItems = new List<RectTransform>();
            var mediumBossDot = (RectTransform)null;
            for (var i = 0; i < 30; i++)
            {
                var level = currentLevel + i;
                var item = Instantiate(_mapItemLevelPrefab, _contentRectTransform);
                var levelConfig = LevelsConfigs.GetRemoteLevelNameSimple(level - 1);
                item.Init(currentLevel, level, levelConfig.LevelDifficultyType, false);
                var itemRT = item.GetComponent<RectTransform>();
                itemRT.anchoredPosition = Vector2.up * posY;

                if (i == 0)
                {
                    mapItemCurrent = item;
                    posY += 145f;
                    if (levelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
                    {
                        posY += 48f;
                    }
                }
                else
                {
                    posY += 96f;
                    if (levelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
                    {
                        posY += 40f;
                    }
                }

                upperItems.Add(itemRT);
                for (var j = 0; j < 3; j++)
                {
                    if (j == 1)
                    {
                        posY += 30f;
                        var item1 = Instantiate(_mapItemBossPrefab, _contentRectTransform);
                        item1.Init(false, false, false, _flyCups);
                        var item1RT = item1.GetComponent<RectTransform>();
                        item1RT.anchoredPosition = Vector2.up * posY;
                        item1.transform.SetAsFirstSibling();
                        posY += 39f + 30f;
                        upperItems.Add(item1RT);
                    }
                    else
                    {
                        var item1 = Instantiate(_mapItemDotPrefab, _contentRectTransform);
                        item1.Init(false);
                        var item1RT = item1.GetComponent<RectTransform>();
                        item1RT.anchoredPosition = Vector2.up * posY;
                        item1.transform.SetAsFirstSibling();
                        upperItems.Add(item1RT);
                        if (j < 2)
                        {
                            posY += 39f;
                        }
                        else
                        {
                            posY += 105.5f;
                            var nextLevelConfig = LevelsConfigs.GetRemoteLevelNameSimple(level);
                            if (nextLevelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
                            {
                                posY += 7f;
                            }
                        }
                    }
                }
            }


            posY = startPos;
            for (var i = 0; i < 10; i++)
            {
                var level = currentLevel - i;
                if (level < 1)
                {
                    continue;
                }

                var levelConfig = LevelsConfigs.GetRemoteLevelNameSimple(level - 1);
                if (i > 0)
                {
                    if (levelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
                    {
                        posY -= 53f;
                    }

                    var item = Instantiate(_mapItemLevelPrefab, _contentRectTransform);

                    item.Init(currentLevel, level, levelConfig.LevelDifficultyType, false);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.up * posY;
                    if (i == 1)
                    {
                        mapItemPrev = item;
                    }
                }

                if (i == 0)
                {
                    posY -= 148f;
                    if (levelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
                    {
                        posY -= 13f;
                    }
                }
                else
                {
                    posY -= 105f;
                    if (levelConfig.LevelDifficultyType != LevelDifficultyType.Normal)
                    {
                        posY -= 5f;
                    }
                }

                if (level < 2)
                {
                    continue;
                }

                for (var j = 0; j < 3; j++)
                {
                    if (j == 1)
                    {
                        posY -= 30f;
                        var item1 = Instantiate(_mapItemBossPrefab, _contentRectTransform);
                        item1.Init(false, false, true, _flyCups);
                        var item1RT = item1.GetComponent<RectTransform>();
                        item1RT.anchoredPosition = Vector2.up * posY;
                        item1.transform.SetAsFirstSibling();
                        posY -= 39f + 30f;
                        if (i == 0)
                        {
                            mediumBossDot = item1RT;
                            upperItems.Add(item1RT);
                            dots.Add(item1);
                        }
                    }
                    else
                    {
                        var item1 = Instantiate(_mapItemDotPrefab, _contentRectTransform);
                        item1.Init(true);
                        var item1RT = item1.GetComponent<RectTransform>();
                        item1RT.anchoredPosition = Vector2.up * posY;
                        item1.transform.SetAsFirstSibling();
                        if (j < 2)
                        {
                            posY -= 39f;
                        }
                        else
                        {
                            posY -= 96f + 12f;
                        }

                        if (i == 0)
                        {
                            upperItems.Add(item1RT);
                            dots.Add(item1);
                        }
                    }
                }
            }

            if (mediumBossDot)
            {
                startPos0 += 60f;
                startPos1 += 60f;
            }

            var contentSizeDelta = _contentRectTransform.sizeDelta = _contentRectTransform.sizeDelta.AddY(3000f);
            var height = contentSizeDelta.y - _scrollRect.viewport.sizeDelta.y;

            var normPos1 = Mathf.InverseLerp(0f, height, startPos - startPos1);
            if (currentLevel < 2 || !GameLogicUtil.ShouldShowLobbyLevelTransition)
            {
                _scrollRect.verticalNormalizedPosition = normPos1;

                return;
            }

            lobbyScreen.LockControl();

            GameLogicUtil.ShouldShowLobbyLevelTransition = false;
            var normPos0 = Mathf.InverseLerp(0f, height, startPos - startPos0);
            _scrollRect.verticalNormalizedPosition = normPos0;

            mapItemPrev.InitPrevAnimation();

            StartCoroutine(Co_Animate());

            IEnumerator Co_Animate()
            {
                yield return null;

                mapItemCurrent.InitCurrentAnimation();

                var canShowLobbyMapAnimation = false;

                yield return new WaitWhile(() => !canShowLobbyMapAnimation);

                Animate();
            }

            void Animate()
            {
                var mainSeq = DOTween.Sequence().SetLink(gameObject);
                mainSeq.InsertCallback(0.1f, mapItemPrev.ShowPrevAnimation);

                dots.Reverse();
                var durationSum = 0f;
                upperItems.Reverse();
                for (var i = 0; i < dots.Count; i++)
                {
                    var dot = dots[i];
                    dot.InitDotAnimation();

                    var animInfo = dot.ShowDotAnimation(() =>
                    {
                        var seq = DOTween.Sequence().SetLink(gameObject);
                        upperItems.RemoveAt(0);
                        upperItems.ForEach(i =>
                        {
                            seq.Join(i.DOAnchorPosY(i.anchoredPosition.y - 30f, 0.3f));
                        });
                        seq.OnComplete(() =>
                        {
                            seq = DOTween.Sequence().SetLink(gameObject);
                            upperItems.RemoveAt(0);
                            upperItems.ForEach(i =>
                            {
                                seq.Join(i.DOAnchorPosY(i.anchoredPosition.y - 30f, 0.3f));
                            });
                        });
                    }, mainSeq, tabBarScreen);
                    var dotSeq = animInfo.seq;
                    var duration = animInfo.duration;
                    mainSeq.Insert(0.3f + durationSum + 0.3f, dotSeq);
                    durationSum += duration;
                }


                mainSeq.InsertCallback(0.3f + durationSum + 0.3f, () =>
                {
                    mapItemCurrent.ShowCurrentAnimation(() =>
                    {
                        var seq = DOTween.Sequence()
                            .SetLink(gameObject)
                            .Append(_scrollRect.DOVerticalNormalizedPos(normPos1, 0.6f).SetEase(Ease.OutCubic))
                            .OnComplete(() =>
                            {
                                lobbyScreen.ShinePlayButton();
                            });
                        lobbyScreen.UnlockControl();
                    });
                });
            }
        }
    }
}
