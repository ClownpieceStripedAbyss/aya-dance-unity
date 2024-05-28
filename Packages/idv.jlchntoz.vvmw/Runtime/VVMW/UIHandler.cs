using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using JLChnToZ.VRC.VVMW.I18N;

namespace JLChnToZ.VRC.VVMW {
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DisallowMultipleComponent]
    [AddComponentMenu("VizVid/UI Handler")]
    [DefaultExecutionOrder(2)]
    [HelpURL("https://github.com/JLChnToZ/VVMW/blob/main/Packages/idv.jlchntoz.vvmw/README.md#default-ui--screen-with-overlay")]
    public class UIHandler : VizVidBehaviour {
        [Header("Main Reference")]
        [SerializeField, Locatable(
            InstaniatePrefabPath = "Packages/idv.jlchntoz.vvmw/VVMW (No Controls).prefab",
            InstaniatePrefabPosition = LocatableAttribute.InstaniatePrefabHierachyPosition.Before
        ), BindUdonSharpEvent] Core core;
        [Locatable(
            InstaniatePrefabPath = "Packages/idv.jlchntoz.vvmw/VVMW (No Controls).prefab",
            InstaniatePrefabPosition = LocatableAttribute.InstaniatePrefabHierachyPosition.Before
        ), BindUdonSharpEvent] public FrontendHandler handler;

        [SerializeField, HideInInspector, BindUdonSharpEvent] LanguageManager languageManager;

        [Header("URL Input")]
        [BindEvent(nameof(VRCUrlInputField.onValueChanged), nameof(_OnURLChanged))]
        [BindEvent(nameof(VRCUrlInputField.onEndEdit), nameof(_OnURLEndEdit))]
        [SerializeField] VRCUrlInputField urlInput; 
        [SerializeField] GameObject videoPlayerSelectButtonTemplate;
        [SerializeField] GameObject videoPlayerSelectRoot, videoPlayerSelectPanel;
        [BindEvent(nameof(Button.onClick), nameof(_VideoPlayerSelect))]
        [SerializeField] Button videoPlayerSelectButton;
        [BindEvent(nameof(Button.onClick), nameof(_InputCancelClick))]
        [SerializeField] Button cancelButton;
        [BindEvent(nameof(Button.onClick), nameof(_InputConfirmClick))]
        [SerializeField] Button urlInputConfirmButton;
        [SerializeField] Text selectdPlayerText;
        [SerializeField] Text queueModeText;
        [SerializeField] GameObject otherObjectUnderUrlInput;

        [Header("Playback Controls")]
        [SerializeField] Animator playbackControlsAnimator;
        [BindEvent(nameof(Button.onClick), nameof(_Play))]
        [SerializeField] Button playButton;
        [BindEvent(nameof(Button.onClick), nameof(_Pause))]
        [SerializeField] Button pauseButton;
        [BindEvent(nameof(Button.onClick), nameof(_Stop))]
        [SerializeField] Button stopButton;
        [BindEvent(nameof(Button.onClick), nameof(_LocalSync))]
        [SerializeField] Button reloadButton;
        [BindEvent(nameof(Button.onClick), nameof(_GlobalSync))]
        [SerializeField] Button globalReloadButton;
        [BindEvent(nameof(Button.onClick), nameof(_Skip))]
        [SerializeField] Button playNextButton;
        [SerializeField] Text enqueueCountText;
        [BindEvent(nameof(Button.onClick), nameof(_RepeatOne))]
        [SerializeField] Button repeatOffButton;
        [BindEvent(nameof(Button.onClick), nameof(_RepeatAll))]
        [SerializeField] Button repeatOneButton;
        [BindEvent(nameof(Button.onClick), nameof(_RepeatOff))]
        [FormerlySerializedAs("RepeatAllButton")]
        [SerializeField] Button repeatAllButton;
        [BindEvent(nameof(Button.onClick), nameof(_ShuffleOn))]
        [SerializeField] Button shuffleOffButton;
        [BindEvent(nameof(Button.onClick), nameof(_ShuffleOff))]
        [SerializeField] Button shuffleOnButton;
        [BindEvent(nameof(Slider.onValueChanged), nameof(_OnSeek))]
        [SerializeField] Slider progressSlider;
        [SerializeField] Text statusText, timeText, durationText;
        [SerializeField] GameObject timeContainer;

        [Header("Volume Control")]
        [BindEvent(nameof(Slider.onValueChanged), nameof(_OnVolumeSlide))]
        [SerializeField] Slider volumeSlider;
        [BindEvent(nameof(Button.onClick), nameof(_OnMute))]
        [SerializeField] Button muteButton, unmuteButton;

        [Header("Idle Screen")]
        [SerializeField] GameObject idleScreenRoot;

        [Header("Current Video Title")]
        [SerializeField] Text currentVideoTitleText;

        [Header("Queue List / Play List")]
        [SerializeField] GameObject playListPanelRoot;
        [SerializeField, BindUdonSharpEvent] PooledScrollView playListScrollView;
        [BindEvent(nameof(Button.onClick), nameof(_PlayListTogglePanel))]
        [SerializeField] Button playListTogglePanelButton;
        [SerializeField, BindUdonSharpEvent] PooledScrollView queueListScrollView;
        [SerializeField] GameObject playNextIndicator;
        [SerializeField] Text selectedPlayListText;

        [Header("Video List / Category List")]
        [SerializeField] GameObject videoListPanelRoot;
        [SerializeField, BindUdonSharpEvent] PooledScrollView videoCategoryListScrollView;
        [BindEvent(nameof(Button.onClick), nameof(_VideoCategoryListTogglePanel))]
        [SerializeField] Button videoCategoryListTogglePanelButton;
        [SerializeField] Text selectedCategoryText;
        [SerializeField, BindUdonSharpEvent] PooledScrollView videoListScrollView;
        [BindEvent(nameof(Button.onClick), nameof(_VideoListQueueUp))]
        [SerializeField] Button videoListQueueUpButton;
        [BindEvent(nameof(Button.onClick), nameof(_VideoListFavorite))]
        [SerializeField] Button videoListFavoriteButton;

        [Header("Search Box")]
        [BindEvent(nameof(InputField.onValueChanged), nameof(_OnSearchBoxValueChanged))]
        [BindEvent(nameof(InputField.onEndEdit), nameof(_OnSearchBoxEndEdit))]
        [SerializeField] InputField searchBox;

        [Header("Screen/Mirror/Follow/Adaptive Control")]
        [SerializeField] UnityEngine.Object playingScreen;
        [BindEvent(nameof(Button.onClick), nameof(_ManualMirrorOn))]
        [SerializeField] Button mirrorOnButton;
        [BindEvent(nameof(Button.onClick), nameof(_ManualMirrorOff))]
        [SerializeField] Button mirrorOffButton;
        [BindEvent(nameof(Button.onClick), nameof(_ScreenFollowOn))]
        [SerializeField] Button screenFollowOnButton;
        [BindEvent(nameof(Button.onClick), nameof(_ScreenFollowOff))]
        [SerializeField] Button screenFollowOffButton;
        [BindEvent(nameof(Button.onClick), nameof(_ScreenAdaptiveOn))]
        [SerializeField] Button screenAdaptiveOnButton;
        [BindEvent(nameof(Button.onClick), nameof(_ScreenAdaptiveOff))]
        [SerializeField] Button screenAdaptiveOffButton;

        [Header("Typewriter Ad-hoc Video Player")]
        [SerializeField] BaseVRCVideoPlayer typewriterVideoPlayer;

        [Header("Sync Offset Controls")]
        [BindEvent(nameof(Button.onClick), nameof(_ShiftBack100ms))]
        [SerializeField] Button shiftBack100msButton;
        [BindEvent(nameof(Button.onClick), nameof(_ShiftBack50ms))]
        [SerializeField] Button shiftBack50msButton;
        [BindEvent(nameof(Button.onClick), nameof(_ShiftForward50ms))]
        [SerializeField] Button shiftForward50msButton;
        [BindEvent(nameof(Button.onClick), nameof(_ShiftForward100ms))]
        [SerializeField] Button shiftForward100msButton;
        [BindEvent(nameof(Button.onClick), nameof(_ShiftReset))]
        [SerializeField] Button shiftResetButton;
        [SerializeField] Text shiftOffsetText;
        
        [Header("Data Persistence")]
        [SerializeField] PWIManager vrcxPersistence;

        [Header("Aya Dance Features")]
        [BindEvent(nameof(VRCUrlInputField.onValueChanged), nameof(_OnRemoteReceiptApiURLValueChanged))]
        [BindEvent(nameof(VRCUrlInputField.onEndEdit), nameof(_OnRemoteReceiptApiURLEndEdit))]
        [SerializeField] VRCUrlInputField remoteReceiptApiUrlInput; 
        [SerializeField] Text remoteReceiptStatusText;
        [SerializeField] ReceiptClient remoteReceiptClient;
        [BindEvent(nameof(Button.onClick), nameof(_RemoteReceiptAutoAcceptOn))]
        [SerializeField] Button remoteReceiptAutoAcceptOnButton;
        [BindEvent(nameof(Button.onClick), nameof(_RemoteReceiptAutoAcceptOff))]
        [SerializeField] Button remoteReceiptAutoAcceptOffButton;

        private string KEY_ScreenFollow = "AyaScreenFollow";
        private string KEY_ScreenAdaptive = "AyaScreenAdaptive";
        private string KEY_FavoriteVideos = "AyaFavoriteVideos";
        private string KEY_SyncOffset = "AyaSyncOffset";

        VRCPlayerApi localPlayer;

        // string[] playListNames;
        ButtonEntry[] videoPlayerSelectButtons;
        [NonSerialized] public byte loadWithIndex;
        int lastSelectedPlayListIndex, lastPlayingIndex;
        int lastDisplayCount;
        bool hasUpdate, wasUnlocked, hasUnlockInit, playListUpdateRequired;
        string enqueueCountFormat;
        byte selectedPlayer = 1;
        int interactTriggerId;
        DateTime joinTime, playListLastInteractTime;
        TimeSpan interactCoolDown = TimeSpan.FromSeconds(5);
        bool afterFirstRun;
        bool userChangedVolume = false;
        bool userChangedMirror = false;
        bool localIsVideoMirrored = false;
        bool localIsScreenFollowing = false;
        bool localIsScreenAdaptive = false;
        DataList localAwaitingReceipts = new DataList();
        bool localIsRemoteReceiptAutoAccept = false;

        string searchBoxBuffer = "";
        string[] videoListEntryTitles = new string[0];
        int[] videoListEntryIds = new int[0];
        
        bool onlyEmmmerAndKiva = false;

        // Since PlayList are moved to CategoryList, which is a separate ScrollView,
        // the SelectedPlayListIndex should also be 0.
        int SelectedPlayListIndex {
            get {
                if (playListScrollView == null) return 0;
                int selectedIndex = playListScrollView.SelectedIndex;
                if (handler != null && !handler.HasQueueList) selectedIndex++;
                return selectedIndex;
            }
            set {
                if (playListScrollView == null) return;
                if (handler != null && !handler.HasQueueList) value--;
                playListScrollView.SelectedIndex = value;
            }
        }

        int SelectedCategoryIndex {
            get {
                if (videoCategoryListScrollView == null) return 0;
                int selectedIndex = videoCategoryListScrollView.SelectedIndex;
                return selectedIndex;
            }
            set {
                if (videoCategoryListScrollView == null) return;
                videoCategoryListScrollView.SelectedIndex = value;
            }
        }

        void CheckOnlyEmmmerAndKiva(string reason) {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            foreach (var player in players) {
                Debug.Log($"{reason}: Player: {player.displayName}");
            }
            bool yes = true;
            foreach (var player in players) {
                if (player.displayName != "Emmmer" && player.displayName != "imkiva") {
                    yes = false;
                    break;
                }
            }
            onlyEmmmerAndKiva = yes;
            _OnUIUpdate();
        }

        public override void OnPlayerJoined(VRCPlayerApi player) => CheckOnlyEmmmerAndKiva("OnPlayerJoined");
        public override void OnPlayerLeft(VRCPlayerApi player) => CheckOnlyEmmmerAndKiva("OnPlayerLeft");

        void OnEnable() {
            if (playbackControlsAnimator != null) playbackControlsAnimator.SetTrigger("Init");
            if (afterFirstRun) return;
            afterFirstRun = true;
            joinTime = DateTime.UtcNow;

            localPlayer = Networking.LocalPlayer;
            Debug.Log($"LocalPlayer: {localPlayer}");

            var hasHandler = Utilities.IsValid(handler);
            if (hasHandler) core = handler.core;
            if (hasHandler) handler.typewriterVideoPlayer = typewriterVideoPlayer;
            if (enqueueCountText != null) {
                enqueueCountFormat = enqueueCountText.text;
                enqueueCountText.text = string.Format(enqueueCountFormat, 0);
            }
            if (playListPanelRoot != null) playListPanelRoot.SetActive(true);
            if (videoListPanelRoot != null) videoListPanelRoot.SetActive(true);
            // if (playListScrollView != null) {
            //     playListNames = hasHandler ? handler.PlayListTitles : null;
            //     if (playListNames != null) {
            //         if (handler.HasQueueList) {
            //             var temp = new string[playListNames.Length + 1];
            //             temp[0] = languageManager.GetLocale("QueueList");
            //             Array.Copy(playListNames, 0, temp, 1, playListNames.Length);
            //             playListNames = temp;
            //         }
            //     } else if (playListNames == null)
            //         playListNames = new [] { languageManager.GetLocale("QueueList") };
            //     bool hasPlayList = playListNames.Length > 1;
            //     playListScrollView.EventPrefix = "_OnPlayList";
            //     playListScrollView.CanDelete = false;
            //     playListScrollView.EntryNames = playListNames;
            //     SelectedPlayListIndex = hasHandler ? handler.PlayListIndex : 0;
            //     if (playListTogglePanelButton != null)
            //         playListScrollView.gameObject.SetActive(false);
            //     else
            //         playListScrollView.gameObject.SetActive(hasPlayList);
            // }
            BuildVideoCategoryListScrollView();
            if (queueListScrollView != null) {
                queueListScrollView.EventPrefix = "_OnQueueList";
                queueListScrollView.gameObject.SetActive(hasHandler);
            }
            if (videoListScrollView != null) {
                videoListScrollView.EventPrefix = "_OnVideoList";
                videoListScrollView.gameObject.SetActive(true);
                videoListScrollView.CanDelete = false;
            }
            if (videoPlayerSelectButtonTemplate != null) {
                var templateTransform = videoPlayerSelectButtonTemplate.transform;
                var parent = videoPlayerSelectRoot.transform;
                var sibling = templateTransform.GetSiblingIndex() + 1;
                var videoPlayerNames = core.PlayerNames;
                videoPlayerSelectButtons = new ButtonEntry[videoPlayerNames.Length];
                for (int i = 0; i < videoPlayerNames.Length; i++) {
                    var button = Instantiate(videoPlayerSelectButtonTemplate);
                    button.SetActive(true);
                    var buttonTransform = button.transform;
                    buttonTransform.SetParent(parent, false);
                    buttonTransform.SetSiblingIndex(sibling + i);
                    var buttonControl = button.GetComponent<ButtonEntry>();
                    buttonControl.LanguageManager = languageManager;
                    buttonControl.Key = videoPlayerNames[i];
                    buttonControl.callbackTarget = this;    
                    buttonControl.callbackEventName = nameof(_LoadPlayerClick);
                    buttonControl.callbackVariableName = nameof(loadWithIndex);
                    buttonControl.callbackUserData = (byte)(i + 1);
                    videoPlayerSelectButtons[i] = buttonControl;
                }
                videoPlayerSelectButtonTemplate.SetActive(false);
            }
            if (playNextIndicator != null) playNextIndicator.SetActive(false);
            bool isSynced = core.IsSynced;
            if (shiftBack100msButton != null) shiftBack100msButton.gameObject.SetActive(isSynced);
            if (shiftBack50msButton != null) shiftBack50msButton.gameObject.SetActive(isSynced);
            if (shiftForward50msButton != null) shiftForward50msButton.gameObject.SetActive(isSynced);
            if (shiftForward100msButton != null) shiftForward100msButton.gameObject.SetActive(isSynced);
            if (shiftResetButton != null) shiftResetButton.gameObject.SetActive(isSynced);
            if (shiftOffsetText != null) shiftOffsetText.gameObject.SetActive(isSynced);
            _OnUIUpdate();
            _OnVolumeChange();
            _OnSyncOffsetChange();
            UpdatePlayerText();
            InitializePersistentData();
            LoadSongIndex();
        }

        void BuildVideoCategoryListScrollView() {
            var hasHandler = Utilities.IsValid(handler);
            if (videoCategoryListScrollView != null && hasHandler) {
                var playListNames = new string[handler.PlayListTitles.Length];
                for (int i = 0; i < playListNames.Length; i++) {
                    var count = GetVideoCategoryItemCount(i);
                    playListNames[i] = $"{handler.PlayListTitles[i]} ({count})";
                }
                bool hasCategories = playListNames.Length > 1;
                videoCategoryListScrollView.EventPrefix = "_OnVideoCategoryList";
                videoCategoryListScrollView.CanDelete = false;
                videoCategoryListScrollView.EntryNames = playListNames;
                SelectedCategoryIndex = hasHandler ? handler.PlayListIndex : 0;

                if (videoCategoryListTogglePanelButton != null)
                    videoCategoryListScrollView.gameObject.SetActive(false);
                else
                    videoCategoryListScrollView.gameObject.SetActive(hasCategories);
            }
        }

        void LoadSongIndex() {
            if (Utilities.IsValid(handler)) {
                VRCStringDownloader.LoadUrl(handler.SongIndexUrl, (IUdonEventReceiver) this);
            } else {
                _OnUIUpdate();
            }
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result) {
            string resultAsUTF8 = result.Result;
            Debug.Log($"LoadSongIndex: UTF8: {resultAsUTF8}");
            var ok = handler.LoadSongIndexJson(resultAsUTF8);
            Debug.Log($"LoadSongIndex: OK: {ok}");
            BuildVideoCategoryListScrollView();
            _OnUIUpdate();
        }

        public override void OnStringLoadError(IVRCStringDownload result) {
            Debug.LogError($"LoadSongIndex: Error loading string: {result.ErrorCode} - {result.Error}");
            _OnUIUpdate();
        }

        public void _Play() {
            if (Utilities.IsValid(handler))
                handler._Play();
            else
                core.Play();
            _InputCancelClick();
        }

        public void _Pause() {
            if (Utilities.IsValid(handler))
                handler._Pause();
            else
                core.Pause();
            _InputCancelClick();
        }

        public void _Stop() {
            if (Utilities.IsValid(handler))
                handler._Stop();
            else
                core.Stop();
            if (enqueueCountText != null) enqueueCountText.text = string.Format(enqueueCountFormat, 0);
            _InputCancelClick();
        }

        public void _Skip() {
            if (!Utilities.IsValid(handler)) return;
            handler._Skip();
            _InputCancelClick();
        }

        public void _RepeatOff() {
            if (Utilities.IsValid(handler))
                handler.NoRepeat();
            else
                core.Loop = false;
        }

        public void _RepeatOne() {
            if (Utilities.IsValid(handler))
                handler.RepeatOne = true;
            else
                core.Loop = true;
        }

        public void _RepeatAll() {
            if (Utilities.IsValid(handler))
                handler.RepeatAll = true;
            else
                core.Loop = true;
        }

        public void _ShuffleOff() {
            if (Utilities.IsValid(handler))
                handler.Shuffle = false;
        }

        public void _ShuffleOn() {
            if (Utilities.IsValid(handler))
                handler.Shuffle = true;
        }

        public void _LocalSync() {
            if (Utilities.IsValid(handler))
                handler._LocalSync();
            else
                core.LocalSync();
            _InputCancelClick();
        }

        public void _GlobalSync() {
            if (Utilities.IsValid(handler))
                handler._GlobalSync();
            else
                core.GlobalSync();
            _InputCancelClick();
        }

        public void _OnSeek() {
            core.Progress = progressSlider.value;
        }

        public void _OnVolumeSlide() {
            userChangedVolume = true;
            core.Volume = volumeSlider.value;
        }

        public void FixedUpdate() {
            UpdateScreenFollowingOrAdaptivePosition();
        }

        public void UpdateAdvancedSettinsUI() {
            var flip = localIsVideoMirrored;
            if (mirrorOnButton != null) mirrorOnButton.gameObject.SetActive(!flip);
            if (mirrorOffButton != null) mirrorOffButton.gameObject.SetActive(flip);
            var follow = localIsScreenFollowing;
            if (screenFollowOnButton != null) screenFollowOnButton.gameObject.SetActive(!follow);
            if (screenFollowOffButton != null) screenFollowOffButton.gameObject.SetActive(follow);
            var adaptive = localIsScreenAdaptive;
            if (screenAdaptiveOnButton != null) screenAdaptiveOnButton.gameObject.SetActive(!adaptive);
            if (screenAdaptiveOffButton != null) screenAdaptiveOffButton.gameObject.SetActive(adaptive);
            if (remoteReceiptClient != null && remoteReceiptApiUrlInput != null) {
                if (remoteReceiptClient.ReceiptClientEnabled) {
                    remoteReceiptApiUrlInput.SetUrl(remoteReceiptClient.ApiUrl);
                    if (remoteReceiptStatusText != null) {
                        remoteReceiptStatusText.text = remoteReceiptClient.ReceiptClientConnected 
                            ? languageManager.GetLocale("RemoteReceiptEnabled")
                            : languageManager.GetLocale("RemoteReceiptConnecting");
                    }
                } else {
                    remoteReceiptApiUrlInput.SetUrl(remoteReceiptClient.defaultReceiptApiUrlPrefix);
                    if (remoteReceiptStatusText != null)
                        remoteReceiptStatusText.text = languageManager.GetLocale("RemoteReceipt");
                }
            }
            if (remoteReceiptAutoAcceptOnButton != null) remoteReceiptAutoAcceptOnButton.gameObject.SetActive(!localIsRemoteReceiptAutoAccept);
            if (remoteReceiptAutoAcceptOffButton != null) remoteReceiptAutoAcceptOffButton.gameObject.SetActive(localIsRemoteReceiptAutoAccept);
        }

        public void _OnRemoteReceiptApiURLValueChanged() {
            Debug.Log("Remote receipt API URL value changed");
        }

        public void _OnRemoteReceiptApiURLEndEdit() {
            if (remoteReceiptClient == null || remoteReceiptApiUrlInput == null) return;
            _OnRemoteReceiptApiURLValueChanged();
            Debug.Log("Remote receipt API URL end edit");

            var url = remoteReceiptApiUrlInput.GetUrl();
            if (url == null || url == VRCUrl.Empty || string.IsNullOrEmpty(url.Get()) || url.Equals(remoteReceiptClient.defaultReceiptApiUrlPrefix)) {
                remoteReceiptClient.ReceiptClientEnabled = false;
                Debug.Log($"Remote receipt enabled: {remoteReceiptClient.ReceiptClientEnabled}");
            } else {
                remoteReceiptClient.ApiUrl = url;
                remoteReceiptClient.EventListener = this;
                remoteReceiptClient.EventListenerPrefix = "_OnRemoteReceipts";
                remoteReceiptClient.ReceiptClientEnabled = true;
                Debug.Log($"Remote receipt enabled: {remoteReceiptClient.ReceiptClientEnabled}");
            }
            UpdateAdvancedSettinsUI();
        }

        public void _OnRemoteReceiptsConnected() {
            UpdateAdvancedSettinsUI();
        }

        public void _OnRemoteReceiptsDisconnected() {
            UpdateAdvancedSettinsUI();
        }

        public void _OnRemoteReceiptsUpdated() {
            Debug.Log("=======================================");
            if (localIsRemoteReceiptAutoAccept) {
                var awaitingReceipts = ComputeAwatingReceipts();
                for (int i = 0; i < awaitingReceipts.Count; i++) {
                    var receipt = ReceiptClient.ReceiptAtIndex(awaitingReceipts, i);
                    AcceptReceipt(receipt, "automatically");
                }
            } else {
                Debug.Log($"UI Handler: Remote receipts updated, refreshing play list");
                UpdatePlayList();
            }
            Debug.Log("=======================================");
        }

        public void _RemoteReceiptAutoAcceptOn() {
            localIsRemoteReceiptAutoAccept = true;
            UpdateAdvancedSettinsUI();
        }

        public void _RemoteReceiptAutoAcceptOff() {
            localIsRemoteReceiptAutoAccept = false;
            UpdateAdvancedSettinsUI();
        }

        public string FormatReceiptMessage(DataDictionary receipt) {
            var target = ReceiptClient.ReceiptTarget(receipt);
            var sender = ReceiptClient.ReceiptSender(receipt);
            var message = ReceiptClient.ReceiptMessage(receipt);

            sender = sender != null ? sender : "???";
            var extraMessage = message != null 
                ? $"{sender} -> {target}: {message}"
                : $"{sender} -> {target}";
            return extraMessage;
        }

        public void UpdateScreenFollowingOrAdaptivePosition() {
            if (playingScreen == null) return;
            if (!localIsScreenFollowing && !localIsScreenAdaptive) return;

            var screenTransform = ((GameObject) playingScreen).transform;
            

            var screenX = screenTransform.localPosition.x;
            var screenY = screenTransform.localPosition.y;

            if (localIsScreenFollowing) {
                // Get the origin point of the player.
                var trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
                var position = trackingData.position;
                
                // NOTE: the dance room is 15x15, as set in unity editor, so
                // the player's x position should be in [-7.5, 7.5] range.
                var x = position.x;
                // the screen's X position should be in [-0.75, 0.75] range, as seen in unity editor, so
                // we need to map the player's X to the screen's X.
                var sx = x / 10.0f;
                // ensure the screen's X is in [-0.75, 0.75] range!
                sx = Mathf.Clamp(x, -0.75f, 0.75f);
                // finally, set the screen's X position.
                screenX = sx;
            }

            if (localIsScreenAdaptive) {
                // Get the head position of the player.
                var trackingData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                var position = trackingData.position;
                
                var y = position.y;
                var z = position.z;

                // let L be the distance between the mirror and the screen.
                // The dance room is 15x15, as set in unity editor.
                // If the user is in front area, the L should be 15.0f.
                // If the user is in back area, the L should be 7.5f.
                // |   |   |
                // ^ front mirror    (where z = -7.5f)
                //     ^ back mirror (where z = 0)
                //         ^ screen  (where z = 7.5f)
                var L = 0.0f;
                if (z < 0) L = 15.0f; // the user is in front area
                else       L = 7.5f;  // the user is in back area

                // let z be the distance between the player and the screen.
                var disZ = 0.0f;
                if (z < 0) disZ = z - (-7.5f); // the user is in front area
                else       disZ = z - 0.0f;    // the user is in back area

                // let f be the focus of the camera, we don't know the exact value,
                // but we believe physics.
                var f = 0.0f;
                if (z < 0) f = 2.0f * 2.718f; // the user is in front area
                else       f = 1.5f * 2.718f; // the user is in back area

                // the screen height should be abs(y)*(L+f)/(abs(z)+f)
                var absY = Mathf.Abs(y);
                var absZ = Mathf.Abs(z);
                var h = absY * (L + f) / (disZ + f);

                // ensure the screen's Y is in [2.1, 5.0f] range!
                if (h > 2.1f && h < 5.0f) screenY = h;
                Debug.Log($"Screen Adaptive: player={position}, L={L}, f={f}, y={absY}, z={disZ}, h={h}");
            }

            screenTransform.localPosition = new Vector3(screenX, screenY, screenTransform.localPosition.z);
            // Debug.Log($"Screen following or adaptive: x={screenX}, y={screenY}, screen position: {screenTransform.localPosition}");
        }

        public void InitializePersistentData() {
            if (vrcxPersistence == null) return;
            if (!vrcxPersistence.IsReady()) {
                // Retry later
                Debug.Log("[Persistence] Waiting for VRCX persistence to be ready...");
                SendCustomEventDelayedFrames(nameof(InitializePersistentData), 1);
                return;
            }

            // Screen following
            bool screenFollow = false;
            if (!vrcxPersistence.TryGetBool(KEY_ScreenFollow, out screenFollow)) {
                screenFollow = false;
                Debug.Log($"[Persistence] Failed to load screen following setting, using default: {screenFollow}");
            }
            Debug.Log($"[Persistence] Screen following: {screenFollow}");
            SetScreenFollowing(screenFollow);
            // Screen adaptive
            bool screenAdaptive = false;
            if (!vrcxPersistence.TryGetBool(KEY_ScreenAdaptive, out screenAdaptive)) {
                screenAdaptive = false;
                Debug.Log($"[Persistence] Failed to load screen adaptive setting, using default: {screenAdaptive}");
            }
            Debug.Log($"[Persistence] Screen adaptive: {screenAdaptive}");
            SetScreenAdaptive(screenAdaptive);
            // Sync offset
            float syncOffset = 0.0f;
            if (!vrcxPersistence.TryGetFloat(KEY_SyncOffset, out syncOffset)) {
                syncOffset = 0.0f;
                Debug.Log($"[Persistence] Failed to load sync offset setting, using default: {syncOffset}");
            }
            Debug.Log($"[Persistence] Sync offset: {syncOffset}");
            core.SyncOffset = syncOffset;
        }

        public void SetVideoMirrored(bool flip) {
            if (playingScreen == null) return;
            // Update UI status
            localIsVideoMirrored = flip;
            UpdateAdvancedSettinsUI();
            
            // OK, chane the shader parameter
            var shaderInput = !flip; // YES! this is correct!
            var playingScreenRenderer = ((GameObject) playingScreen).GetComponent<Renderer>();
            playingScreenRenderer.material.SetInteger("_IsMirror", shaderInput ? 1 : 0);
        }

        public void SetScreenFollowing(bool follow) {
            if (playingScreen == null) return;
            // Update UI status
            localIsScreenFollowing = follow;
            UpdateAdvancedSettinsUI();
            if (!follow) {
                // reset the screen X position
                var screenTransform = ((GameObject) playingScreen).transform;
                screenTransform.localPosition = new Vector3(0, screenTransform.localPosition.y, screenTransform.localPosition.z);
            }
        }

        public void SetScreenAdaptive(bool adaptive) {
            if (playingScreen == null) return;
            // Update UI status
            localIsScreenAdaptive = adaptive;
            UpdateAdvancedSettinsUI();
            if (!adaptive) {
                // reset the screen Y position
                var screenTransform = ((GameObject) playingScreen).transform;
                screenTransform.localPosition = new Vector3(screenTransform.localPosition.x, 2.1f, screenTransform.localPosition.z);
            }
        }

        public void PersistentScreenFollowing() {
            if (vrcxPersistence == null) return;
            Debug.Log($"[Persistence] Persisting screen following: {localIsScreenFollowing}");
            vrcxPersistence.StoreData(KEY_ScreenFollow, localIsScreenFollowing);
        }

        public void PersistentScreenAdaptive() {
            if (vrcxPersistence == null) return;
            Debug.Log($"[Persistence] Persisting screen adaptive: {localIsScreenAdaptive}");
            vrcxPersistence.StoreData(KEY_ScreenAdaptive, localIsScreenAdaptive);
        }

        public void _ManualMirrorOn() {
            userChangedMirror = true;
            SetVideoMirrored(true);
        }

        public void _ManualMirrorOff() {
            userChangedMirror = true;
            SetVideoMirrored(false);
        }

        public void _ScreenFollowOn() {
            SetScreenFollowing(true);
            PersistentScreenFollowing();
        }

        public void _ScreenFollowOff() {
            SetScreenFollowing(false);
            PersistentScreenFollowing();
        }

        public void _ScreenAdaptiveOn() {
            SetScreenAdaptive(true);
            PersistentScreenAdaptive();
        }

        public void _ScreenAdaptiveOff() {
            SetScreenAdaptive(false);
            PersistentScreenAdaptive();
        }

        public void _OnMute() {
            core.Muted = !core.Muted;
        }

        public void _OnVolumeChange() {
            if (!afterFirstRun) return;
            if (volumeSlider != null)
                volumeSlider.SetValueWithoutNotify(core.Volume);
            if (muteButton != null && unmuteButton != null) {
                var muted = core.Muted;
                muteButton.gameObject.SetActive(!muted);
                unmuteButton.gameObject.SetActive(muted);
            }
        }

        public void _OnURLChanged() {
            bool isEmpty =  string.IsNullOrEmpty(urlInput.textComponent.text);
            if (otherObjectUnderUrlInput != null) otherObjectUnderUrlInput.SetActive(isEmpty);
            if (videoPlayerSelectPanel != null) videoPlayerSelectPanel.SetActive(!isEmpty);
        }

        public void _OnURLEndEdit() {
            _OnURLChanged();
            if (urlInputConfirmButton == null) _InputConfirmClick();
        }

        public void _InputConfirmClick() {
            var url = urlInput.GetUrl();
            var title = $"Custom URL: {url.Get()}";
            AddUrlToQueueList(url, title);
        }

        public void AddUrlToQueueList(VRCUrl url, string title = null, int id = -1, string extraMessage = null) {
            if (Utilities.IsValid(url) && !string.IsNullOrEmpty(url.Get())) {
                playListLastInteractTime = joinTime;
                if (Utilities.IsValid(handler)) {
                    handler.PlayUrl(url, selectedPlayer, title, id, extraMessage);
                    if (queueListScrollView != null)
                        SelectedPlayListIndex = handler.PlayListIndex;
                    UpdatePlayList();
                } else
                    core.PlayUrl(url, selectedPlayer);
                _InputCancelClick();
            }
        }
        
        public void _VideoPlayerSelect() {
            if (videoPlayerSelectRoot == null) return;
            videoPlayerSelectRoot.SetActive(!videoPlayerSelectRoot.activeSelf);
        }

        public void _InputCancelClick() {
            urlInput.SetUrl(VRCUrl.Empty);
            _OnUIUpdate();
            _OnURLChanged();
        }

        public void _PlayListTogglePanel() {
            if (playListScrollView == null) return;
            var playListGameObject = playListScrollView.gameObject;
            playListGameObject.SetActive(!playListGameObject.activeSelf);
        }

        public void _VideoCategoryListTogglePanel() {
            if (videoCategoryListScrollView == null) return;
            var videoCategoryListGameObject = videoCategoryListScrollView.gameObject;
            videoCategoryListGameObject.SetActive(!videoCategoryListGameObject.activeSelf);
        }

        public void _VideoListFavorite() {
            // TODO[kiva]: Implement favorite
        }

        public void _OnLanguageChanged() {
            if (!afterFirstRun) return;
            _OnUIUpdate();
            _OnSyncOffsetChange();
            // if (Utilities.IsValid(handler) && handler.HasQueueList && playListNames != null) {
            //     playListNames[0] = languageManager.GetLocale("QueueList");
            //     if (playListScrollView != null) 
            //         playListScrollView.EntryNames = playListNames;
            // }
            UpdatePlayerText();
        }

        public void _LoadPlayerClick() {
            selectedPlayer = loadWithIndex;
            UpdatePlayerText();
            if (videoPlayerSelectRoot != null) videoPlayerSelectRoot.SetActive(false);
        }

        void UpdatePlayerText() {
            selectdPlayerText.text = videoPlayerSelectButtons[selectedPlayer - 1].Text;
        }

        public void _OnUIUpdate() {
            if (!afterFirstRun) return;
            bool hasHandler = Utilities.IsValid(handler);
            bool unlocked = !hasHandler || !handler.Locked;
            bool canPlay = false;
            bool canPause = false;
            // bool canStop = false;
            // bool canLocalSync = false;
            bool canSeek = false;
            switch (core.State) {
                case 0: // Idle
                    if (idleScreenRoot != null) idleScreenRoot.SetActive(true);
                    if (statusText != null) {
                        if (timeContainer != null) {
                            timeContainer.SetActive(false);
                            statusText.enabled = true;
                        }
                        statusText.text = "";
                    }
                    if (currentVideoTitleText != null) {
                        currentVideoTitleText.text = "QwQ";
                    }
                    if (durationText != null) durationText.text = languageManager.GetLocale("TimeIdleFormat");
                    if (timeText != null) timeText.text = languageManager.GetLocale("TimeIdleFormat");
                    break;
                case 1: // Loading
                    if (idleScreenRoot != null) idleScreenRoot.SetActive(true);
                    if (statusText != null) {
                        if (timeContainer != null) {
                            timeContainer.SetActive(false);
                            statusText.enabled = true;
                        }
                        statusText.text = languageManager.GetLocale("Loading");
                    }
                    if (durationText != null) durationText.text = languageManager.GetLocale("TimeIdleFormat");
                    if (timeText != null) timeText.text = languageManager.GetLocale("TimeIdleFormat");
                    // canStop = unlocked;
                    break;
                case 2: // Error
                    if (idleScreenRoot != null) idleScreenRoot.SetActive(true);
                    if (statusText == null) break;
                    if (timeContainer != null) {
                        statusText.enabled = true;
                        timeContainer.SetActive(false);
                    }
                    var errorCode = core.LastError;
                    switch (errorCode) {
                        case VideoError.InvalidURL: statusText.text = languageManager.GetLocale("InvalidURL"); break;
                        case VideoError.AccessDenied: statusText.text = languageManager.GetLocale(core.IsTrusted ? "AccessDenied" : "AccessDeniedUntrusted"); break;
                        case VideoError.PlayerError: statusText.text = languageManager.GetLocale("PlayerError"); break;
                        case VideoError.RateLimited: statusText.text = languageManager.GetLocale("RateLimited"); break;
                        default: statusText.text = string.Format(languageManager.GetLocale("Unknown"), (int)errorCode); break;
                    }
                    if (durationText != null) durationText.text = "";
                    if (timeText != null) timeText.text = "";
                    // canStop = unlocked;
                    break;
                case 3: // Ready
                    if (idleScreenRoot != null) idleScreenRoot.SetActive(true);
                    if (statusText != null) {
                        if (timeContainer != null) {
                            timeContainer.SetActive(false);
                            statusText.enabled = true;
                        }
                        statusText.text = languageManager.GetLocale("Ready");
                    }
                    if (progressSlider != null) {
                        progressSlider.SetValueWithoutNotify(1);
                        progressSlider.interactable = false;
                    }
                    canPlay = unlocked;
                    break;
                case 4: // Playing
                    if (idleScreenRoot != null) idleScreenRoot.SetActive(false);
                    if (timeContainer != null && statusText != null) {
                        timeContainer.SetActive(true);
                        statusText.enabled = false;
                    }
                    int id = core.LocalSongId;
                    int ownerId = core.LocalOwnerId;
                    string textInTitle = !string.IsNullOrEmpty(core.LocalTitle) ? core.LocalTitle : core.Url.Get();
                    Debug.Log($"core.LocalSongId = {id}, core.LocalOwnerId = {ownerId}");

                    int offset = handler.SongIdToOffset(id);
                    // Don't flip the video by default.
                    var flip = offset != -1 ? handler.PlayListEntryFlips[offset] : false;
                    // Give a safe volume for the video which we don't know.
                    var volume = offset != -1 ? handler.PlayListEntryVolumes[offset] : 0.3;

                    Debug.Log($"Song id {id}: default flip={flip}, default volume={volume}");

                    // Apply default mirror setting according to the video metadata,
                    // but respect user's manual setting for this video.
                    if (playingScreen != null) {
                        if (!userChangedMirror) SetVideoMirrored(flip);
                        textInTitle = $"{textInTitle} {(flip ? "·" : "")}";
                    }

                    // Apply initial suggested volume to the video,
                    // but respect user's manual setting for this video.
                    if (!userChangedVolume) core.Volume = (float) volume;

                    if (currentVideoTitleText != null) {
                        currentVideoTitleText.text = textInTitle;
                    }

                    canPause = unlocked;
                    // canStop = unlocked;
                    canSeek = true;
                    break;
                case 5: // Paused
                    if (idleScreenRoot != null) idleScreenRoot.SetActive(false);
                    if (timeContainer != null && statusText != null) {
                        timeContainer.SetActive(true);
                        statusText.enabled = false;
                    }
                    canPlay = unlocked;
                    // canStop = unlocked;
                    canSeek = true;
                    break;
            }

            // OK, apply status variables to UI
            UpdateAdvancedSettinsUI();

            // Always shoudl the re-sync button
            // if (reloadButton != null) {
            //     var localUrl = core.Url;
            //     canLocalSync = Utilities.IsValid(localUrl) && !localUrl.Equals(VRCUrl.Empty);
            // }
            if (playButton != null) playButton.gameObject.SetActive(canPlay);
            if (pauseButton != null) pauseButton.gameObject.SetActive(canPause);
            // if (stopButton != null) stopButton.gameObject.SetActive(canStop);
            // if (reloadButton != null) reloadButton.gameObject.SetActive(canLocalSync);

            if (progressSlider != null) {
                if (canSeek) {
                    UpdateProgressOnce();
                    if (!hasUpdate) {
                        hasUpdate = true;
                        _UpdateProgress();
                    }
                    progressSlider.interactable = unlocked;
                } else {
                    progressSlider.SetValueWithoutNotify(1);
                    progressSlider.interactable = false;
                }
            }
            if (wasUnlocked != unlocked || !hasUnlockInit) {
                hasUnlockInit = true;
                wasUnlocked = unlocked;
                if (queueListScrollView != null) queueListScrollView.CanInteract = unlocked;
                if (playListScrollView != null) playListScrollView.CanInteract = unlocked;
                if (repeatOffButton != null) repeatOffButton.interactable = unlocked;
                if (repeatOneButton != null) repeatOneButton.interactable = unlocked;
                if (repeatAllButton != null) repeatAllButton.interactable = unlocked;
                if (shuffleOnButton != null) shuffleOnButton.interactable = unlocked;
                if (playNextButton != null) playNextButton.interactable = unlocked;
                // if (playListTogglePanelButton != null)
                //     playListTogglePanelButton.interactable = unlocked && playListNames != null && playListNames.Length > 1;
                if (videoCategoryListTogglePanelButton != null) 
                    videoCategoryListTogglePanelButton.interactable = unlocked && handler.PlayListTitles != null && handler.PlayListTitles.Length > 1;
                if (urlInput != null) {
                    urlInput.interactable = unlocked;
                    if (!unlocked) urlInput.SetUrl(VRCUrl.Empty);
                }
            }
            if (hasHandler) {
                bool isRepeatOne = handler.RepeatOne;
                bool isRepeatAll = handler.RepeatAll;
                bool isShuffle = handler.Shuffle;
                if (repeatOffButton != null) repeatOffButton.gameObject.SetActive(!isRepeatOne && !isRepeatAll);
                if (repeatOneButton != null) repeatOneButton.gameObject.SetActive(isRepeatOne);
                if (repeatAllButton != null) repeatAllButton.gameObject.SetActive(isRepeatAll);
                if (shuffleOffButton != null) {
                    shuffleOffButton.gameObject.SetActive(!isShuffle);
                    shuffleOffButton.interactable = unlocked;
                }
                if (shuffleOnButton != null) shuffleOnButton.gameObject.SetActive(isShuffle);
                UpdatePlayList();
                UpdateVideoList();
                if (playNextIndicator != null)
                    playNextIndicator.SetActive(!isShuffle && SelectedPlayListIndex == 0 && handler.PlayListIndex == 0 && handler.PendingCount > 0);
                if (queueModeText != null)
                    queueModeText.text = languageManager.GetLocale(
                        handler.PlayListIndex == 0 && handler.HasQueueList && (core.IsReady || core.IsLoading || handler.QueueUrls.Length > 0) ?
                        "QueueModeNext" : "QueueModeInstant"
                    );
            } else {
                bool isRepeatOne = core.Loop;
                if (repeatOffButton != null) repeatOffButton.gameObject.SetActive(!isRepeatOne);
                if (repeatOneButton != null) repeatOneButton.gameObject.SetActive(isRepeatOne);
                if (repeatAllButton != null) repeatAllButton.gameObject.SetActive(false);
                if (shuffleOffButton != null) {
                    shuffleOffButton.gameObject.SetActive(true);
                    shuffleOffButton.interactable = false;
                }
                if (shuffleOnButton != null) shuffleOnButton.gameObject.SetActive(false);
                if (queueModeText != null) queueModeText.text = languageManager.GetLocale("QueueModeInstant");
            }

            // Only if the world is our own, we can do anything we like.
            if (progressSlider != null) progressSlider.interactable = onlyEmmmerAndKiva;
            if (pauseButton != null) pauseButton.gameObject.SetActive(canPause && onlyEmmmerAndKiva);
            if (playButton != null) playButton.gameObject.SetActive(canPlay && onlyEmmmerAndKiva);
            if (stopButton != null) stopButton.gameObject.SetActive(onlyEmmmerAndKiva);
        }

        public void _DeferUpdatePlayList() {
            if (playListUpdateRequired && !UpdatePlayList() && playListUpdateRequired)
                SendCustomEventDelayedFrames(nameof(_DeferUpdatePlayList), 0);
        }

        private int GetVideoCategoryItemCount(int categoryIndex) {
            int[] urlOffsets = handler.PlayListUrlOffsets;
            int offset = urlOffsets[categoryIndex];
            return (categoryIndex + 1 < urlOffsets.Length ? urlOffsets[categoryIndex + 1] : handler.PlayListUrls.Length) - offset;
        }

        public void _OnSearchBoxValueChanged() {
            var text = searchBox.text;
            searchBoxBuffer = text;
            UpdateVideoList();
        }

        public void _OnSearchBoxEndEdit() {
            // Seems this is never called
        }

        public void _VideoListQueueUp() {
            var clickedIndex = videoListScrollView.lastInteractIndex;

            bool hasSearchContent = !string.IsNullOrEmpty(searchBoxBuffer);
            if (hasSearchContent) {
                var clickedId = videoListEntryIds[clickedIndex];
                handler.PlaySongById(clickedId, null);

            } else {
                int categoryListIndex = SelectedCategoryIndex;
                int[] entryIds = handler.PlayListEntryIds;
                int[] urlOffsets = handler.PlayListUrlOffsets;
                int offset = urlOffsets[categoryListIndex];

                var id = entryIds[offset + clickedIndex];
                handler.PlaySongById(id, null);
            }
        }

        bool VaguelyMatches(string[] kw, string titleSpell) {
            // If there is no keyword, it's impossible to match
            if (kw == null || kw.Length == 0 || titleSpell == null) return false;

            var spell = titleSpell.Split(' ');
            // Length check: if the keyword is longer than the text, it's impossible to match
            if (kw.Length > spell.Length) return false;

            // A sliding window to match the keyword in spell code
            for (int i = 0; i < spell.Length - kw.Length + 1; i++) {
                bool matches = true;
                for (int j = 0; j < kw.Length; j++) {
                    if (!spell[i + j].ToLower().StartsWith(kw[j].ToLower())) {
                        matches = false;
                        break;
                    }
                }
                if (matches) return true;
            }
            
            return false;
        }

        bool UpdateVideoList() {
            int categoryListIndex = SelectedCategoryIndex;
            bool hasSearchContent = !string.IsNullOrEmpty(searchBoxBuffer);
            videoListScrollView.CanDelete = false;
            videoListScrollView.CanSecondary = false;

            string[] entryTitles = handler.PlayListEntryTitles;
            var entryTitleSpells = handler.PlayListEntryTitleSpells;
            int[] entryIds = handler.PlayListEntryIds;
            int[] urlOffsets = handler.PlayListUrlOffsets;

            // offset of the first song in currently selected category
            int offset = urlOffsets[categoryListIndex];
            // how many songs in the category
            int numSongInCat = GetVideoCategoryItemCount(categoryListIndex);

            videoListEntryTitles = new string[entryTitles.Length];
            videoListEntryIds = new int[entryIds.Length];

            if (hasSearchContent) {
                // To UdonSharp: Please, give me the ability to use LINQ! Classic for-loop is so boilerplate!
                // To UdonSharp: Please, give me the ability to define classes and methods! I don't want to tear an object into pieces!
                // To UdonSharp: Please, I really really need abstracting over datatypes!
                // To UdonSharp: Please, stop making garbage programming languages! If you really want a DSL, learn some PL theory!
                // ONLY PEOPLE WHO ARE SOPHISTICATED IN CODING LIKE ME CAN MAINTAIN THIS PIECE OF SHIT!
                int count = 0;
                var kw = searchBoxBuffer.Split(' ');
                // Search vaguely only when there are not spaces
                string[] kwVague = null;
                if (searchBoxBuffer.IndexOf(' ') == -1) {
                    var chars = searchBoxBuffer.ToCharArray();
                    kwVague = new string[chars.Length];
                    for (int i = 0; i < chars.Length; i++) {
                        kwVague[i] = chars[i].ToString();
                    }
                }

                // only search in the current category
                for (int i = offset; i < offset + numSongInCat; i++) {
                    var title = entryTitles[i];
                    var spell = entryTitleSpells[i];
                    bool matches = VaguelyMatches(kwVague, spell) || VaguelyMatches(kw, spell != null ? spell : title);

                    if (matches) {
                        videoListEntryTitles[count] = $"{entryIds[i]}: {title}";
                        videoListEntryIds[count] = entryIds[i];
                        count++;
                    }
                }
                if (selectedCategoryText != null) {
                    var text = $"\"{searchBoxBuffer}\" in {handler.PlayListTitles[categoryListIndex]} ({count})";
                    selectedCategoryText.text = text;
                }
                videoListScrollView.SetEntries(videoListEntryTitles, 0, count);

            } else {
                for (int i = 0; i < entryTitles.Length; i++) {
                    videoListEntryTitles[i] = $"{entryIds[i]}: {entryTitles[i]}";
                    videoListEntryIds[i] = entryIds[i];
                }
                if (selectedCategoryText != null) {
                    var text = $"{handler.PlayListTitles[categoryListIndex]} ({numSongInCat})";
                    selectedCategoryText.text = text;
                }
                videoListScrollView.SetEntries(videoListEntryTitles, offset, numSongInCat);
            }

            return true;
        }

        bool UpdatePlayList() {
            int playListIndex = handler.PlayListIndex;
            int playingIndex = handler.CurrentPlayingIndex;
            int displayCount, offset;
            int pendingCount = handler.PendingCount;

            VRCUrl[] queuedUrls = handler.QueueUrls, playListUrls = handler.PlayListUrls;
            string[] entryTitles = handler.PlayListEntryTitles;
            string[] queuedTitles = handler.QueueTitles;
            int[] queuedSongIds = handler.QueueSongIds;
            int[] queuedOwnerIds = handler.QueueOwnerIds;

            int[] urlOffsets = handler.PlayListUrlOffsets;
            if (playListIndex > 0) {
                offset = urlOffsets[playListIndex - 1];
                displayCount = (playListIndex < urlOffsets.Length ? urlOffsets[playListIndex] : playListUrls.Length) - offset;
            } else {
                offset = 0;
                displayCount = queuedUrls.Length;
            }
            bool hasPending = pendingCount > 0;
            bool isEntryContainerInactive = queueListScrollView == null || !queueListScrollView.gameObject.activeInHierarchy;
            int selectedPlayListIndex = SelectedPlayListIndex;
            bool isNotCoolingDown = (DateTime.UtcNow - playListLastInteractTime) >= interactCoolDown;
            if (isEntryContainerInactive || isNotCoolingDown)
                SelectedPlayListIndex = selectedPlayListIndex = playListIndex;
            if (playNextButton != null) playNextButton.gameObject.SetActive(hasPending && onlyEmmmerAndKiva);
            if (enqueueCountText != null)
                enqueueCountText.text = string.Format(enqueueCountFormat, pendingCount);
            if (selectedPlayListText != null)
                selectedPlayListText.text = selectedPlayListIndex > 0 ?
                    handler.PlayListTitles[selectedPlayListIndex - 1] :
                    languageManager.GetLocale("QueueList");
            bool shouldRefreshQueue = playListUpdateRequired || selectedPlayListIndex <= 0 || lastSelectedPlayListIndex != selectedPlayListIndex || lastPlayingIndex != playingIndex;
            lastSelectedPlayListIndex = selectedPlayListIndex;
            lastPlayingIndex = playingIndex;
            if (!shouldRefreshQueue || queueListScrollView == null)
                return false;
            if (isEntryContainerInactive) {
                if (!playListUpdateRequired) {
                    playListUpdateRequired = true;
                    SendCustomEventDelayedFrames(nameof(_DeferUpdatePlayList), 0);
                }
                return false;
            }
            playListUpdateRequired = false;
            if (selectedPlayListIndex != playListIndex) {
                if (selectedPlayListIndex > 0) {
                    offset = urlOffsets[selectedPlayListIndex - 1];
                    displayCount = (selectedPlayListIndex < urlOffsets.Length ? urlOffsets[selectedPlayListIndex] : playListUrls.Length) - offset;
                } else {
                    offset = 0;
                    displayCount = queuedUrls.Length;
                }
                playingIndex = -1;
            }
            if (selectedPlayListIndex == 0) {
                var entryCanDelete = new bool[queuedTitles.Length];
                var entryCanSecondary = new bool[queuedTitles.Length];
                var myOwnerId = localPlayer.displayName.GetHashCode();
                for (int i = 0; i < queuedTitles.Length; i++) {
                    entryCanDelete[i] = onlyEmmmerAndKiva || myOwnerId == queuedOwnerIds[i];
                    entryCanSecondary[i] = false;
                }

                // Now append remote receipts to the end of the queue list
                string[] joined = null;
                if (remoteReceiptClient == null) {
                    joined = queuedTitles;
                } else {
                    var awaitingReceipts = ComputeAwatingReceipts();
                    // Ok, apply local receipts to the queue list
                    joined = new string[queuedTitles.Length + awaitingReceipts.Count];
                    Array.Copy(queuedTitles, 0, joined, 0, queuedTitles.Length);
                    for (int i = 0; i < awaitingReceipts.Count; i++) {
                        var receipt = ReceiptClient.ReceiptAtIndex(awaitingReceipts, i);
                        var song_id = ReceiptClient.ReceiptSongId(receipt);
                        var message = FormatReceiptMessage(receipt);
                        var title = handler.SongTitleById(song_id, null); // should always not null
                        var format = $"{message}\n{song_id}: {title}";
                        joined[queuedTitles.Length + i] = format;
                    }
                    // finally, update global variables
                    localAwaitingReceipts = awaitingReceipts;
                }

                queueListScrollView.CanDelete = false;
                queueListScrollView.CanSecondary = true;
                queueListScrollView.EntryNames = joined;
                queueListScrollView.EntryCanDelete = entryCanDelete;
                queueListScrollView.EntryCanSecondary = entryCanSecondary;
                queueListScrollView.SetIndexWithoutScroll(-1);
            } else {
                queueListScrollView.CanDelete = false;
                queueListScrollView.CanSecondary = false;
                queueListScrollView.SetEntries(entryTitles, offset, displayCount);
                queueListScrollView.SetIndexWithoutScroll(playingIndex);
            }
            if (isNotCoolingDown) queueListScrollView.ScrollToSelected();
            return true;
        }

        public DataList ComputeAwatingReceipts() {
            var awaitingReceipts = new DataList();
            if (remoteReceiptClient == null) return awaitingReceipts;

            var receipts = remoteReceiptClient.GetReceipts();
            for (int i = 0; i < receipts.Count; i++) {
                var receipt = ReceiptClient.ReceiptAtIndex(receipts, i);
                var target = ReceiptClient.ReceiptTarget(receipt);
                if (!target.Equals(Networking.LocalPlayer.displayName))
                    continue;

                var format = ReceiptClient.ReceiptToString(receipt);
                var accepted = remoteReceiptClient.ReceiptIsAlreadyAccepted(receipt);
                if (accepted) {
                    Debug.Log($"Skipping already accepted receipt: {format}");
                    continue;
                }

                // TODO: support song url
                var song_id = ReceiptClient.ReceiptSongId(receipt);
                var try_song_offset = handler.SongIdToOffset(song_id);
                if (try_song_offset == -1) {
                    Debug.Log($"Skipping non-existing song in receipt: {format}");
                    continue;
                }

                awaitingReceipts.Add(receipt);
            }
            return awaitingReceipts;
        }

        public void _OnPlayListEntryClick() {
            playListLastInteractTime = DateTime.UtcNow;
            UpdatePlayList();
            queueListScrollView.ScrollToSelected();
        }

        public void _OnPlayListScroll() {
            playListLastInteractTime = DateTime.UtcNow;
        }

        public void _OnVideoCategoryListEntryClick() {
            UpdateVideoList();
        }

        // TODO[kiva]: _OnVideoCategoryListScroll

        public void _OnVideoListEntryClick() {
            _VideoListQueueUp();
        }

        public void _OnQueueListScroll() {
            playListLastInteractTime = DateTime.UtcNow;
        }

        // public void _OnQueueListEntryClick() {
        //     playListLastInteractTime = DateTime.UtcNow;
        //     handler._PlayAt(SelectedPlayListIndex, queueListScrollView.lastInteractIndex, false);
        // }

        public void _OnQueueListEntryDelete() {
            playListLastInteractTime = DateTime.UtcNow;
            var index = queueListScrollView.lastInteractIndex;
            handler._PlayAt(SelectedPlayListIndex, index, true);
        }

        public void _OnQueueListEntrySecondary() {
            if (remoteReceiptClient == null) return;

            playListLastInteractTime = DateTime.UtcNow;
            var index = queueListScrollView.lastInteractIndex;
            var queuedTitles = handler.QueueTitles;
            if (index > 0 && index < queuedTitles.Length) return;
            var receiptIndex = index - queuedTitles.Length;
            var approved = ReceiptClient.ReceiptAtIndex(localAwaitingReceipts, receiptIndex);
            if (approved == null) return; // race condition, just ignore

            AcceptReceipt(approved, "manually");
        }

        public void AcceptReceipt(DataDictionary approved, string reason) {
            if (remoteReceiptClient == null) return;
            remoteReceiptClient.AcceptReceipt(approved);
            Debug.Log($"UI Handler: Receipt accepted {reason}: {ReceiptClient.ReceiptToString(approved)}");
            
            var song_id = ReceiptClient.ReceiptSongId(approved);
            var extraMessage = FormatReceiptMessage(approved);
            handler.PlaySongById(song_id, extraMessage);
        }

        public void _UpdateProgress() {
            if (!core.IsPlaying) {
                hasUpdate = false;
                return;
            }
            UpdateProgressOnce();
            SendCustomEventDelayedSeconds(nameof(_UpdateProgress), 0.25F);
        }

        void UpdateProgressOnce() {
            var duration = core.Duration;
            if (duration <= 0 || float.IsInfinity(duration)) {
                if (timeContainer != null && statusText != null) {
                    timeContainer.SetActive(false);
                    statusText.enabled = true;
                }
                if (timeText != null)
                    timeText.text = languageManager.GetLocale("TimeIdleFormat");
                if (durationText != null)
                    durationText.text = languageManager.GetLocale("TimeIdleFormat");
                if (statusText != null) {
                    if (!string.IsNullOrEmpty(core.LocalTitle) || !string.IsNullOrEmpty(core.LocalAuthor))
                        statusText.text = string.Format(languageManager.GetLocale("StreamingWithTitle"), core.LocalTitle, core.LocalAuthor);
                    else
                        statusText.text = languageManager.GetLocale("Streaming");
                }
                if (progressSlider != null) {
                    progressSlider.SetValueWithoutNotify(1);
                    progressSlider.interactable = false;
                }
            } else {
                if (timeContainer != null && statusText != null) {
                    timeContainer.SetActive(true);
                    statusText.enabled = false;
                }
                var time = TimeSpan.FromSeconds(core.Time);
                var durationTS = TimeSpan.FromSeconds(duration);
                if (durationText != null)
                    durationText.text = string.Format(languageManager.GetLocale("TimeFormat"), durationTS);
                if (timeText != null)
                    timeText.text = string.Format(languageManager.GetLocale("TimeFormat"), time);
                if (statusText != null) {
                    if (core.IsPaused)
                        statusText.text = string.Format(languageManager.GetLocale("Paused"), time, durationTS);
                    else if (!string.IsNullOrEmpty(core.LocalTitle) || !string.IsNullOrEmpty(core.LocalAuthor))
                        statusText.text = string.Format(languageManager.GetLocale("PlayingWithTitle"), time, durationTS, core.LocalTitle, core.LocalAuthor);
                    else
                        statusText.text = string.Format(languageManager.GetLocale("Playing"), time, durationTS);
                }
                if (progressSlider != null) {
                    progressSlider.SetValueWithoutNotify(core.Progress);
                    progressSlider.interactable = true;
                }
            }
        }

        public void _ShiftBack100ms() {
            core.SyncOffset -= 0.1F;
            PersistentSyncOffset();
        }
        public void _ShiftBack50ms() {
            core.SyncOffset -= 0.05F;
            PersistentSyncOffset();
        }
        public void _ShiftForward50ms() {
            core.SyncOffset += 0.05F;
            PersistentSyncOffset();
        }
        public void _ShiftForward100ms() {
            core.SyncOffset += 0.1F;
            PersistentSyncOffset();
        }
        public void _ShiftReset() {
            core.SyncOffset = 0;
            PersistentSyncOffset();
        }
        public void PersistentSyncOffset() {
            if (vrcxPersistence == null) return;
            Debug.Log($"[Persistence] Persisting sync offset: {core.SyncOffset}");
            vrcxPersistence.StoreData(KEY_SyncOffset, core.SyncOffset);
        }
        public void _OnSyncOffsetChange() {
            if (!afterFirstRun) return;
            if (shiftOffsetText != null) shiftOffsetText.text = string.Format(languageManager.GetLocale("TimeDrift"), core.SyncOffset);
        }

        #region Core Callbacks
        public override void OnVideoReady() => _OnUIUpdate();
        public override void OnVideoStart() => _OnUIUpdate();
        public override void OnVideoPlay() => _OnUIUpdate();
        public override void OnVideoPause() => _OnUIUpdate();
        public override void OnVideoEnd() {
            userChangedVolume = false;
            userChangedMirror = false;
            _OnUIUpdate();
        }
        public void _OnVideoError() => _OnUIUpdate();
        public void _OnVideoBeginLoad() => _OnUIUpdate();
        #endregion
    }
}