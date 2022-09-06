using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Settings;
using GameManagers;
using Utility;
using SimpleJSONFixed;
using ApplicationManagers;

namespace UI
{
    class InGameMenu: BaseMenu
    {
        public EmoteHandler EmoteHandler;
        public BaseMenu HUDFrameMenu;
        public BasePopup _settingsPopup;
        public BasePopup _createGamePopup;
        public BasePopup _pausePopup;
        public BasePopup _characterPopup;
        public BasePopup _scoreboardPopup;
        public CutsceneDialoguePanel _cutsceneDialoguePanel;
        private LoadingBackgroundPanel _loadingBackgroundPanel;
        private LoadingTipPanel _loadingTipPanel;
        private LoadingProgressPanel _loadingProgressPanel;
        private List<BasePopup> _allPausePopups = new List<BasePopup>();
        public static JSONNode LoadingBackgroundInfo = null;
        public static string Tip;

        public override void Setup()
        {
            base.Setup();
            SetupLoading();
            EmoteHandler = gameObject.AddComponent<EmoteHandler>();
            HUDFrameMenu = ElementFactory.CreateDefaultMenu<HUDFrameMenu>().GetComponent<HUDFrameMenu>();
            HUDFrameMenu.Setup();
            HUDFrameMenu.ApplyScale();
            HideAllMenus();
        }

        public void UpdateLoading(float percentage, bool finished = false)
        {
            percentage = Mathf.Clamp(percentage, 0f, 1f);
            _loadingProgressPanel.Show(percentage);
            if (finished)
                OnFinishLoading();
        }

        private void SetupLoading()
        {
            if (LoadingBackgroundInfo == null)
                LoadingBackgroundInfo = JSON.Parse(AssetBundleManager.LoadText("LoadingBackgroundInfo"));
            _loadingBackgroundPanel = ElementFactory.CreateDefaultPopup<LoadingBackgroundPanel>(transform);
            _loadingProgressPanel = ElementFactory.CreateDefaultPopup<LoadingProgressPanel>(transform);
            JSONNode tips = LoadingBackgroundInfo["Tips"];
            Tip = "Tip: " + tips[Random.Range(0, tips.Count)].Value;
            _loadingTipPanel = ElementFactory.CreateDefaultPopup<LoadingTipPanel>(transform);
            ElementFactory.SetAnchor(_loadingTipPanel.gameObject, TextAnchor.LowerLeft, TextAnchor.LowerLeft, new Vector2(20f, 20f));
            _loadingBackgroundPanel.Show();
            _loadingTipPanel.Show();
            _loadingProgressPanel.Show(0f);
            UpdateLoading(0f);
        }

        private void OnFinishLoading()
        {
            _loadingTipPanel.Hide();
            _loadingProgressPanel.Hide();
            _loadingBackgroundPanel.Hide();
            _characterPopup = ElementFactory.CreateDefaultPopup<CharacterPopup>(transform);
            _scoreboardPopup = ElementFactory.CreateDefaultPopup<ScoreboardPopup>(transform);
            _cutsceneDialoguePanel = ElementFactory.CreateDefaultPopup<CutsceneDialoguePanel>(transform);
            ElementFactory.SetAnchor(_cutsceneDialoguePanel.gameObject, TextAnchor.LowerCenter, TextAnchor.LowerCenter, new Vector2(0f, 20f));
            _popups.Add(_characterPopup);
            _popups.Add(_scoreboardPopup);
        }

        public bool InMenu()
        {
            foreach (BasePopup popup in _popups)
            {
                if (popup.IsActive)
                    return true;
            }
            return EmoteHandler.IsActive;
        }

        public void SetPauseMenu(bool enabled)
        {
            if (enabled && !IsPauseMenuActive())
            {
                HideAllMenus();
                _pausePopup.Show();
            }
            else if (!enabled)
            {
                HideAllMenus();
            }
        }

        public void ToggleScoreboardMenu()
        {
            SetScoreboardMenu(!_scoreboardPopup.gameObject.activeSelf);
        }

        public void SetScoreboardMenu(bool enabled)
        {
            if (enabled && !InMenu())
            {
                HideAllMenus();
                _scoreboardPopup.Show();
            }
            else if (!enabled)
            {
                _scoreboardPopup.Hide();
            }
        }

        public void SetCharacterMenu(bool enabled)
        {
            if (enabled && !InMenu())
            {
                HideAllMenus();
                _characterPopup.Show();
            }
            else if (!enabled)
                _characterPopup.Hide();
        }

        public void ShowCutsceneMenu(string icon, string title, string content)
        {
            _cutsceneDialoguePanel.Show(icon, title, content);
        }

        public void HideCutsceneMenu()
        {
            _cutsceneDialoguePanel.Hide();
        }

        public bool IsPauseMenuActive()
        {
            foreach (BasePopup popup in _allPausePopups)
            {
                if (popup.gameObject.activeSelf)
                    return true;
            }
            return false;
        }

        private void HideAllMenus()
        {
            HideAllPopups();
            EmoteHandler.SetEmoteWheel(false);
        }

        protected override void SetupPopups()
        {
            base.SetupPopups();
            _settingsPopup = ElementFactory.CreateHeadedPanel<SettingsPopup>(transform).GetComponent<BasePopup>();
            _pausePopup = ElementFactory.CreateHeadedPanel<PausePopup>(transform).GetComponent<PausePopup>();
            _createGamePopup = ElementFactory.CreateHeadedPanel<CreateGamePopup>(transform).GetComponent<CreateGamePopup>();
            _popups.Add(_settingsPopup);
            _popups.Add(_pausePopup);
            _popups.Add(_createGamePopup);
            _allPausePopups.Add(_settingsPopup);
            _allPausePopups.Add(_pausePopup);
            _allPausePopups.Add(_createGamePopup);
        }
    }
}
