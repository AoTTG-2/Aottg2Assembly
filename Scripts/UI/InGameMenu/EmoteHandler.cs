using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Settings;
using Characters;
using GameManagers;
using ApplicationManagers;

namespace UI
{
    class EmoteHandler : MonoBehaviour
    {
        public static Dictionary<string, Texture2D> EmojiTextures = new Dictionary<string, Texture2D>();
        public static List<string> AvailableEmojis = new List<string>() { "Smile", "ThumbsUp", "Cool", "Love", "Shocked", "Crying", "Annoyed", "Angry" };
        public static List<string> AvailableText = new List<string>() { "Help", "Thanks", "Sorry", "Titan here", "Good game", "Nice hit", "Oops", "Welcome" };
        private List<BasePopup> _emoteTextPopups = new List<BasePopup>();
        private List<BasePopup> _emoteEmojiPopups = new List<BasePopup>();
        private BasePopup _emoteWheelPopup;
        private EmoteWheelState _currentEmoteWheelState = EmoteWheelState.Text;
        private float _currentEmoteCooldown;
        public float EmoteCooldown = 4f;
        public bool IsActive;
        private InGameManager _inGameManager;

        private void Awake()
        {
            for (int i = 0; i < 5; i++)
            {
                BasePopup emoteTextPopup = ElementFactory.InstantiateAndSetupPanel<EmoteTextPopup>(transform, "EmoteTextPopup").GetComponent<BasePopup>();
                _emoteTextPopups.Add(emoteTextPopup);
                BasePopup emoteEmojiPopup = ElementFactory.InstantiateAndSetupPanel<EmoteEmojiPopup>(transform, "EmoteEmojiPopup").GetComponent<BasePopup>();
                _emoteEmojiPopups.Add(emoteEmojiPopup);
            }
            _emoteWheelPopup = ElementFactory.InstantiateAndSetupPanel<WheelPopup>(transform, "WheelMenu").GetComponent<BasePopup>();
            _inGameManager = (InGameManager)SceneLoader.CurrentGameManager;
        }

        public static void OnEmoteTextRPC(int viewId, string text, PhotonMessageInfo info)
        {
            if (UIManager.CurrentMenu == null || !SettingsManager.UISettings.ShowEmotes.Value)
                return;
            EmoteHandler handler = UIManager.CurrentMenu.GetComponent<EmoteHandler>();
            Transform t = GetTransformFromViewId(viewId, info);
            if (t != null && handler != null)
                handler.ShowEmoteText(text, t);
        }

        public static void OnEmoteEmojiRPC(int viewId, string emoji, PhotonMessageInfo info)
        {
            if (UIManager.CurrentMenu == null || !SettingsManager.UISettings.ShowEmotes.Value)
                return;
            EmoteHandler handler = UIManager.CurrentMenu.GetComponent<EmoteHandler>();
            Transform t = GetTransformFromViewId(viewId, info);
            if (t != null && handler != null)
                handler.ShowEmoteEmoji(emoji, t);
        }

        private void ShowEmoteText(string text, Transform parent)
        {
            EmoteTextPopup popup = (EmoteTextPopup)GetAvailablePopup(_emoteTextPopups);
            if (text.Length > 20)
                text = text.Substring(0, 20);
            popup.Show(text, parent);
        }

        private void ShowEmoteEmoji(string emoji, Transform parent)
        {
            EmoteEmojiPopup popup = (EmoteEmojiPopup)GetAvailablePopup(_emoteEmojiPopups);
            popup.Show(emoji, parent);
        }

        public void ToggleEmoteWheel()
        {
            SetEmoteWheel(!IsActive);
        }

        public void SetEmoteWheel(bool enable)
        {
            if (enable)
            {
                if (!((InGameMenu)UIManager.CurrentMenu).InMenu())
                {
                    ((WheelPopup)_emoteWheelPopup).Show(SettingsManager.InputSettings.Interaction.EmoteMenu.ToString(),
                        GetEmoteWheelOptions(_currentEmoteWheelState), () => OnEmoteWheelSelect());
                    IsActive = true;
                }
            }
            else
            {
                _emoteWheelPopup.Hide();
                IsActive = false;
            }
        }

        public void NextEmoteWheel()
        {
            if (!_emoteWheelPopup.gameObject.activeSelf || !IsActive)
                return;
            _currentEmoteWheelState++;
            if (_currentEmoteWheelState > EmoteWheelState.Action)
                _currentEmoteWheelState = 0;
            ((WheelPopup)_emoteWheelPopup).Show(SettingsManager.InputSettings.Interaction.EmoteMenu.ToString(),
                    GetEmoteWheelOptions(_currentEmoteWheelState), () => OnEmoteWheelSelect());
        }

        private void OnEmoteWheelSelect()
        {
            if (_currentEmoteWheelState != EmoteWheelState.Action)
            {
                if (_currentEmoteCooldown > 0f)
                    return;
                _currentEmoteCooldown = EmoteCooldown;
            }
            BaseCharacter character = _inGameManager.CurrentCharacter;
            if (character != null)
            {
                if (_currentEmoteWheelState == EmoteWheelState.Text)
                {
                    string text = AvailableText[((WheelPopup)_emoteWheelPopup).SelectedItem];
                    RPCManager.PhotonView.RPC("EmoteTextRPC", PhotonTargets.All, new object[] { character.Cache.PhotonView.viewID, text });
                }
                else if (_currentEmoteWheelState == EmoteWheelState.Emoji)
                {
                    string emoji = AvailableEmojis[((WheelPopup)_emoteWheelPopup).SelectedItem];
                    RPCManager.PhotonView.RPC("EmoteEmojiRPC", PhotonTargets.All, new object[] { character.Cache.PhotonView.viewID, emoji });
                }
                else if (_currentEmoteWheelState == EmoteWheelState.Action)
                {
                    string action = character.EmoteActions[((WheelPopup)_emoteWheelPopup).SelectedItem];
                    character.Emote(action);
                }
            }
            _emoteWheelPopup.Hide();
            IsActive = false;
        }

        private static Transform GetTransformFromViewId(int viewId, PhotonMessageInfo info)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (view != null && view.owner == info.sender)
            {
                return view.transform;
            }
            return null;
        }

        private List<string> GetEmoteWheelOptions(EmoteWheelState state)
        {
            if (state == EmoteWheelState.Text)
                return AvailableText;
            else if (state == EmoteWheelState.Emoji)
                return AvailableEmojis;
            else
            {
                if (_inGameManager.CurrentCharacter == null)
                    return new List<string>();
                return _inGameManager.CurrentCharacter.EmoteActions;
            }
        }

        private BasePopup GetAvailablePopup(List<BasePopup> popups)
        {
            foreach (BasePopup popup in popups)
            {
                if (!popup.gameObject.activeSelf)
                    return popup;
            }
            return popups[0];
        }

        private void Update()
        {
            _currentEmoteCooldown -= Time.deltaTime;
        }
    }

    public enum EmoteWheelState
    {
        Text,
        Emoji,
        Action
    }
}
