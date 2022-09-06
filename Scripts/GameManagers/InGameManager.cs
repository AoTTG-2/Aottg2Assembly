using System.Collections.Generic;
using UnityEngine;
using Weather;
using UI;
using Utility;
using CustomSkins;
using ApplicationManagers;
using System.Diagnostics;
using Characters;
using Settings;
using CustomLogic;
using Effects;
using Map;
using System.Collections;

namespace GameManagers
{
    class InGameManager : BaseGameManager
    {
        private SkyboxCustomSkinLoader _skyboxCustomSkinLoader;
        private ForestCustomSkinLoader _forestCustomSkinLoader;
        private CityCustomSkinLoader _cityCustomSkinLoader;
        private CustomLevelCustomSkinLoader _customLevelCustomSkinLoader;
        private GeneralInputSettings _generalInputSettings;
        private InGameMenu _inGameMenu;
        public HashSet<Human> Humans = new HashSet<Human>();
        public HashSet<BasicTitan> Titans = new HashSet<BasicTitan>();
        public HashSet<BaseShifter> Shifters = new HashSet<BaseShifter>();
        public bool IsEnding;
        public GameState State = GameState.Loading;
        public BaseCharacter CurrentCharacter;
        public string[] ScoreboardHeaders = new string[0];
        public string[] ScoreboardProperties = new string[0];
        private bool _gameSettingsLoaded = false;
        public static Dictionary<int, PlayerInfo> AllPlayerInfo = new Dictionary<int, PlayerInfo>();
        public static PlayerInfo MyPlayerInfo = new PlayerInfo();
        private static bool _needSendPlayerInfo;

        public static void RestartGame()
        {
            if (!PhotonNetwork.isMasterClient)
                return;
            PhotonNetwork.DestroyAll();
            RPCManager.PhotonView.RPC("RestartGameRPC", PhotonTargets.All, new object[0]);
        }

        public static void OnRestartGameRPC(PhotonMessageInfo info)
        {
            if (!info.sender.isMasterClient)
                return;
            ResetRoundPlayerProperties();
            SceneLoader.LoadScene(SceneName.InGame);
        }

        public static void LeaveRoom()
        {
            ResetPersistentPlayerProperties();
            ResetRoundPlayerProperties();
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.DestroyAll();
            if (PhotonNetwork.connected)
                PhotonNetwork.Disconnect();
            SettingsManager.InGameCurrent.SetDefault();
            SettingsManager.InGameUI.SetDefault();
            SettingsManager.InGameCharacterSettings.SetDefault();
            SceneLoader.LoadScene(SceneName.MainMenu);
        }

        public static void OnJoinRoom()
        {
            ResetPersistentPlayerProperties();
            ResetRoundPlayerProperties();
            ResetPlayerInfo();
            _needSendPlayerInfo = true;
            SceneLoader.LoadScene(SceneName.InGame);
        }

        public void OnPhotonPlayerJoined(PhotonPlayer player)
        {
            if (!AllPlayerInfo.ContainsKey(player.ID))
                AllPlayerInfo.Add(player.ID, new PlayerInfo());
            RPCManager.PhotonView.RPC("PlayerInfoRPC", player, new object[] { StringCompression.Compress(MyPlayerInfo.SerializeToJsonString()) });
            if (PhotonNetwork.isMasterClient)
                RPCManager.PhotonView.RPC("GameSettingsRPC", player, new object[] { StringCompression.Compress(SettingsManager.InGameCurrent.SerializeToJsonString()) });
        }

        public static void OnPlayerInfoRPC(byte[] data, PhotonMessageInfo info)
        {
            if (!AllPlayerInfo.ContainsKey(info.sender.ID))
                AllPlayerInfo.Add(info.sender.ID, new PlayerInfo());
            AllPlayerInfo[info.sender.ID].DeserializeFromJsonString(StringCompression.Decompress(data));
        }

        public static void OnGameSettingsRPC(byte[] data, PhotonMessageInfo info)
        {
            if (!info.sender.isMasterClient)
                return;
            SettingsManager.InGameCurrent.DeserializeFromJsonString(StringCompression.Decompress(data));
            ((InGameManager)SceneLoader.CurrentGameManager)._gameSettingsLoaded = true;
        }

        public void SpawnPlayer()
        {
            var settings = SettingsManager.InGameCharacterSettings;
            var character = settings.CharacterType.Value;
            if (settings.ChooseStatus.Value != (int)ChooseCharacterStatus.Chosen)
                return;
            if (CurrentCharacter != null)
                CurrentCharacter.Die();
            if (character == PlayerCharacter.Human)
            {
                var human = (Human)CharacterSpawner.Spawn(CharacterPrefabs.Human, GetHumanSpawnPoint(), Quaternion.identity);
                human.Init(false, GetPlayerTeam(), SettingsManager.InGameCharacterSettings);
                CurrentCharacter = human;
            }
            else if (character == PlayerCharacter.Shifter)
            {
                if (settings.Loadout.Value == "Female")
                {
                    var shifter = (FemaleShifter)CharacterSpawner.Spawn(CharacterPrefabs.FemaleShifter, GetTitanSpawnPoint(), Quaternion.identity);
                    shifter.Init(false, GetPlayerTeam());
                    CurrentCharacter = shifter;
                }
            }
            else if (character == PlayerCharacter.Titan)
            {
                var titan = (BasicTitan)CharacterSpawner.Spawn(CharacterPrefabs.BasicTitan, GetTitanSpawnPoint(), Quaternion.identity);
                titan.Init(false, GetPlayerTeam());
                CurrentCharacter = titan;
            }
            PhotonNetwork.player.SetCustomProperty(PlayerProperty.CharacterViewId, CurrentCharacter.Cache.PhotonView.viewID);
        }

        private Vector3 GetHumanSpawnPoint()
        {
            if (SettingsManager.InGameCurrent.Misc.PVP.Value == (int)PVPMode.Team)
            {
                List<string> tags;
                if (SettingsManager.InGameCharacterSettings.Team.Value == TeamInfo.Blue)
                    tags = new List<string>() { MapTags.HumanSpawnPointBlue, MapTags.HumanSpawnPoint, MapTags.HumanSpawnPointRed };
                else
                    tags = new List<string>() { MapTags.HumanSpawnPointRed, MapTags.HumanSpawnPoint, MapTags.HumanSpawnPointBlue };
                return MapManager.GetRandomTagsPosition(tags, Vector3.zero);
            }
            else
            {
                List<string> tags = new List<string>() { MapTags.HumanSpawnPoint, MapTags.HumanSpawnPointBlue, MapTags.HumanSpawnPointRed};
                return MapManager.GetRandomTagsPosition(tags, Vector3.zero);
            }
        }

        private Vector3 GetTitanSpawnPoint()
        {
            return MapManager.GetRandomTagPosition(MapTags.TitanSpawnPoint, Vector3.zero);
        }

        private string GetPlayerTeam()
        {
            if (SettingsManager.InGameCurrent.Misc.PVP.Value == (int)PVPMode.Team)
                return SettingsManager.InGameCharacterSettings.Team.Value;
            else if (SettingsManager.InGameCurrent.Misc.PVP.Value == (int)PVPMode.FFA)
                return TeamInfo.None;
            else
                return TeamInfo.Human;
        }

        public BaseShifter SpawnAIShifter()
        {
            var character = (BaseShifter)CharacterSpawner.Spawn(CharacterPrefabs.FemaleShifter, GetTitanSpawnPoint(), Quaternion.identity);
            character.Init(true, TeamInfo.Titan);
            return character;
        }

        public BasicTitan SpawnAITitan()
        {
            var titan = (BasicTitan)CharacterSpawner.Spawn(CharacterPrefabs.BasicTitan, GetTitanSpawnPoint(), Quaternion.identity);
            titan.Init(true, TeamInfo.Titan);
            return titan;
        }

        public static void OnSetTopLabelRPC(string message, PhotonMessageInfo info)
        {
            if (info.sender != PhotonNetwork.masterClient)
                return;
            SetTopLabel(message);
        }

        public static void SetTopLabel(string message)
        {
        }

        public void EndGame(string message, float time)
        {
            if (!IsEnding)
            {
                IsEnding = true;
                StartCoroutine(WaitAndEndGame(message, time));
            }
        }

        private IEnumerator WaitAndEndGame(string message, float time)
        {
            yield return new WaitForSeconds(time);
            RestartGame();
        }

        private static void ResetPersistentPlayerProperties()
        {
            var properties = new Dictionary<string, object>
            {
                { PlayerProperty.DisplayName, MyPlayerInfo.Profile.Name.Value },
                { PlayerProperty.DisplayGuild, MyPlayerInfo.Profile.Guild.Value },
                { PlayerProperty.Team, null },
                { PlayerProperty.CustomMapHash, null },
                { PlayerProperty.CustomLogicHash, null }
            };
            PhotonNetwork.player.SetCustomProperties(properties);
        }

        private static void ResetRoundPlayerProperties()
        {
            var properties = new Dictionary<string, object>
            {
                { PlayerProperty.Status, PlayerStatus.Spectating },
                { PlayerProperty.Character, PlayerCharacter.Human },
                { PlayerProperty.Loadout, HumanLoadout.Blades }
            };
            PhotonNetwork.player.SetCustomProperties(properties);
        }

        private static void UpdateRoundPlayerProperties()
        {
            var properties = new Dictionary<string, object>
            {
                { PlayerProperty.Status, PlayerStatus.Spectating },
                { PlayerProperty.Character, PlayerCharacter.Human },
                { PlayerProperty.Loadout, HumanLoadout.Blades }
            };
            PhotonNetwork.player.SetCustomProperties(properties);
        }

        private static void ResetPlayerInfo()
        {
            AllPlayerInfo.Clear();
            PlayerInfo myPlayerInfo = new PlayerInfo();
            myPlayerInfo.Profile.Copy(SettingsManager.ProfileSettings);
            AllPlayerInfo.Add(PhotonNetwork.player.ID, myPlayerInfo);
            MyPlayerInfo = myPlayerInfo;
        }

        protected override void Awake()
        {
            _skyboxCustomSkinLoader = gameObject.AddComponent<SkyboxCustomSkinLoader>();
            _forestCustomSkinLoader = gameObject.AddComponent<ForestCustomSkinLoader>();
            _cityCustomSkinLoader = gameObject.AddComponent<CityCustomSkinLoader>();
            _customLevelCustomSkinLoader = gameObject.AddComponent<CustomLevelCustomSkinLoader>();
            _generalInputSettings = SettingsManager.InputSettings.General;
            ResetRoundPlayerProperties();
            SetDefaultScoreboard();
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.Instantiate("RCAsset/RPCManagerPrefab", Vector3.zero, Quaternion.identity, 0);
            base.Awake();
        }

        protected override void Start()
        {
            _inGameMenu = (InGameMenu)UIManager.CurrentMenu;
            if (PhotonNetwork.isMasterClient)
            {
                SettingsManager.InGameCurrent.Copy(SettingsManager.InGameUI);
                RPCManager.PhotonView.RPC("GameSettingsRPC", PhotonTargets.All, new object[] { StringCompression.Compress(SettingsManager.InGameCurrent.SerializeToJsonString()) });
            }
            if (_needSendPlayerInfo)
            {
                RPCManager.PhotonView.RPC("PlayerInfoRPC", PhotonTargets.Others, new object[] { StringCompression.Compress(MyPlayerInfo.SerializeToJsonString()) });
                _needSendPlayerInfo = false;
            }
            RPCManager.PhotonView.RPC("TestRPC", PhotonTargets.All, new object[] { Color.blue });
            base.Start();
        }

        public override bool IsFinishedLoading()
        {
            return base.IsFinishedLoading() && _gameSettingsLoaded;
        }

        private void Update()
        {
            if (State != GameState.Loading)
                UpdateInput();
            UpdateCleanCharacters();
        }

        protected override void OnFinishLoading()
        {
            base.OnFinishLoading();
            ((InGameMenu)UIManager.CurrentMenu).UpdateLoading(1f, true);
            if (State == GameState.Loading)
                State = GameState.Playing;
            CustomLogicManager.StartLogic(SettingsManager.InGameCurrent.Mode.Current);
        }

        private void UpdateInput()
        {
            if (_generalInputSettings.Pause.GetKeyDown())
                _inGameMenu.SetPauseMenu(true);
            if (_generalInputSettings.ChangeCharacter.GetKeyDown() && !_inGameMenu.InMenu())
            {
                if (CurrentCharacter != null)
                    CurrentCharacter.Die();
                _inGameMenu.SetCharacterMenu(true);
            }
            if (_generalInputSettings.TapScoreboard.Value)
            {
                if (_generalInputSettings.ToggleScoreboard.GetKeyDown())
                    _inGameMenu.ToggleScoreboardMenu();
            }
            else
            {
                if (_generalInputSettings.ToggleScoreboard.GetKey())
                    _inGameMenu.SetScoreboardMenu(true);
                else
                    _inGameMenu.SetScoreboardMenu(false);
            }
        }

        private void UpdateCleanCharacters()
        {
            Util.RemoveNull(Humans);
            Util.RemoveNull(Titans);
            Util.RemoveNull(Shifters);
        }

        private void SetDefaultScoreboard()
        {
            ScoreboardHeaders = new string[] { "Kills", "Deaths" };
            ScoreboardProperties = new string[] { "Kills", "Deaths" };
        }
    }

    public enum GameState
    {
        Loading,
        Playing,
        Paused
    }
}
