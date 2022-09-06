using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ApplicationManagers;
using Settings;

namespace UI
{
    class MainMenu: BaseMenu
    {
        public BasePopup _createGamePopup;
        public BasePopup _multiplayerMapPopup;
        public BasePopup _settingsPopup;
        public BasePopup _toolsPopup;
        public BasePopup _multiplayerRoomListPopup;
        public BasePopup _editProfilePopup;
        public BasePopup _leaderboardPopup;
        public BasePopup _socialPopup;
        public BasePopup _helpPopup;
        public BasePopup _questPopup;
        public BasePopup _tutorialPopup;
        protected Text _multiplayerStatusLabel;
        protected string _lastButtonClicked;

        public override void Setup()
        {
            base.Setup();
            // if (!SettingsManager.GraphicsSettings.AnimatedIntro.Value)
            if (true)
            {
                GameObject background = ElementFactory.InstantiateAndBind(transform, "MainBackground");
                background.AddComponent<IgnoreScaler>();
            }
            SetupIntroPanel();
            SetupLabels();
        }

        public void ShowMultiplayerRoomListPopup()
        {
            HideAllPopups();
            _multiplayerRoomListPopup.Show();
        }

        public void ShowMultiplayerMapPopup()
        {
            HideAllPopups();
            _multiplayerMapPopup.Show();
        }

        protected override void SetupPopups()
        {
            base.SetupPopups();
            _createGamePopup = ElementFactory.CreateHeadedPanel<CreateGamePopup>(transform).GetComponent<CreateGamePopup>();
            _multiplayerMapPopup = ElementFactory.InstantiateAndSetupPanel<MultiplayerMapPopup>(transform, "MultiplayerMapPopup").
                GetComponent<BasePopup>();
            _editProfilePopup = ElementFactory.CreateHeadedPanel<EditProfilePopup>(transform).GetComponent<BasePopup>();
            _settingsPopup = ElementFactory.CreateHeadedPanel<SettingsPopup>(transform).GetComponent<BasePopup>();
            _toolsPopup = ElementFactory.CreateHeadedPanel<ToolsPopup>(transform).GetComponent<BasePopup>();
            _multiplayerRoomListPopup = ElementFactory.InstantiateAndSetupPanel<MultiplayerRoomListPopup>(transform, "MultiplayerRoomListPopup").
                GetComponent<BasePopup>();
            _leaderboardPopup = ElementFactory.CreateHeadedPanel<LeaderboardPopup>(transform).GetComponent<BasePopup>();
            _socialPopup = ElementFactory.CreateHeadedPanel<SocialPopup>(transform).GetComponent<BasePopup>();
            _helpPopup = ElementFactory.CreateHeadedPanel<HelpPopup>(transform).GetComponent<BasePopup>();
            _questPopup = ElementFactory.CreateHeadedPanel<QuestPopup>(transform).GetComponent<BasePopup>();
            _tutorialPopup = ElementFactory.CreateHeadedPanel<TutorialPopup>(transform).GetComponent<BasePopup>();
            _popups.Add(_createGamePopup);
            _popups.Add(_multiplayerMapPopup);
            _popups.Add(_editProfilePopup);
            _popups.Add(_settingsPopup);
            _popups.Add(_toolsPopup);
            _popups.Add(_multiplayerRoomListPopup);
            _popups.Add(_leaderboardPopup);
            _popups.Add(_socialPopup);
            _popups.Add(_helpPopup);
            _popups.Add(_questPopup);
            _popups.Add(_tutorialPopup);
        }

        private void SetupIntroPanel()
        {
            GameObject introPanelBottom = ElementFactory.InstantiateAndBind(transform, "IntroPanelBottom");
            ElementFactory.SetAnchor(introPanelBottom, TextAnchor.LowerRight, TextAnchor.LowerRight, new Vector2(-10f, 30f));
            foreach (Transform transform in introPanelBottom.transform.Find("Buttons"))
            {
                IntroButton introButton = transform.gameObject.AddComponent<IntroButton>();
                introButton.onClick.AddListener(() => OnIntroButtonClick(introButton.name));
            }
            GameObject introPanelTop = ElementFactory.InstantiateAndBind(transform, "IntroPanelTop");
            ElementFactory.SetAnchor(introPanelTop, TextAnchor.UpperRight, TextAnchor.UpperRight, new Vector2(-10f, -10f));
            foreach (Transform transform in introPanelTop.transform)
            {
                Button button = transform.gameObject.GetComponent<Button>();
                button.onClick.AddListener(() => OnIntroButtonClick(transform.name));
                button.colors = UIManager.GetThemeColorBlock("MainMenu", "IntroButton", "");
            }
        }

        private void SetupLabels()
        {
            _multiplayerStatusLabel = ElementFactory.CreateDefaultLabel(transform, ElementStyle.Default, string.Empty).GetComponent<Text>();
            ElementFactory.SetAnchor(_multiplayerStatusLabel.gameObject, TextAnchor.UpperLeft, TextAnchor.UpperLeft, new Vector2(20f, -20f));
            _multiplayerStatusLabel.color = Color.white;
            Text versionText = ElementFactory.CreateDefaultLabel(transform, ElementStyle.Default, string.Empty).GetComponent<Text>();
            ElementFactory.SetAnchor(versionText.gameObject, TextAnchor.LowerCenter, TextAnchor.LowerCenter, new Vector2(0f, 20f));
            versionText.color = Color.white;
            if (ApplicationConfig.DevelopmentMode)
                versionText.text = "RC MOD DEVELOPMENT VERSION";
            else
                versionText.text = "RC Mod Version " + ApplicationConfig.GameVersion + ".";
            versionText.text = "";
        }

        private void Update()
        {
            if (_multiplayerStatusLabel != null)
            {
                if (_multiplayerMapPopup.IsActive || _multiplayerRoomListPopup.IsActive)
                {
                    _multiplayerStatusLabel.text = PhotonNetwork.connectionStateDetailed.ToString();
                    if (PhotonNetwork.connected)
                    {
                        _multiplayerStatusLabel.text += " ping:" + PhotonNetwork.GetPing();
                    }
                }
                else
                    _multiplayerStatusLabel.text = "";
            }
        }

        private bool IsPopupActive()
        {
            foreach (BasePopup popup in _popups)
            {
                if (popup.IsActive)
                    return true;
            }
            return false;
        }

        private void OnIntroButtonClick(string name)
        {
            bool isPopupAactive = IsPopupActive();
            HideAllPopups();
            if (isPopupAactive && _lastButtonClicked == name)
                return;
            _lastButtonClicked = name;
            switch (name)
            {
                case "TutorialButton":
                    _tutorialPopup.Show();
                    break;
                case "SingleplayerButton":
                    ((CreateGamePopup)_createGamePopup).Show(false);
                    break;
                case "MultiplayerButton":
                    _multiplayerMapPopup.Show();
                    break;
                case "ProfileButton":
                    _editProfilePopup.Show();
                    break;
                case "SettingsButton":
                    _settingsPopup.Show();
                    break;
                case "ToolsButton":
                    _toolsPopup.Show();
                    break;
                case "QuitButton":
                    Application.Quit();
                    break;
                case "QuestButton":
                    _questPopup.Show();
                    break;
                case "LeaderboardButton":
                    _leaderboardPopup.Show();
                    break;
                case "SocialButton":
                    _socialPopup.Show();
                    break;
                case "HelpButton":
                    _helpPopup.Show();
                    break;
                case "PatreonButton":
                    ExternalLinkPopup.Show("https://www.patreon.com/aottg2");
                    break;
            }
        }
    }
}
