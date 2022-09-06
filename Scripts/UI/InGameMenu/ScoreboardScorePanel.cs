using Map;
using Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using ApplicationManagers;
using GameManagers;


namespace UI
{
    class ScoreboardScorePanel: ScoreboardCategoryPanel
    {
        private List<Transform> _rows = new List<Transform>();
        private Transform _header;
        private PhotonPlayer[] _lastPlayers;
        private const float MaxSyncDelay = 1f;
        private float _currentSyncDelay = 1f;
        protected override float VerticalSpacing => 10f;
        protected override int VerticalPadding => 15;

        public override void Setup(BasePanel parent = null)
        {
            base.Setup(parent);
            Sync();
        }

        private void Update()
        {
            _currentSyncDelay -= Time.deltaTime;
            if (_currentSyncDelay <= 0f)
                Sync();
        }

        public void Sync()
        {
            _lastPlayers = (PhotonPlayer[])PhotonNetwork.playerList.Clone();
            _lastPlayers = new PhotonPlayer[] { PhotonNetwork.player, PhotonNetwork.player, PhotonNetwork.player };
            ElementStyle style = new ElementStyle(themePanel: ThemePanel);
            SetHeader(style);
            SetRows(style);
            _currentSyncDelay = MaxSyncDelay;
        }

        private void SetRows(ElementStyle style)
        {
            int rowCount = _rows.Count;
            if (rowCount > _lastPlayers.Length)
            {
                for (int i = 0; i < rowCount - _lastPlayers.Length; i++)
                {
                    Destroy(_rows[_rows.Count - 1].gameObject);
                    _rows.RemoveAt(_rows.Count - 1);
                }
            }
            else if (rowCount < _lastPlayers.Length)
            {
                for (int i = 0; i < _lastPlayers.Length - rowCount; i++)
                    _rows.Add(CreateRow(style, _rows.Count));
            }
            for (int i = 0; i < _rows.Count; i++)
                SetRow(_rows[i], _lastPlayers[i]);
        }

        private void SetHeader(ElementStyle style)
        {
            if (_header == null)
            {
                _header = ElementFactory.CreateHorizontalGroup(SinglePanel, 0f, TextAnchor.MiddleCenter).transform;
                ElementFactory.CreateDefaultLabel(_header, style, UIManager.GetLocale("ScoreboardPopup", "Scoreboard", "Player"), FontStyle.Bold, TextAnchor.MiddleCenter);
                ElementFactory.CreateDefaultLabel(_header, style, string.Empty, FontStyle.Bold, TextAnchor.MiddleCenter);
                ElementFactory.CreateDefaultLabel(_header, style, UIManager.GetLocale("ScoreboardPopup", "Scoreboard", "Action"), FontStyle.Bold, TextAnchor.MiddleCenter);
                foreach (Transform t in _header)
                    t.GetComponent<LayoutElement>().preferredWidth = GetPanelWidth() / 3f;
                CreateHorizontalDivider(SinglePanel);
            }
            _header.GetChild(1).GetComponent<Text>().text = string.Join(" / ", ((InGameManager)SceneLoader.CurrentGameManager).ScoreboardHeaders);
        }

        private Transform CreateRow(ElementStyle style, int index)
        {
            Transform row = ElementFactory.CreateHorizontalGroup(SinglePanel, 0f, TextAnchor.MiddleCenter).transform;
            // player
            Transform playerRow = ElementFactory.CreateHorizontalGroup(row, 5f, TextAnchor.MiddleCenter).transform;
            ElementFactory.CreateRawImage(playerRow, style, "Skull1Icon");
            ElementFactory.CreateRawImage(playerRow, style, "Sword3Icon");
            ElementFactory.CreateDefaultLabel(playerRow, style, string.Empty, FontStyle.Normal, TextAnchor.MiddleCenter);
            // score
            ElementFactory.CreateDefaultLabel(row, style, string.Empty, FontStyle.Normal, TextAnchor.MiddleCenter);
            // action
            Transform actionRow = ElementFactory.CreateHorizontalGroup(row, 10f, TextAnchor.MiddleCenter).transform;
            ElementFactory.CreateIconButton(actionRow, style, "UserIcon", elementWidth: 28f, elementHeight: 28f, onClick: () => OnClickProfile(index));
            ElementFactory.CreateIconButton(actionRow, style, "VolumeOffIcon", elementWidth: 30f, elementHeight: 30f, onClick: () => OnClickMute(index));
            if (PhotonNetwork.isMasterClient)
                ElementFactory.CreateIconButton(actionRow, style, "CloseIcon", elementWidth: 25f, elementHeight: 25f, onClick: () => OnClickKick(index));
            foreach (Transform t in row)
                t.GetComponent<LayoutElement>().preferredWidth = GetPanelWidth() / 3f;
            return row;
        }

        private void SetRow(Transform row, PhotonPlayer player)
        {
            // read player props
            string name = "[" + player.ID.ToString() + "] ".FormatColor(TextColor.ScoreboardPlayerID) +
                player.GetStringProperty(PlayerProperty.DisplayName).StripHex().FormatColor(TextColor.ScoreboardPlayerName);
            string status = player.GetStringProperty(PlayerProperty.Status);
            string character = player.GetStringProperty(PlayerProperty.Character);
            string loadout = player.GetStringProperty(PlayerProperty.Loadout);
            List<string> scoreList = new List<string>();
            foreach (string property in ((InGameManager)SceneLoader.CurrentGameManager).ScoreboardProperties)
            {
                object value = player.GetCustomProperty(property);
                string str = value != null ? value.ToString() : string.Empty;
                scoreList.Add(str);
            }
            string score = string.Join(" / ", scoreList.ToArray());

            // update status icon
            Transform playerRow = row.GetChild(0);
            RawImage statusImage = playerRow.GetChild(0).GetComponent<RawImage>();
            if (status == PlayerStatus.Spectating)
            {
                statusImage.gameObject.SetActive(true);
                statusImage.texture = (Texture2D)AssetBundleManager.LoadAsset("SpectateIcon", true);
                statusImage.color = UIManager.GetThemeColor(ThemePanel, "Icon", "SpectateColor");
            }
            else if (status == PlayerStatus.Dead)
            {
                statusImage.gameObject.SetActive(true);
                statusImage.texture = (Texture2D)AssetBundleManager.LoadAsset("Skull1Icon", true);
                statusImage.color = UIManager.GetThemeColor(ThemePanel, "Icon", "DeadColor");
            }
            else
                statusImage.gameObject.SetActive(false);

            // update loadout icon
            RawImage loadoutImage = playerRow.GetChild(1).GetComponent<RawImage>();
            if (character == PlayerCharacter.Human)
            {
                loadoutImage.color = UIManager.GetThemeColor(ThemePanel, "Icon", "LoadoutHuman");
                if (loadout == HumanLoadout.Blades)
                    loadoutImage.texture = (Texture2D)AssetBundleManager.LoadAsset("Sword3Icon", true);
                else if (loadout == HumanLoadout.Guns)
                    loadoutImage.texture = (Texture2D)AssetBundleManager.LoadAsset("Sword1Icon", true);
                else if (loadout == HumanLoadout.ThunderSpears)
                    loadoutImage.texture = (Texture2D)AssetBundleManager.LoadAsset("Sword1Icon", true);
            }
            else if (character == PlayerCharacter.Titan)
            {
                loadoutImage.color = UIManager.GetThemeColor(ThemePanel, "Icon", "LoadoutTitan");
                loadoutImage.texture = (Texture2D)AssetBundleManager.LoadAsset("Skull2Icon", true);
            }
            else if (character == PlayerCharacter.Shifter)
            {
                loadoutImage.color = UIManager.GetThemeColor(ThemePanel, "Icon", "LoadoutShifter");
                loadoutImage.texture = (Texture2D)AssetBundleManager.LoadAsset("Skull2Icon", true);
            }

            // update name and score
            playerRow.GetChild(2).GetComponent<Text>().text = name;
            row.GetChild(1).GetComponent<Text>().text = score;

            // update action icons
            Transform actionRow = row.GetChild(2);
            bool isMine = PhotonNetwork.player == player;
            isMine = false;
            actionRow.GetChild(1).gameObject.SetActive(!isMine);
            if (actionRow.childCount > 2)
                actionRow.GetChild(2).gameObject.SetActive(!isMine);
        }

        private void OnClickProfile(int index)
        {
            ((ScoreboardPopup)Parent)._profilePopup.Show();
        }

        private void OnClickKick(int index)
        {
            ((ScoreboardPopup)Parent)._kickPopup.Show();
        }

        private void OnClickMute(int index)
        {
            ((ScoreboardPopup)Parent)._mutePopup.Show();
        }
    }
}
