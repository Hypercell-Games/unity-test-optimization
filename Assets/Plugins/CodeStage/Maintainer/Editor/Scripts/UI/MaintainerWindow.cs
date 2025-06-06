#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
    using System;
    using Cleaner;
    using Settings;
    using UnityEditor;
    using UnityEngine;

    internal class MaintainerWindow : EditorWindow
    {
        internal enum MaintainerTab
        {
            Issues = 0,
            Cleaner = 1,
            References = 2,
            About = 3
        }

        private static MaintainerWindow windowInstance;

        [NonSerialized]
        private MaintainerTab currentTab;

        [NonSerialized]
        private GUIContent[] tabsCaptions;

        [NonSerialized]
        private IssuesTab issuesTab;

        [NonSerialized]
        private CleanerTab cleanerTab;

        [NonSerialized]
        private ReferencesTab referencesTab;

        [NonSerialized]
        private AboutTab aboutTab;

        [NonSerialized]
        private bool inited;

        public static MaintainerWindow Create()
        {
            windowInstance = GetWindow<MaintainerWindow>(false, "Maintainer", true);
            windowInstance.titleContent = new GUIContent(" Maintainer", CSIcons.Maintainer);
            windowInstance.Focus();

            return windowInstance;
        }

        public static void ShowForScreenshot()
        {
            var window = Create();
            window.minSize = new Vector2(1024, 768);
        }

        public static void ShowIssues()
        {
            Create(MaintainerTab.Issues).Repaint();
        }

        public static void ShowCleaner()
        {
            AssetPreview.SetPreviewTextureCacheSize(50);
            ShowProjectCleanerWarning();

            Create(MaintainerTab.Cleaner).Repaint();
        }

        public static void ShowAssetReferences()
        {
            UserSettings.Instance.referencesFinder.selectedTab = ReferenceFinderTab.Project;
            Create(MaintainerTab.References).Repaint();
        }

        public static void ShowObjectReferences()
        {
            UserSettings.Instance.referencesFinder.selectedTab = ReferenceFinderTab.Scene;
            Create(MaintainerTab.References).Repaint();
        }

        public static void ShowAbout()
        {
            Create(MaintainerTab.About).Repaint();
        }

        public static void ShowNotification(string text)
        {
            EditorApplication.delayCall += () =>
            {
                if (windowInstance)
                {
                    windowInstance.ShowNotification(new GUIContent(text));
                }
            };
        }

        public static void ClearNotification()
        {
            if (windowInstance)
            {
                windowInstance.RemoveNotification();
            }
        }

        public static void RepaintInstance()
        {
            if (windowInstance)
            {
                windowInstance.Repaint();
            }
        }

        private static MaintainerWindow Create(MaintainerTab tab)
        {
            windowInstance = Create();

            if (windowInstance.currentTab != tab)
            {
                windowInstance.currentTab = UserSettings.Instance.selectedTab = tab;
            }
            windowInstance.Refresh(true);

            return windowInstance;
        }

        private static void ShowProjectCleanerWarning()
        {
            if (UserSettings.Cleaner.firstTime)
            {
                EditorUtility.DisplayDialog(ProjectCleaner.ModuleName, "Please note, this module can remove files and folders physically from your system.\nPlease always make a backup of your project before using Project Cleaner!\nUse it on your own peril, author is not responsible for any damage made due to the module usage!\nThis message shows only once.", "Dismiss");
                UserSettings.Cleaner.firstTime = false;
            }
        }

        private void Init()
        {
            if (inited) return;

            CreateTabs();

            Repaint();
            currentTab = UserSettings.Instance.selectedTab;

            Refresh(false);
            inited = true;
        }

        private void CreateTabs()
        {
            if (issuesTab == null)
                issuesTab = new IssuesTab(this);

            if (cleanerTab == null)
                cleanerTab = new CleanerTab(this);

            if (referencesTab == null)
                referencesTab = new ReferencesTab(this);

            if (aboutTab == null)
                aboutTab = new AboutTab(this);

            if (tabsCaptions == null)
            {
                tabsCaptions = new[] { issuesTab.Caption, cleanerTab.Caption, referencesTab.Caption, aboutTab.Caption };
            }
        }

        public void Refresh(bool newData)
        {
            switch (currentTab)
            {
                case MaintainerTab.Issues:
                    issuesTab.Refresh(newData);
                    break;
                case MaintainerTab.Cleaner:
                    cleanerTab.Refresh(newData);
                    break;
                case MaintainerTab.References:
                    referencesTab.Refresh(newData);
                    break;
                case MaintainerTab.About:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Awake()
        {
            EditorApplication.quitting += OnQuit;
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            windowInstance = this;
            Init();
        }

        private void OnLostFocus()
        {
            ProjectSettings.Save();
        }

        private void OnGUI()
        {
            UIHelpers.SetupStyles();

            UserSettings.Instance.scroll =
                GUILayout.BeginScrollView(UserSettings.Instance.scroll, false, false);

            EditorGUI.BeginChangeCheck();
            currentTab = (MaintainerTab)GUILayout.Toolbar((int)currentTab, tabsCaptions, GUILayout.ExpandWidth(false), GUILayout.Height(21));
            if (EditorGUI.EndChangeCheck())
            {
                if (currentTab == MaintainerTab.Cleaner) ShowProjectCleanerWarning();
                UserSettings.Instance.selectedTab = currentTab;

                Refresh(false);
            }

            switch (currentTab)
            {
                case MaintainerTab.Issues:
                    issuesTab.Draw();
                    break;
                case MaintainerTab.Cleaner:
                    cleanerTab.Draw();
                    break;
                case MaintainerTab.References:
                    referencesTab.Draw();
                    break;
                case MaintainerTab.About:
                    aboutTab.Draw();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GUILayout.EndScrollView();
        }

        private void OnQuit()
        {
            ProjectSettings.Save();
        }
    }
}
