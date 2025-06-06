#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Routines;
    using UnityEditor;
    using Debug = UnityEngine.Debug;
    using Settings;
    using Tools;
    using UI;

    /// <summary>
    /// Allows to find issues in your Unity project. See readme for details.
    /// </summary>
    public static class IssuesFinder
    {
        internal const string ModuleName = "Issues Finder";
        private const string ProgressCaption = ModuleName + ": phase {0} of {1} item {2} of {3}";

        internal static bool operationCanceled;

        internal static CSSceneTools.OpenSceneResult lastOpenSceneResult;

        private static int recordsToFixCount;

        #region public methods

        /////////////////////////////////////////////////////////////////////////
        // public methods
        /////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts issues search in opened scenes excluding file assets and project settings.
        /// </summary>
        /// Changes issues search settings and calls StartSearch() after that.
        /// <param name="showResults">Shows results in the %Maintainer window if true.</param>
        /// <returns>Array of IssueRecords in case you wish to manually iterate over them and make custom report.</returns>
        public static IssueRecord[] StartSearchInOpenedScenes(bool showResults)
        {
            ProjectSettings.Issues.scenesSelection = IssuesFinderSettings.ScenesSelection.OpenedScenesOnly;
            ProjectSettings.Issues.lookInScenes = true;
            ProjectSettings.Issues.lookInAssets = false;
            ProjectSettings.Issues.scanGameObjects = true;
            ProjectSettings.Issues.lookInProjectSettings = false;

            return StartSearch(showResults);
        }

        /// <summary>
        /// Starts issues search and generates report. %Maintainer window is not shown.
        /// Useful when you wish to integrate %Maintainer in your build pipeline.
        /// </summary>
        /// <returns>%Issues report, similar to the exported report from the %Maintainer window.</returns>
        public static string SearchAndReport()
        {
            var foundIssues = StartSearch(false);
            return ReportsBuilder.GenerateReport(ModuleName, foundIssues);
        }

        /// <summary>
        /// Starts search with current settings.
        /// </summary>
        /// <param name="showResults">Shows results in the %Maintainer window if true.</param>
        /// <returns>Array of IssueRecords in case you wish to manually iterate over them and make custom report.</returns>
        public static IssueRecord[] StartSearch(bool showResults)
        {
            if (!ProjectSettings.Issues.lookInScenes && !ProjectSettings.Issues.lookInAssets &&
                !ProjectSettings.Issues.lookInProjectSettings)
            {
                MaintainerWindow.ShowNotification("Nowhere to search!");
                return null;
            }

            if (ProjectSettings.Issues.lookInScenes && ProjectSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.OpenedScenesOnly)
            {
                if (!CSSceneTools.SaveCurrentModifiedScenes(true))
                {
                    Debug.Log(Maintainer.ConstructLog("Issues search canceled by user!"));
                    return null;
                }
            }

            var issues = new List<IssueRecord>();

            PrepareToBatchOperation();

            try
            {
                var sw = Stopwatch.StartNew();

                CSTraverseTools.ClearStats();

                var targetAssets = TargetCollector.CollectTargetAssets(out operationCanceled);
                /*foreach (var targetAsset in targetAssets)
				{
					Debug.Log(targetAsset.Path);
				}*/

                if (!operationCanceled)
                {
                    TargetProcessor.SetIssuesList(issues);
                    TargetProcessor.ProcessTargetAssets(targetAssets);
                }

                sw.Stop();

                if (!operationCanceled)
                {
                    var checkedAssets = targetAssets.Length;
                    var traverseStats = CSTraverseTools.GetStats();

                    var result = string.Format(CultureInfo.InvariantCulture, "found issues: {0}\n" +
                                               "Seconds: {1:0.000}; Assets: {2}; Game Objects: {3}; Components: {4}; Properties: {5}",
                        issues.Count, sw.Elapsed.TotalSeconds, checkedAssets, traverseStats.gameObjectsTraversed,
                        traverseStats.componentsTraversed, traverseStats.propertiesTraversed);

                    Debug.Log(Maintainer.ConstructLog(result, ModuleName));

                    SearchResultsStorage.IssuesSearchResults = issues.ToArray();
                    if (showResults)
                        MaintainerWindow.ShowIssues();
                }
                else
                {
                    Debug.Log(Maintainer.ConstructLog("Search canceled by user!", ModuleName));
                }
            }
            catch (Exception e)
            {
                Maintainer.PrintExceptionForSupport("Something went wrong while looking for issues!", ModuleName, e);
            }

            EditorUtility.ClearProgressBar();

            return issues.ToArray();
        }

        /// <summary>
        /// Starts fix of the issues found with StartSearch() method.
        /// </summary>
        /// <param name="recordsToFix">Pass records you wish to fix here or leave null to let it load last search results.</param>
        /// <param name="showResults">Shows results in the %Maintainer window if true.</param>
        /// <param name="showConfirmation">Shows confirmation dialog before performing fix if true.</param>
        /// <returns>Array of IssueRecords which were fixed up.</returns>
        public static IssueRecord[] StartFix(IssueRecord[] recordsToFix = null, bool showResults = true,
            bool showConfirmation = true)
        {
            var records = recordsToFix;
            if (records == null)
            {
                records = SearchResultsStorage.IssuesSearchResults;
            }

            if (records.Length == 0)
            {
                Debug.Log(Maintainer.ConstructLog("Nothing to fix!"));
                return null;
            }

            recordsToFixCount = 0;

            foreach (var record in records)
            {
                if (record.selected) recordsToFixCount++;
            }

            if (recordsToFixCount == 0)
            {
                EditorUtility.DisplayDialog(ModuleName, "Please select issues to fix!", "Ok");
                return null;
            }

            if (!CSSceneTools.SaveCurrentModifiedScenes(false))
            {
                Debug.Log(Maintainer.ConstructLog("Issues batch fix canceled by user!"));
                return null;
            }

            if (showConfirmation && !EditorUtility.DisplayDialog("Confirmation",
                    "Do you really wish to let Maintainer automatically fix " + recordsToFixCount + " issues?\n" +
                    Maintainer.DataLossWarning, "Go for it!", "Cancel"))
            {
                return null;
            }

            try
            {
                var sw = Stopwatch.StartNew();

                var fixedRecords = new List<IssueRecord>(records.Length);
                var notFixedRecords = new List<IssueRecord>(records.Length);

                PrepareToBatchOperation();

                lastOpenSceneResult = null;
                CSEditorTools.lastRevealSceneOpenResult = null;

                IssuesFixer.FixRecords(records);

                foreach (var record in records)
                {
                    if (record.fixResult != null && record.fixResult.Success)
                    {
                        fixedRecords.Add(record);
                    }
                    else
                    {
                        notFixedRecords.Add(record);
                    }
                }

                records = notFixedRecords.ToArray();

                sw.Stop();

                if (!operationCanceled)
                {
                    var results = fixedRecords.Count +
                                  " issues fixed in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
                                  " seconds";

                    Debug.Log(Maintainer.ConstructLog("Results: " + results, ModuleName));
                    MaintainerWindow.ShowNotification(results);
                }
                else
                {
                    Debug.Log(Maintainer.ConstructLog("Fix canceled by user!", ModuleName));
                }

                if (lastOpenSceneResult != null)
                {
                    CSSceneTools.SaveScene(lastOpenSceneResult.scene);
                    CSSceneTools.CloseOpenedSceneIfNeeded(lastOpenSceneResult);
                    lastOpenSceneResult = null;
                }

                SearchResultsStorage.IssuesSearchResults = records;
                if (showResults)
                    MaintainerWindow.ShowIssues();

                return fixedRecords.ToArray();
            }
            catch (Exception e)
            {
                Maintainer.PrintExceptionForSupport("Something went wrong while fixing issues!", ModuleName, e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return null;
        }

        #endregion

        internal static bool ShowProgressBar(int currentPhase, int totalPhases, int currentItem, int totalItems, string info)
        {
            return ShowProgressBar(currentPhase, totalPhases, currentItem, totalItems, info, (float)currentItem / totalItems);
        }

        internal static bool ShowProgressBar(int currentPhase, int totalPhases, int currentItem, int totalItems, string info, float progress)
        {
            return EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, totalPhases, currentItem + 1, totalItems), info, progress);
        }

        private static void PrepareToBatchOperation()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            lastOpenSceneResult = null;
            CSEditorTools.lastRevealSceneOpenResult = null;
            operationCanceled = false;
        }

        #region fixer

        /////////////////////////////////////////////////////////////////////////
        // fixer
        /////////////////////////////////////////////////////////////////////////



        #endregion
    }
}
