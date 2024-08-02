// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Editor.Common.Utils;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Colors;
using Doozy.Runtime.Common.Attributes;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace Doozy.Editor.Soundy
{
    public static class SoundyEditorPlayer
    {
        private static readonly List<Player> Players = new List<Player>();

        [ExecuteOnReload]
        private static void OnReload()
        {
            StopAllPlayers();
        }

        /// <summary> Get a player for the given SoundObject </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        /// <param name="outputAudioMixerGroup"> Optional AudioMixerGroup reference </param>
        public static Player GetPlayer(SoundObject soundObject, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (soundObject == null) return null;
            var player = new Player(soundObject, outputAudioMixerGroup);
            Players.Add(player);
            return player;
        }

        /// <summary> Get a player for the given MusicObject </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        /// <param name="outputAudioMixerGroup"> Optional AudioMixerGroup reference </param>
        public static Player GetPlayer(MusicObject musicObject, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (musicObject == null) return null;
            var player = new Player(musicObject, outputAudioMixerGroup);
            Players.Add(player);
            return player;
        }

        /// <summary> Get a player for the given AudioClip </summary>
        /// <param name="audioClip"> AudioClip reference </param>
        /// <param name="outputAudioMixerGroup"> Optional AudioMixerGroup reference </param>
        public static Player GetPlayer(AudioClip audioClip, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (audioClip == null) return null;
            var player = new Player(audioClip, outputAudioMixerGroup);
            Players.Add(player);
            return player;
        }

        /// <summary> Stop all the players regardless of what they are used for (SoundObjects, MusicObjects, AudioClips, etc.) </summary>
        public static void StopAllPlayers()
        {
            foreach (var player in Players.ToList()) player.Stop();
            Players.Clear();
        }

        /// <summary>
        /// Internal class used to play SoundObjects, MusicObjects and AudioClips
        /// </summary>
        public class Player
        {
            public enum Mode
            {
                SoundObject,
                MusicObject,
                AudioClip
            }

            public Mode mode { get; private set; }
            public SoundObject soundObject { get; private set; }
            public MusicObject musicObject { get; private set; }
            public AudioClip audioClip { get; private set; }
            public AudioMixerGroup outputAudioMixerGroup { get; private set; }

            private DelayedCall selfDestruct { get; set; }

            private AudioSource m_AudioSource;
            public AudioSource audioSource
            {
                get
                {
                    if (m_AudioSource != null) return m_AudioSource;
                    m_AudioSource = EditorUtility.CreateGameObjectWithHideFlags("Soundy Player", HideFlags.DontSave, typeof(AudioSource)).GetComponent<AudioSource>();
                    m_AudioSource.playOnAwake = false; //do not play on awake - we want to control when the audio starts playing
                    return m_AudioSource;
                }
            }

            public string outputAudioMixerGroupName =>
                outputAudioMixerGroup == null
                    ? "No Output AudioMixerGroup"
                    : $"{outputAudioMixerGroup.name} ({outputAudioMixerGroup.audioMixer.name})";

            public bool isPlaying => m_AudioSource != null && m_AudioSource.isPlaying;
            public bool isPaused { get; private set; }

            public float progress =>
                m_AudioSource != null && (isPlaying || isPaused)
                    ? (float)Math.Round(m_AudioSource.time / m_AudioSource.clip.length, 3)
                    : 0f;

            public string playbackTimeLabel =>
                AudioUtils.GetTimePretty(m_AudioSource != null && m_AudioSource.clip != null && isPlaying ? m_AudioSource.time : -1f);

            public string clipLengthLabel =>
                AudioUtils.GetTimePretty(m_AudioSource != null && m_AudioSource.clip != null ? m_AudioSource.clip.length : -1f);

            public string clipNameText =>
                m_AudioSource != null && m_AudioSource.clip != null
                    ? m_AudioSource.clip.name
                    : "---";

            public string durationText =>
                isPlaying
                    ? $"{playbackTimeLabel} / {clipLengthLabel}"
                    : string.Empty;

            private bool canPlay
            {
                get
                {
                    switch (mode)
                    {
                        case Mode.SoundObject: return soundObject != null && soundObject.canPlay;
                        case Mode.MusicObject: return musicObject != null && musicObject.canPlay;
                        case Mode.AudioClip: return audioClip != null;
                        default: return false;
                    }
                }
            }

            public UnityAction onPlay { get; set; }
            public UnityAction onStop { get; set; }
            public UnityAction onUpdate { get; set; }

            /// <summary> Create a new Player to play a SoundObject </summary>
            /// <param name="soundObject"> SoundObject to play </param>
            /// <param name="outputAudioMixerGroup"> Optional AudioMixerGroup to output the sound to </param>
            public Player(SoundObject soundObject, AudioMixerGroup outputAudioMixerGroup = null)
            {
                this.mode = Mode.SoundObject;
                this.soundObject = soundObject;
                this.musicObject = null;
                this.audioClip = null;
                this.outputAudioMixerGroup = outputAudioMixerGroup;
            }

            /// <summary> Create a new Player to play a MusicObject </summary>
            /// <param name="musicObject"> MusicObject to play </param>
            /// <param name="outputAudioMixerGroup"> Optional AudioMixerGroup to output the sound to </param>
            public Player(MusicObject musicObject, AudioMixerGroup outputAudioMixerGroup = null)
            {
                this.mode = Mode.MusicObject;
                this.soundObject = null;
                this.musicObject = musicObject;
                this.audioClip = null;
                this.outputAudioMixerGroup = outputAudioMixerGroup;
            }

            /// <summary> Create a new Player to play an AudioClip </summary>
            /// <param name="audioClip"> AudioClip to play </param>
            /// <param name="outputAudioMixerGroup"> Optional AudioMixerGroup to output the sound to </param>
            public Player(AudioClip audioClip, AudioMixerGroup outputAudioMixerGroup = null)
            {
                this.mode = Mode.AudioClip;
                this.soundObject = null;
                this.musicObject = null;
                this.audioClip = audioClip;
                this.outputAudioMixerGroup = outputAudioMixerGroup;
            }

            /// <summary>
            /// Initialize the AudioSource to prepare it for playing.
            /// Use this if you want to set custom values to the AudioSource before playing (calling Play() will automatically initialize the AudioSource)
            /// </summary>
            /// <returns> Returns the Player </returns>
            public Player InitializeAudioSource()
            {
                m_AudioSource = audioSource;
                return this;
            }

            /// <summary> Plays the set SoundObject, MusicObject or AudioClip </summary>
            /// <returns> Returns the Player </returns>
            public Player Play()
            {
                if (!canPlay) return this;
                if (isPlaying || isPaused)
                {
                    Stop();
                }
                switch (mode)
                {
                    case Mode.SoundObject:
                        if (soundObject == null) return this;
                        audioSource.clip = soundObject.LoadNext();
                        audioSource.volume = soundObject.volume;
                        break;
                    case Mode.MusicObject:
                        if (musicObject == null) return this;
                        audioSource.clip = musicObject.data.Clip;
                        audioSource.volume = musicObject.volume;
                        break;
                    case Mode.AudioClip:
                        if (audioClip == null) return this;
                        audioSource.clip = audioClip;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                audioSource.outputAudioMixerGroup = outputAudioMixerGroup;

                isPaused = false;

                audioSource.Play();

                string duration = audioSource.clip.length.ToString("0.00");
                audioSource.gameObject.name = $"AudioPlayer - {audioSource.clip.name} ({duration} sec)";

                onPlay?.Invoke();

                float clipDuration = audioSource.clip.length;
                selfDestruct = GetSelfDestruct(clipDuration);
                return this;
            }

            /// <summary>
            /// Get a DelayedCall that will stop the audio source after the clip duration.
            /// This is needed because the audio source will not stop automatically when the clip is finished playing.
            /// In case the audio source is looping, this method will call itself recursively.
            /// </summary>
            /// <param name="clipDuration"> Duration of the clip to determine when to stop the audio source </param>
            /// <returns> Returns the DelayedCall </returns>
            public DelayedCall GetSelfDestruct(float clipDuration)
            {
                var call = new DelayedCall();

                call
                    .SetDelay(clipDuration)
                    .OnFinish(() =>
                    {
                        if (m_AudioSource != null && m_AudioSource.loop) //this audio source is looping -> recursive call selfDestruct
                        {
                            clipDuration = m_AudioSource.clip != null ? m_AudioSource.clip.length : 0f;
                            if (clipDuration > 0f)
                            {
                                selfDestruct = GetSelfDestruct(clipDuration);
                                call.Cancel();
                                return;
                            }
                        }
                        Stop();
                    })
                    .OnUpdate(() => onUpdate?.Invoke())
                    .Start();

                return call;
            }

            /// <summary> Stops the audio source </summary>
            public Player Stop(bool dispose = true)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.Stop();
                onStop?.Invoke();
                if (dispose)
                {
                    Dispose();
                    return this;
                }
                selfDestruct?.Cancel();
                return this;
            }

            /// <summary> Disposes the audio source and destroys the game object </summary>
            public void Dispose()
            {
                if (m_AudioSource == null) return;
                Object.DestroyImmediate(m_AudioSource.gameObject);
                m_AudioSource = null;

                if (Players.Contains(this))
                    Players.Remove(this);
            }

            /// <summary> Set the volume of the audio source </summary>
            /// <param name="volume"> The volume to set </param>
            /// <returns> The current player instance </returns>
            public Player SetVolume(float volume)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.volume = volume;
                return this;
            }

            /// <summary> Set the pitch of the audio source </summary>
            /// <param name="pitch"> The pitch to set </param>
            /// <returns> The current player instance </returns>
            public Player SetPitch(float pitch)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.pitch = pitch;
                return this;
            }

            /// <summary> Set the output audio mixer group of the audio source </summary>
            /// <param name="group"> The output audio mixer group to set </param>
            /// <returns> The current player instance </returns>
            public Player SetOutputAudioMixerGroup(AudioMixerGroup group)
            {
                outputAudioMixerGroup = group;
                if (m_AudioSource == null) return this;
                m_AudioSource.outputAudioMixerGroup = group;
                return this;
            }

            /// <summary> Set the priority of the audio source </summary>
            /// <param name="priority"> The priority to set </param>
            /// <returns> The current player instance </returns>
            public Player SetPriority(int priority)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.priority = priority;
                return this;
            }

            /// <summary> Set the pan stereo of the audio source </summary>
            /// <param name="panStereo"> The pan stereo to set </param>
            /// <returns> The current player instance </returns>
            public Player SetPanStereo(float panStereo)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.panStereo = panStereo;
                return this;
            }

            /// <summary> Set the spatial blend of the audio source </summary>
            /// <param name="spatialBlend"> The spatial blend to set </param>
            /// <returns> The current player instance </returns>
            public Player SetSpatialBlend(float spatialBlend)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.spatialBlend = spatialBlend;
                return this;
            }

            /// <summary> Set the reverb zone mix of the audio source </summary>
            /// <param name="reverbZoneMix"> The reverb zone mix to set </param>
            /// <returns> The current player instance </returns>
            public Player SetReverbZoneMix(float reverbZoneMix)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.reverbZoneMix = reverbZoneMix;
                return this;
            }

            /// <summary> Set the doppler level of the audio source </summary>
            /// <param name="dopplerLevel"> The doppler level to set </param>
            /// <returns> The current player instance </returns>
            public Player SetDopplerLevel(float dopplerLevel)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.dopplerLevel = dopplerLevel;
                return this;
            }

            /// <summary> Set the spread of the audio source </summary>
            /// <param name="spread"> The spread to set </param>
            /// <returns> The current player instance </returns>
            public Player SetSpread(float spread)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.spread = spread;
                return this;
            }

            /// <summary> Set the rolloff mode of the audio source </summary>
            /// <param name="rolloffMode"> The rolloff mode to set </param>
            /// <returns> The current player instance </returns>
            public Player SetRolloffMode(AudioRolloffMode rolloffMode)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.rolloffMode = rolloffMode;
                return this;
            }

            /// <summary> Set the min distance of the audio source </summary>
            /// <param name="minDistance"> The min distance to set </param>
            /// <returns> The current player instance </returns>
            public Player SetMinDistance(float minDistance)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.minDistance = minDistance;
                return this;
            }

            /// <summary> Set the max distance of the audio source </summary>
            /// <param name="maxDistance"> The max distance to set </param>
            /// <returns> The current player instance </returns>
            public Player SetMaxDistance(float maxDistance)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.maxDistance = maxDistance;
                return this;
            }

            /// <summary> Set the loop of the audio source </summary>
            /// <param name="loop"> The loop to set </param>
            /// <returns> The current player instance </returns>
            public Player SetLoop(bool loop)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.loop = loop;
                return this;
            }

            /// <summary> Set the ignore listener pause of the audio source </summary>
            /// <param name="ignoreListenerPause"> The ignore listener pause to set </param>
            /// <returns> The current player instance </returns>
            public Player SetIgnoreListenerPause(bool ignoreListenerPause)
            {
                if (m_AudioSource == null) return this;
                m_AudioSource.ignoreListenerPause = ignoreListenerPause;
                return this;
            }
        }

        public class PlayerElement : BasePlayerElement
        {
            private VisualElement loadingBarContainer { get; set; }
            private VisualElement loadingBarIndicator { get; set; }
            private VisualElement topScrubIndicator { get; set; }
            private VisualElement bottomScrubIndicator { get; set; }
            private VisualElement scrubber { get; set; }
            private Label scrubberLabel { get; set; }
            private Label nameLabel { get; set; }
            private Label durationLabel { get; set; }

            private Color loadingBarBackgroundColor => EditorColors.Default.TextSubtitle.WithAlpha(0.2f);
            private Color loadingBarIndicatorColor => SoundyEditorUtils.accentColor;
            private Color scrubProgressIndicatorColor => loadingBarIndicatorColor.WithAlpha(0.3f);
            private Color scrubberPlayingColor => EditorColors.Default.BoxBackground;
            private Color scrubberIdleColor => loadingBarIndicatorColor;

            public PlayerElement()
            {
                AddToClassList("player");

                Initialize();
                Compose();

                UpdateData();
            }

            protected sealed override void UpdateData()
            {
                if (player == null)
                {
                    nameLabel.SetText("---");
                    durationLabel.SetText("");
                    loadingBarIndicator.SetStyleWidth(0f);
                    return;
                }

                nameLabel.SetText(player.clipNameText);
                durationLabel.SetText(player.durationText);

                float width = player.progress * loadingBarContainer.resolvedStyle.width;
                loadingBarIndicator.SetStyleWidth(width);
            }

            private void UpdateScrubber(IPointerEvent evt)
            {
                Vector3 pointerLocalPosition = evt.localPosition;
                // use a magic number that takes into account paddings and the preview button width
                const float padding = 40;

                // if the pointer is outside the loading bar container, return
                if (pointerLocalPosition.x < padding)
                {
                    scrubberLabel.SetStyleOpacity(0f);
                    scrubber.SetStyleDisplay(DisplayStyle.None);
                    topScrubIndicator.SetStyleDisplay(DisplayStyle.None);
                    bottomScrubIndicator.SetStyleDisplay(DisplayStyle.None);
                    return;
                }

                // show the scrubber
                scrubber.SetStyleDisplay(DisplayStyle.Flex);
                topScrubIndicator.SetStyleDisplay(DisplayStyle.Flex);
                bottomScrubIndicator.SetStyleDisplay(DisplayStyle.Flex);

                // offset the pointer position by the padding
                pointerLocalPosition.x -= padding;
                // clamp the pointer position to the loading bar container width
                pointerLocalPosition.x = Mathf.Clamp(pointerLocalPosition.x, 0, loadingBarContainer.resolvedStyle.width);
                // set the scrubber position
                scrubber.style.left = pointerLocalPosition.x - scrubber.resolvedStyle.width / 2f;
                // set the scrubber indicators width
                topScrubIndicator.SetStyleDisplay(DisplayStyle.Flex).SetStyleWidth(pointerLocalPosition.x);
                bottomScrubIndicator.SetStyleDisplay(DisplayStyle.Flex).SetStyleWidth(pointerLocalPosition.x);

                // if the player is not playing, hide the scrubber label
                if (player == null || !player.isPlaying)
                {
                    scrubberLabel.SetStyleOpacity(0f);
                    // scrubberLabel.SetStyleDisplay(DisplayStyle.None);
                    scrubber.SetStyleBackgroundColor(scrubberIdleColor);
                    return;
                }

                // scrubberLabel.SetStyleDisplay(DisplayStyle.Flex);
                scrubber.SetStyleBackgroundColor(scrubberPlayingColor);

                // calculate the progress based on the scrubber position and set the scrubber label text
                float progress = pointerLocalPosition.x / loadingBarContainer.resolvedStyle.width; // calculate the progress based on the scrubber position
                progress = Mathf.Clamp01(progress);                                                // clamp the progress between 0 and 1 to avoid errors
                var timeValue = Mathf.Lerp(0, player.audioSource.clip.length, progress);           // convert the progress to a time value
                scrubberLabel.SetText
                (
                    progress.Approximately(0f)                          // if the progress is 0
                        ? "Start"                                       // set the scrubber label text to "Start"
                        : progress.Approximately(1f)                    // if the progress is 1
                            ? "End"                                     // set the scrubber label text to "End"
                            : AudioUtils.GetTimePretty(timeValue, true) // otherwise set the scrubber label text to the time value
                );

                //adjust scrubber label width to fit the text and add padding
                scrubberLabel.style.width = scrubberLabel.MeasureTextSize(scrubberLabel.text, scrubberLabel.resolvedStyle.width, MeasureMode.Undefined, scrubberLabel.resolvedStyle.height, MeasureMode.Undefined).x + DesignUtils.k_Spacing2X;

                // set the scrubber label position to be relative to scrubber position
                float scrubberLabelPosition = scrubber.resolvedStyle.left + scrubber.resolvedStyle.width / 2f;
                scrubberLabel.style.left = scrubberLabelPosition - scrubberLabel.resolvedStyle.width / 2f;

                //calculate the scrubber label position to put it above the scrubber if it's too close to the right edge of the loading bar container
                if (scrubberLabelPosition + scrubberLabel.resolvedStyle.width / 2f > loadingBarContainer.resolvedStyle.width)
                {
                    scrubberLabel.style.left = loadingBarContainer.resolvedStyle.width - scrubberLabel.resolvedStyle.width;
                }
                else if (scrubberLabelPosition - scrubberLabel.resolvedStyle.width / 2f < 0)
                {
                    scrubberLabel.style.left = 0;
                }
                else
                {
                    scrubberLabel.style.left = scrubberLabelPosition - scrubberLabel.resolvedStyle.width / 2f;
                }

                //set the y position of the scrubber label to be above the scrubber
                scrubberLabel.style.top = scrubber.resolvedStyle.top - scrubberLabel.resolvedStyle.height;

                scrubberLabel.SetStyleOpacity(1f);
            }

            private void Initialize()
            {
                loadingBarContainer =
                    new VisualElement()
                        .SetName("loading-bar-container")
                        .SetStyleBackgroundColor(loadingBarBackgroundColor)
                        .SetStyleFlexGrow(1)
                        .SetStyleHeight(2, 2, 2);

                loadingBarIndicator =
                    new VisualElement()
                        .SetName("loading-bar-indicator")
                        .SetStyleBackgroundColor(loadingBarIndicatorColor)
                        .SetStyleFlexGrow(1);

                VisualElement GetScrubIndicator() =>
                    new VisualElement()
                        .SetName("scrubber-indicator")
                        .SetStyleBackgroundColor(scrubProgressIndicatorColor)
                        .SetStyleFlexGrow(1)
                        .SetStyleHeight(2, 2, 2)
                        .SetStyleDisplay(DisplayStyle.None);

                topScrubIndicator = GetScrubIndicator().SetStyleMarginTop(-1).SetStyleMarginBottom(-1);
                bottomScrubIndicator = GetScrubIndicator().SetStyleMarginTop(-1).SetStyleMarginBottom(-1);

                scrubber =
                    new VisualElement()
                        .SetName("scrubber")
                        .SetStyleBackgroundColor(scrubberIdleColor)
                        .SetStylePosition(Position.Absolute)
                        .SetStyleTop(-3)
                        .SetStyleWidth(3)
                        .SetStyleHeight(7)
                        .SetStyleDisplay(DisplayStyle.None);

                scrubberLabel =
                    DesignUtils.NewLabel()
                        .SetName("scrubber-label")
                        .SetStyleColor(scrubberPlayingColor)
                        .SetStyleBackgroundColor(loadingBarIndicatorColor)
                        .SetStylePosition(Position.Absolute)
                        .SetStylePadding(DesignUtils.k_Spacing)
                        .SetStyleBorderRadius(DesignUtils.k_Spacing)
                        .SetStyleTextAlign(TextAnchor.MiddleCenter)
                        .SetStyleOpacity(0f)
                        .SetStyleWidth(48)
                        .SetStyleHeight(16);

                nameLabel =
                    GetLabel()
                        .SetName("name-label");

                durationLabel =
                    GetLabel()
                        .SetName("duration-label");

                previewButton
                    .SetElementSize(ElementSize.Normal);

                this
                    .SetStyleFlexGrow(1)
                    .SetStyleFlexDirection(FlexDirection.Row)
                    .SetStyleBackgroundColor(EditorColors.Default.BoxBackground)
                    .SetStyleBorderRadius(DesignUtils.k_Spacing)
                    .SetStylePadding(DesignUtils.k_Spacing)
                    .SetStylePaddingRight(DesignUtils.k_Spacing2X);

                //Setup Scrubber
                RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    scrubber.SetStyleDisplay(DisplayStyle.None);
                    scrubberLabel.SetStyleOpacity(0f);
                    topScrubIndicator.SetStyleDisplay(DisplayStyle.None);
                    bottomScrubIndicator.SetStyleDisplay(DisplayStyle.None);
                });

                RegisterCallback<PointerMoveEvent>(UpdateScrubber);

                RegisterCallback<PointerDownEvent>(e =>
                {
                    if (player == null || !player.isPlaying)
                    {
                        previewButton.OnClick();                                // trigger the player to start
                        scrubber.SetStyleBackgroundColor(scrubberPlayingColor); // set the scrubber color to idle
                        return;
                    }

                    // calculate the progress based on the scrubber position and set the audio source time
                    float progress = (scrubber.resolvedStyle.left + scrubber.resolvedStyle.width / 2f) / loadingBarContainer.resolvedStyle.width;
                    progress = Mathf.Clamp01(progress); // clamp the progress between 0 and 1 to avoid errors
                    if (progress.Approximately(1f))
                    {
                        player.Stop();                                       // stop the audio source if the progress is 1 to avoid errors
                        scrubberLabel.SetStyleOpacity(0f);                   // hide the scrubber label
                        scrubber.SetStyleBackgroundColor(scrubberIdleColor); // set the scrubber color to idle
                        return;                                              // return to avoid setting the audio source time
                    }
                    progress = Mathf.Lerp(0, player.audioSource.clip.length, progress);  // convert the progress to a time value
                    progress = Mathf.Clamp(progress, 0, player.audioSource.clip.length); // clamp the progress between 0 and the clip length to avoid errors
                    player.audioSource.time = progress;
                });
            }

            private static Label GetLabel(string text = "") =>
                DesignUtils.NewLabel()
                    .SetText(text)
                    .SetStyleFontSize(10)
                    .SetStyleColor(EditorColors.Default.TextDescription);

            private void Compose()
            {
                this
                    .AddChild(previewButton)
                    .AddSpaceBlock(2)
                    .AddChild
                    (
                        DesignUtils.column
                            .SetStyleAlignSelf(Align.Center)
                            .AddChild
                            (
                                DesignUtils.row
                                    .SetStyleFlexGrow(0)
                                    .AddChild(nameLabel)
                                    .AddSpaceBlock()
                                    .AddFlexibleSpace()
                                    .AddSpaceBlock()
                                    .AddChild(durationLabel)
                            )
                            .AddSpaceBlock()
                            .AddChild(topScrubIndicator)
                            .AddChild
                            (
                                loadingBarContainer
                                    .AddChild(loadingBarIndicator)
                                    .AddChild
                                    (
                                        scrubber
                                    )
                                    .AddChild(scrubberLabel)
                            )
                            .AddChild(bottomScrubIndicator)
                    )
                    ;
            }
        }

        public class MiniPlayerElement : BasePlayerElement
        {
            public MiniPlayerElement()
            {
                AddToClassList("mini-player");

                this
                    .AddChild(previewButton);
            }

            protected override void UpdateData()
            {
                // Do nothing    
            }
        }

        public abstract class BasePlayerElement : VisualElement
        {
            public static IEnumerable<Texture2D> playIcon => EditorSpriteSheets.EditorUI.Icons.Play;
            public static IEnumerable<Texture2D> stopIcon => EditorSpriteSheets.EditorUI.Icons.Stop;

            public Func<AudioClip> audioClipGetter { get; set; }
            public Func<float> volumeGetter { get; set; }
            public Func<float> pitchGetter { get; set; }
            public Func<AudioMixerGroup> outputAudioMixerGroupGetter { get; set; }
            public Func<int> priorityGetter { get; set; }
            public Func<float> panStereoGetter { get; set; }
            public Func<float> spatialBlendGetter { get; set; }
            public Func<float> reverbZoneMixGetter { get; set; }
            public Func<float> dopplerLevelGetter { get; set; }
            public Func<float> spreadGetter { get; set; }
            public Func<float> minDistanceGetter { get; set; }
            public Func<float> maxDistanceGetter { get; set; }
            public Func<bool> loopGetter { get; set; }
            public Func<bool> ignoreListenerPauseGetter { get; set; }

            public Player player { get; protected set; }

            public FluidButton previewButton { get; protected set; }

            protected BasePlayerElement()
            {
                InitializePreviewButton();
            }

            private void InitializePreviewButton()
            {
                previewButton =
                    PlayerButton()
                        .SetIcon(playIcon);

                previewButton
                    .SetOnClick(() =>
                    {
                        // if the player is playing, stop it
                        if (player != null && player.isPlaying)
                        {
                            player.Stop();

                            player.onPlay -= UpdateData;
                            player.onStop -= UpdateData;
                            player.onUpdate -= UpdateData;

                            previewButton.SetIcon(playIcon);
                            previewButton.iconReaction.Play();
                            return;
                        }

                        // otherwise, play it
                        player?.Stop();
                        if (player != null)
                        {
                            player.onPlay -= UpdateData;
                            player.onStop -= UpdateData;
                            player.onUpdate -= UpdateData;
                        }

                        var audioClip = audioClipGetter?.Invoke();
                        if (audioClip == null)
                            return;

                        player =
                            GetPlayer(audioClip)
                                .InitializeAudioSource()
                                .SetOutputAudioMixerGroup(outputAudioMixerGroupGetter?.Invoke())
                                .SetVolume(volumeGetter?.Invoke() ?? SoundySettings.k_DefaultVolume)
                                .SetPitch(pitchGetter?.Invoke() ?? SoundySettings.k_DefaultPitch)
                                .SetPriority(priorityGetter?.Invoke() ?? SoundySettings.k_DefaultPriority)
                                .SetPanStereo(panStereoGetter?.Invoke() ?? SoundySettings.k_DefaultPanStereo)
                                .SetSpatialBlend(spatialBlendGetter?.Invoke() ?? SoundySettings.k_DefaultSpatialBlend)
                                .SetReverbZoneMix(reverbZoneMixGetter?.Invoke() ?? SoundySettings.k_DefaultReverbZoneMix)
                                .SetDopplerLevel(dopplerLevelGetter?.Invoke() ?? SoundySettings.k_DefaultDopplerLevel)
                                .SetSpread(spreadGetter?.Invoke() ?? SoundySettings.k_DefaultSpread)
                                .SetMinDistance(minDistanceGetter?.Invoke() ?? SoundySettings.k_DefaultMinDistance)
                                .SetMaxDistance(maxDistanceGetter?.Invoke() ?? SoundySettings.k_DefaultMaxDistance)
                                .SetLoop(loopGetter?.Invoke() ?? SoundySettings.k_DefaultLoop)
                                .SetIgnoreListenerPause(ignoreListenerPauseGetter?.Invoke() ?? SoundySettings.k_DefaultIgnoreListenerPause)
                                .Play();

                        player.onPlay += UpdateData;
                        player.onStop += UpdateData;
                        player.onUpdate += UpdateData;

                        // when the player starts, set the icon to stop icon to show the user that the player is playing (and can be stopped)
                        previewButton.SetIcon(stopIcon);
                        previewButton.iconReaction.Play();

                        // when the player stops, set the icon back to the play icon
                        player.onStop += () =>
                        {
                            previewButton.SetIcon(EditorSpriteSheets.EditorUI.Icons.Play);
                            previewButton.iconReaction.Play();
                        };
                    });

                // when the player visual element is detached from the panel, make sure we stop the player 
                this.RegisterCallback<DetachFromPanelEvent>
                (
                    _ =>
                    {
                        player?.Stop();
                        if (player != null)
                        {
                            player.onPlay -= UpdateData;
                            player.onStop -= UpdateData;
                            player.onUpdate -= UpdateData;
                        }
                        player = null;
                    }
                );
            }

            protected abstract void UpdateData();

            public static FluidButton PlayerButton() =>
                FluidButton.Get()
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Tiny)
                    .SetStyleFlexShrink(0)
                    .SetAccentColor(SoundyEditorUtils.selectableAccentColor);
        }

        #region BasePlayerElement Extensions
        
        public static T SetSoundObjectGetter<T>(this T playerElement, Func<SoundObject> getSoundObject, Func<AudioMixerGroup> getAudioMixerGroup = null) where T : BasePlayerElement
        {
            SoundObject soundObject = getSoundObject?.Invoke();
            if (soundObject == null || !soundObject.canPlay)
                return playerElement;

            playerElement
                .SetAudioClipGetter(() => soundObject.LoadNext())
                .SetVolumeGetter(() => soundObject.GetVolume())
                .SetPitchGetter(() => soundObject.GetPitch())
                .SetPriorityGetter(() => soundObject.priority)
                .SetPanStereoGetter(() => soundObject.panStereo)
                .SetSpatialBlendGetter(() => soundObject.spatialBlend)
                .SetReverbZoneMixGetter(() => soundObject.reverbZoneMix)
                .SetDopplerLevelGetter(() => soundObject.dopplerLevel)
                .SetSpreadGetter(() => soundObject.spread)
                .SetMinDistanceGetter(() => soundObject.minDistance)
                .SetMaxDistanceGetter(() => soundObject.maxDistance)
                .SetLoopGetter(() => soundObject.loop)
                .SetIgnoreListenerPauseGetter(() => soundObject.ignoreListenerPause)
                .SetOutputAudioMixerGroupGetter(() => getAudioMixerGroup?.Invoke())
                ;

            return playerElement;
        }

        public static T SetMusicObjectGetter<T>(this T playerElement, Func<MusicObject> getMusicObject, Func<AudioMixerGroup> getAudioMixerGroup = null) where T : BasePlayerElement
        {
            MusicObject musicObject = getMusicObject?.Invoke();
            if (musicObject == null || !musicObject.canPlay)
                return playerElement;

            playerElement
                .SetAudioClipGetter(() => musicObject.data?.Clip ? musicObject.data?.Clip : null)
                .SetVolumeGetter(() => musicObject.GetVolume())
                .SetPitchGetter(() => musicObject.GetPitch())
                .SetPriorityGetter(() => musicObject.priority)
                .SetPanStereoGetter(() => musicObject.panStereo)
                .SetSpatialBlendGetter(() => musicObject.spatialBlend)
                .SetReverbZoneMixGetter(() => musicObject.reverbZoneMix)
                .SetDopplerLevelGetter(() => musicObject.dopplerLevel)
                .SetSpreadGetter(() => musicObject.spread)
                .SetMinDistanceGetter(() => musicObject.minDistance)
                .SetMaxDistanceGetter(() => musicObject.maxDistance)
                .SetLoopGetter(() => musicObject.loop)
                .SetIgnoreListenerPauseGetter(() => musicObject.ignoreListenerPause)
                .SetOutputAudioMixerGroupGetter(() => getAudioMixerGroup?.Invoke())
                ;

            return playerElement;
        }

        public static T SetAudioClipGetter<T>(this T playerElement, Func<AudioClip> audioClipGetter) where T : BasePlayerElement
        {
            playerElement.audioClipGetter = audioClipGetter;
            return playerElement;
        }

        public static T SetVolumeGetter<T>(this T playerElement, Func<float> volumeGetter) where T : BasePlayerElement
        {
            playerElement.volumeGetter = volumeGetter;
            return playerElement;
        }

        public static T SetPitchGetter<T>(this T playerElement, Func<float> pitchGetter) where T : BasePlayerElement
        {
            playerElement.pitchGetter = pitchGetter;
            return playerElement;
        }

        public static T SetOutputAudioMixerGroupGetter<T>(this T playerElement, Func<AudioMixerGroup> outputAudioMixerGroupGetter) where T : BasePlayerElement
        {
            playerElement.outputAudioMixerGroupGetter = outputAudioMixerGroupGetter;
            return playerElement;
        }

        public static T SetPriorityGetter<T>(this T playerElement, Func<int> priorityGetter) where T : BasePlayerElement
        {
            playerElement.priorityGetter = priorityGetter;
            return playerElement;
        }

        public static T SetPanStereoGetter<T>(this T playerElement, Func<float> panStereoGetter) where T : BasePlayerElement
        {
            playerElement.panStereoGetter = panStereoGetter;
            return playerElement;
        }

        public static T SetSpatialBlendGetter<T>(this T playerElement, Func<float> spatialBlendGetter) where T : BasePlayerElement
        {
            playerElement.spatialBlendGetter = spatialBlendGetter;
            return playerElement;
        }

        public static T SetReverbZoneMixGetter<T>(this T playerElement, Func<float> reverbZoneMixGetter) where T : BasePlayerElement
        {
            playerElement.reverbZoneMixGetter = reverbZoneMixGetter;
            return playerElement;
        }

        public static T SetDopplerLevelGetter<T>(this T playerElement, Func<float> dopplerLevelGetter) where T : BasePlayerElement
        {
            playerElement.dopplerLevelGetter = dopplerLevelGetter;
            return playerElement;
        }

        public static T SetSpreadGetter<T>(this T playerElement, Func<float> spreadGetter) where T : BasePlayerElement
        {
            playerElement.spreadGetter = spreadGetter;
            return playerElement;
        }

        public static T SetMinDistanceGetter<T>(this T playerElement, Func<float> minDistanceGetter) where T : BasePlayerElement
        {
            playerElement.minDistanceGetter = minDistanceGetter;
            return playerElement;
        }

        public static T SetMaxDistanceGetter<T>(this T playerElement, Func<float> maxDistanceGetter) where T : BasePlayerElement
        {
            playerElement.maxDistanceGetter = maxDistanceGetter;
            return playerElement;
        }

        public static T SetLoopGetter<T>(this T playerElement, Func<bool> loopGetter) where T : BasePlayerElement
        {
            playerElement.loopGetter = loopGetter;
            return playerElement;
        }

        public static T SetIgnoreListenerPauseGetter<T>(this T playerElement, Func<bool> ignoreListenerPauseGetter) where T : BasePlayerElement
        {
            playerElement.ignoreListenerPauseGetter = ignoreListenerPauseGetter;
            return playerElement;
        }

        #endregion
    }
}
