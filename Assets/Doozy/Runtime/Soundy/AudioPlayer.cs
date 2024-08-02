// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.Ids;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable Unity.RedundantEventFunction

namespace Doozy.Runtime.Soundy
{
    /// <summary> Base class for audio players </summary>
    public abstract class AudioPlayer : MonoBehaviour
    {
        /// <summary> Soundy global settings </summary>
        public static SoundySettings settings => SoundySettings.instance;

        /// <summary> Audio player pool that this player belongs to </summary>
        internal SoundyService.AudioPlayerPool pool { get; set; }

        #region RecycleAfterUse

        protected bool RecycleAfterUse = true;
        /// <summary> After the audio clip has finished playing, recycle the audio player and put it back in the pool </summary>
        public bool recycleAfterUse
        {
            get => RecycleAfterUse;
            set => RecycleAfterUse = value;
        }

        #endregion // RecycleAfterUse

        #region PlayState

        /// <summary> Current play state of this audio player </summary>
        public PlayState playState { get; protected internal set; } = PlayState.Created;

        /// <summary> Check if this audio player is currently in use (out of the pool) </summary>
        public bool inUse => playState != PlayState.InPool;

        /// <summary> Check if this audio player is currently in the pool (not in use) </summary>
        public bool inPool => playState == PlayState.InPool;

        /// <summary> Check if this audio player is currently idle (it is not playing or paused and it has been idle for more than the idle time) </summary>
        public bool isIdle => playState == PlayState.Idle;

        /// <summary> Check if this audio player is currently playing an audio clip </summary>
        public bool isPlaying => playState == PlayState.Playing;

        /// <summary> Check if this audio player is currently stopped (it is not playing and the AudioSource is not paused) </summary>
        public bool isStopped => playState == PlayState.Stopped;

        /// <summary> Check if this audio player is currently paused (it is playing but the AudioSource is paused) </summary>
        public bool isPaused => playState == PlayState.Paused;

        /// <summary> Last time this audio player played an audio clip (in seconds). This is used to detect when the player is idle </summary>
        public virtual float lastPlayedTime { get; protected set; }

        /// <summary> Duration (in seconds) since this audio player last played an audio clip. This is used to mark the player as idle </summary>
        public virtual float idleDuration => Time.realtimeSinceStartup - lastPlayedTime;

        /// <summary>
        /// Check if this audio player can be recycled.
        /// This means that the player is idle and it has been idle for more than the idle time.
        /// Also, the player must be set to recycle after use (RecycleAfterUse = true)
        /// </summary>
        public bool canBeRecycled => recycleAfterUse && isIdle && idleDuration >= settings.IdleTime;

        #endregion // PlayState

        #region Callbacks: Play, Stop, Finish, Update, Pause, Resume, Recycle, Dispose

        /// <summary> Callback executed when the audio player starts playing </summary>
        public UnityAction onPlay { get; set; }

        /// <summary> Callback executed when the audio player stops playing </summary>
        public UnityAction onStop { get; set; }

        /// <summary> Callback executed when the audio player finishes playing </summary>
        public UnityAction onFinish { get; set; }

        /// <summary> Callback executed every frame while the audio player is playing </summary>
        public UnityAction onUpdate { get; set; }

        /// <summary> Callback executed when the audio player pauses </summary>
        public UnityAction onPause { get; set; }

        /// <summary> Callback executed when the audio player resumes </summary>
        public UnityAction onResume { get; set; }

        /// <summary> Callback executed when the audio player is recycled </summary>
        public UnityAction onRecycle { get; set; }

        /// <summary> Callback executed when the audio player is disposed </summary>
        public UnityAction onDispose { get; set; }

        #endregion // Callbacks: Play, Stop, Finish, Update, Pause, Resume, Recycle, Dispose

        #region SoundObject

        /// <summary> The SoundObject that is currently loaded in this audio player </summary>
        private SoundObject m_SoundObject;
        /// <summary>
        /// The SoundObject that is currently loaded in this audio player.
        /// When a SoundObject is loaded, the loaded MusicObject is set to null.
        /// </summary>
        public SoundObject loadedSoundObject
        {
            get => m_SoundObject;
            set
            {
                m_SoundObject = value;
                m_MusicObject = null;
            }
        }

        /// <summary> Check if this audio player is currently playing a sound object </summary>
        public bool isPlayingSound => loadedSoundObject != null;

        #endregion // SoundObject

        #region MusicObject

        /// <summary> The MusicObject that is currently loaded in this audio player </summary>
        private MusicObject m_MusicObject;
        /// <summary>
        /// The MusicObject that is currently loaded in this audio player.
        /// When a MusicObject is loaded, the loaded SoundObject is set to null.
        /// </summary>
        public MusicObject loadedMusicObject
        {
            get => m_MusicObject;
            set
            {
                m_MusicObject = value;
                m_SoundObject = null;
            }
        }

        /// <summary> Check if this audio player is currently playing a music object </summary>
        public bool isPlayingMusic => loadedMusicObject != null;

        #endregion // MusicObject

        #region FollowTarget

        /// <summary> Target transform to follow (usually a Transform that is attached to a GameObject that is moving) while playing </summary>
        protected Transform FollowTransform;
        /// <summary> Target transform to follow (usually a Transform that is attached to a GameObject that is moving) while playing </summary>
        public virtual Transform followTarget
        {
            get => FollowTransform;
            set
            {
                isFollowingTarget = value != null;
                FollowTransform = value;
            }
        }

        /// <summary> Check if this audio player is currently following a target </summary>
        public bool isFollowingTarget { get; protected set; }

        #endregion // FollowTarget

        #region AudioClip

        /// <summary> Audio clip that is currently loaded in this audio player </summary>
        protected AudioClip Clip;
        /// <summary> Audio clip that is currently loaded in this audio player </summary>
        public virtual AudioClip clip
        {
            get => Clip;
            set => Clip = value;
        }

        /// <summary> Check if this audio player has an audio clip loaded </summary>
        public bool hasClip => clip != null;

        /// <summary> Duration (in seconds) of the audio clip that is currently playing </summary>
        public float clipLength => hasClip ? clip.length : 0;

        #endregion // AudioClip

        #region AudioMixerGroup

        /// <summary> Audio mixer group to route the signal to. </summary>
        protected AudioMixerGroup OutputAudioMixerGroup;
        /// <summary> Audio mixer to route the signal to </summary>
        public virtual AudioMixerGroup outputAudioMixerGroup
        {
            get => OutputAudioMixerGroup;
            set => OutputAudioMixerGroup = value;
        }

        #endregion // AudioMixerGroup

        #region Volume

        /// <summary> Volume of the audio player </summary>
        protected float Volume;
        /// <summary> Volume of the audio player </summary>
        public virtual float volume
        {
            get => Volume;
            set => Volume = Mathf.Clamp(value, SoundySettings.k_MinVolume, SoundySettings.k_MaxVolume);
        }

        /// <summary> Volume value used as a reference when fading in or out the audio player's volume </summary>
        protected float referenceVolume { get; set; }

        #endregion // Volume

        #region Mute

        /// <summary> Check if this audio player is currently muted </summary>
        public virtual bool mute { get; set; }

        /// <summary> Check if this audio player is currently muted </summary>
        public bool isMuted => mute;

        #endregion // Mute

        #region CrossFade

        /// <summary> Enable or disable cross fading when starting or stopping a sound </summary>
        protected bool CrossFade;
        /// <summary> Should the audio player fade in and out when starting or stopping a sound </summary>
        public virtual bool crossFade
        {
            get => CrossFade;
            set => CrossFade = value;
        }

        /// <summary> If cross fade is enabled, this is the duration (in seconds) of the fade in and out of the audio player's volume when starting or stopping a sound </summary>
        protected float CrossFadeDuration;
        /// <summary> Fade in and out duration (in seconds) of the audio player's volume when starting or stopping a sound </summary>
        public virtual float crossFadeDuration
        {
            get => CrossFadeDuration;
            set
            {
                CrossFadeDuration = Mathf.Max(0, value);
                if (!CrossFade) return;
                if (CrossFadeDuration <= 0)
                    CrossFade = false;
            }
        }

        /// <summary>
        /// In case the cross fade duration is longer than the clip length, we want to make sure the cross fade duration is not longer than half the clip length.
        /// This is the real fade duration used to fade in and out the audio player's volume.
        /// </summary>
        protected float adjustedCrossFadeDuration => Mathf.Min(crossFadeDuration, clipLength / 2f);

        /// <summary>
        /// Check if the audio player has cross fade enabled and a cross fade duration greater than 0.
        /// </summary>
        public bool hasCrossFade => CrossFade && CrossFadeDuration > 0;

        /// <summary>
        /// Check if this audio player is currently fading in.
        /// This means that the audio player started playing an audio clip and is fading the volume in.
        /// </summary>
        public bool isFadingIn { get; protected set; }

        /// <summary>
        /// Check if this audio player is currently fading out.
        /// This means that the audio player is fading the volume out and will stop playing the audio clip when the volume reaches 0.
        /// </summary>
        public bool isFadingOut { get; protected set; }

        /// <summary> Check if this audio player is currently fading in or out </summary>
        public bool isFading => isFadingIn || isFadingOut;

        /// <summary>> Callback executed when the audio player starts fading in </summary>
        public UnityAction onFadeInStart { get; set; }

        /// <summary> > Callback executed when the audio player finishes fading in </summary>
        public UnityAction onFadeInFinish { get; set; }

        /// <summary> Callback executed when the audio player starts fading out </summary>
        public UnityAction onFadeOutStart { get; set; }

        /// <summary> Callback executed when the audio player finishes fading out </summary>
        public UnityAction onFadeOutFinish { get; set; }

        #endregion // CrossFade

        #region Pitch

        /// <summary> Pitch of the audio player. </summary>
        protected float Pitch;
        /// <summary> Pitch of the audio player </summary>
        public virtual float pitch
        {
            get => Pitch;
            set => Pitch = Mathf.Clamp(value, SoundySettings.k_MinPitch, SoundySettings.k_MaxPitch);
        }

        #endregion // Pitch

        #region Priority

        /// <summary> Audio player's priority </summary>
        protected int Priority;
        /// <summary>
        /// Set the priority of the audio.
        /// Unity virtualizes AudioSources when there are more AudioSources playing than available hardware channels.
        /// The AudioSources with lowest priority (and audibility) are virtualized first.
        /// Priority is an integer between 0 and 255. Where, 0 = high priority, 255 = low priority.
        /// </summary>
        public virtual int priority
        {
            get => Priority;
            set => Priority = Mathf.Clamp(value, SoundySettings.k_MinPriority, SoundySettings.k_MaxPriority);
        }

        #endregion // Priority

        #region Pan Stereo

        /// <summary>
        /// Set the pan level (left / right bias) of the audio.
        /// This only applies to sounds that are Mono or Stereo.
        /// </summary>
        protected float PanStereo;
        /// <summary>
        /// Set the pan level (left / right bias) of the audio.
        /// This only applies to sounds that are Mono or Stereo.
        /// </summary>
        public virtual float panStereo
        {
            get => PanStereo;
            set => PanStereo = Mathf.Clamp(value, SoundySettings.k_MinPanStereo, SoundySettings.k_MaxPanStereo);
        }

        #endregion // Pan Stereo

        #region Spatial Blend

        /// <summary> Set how much this audio player is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3. </summary>        
        protected float SpatialBlend;
        /// <summary> Set how much this audio player is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3. </summary>
        public virtual float spatialBlend
        {
            get => SpatialBlend;
            set => SpatialBlend = Mathf.Clamp(value, SoundySettings.k_MinSpatialBlend, SoundySettings.k_MaxSpatialBlend);
        }

        #endregion // Spatial Blend

        #region Reverb Zone Mix

        /// <summary> The amount by which the signal from this sound will be mixed into the global reverb associated with the Reverb Zones. </summary>
        protected float ReverbZoneMix;
        /// <summary> The amount by which the signal from this sound will be mixed into the global reverb associated with the Reverb Zones. </summary>
        public virtual float reverbZoneMix
        {
            get => ReverbZoneMix;
            set => ReverbZoneMix = Mathf.Clamp(value, SoundySettings.k_MinReverbZoneMix, SoundySettings.k_MaxReverbZoneMix);
        }

        #endregion // Reverb Zone Mix

        #region Doppler Level

        /// <summary> Doppler scale for this audio player </summary>
        protected float DopplerLevel;
        /// <summary> Doppler scale for this audio player </summary>
        public virtual float dopplerLevel
        {
            get => DopplerLevel;
            set => DopplerLevel = Mathf.Clamp(value, SoundySettings.k_MinDopplerLevel, SoundySettings.k_MaxDopplerLevel);
        }

        #endregion // Doppler Level

        #region Spread

        /// <summary> Set the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space </summary>
        protected float Spread;
        /// <summary> Set the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space </summary>
        public virtual float spread
        {
            get => Spread;
            set => Spread = Mathf.Clamp(value, SoundySettings.k_MinSpread, SoundySettings.k_MaxSpread);
        }

        #endregion // Spread

        #region Min Distance & Max Distance (Logarithmic rolloff)

        /// <summary> Set the minimum distance within the audio source will cease to grow louder in volume </summary>
        protected float MinDistance;
        /// <summary> Set the minimum distance within the audio source will cease to grow louder in volume </summary>
        public virtual float minDistance
        {
            get => MinDistance;
            set => MinDistance = Mathf.Clamp(value, SoundySettings.k_MinMinDistance, SoundySettings.k_MaxMinDistance);
        }

        /// <summary> (Logarithmic rolloff) Set the distance a sounds stops attenuating at </summary>
        protected float MaxDistance;
        /// <summary> (Logarithmic rolloff) Set the distance a sounds stops attenuating at </summary>
        public virtual float maxDistance
        {
            get => MaxDistance;
            set => MaxDistance = Mathf.Clamp(value, SoundySettings.k_MinMaxDistance, SoundySettings.k_MaxMaxDistance);
        }

        #endregion // Min Distance & Max Distance (Logarithmic rolloff)

        #region Loop

        /// <summary> Check if the audio player is looping the currently playing audio clip </summary>
        protected bool Loop;
        /// <summary> Check if the audio player is looping the currently playing audio clip </summary>
        public bool loop
        {
            get => Loop;
            set => Loop = value;
        }

        /// <summary> Check if this audio player is currently looping the audio clip that is playing </summary>
        public bool isLooping => loop;

        #endregion

        #region Ignore Listener Pause

        /// <summary>
        /// Allow this audio player to play even though AudioListener.pause is set to true.
        /// This is useful for the menu element sounds or background music in pause menus.
        /// </summary>
        protected bool IgnoreListenerPause;
        /// <summary>
        /// Allow this audio player to play even though AudioListener.pause is set to true.
        /// This is useful for the menu element sounds or background music in pause menus.
        /// </summary>
        public virtual bool ignoreListenerPause
        {
            get => IgnoreListenerPause;
            set => IgnoreListenerPause = value;
        }

        #endregion // Ignore Listener Pause

        /// <summary>
        /// Playback position in seconds. Read current playback position, or seek to a new playback time.
        /// Be aware that this is not sample-exact seeking, but rather a best effort approximation depending on the audio format.
        /// On a compressed audio file, the seek point will likely not be exactly playable, and instead the actual seek point will be some time before or after the requested seek time.
        /// </summary>
        public abstract float time { get; set; }

        /// <summary>
        /// Playback position in PCM samples. This is a convenience wrapper for timeSamples / frequency.
        /// Use this to read current playback position, or to seek to a new playback position in samples (fractions rounded to nearest int).
        /// It is more precise than time when the clip is compressed.
        /// </summary>
        public virtual int timeSamples { get; set; }

        /// <summary> Play progress (from 0 to 1) of the audio clip that is currently playing </summary>
        public virtual float playProgress { get; protected set; }

        // /// <summary> Elapsed play time (in seconds) of the audio clip that is currently playing </summary>
        // public float elapsedPlayTime { get; protected set; }

        /// <summary> Internal variable used to detect when Unity pauses this player's AudioSource (this happens on app focus lost, for example) </summary>
        protected virtual bool autoPaused => inUse && !isPaused && isPlaying && playProgress > 0 && playProgress < 1;

        /// <summary> Coroutine that ticks every frame and updates the audio player's state </summary>
        private Coroutine ticker { get; set; }

        protected virtual void Awake()
        {
            // do nothing
        }

        protected virtual void OnEnable()
        {
            // do nothing
        }

        protected virtual void OnDisable()
        {
            StopFadeIn();
            StopFadeOut();
            StopTicker();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary> Return the audio player to the pool it came from </summary>
        public virtual void Recycle()
        {
            StopFadeIn();
            StopFadeOut();
            StopTicker();
            onRecycle?.Invoke();
            pool?.Recycle(this);
        }

        /// <summary>
        /// Dispose of this audio player and do not return it to the pool it came from.
        /// This is useful when you want to destroy the audio player and not return it to the pool, thus releasing its resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (ticker != null) StopCoroutine(ticker);
            ticker = null;
            onDispose?.Invoke();
            pool?.Dispose(this);
        }

        protected void StartTicker()
        {
            if (ticker != null) return;
            ticker = StartCoroutine(TickEveryFrame());
        }

        protected void StopTicker()
        {
            if (ticker == null) return;
            StopCoroutine(ticker);
            ticker = null;
        }

        /// <summary> Set the position, in world space, of this audio player </summary>
        /// <param name="position"> The position to set </param>
        public void SetPosition(Vector3 position) =>
            transform.position = position;

        private void FollowTarget()
        {
            if (followTarget == null) return;
            SetPosition(followTarget.position);
        }

        /// <summary> Load the next audio clip from the given soundId </summary>
        /// <param name="soundId"> The soundId to load the audio clip from </param>
        /// <returns> True if the audio clip was loaded, false otherwise </returns>
        public bool LoadSound(SoundId soundId) =>
            soundId != null && soundId.isValid && LoadSound(soundId.GetSoundObject(), soundId.GetOutputAudioMixerGroup());

        /// <summary>
        /// Load the next audio clip from the given sound library and sound name.
        /// If the sound library or sound name is null, or if the sound library has no sound with the given name, this audio player will be stopped.
        /// </summary>
        /// <param name="libraryName"> The name of the sound library to load the sound from </param>
        /// <param name="soundName"> The name of the sound to load </param>
        /// <param name="audioMixerGroup"> The output audio mixer group to use for the audio clip </param>
        /// <returns> True if the audio clip was loaded, false otherwise </returns>
        public bool LoadSound(string libraryName, string soundName, AudioMixerGroup audioMixerGroup = null) =>
            LoadSound(SoundyService.GetSoundObject(libraryName, soundName), audioMixerGroup);

        /// <summary>
        /// Load the next audio clip from the given sound object.
        /// If the sound object is null, or if it has no more clips to play, this audio player will be stopped.
        /// </summary>
        /// <param name="soundObject"> The sound object to load the next audio clip from </param>
        /// <param name="audioMixerGroup"> The output audio mixer group to use for the audio clip </param>
        /// <returns> True if the audio clip was loaded, false otherwise </returns>
        public virtual bool LoadSound(SoundObject soundObject, AudioMixerGroup audioMixerGroup = null)
        {
            ResetAudioPlayer();
            if (soundObject == null || !soundObject.canPlay)
                return false;

            loadedSoundObject = soundObject;

            clip = soundObject.LoadNext();
            outputAudioMixerGroup = audioMixerGroup;
            volume = soundObject.GetVolume();
            referenceVolume = volume;
            pitch = soundObject.GetPitch();
            priority = soundObject.priority;
            panStereo = soundObject.panStereo;
            spatialBlend = soundObject.spatialBlend;
            reverbZoneMix = soundObject.reverbZoneMix;
            dopplerLevel = soundObject.dopplerLevel;
            spread = soundObject.spread;
            minDistance = soundObject.minDistance;
            maxDistance = soundObject.maxDistance;
            loop = soundObject.loop;
            ignoreListenerPause = soundObject.ignoreListenerPause;
            return true;
        }

        /// <summary> Load the next audio clip from the given musicId </summary>
        /// <param name="musicId"> The musicId to load the audio clip from </param>
        /// <returns> True if the audio clip was loaded, false otherwise </returns>
        public bool LoadMusic(MusicId musicId) =>
            musicId != null && musicId.isValid && LoadMusic(musicId.GetMusicObject(), musicId.GetOutputAudioMixerGroup());

        /// <summary>
        /// Load the audio clip from the given music library and music name.
        /// If the music library or music name is null, this audio player will be stopped.
        /// </summary>
        /// <param name="libraryName"> The name of the music library to load the music from </param>
        /// <param name="musicName"> The name of the music to load </param>
        /// <param name="audioMixerGroup"> The output audio mixer group to use for the audio clip </param>
        /// <returns> True if the audio clip was loaded, false otherwise </returns>
        public bool LoadMusic(string libraryName, string musicName, AudioMixerGroup audioMixerGroup = null) =>
            LoadMusic(SoundyService.GetMusicObject(libraryName, musicName), audioMixerGroup);

        /// <summary>
        /// Load the audio clip from the given music object.
        /// If the music object is null, this audio player will be stopped.
        /// </summary>
        /// <param name="musicObject"> The music object to load the audio clip from </param>
        /// <param name="audioMixerGroup"> The output audio mixer group to use for the audio clip </param>
        /// <returns> True if the audio clip was loaded, false otherwise </returns>
        public virtual bool LoadMusic(MusicObject musicObject, AudioMixerGroup audioMixerGroup = null)
        {
            ResetAudioPlayer();
            if (musicObject == null || !musicObject.canPlay)
                return false;

            loadedMusicObject = musicObject;

            clip = musicObject.data.Clip;
            outputAudioMixerGroup = audioMixerGroup;
            volume = musicObject.GetVolume();
            referenceVolume = volume;
            pitch = musicObject.GetPitch();
            priority = musicObject.priority;
            panStereo = musicObject.panStereo;
            spatialBlend = musicObject.spatialBlend;
            reverbZoneMix = musicObject.reverbZoneMix;
            dopplerLevel = musicObject.dopplerLevel;
            spread = musicObject.spread;
            minDistance = musicObject.minDistance;
            maxDistance = musicObject.maxDistance;
            loop = musicObject.loop;
            ignoreListenerPause = musicObject.ignoreListenerPause;
            return true;
        }

        /// <summary> Play the currently set audio clip </summary>
        public virtual void Play()
        {
            if (isPlaying) Stop();
            SetPlayState(PlayState.Playing);
            StartTicker();

            autoFadeInStarted = false;
            autoFadeOutStarted = false;

            if (hasCrossFade)
            {
                autoFadeInStarted = true;
                StartFadeIn();
            }
        }

        /// <summary> Stop the currently playing audio </summary>
        public virtual void Stop()
        {
            StopFadeIn();
            StopFadeOut();
            UnMute();
            UnPause();
            UpdateLastPlayedTime();

            if (isStopped) return;
            SetPlayState(PlayState.Stopped);
        }

        /// <summary>
        /// Performs the necessary actions to either recycle it (by returning it to the pool) or to stop it.
        /// </summary>
        public virtual void Finish()
        {
            switch (playState)
            {
                // --- Created --- is an impossible case as this state is used only for initialization
                case PlayState.Created:

                // --- InPool --- is an impossible case as this state is used when the audio player is in the pool                    
                case PlayState.InPool:
                    Recycle();
                    return;

                // --- Idle --- means that the audio player is not playing anything and is ready to be used
                case PlayState.Idle:
                    //do nothing
                    break;

                // --- Stopped --- means that the audio player has finished playing something and is ready to be used
                case PlayState.Stopped:
                    if (!recycleAfterUse)
                    {
                        SetPlayState(PlayState.Idle);
                        return;
                    }
                    break;

                // --- Playing --- means that the audio player is currently playing something
                case PlayState.Playing:

                // --- Paused --- means that the audio player is currently paused
                case PlayState.Paused:
                    Stop();
                    OnFinished();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (recycleAfterUse)
                Recycle();
        }

        /// <summary> Pause the currently playing audio </summary>
        public virtual void Pause()
        {
            if (!isPlaying) return;
            SetPlayState(PlayState.Paused);
        }

        /// <summary> Resume the currently paused audio </summary>
        public virtual void UnPause()
        {
            if (!isPaused) return;
            SetPlayState(PlayState.Playing);
        }

        /// <summary> Mute the audio player </summary>
        public virtual void Mute() =>
            mute = true;

        /// <summary> Unmute the audio player </summary>
        public virtual void UnMute() =>
            mute = false;

        protected internal void SetPlayState(PlayState newPlayState)
        {
            bool changed = playState != newPlayState;
            if (!changed) return;
            // ReSharper disable once UnusedVariable
            PlayState previousPlayState = playState;

            switch (newPlayState)
            {
                // --- Created --- is an impossible case as this state is used only for initialization
                case PlayState.Created:
                    playState = PlayState.Created;
                    break;

                // --- Pooled ---
                case PlayState.InPool:
                    playState = PlayState.InPool;
                    break;

                // --- Idle ---
                case PlayState.Idle:
                    playState = PlayState.Idle;
                    break;

                // --- Playing ---
                case PlayState.Playing:
                    if (playState == PlayState.Paused)
                    {
                        OnResumed();
                        playState = PlayState.Playing;
                        break;
                    }
                    OnPlaying();
                    playState = PlayState.Playing;
                    break;

                // --- Paused ---
                case PlayState.Paused:
                    if (playState == PlayState.Playing)
                    {
                        OnPaused();
                        playState = PlayState.Paused;
                    }
                    break;

                // --- Stopped ---
                case PlayState.Stopped:
                    OnStopped();
                    playState = PlayState.Stopped;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newPlayState), newPlayState, null);
            }

            UpdateGameObjectName();

            // Debug.Log($"[{frameCountInRed}] {name} - SetPlayState - from {previousPlayState} to {newPlayState}");
        }

        /// <summary>
        /// Change the game object name to reflect the current state of the audio player.
        /// </summary>
        private void UpdateGameObjectName()
        {
            #if UNITY_EDITOR
            string result = $"AudioPlayer ({playState})";
            switch (playState)
            {
                case PlayState.InPool:
                case PlayState.Idle:
                    break;
                case PlayState.Playing:
                case PlayState.Paused:
                case PlayState.Stopped:
                    result += $" - {(isPlayingSound ? "Sound - " + loadedSoundObject.audioName : isPlayingMusic ? "Music - " + loadedMusicObject.audioName : clip != null ? clip.name : "No Clip")}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            gameObject.name = result;
            #endif
        }

        /// <summary> Called when the audio player became idle. </summary>
        protected virtual void OnIdle()
        {

        }

        /// <summary> Called when the audio player started playing. </summary>
        protected virtual void OnPlaying()
        {
            onPlay?.Invoke();
        }

        /// <summary> Called when the audio player was paused. </summary>
        protected virtual void OnPaused()
        {
            onPause?.Invoke();
        }

        /// <summary> Called when the audio player was resumed. </summary>
        protected virtual void OnResumed()
        {
            onResume?.Invoke();
        }

        /// <summary> Called when the audio player was stopped. </summary>
        protected virtual void OnStopped()
        {
            onStop?.Invoke();
        }

        /// <summary> Called when the audio player finished playing. </summary>
        protected virtual void OnFinished()
        {
            onFinish?.Invoke();
        }

        /// <summary> Reset the audio player to its default values </summary>
        public virtual void ResetAudioPlayer()
        {
            if (isPlaying) Stop(); // if the audio player is playing, stop it

            StopFadeIn();
            StopFadeOut();
            StopTicker();

            loadedSoundObject = null;
            loadedMusicObject = null;


            autoFadeInStarted = false;
            autoFadeOutStarted = false;

            mute = false;

            timeSamples = 0;
            playProgress = 0;
            // elapsedPlayTime = 0;

            recycleAfterUse = true;

            followTarget = null;

            clip = null;
            outputAudioMixerGroup = null;

            volume = SoundySettings.k_DefaultVolume;
            referenceVolume = volume;

            crossFade = SoundySettings.k_DefaultCrossFade;
            crossFadeDuration = SoundySettings.k_DefaultCrossFadeDuration;

            pitch = SoundySettings.k_DefaultPitch;
            priority = SoundySettings.k_DefaultPriority;
            panStereo = SoundySettings.k_DefaultPanStereo;
            spatialBlend = SoundySettings.k_DefaultSpatialBlend;
            reverbZoneMix = SoundySettings.k_DefaultReverbZoneMix;
            dopplerLevel = SoundySettings.k_DefaultDopplerLevel;
            spread = SoundySettings.k_DefaultSpread;
            minDistance = SoundySettings.k_DefaultMinDistance;
            maxDistance = SoundySettings.k_DefaultMaxDistance;
            loop = SoundySettings.k_DefaultLoop;
            ignoreListenerPause = SoundySettings.k_DefaultIgnoreListenerPause;
        }

        /// <summary> Reset the audio player callbacks to null </summary>
        public virtual void ResetCallbacks()
        {
            this.ClearCallbacks();
        }

        /// <summary> Update the last played time to the current time </summary>
        internal void UpdateLastPlayedTime() =>
            lastPlayedTime = Time.realtimeSinceStartup;

        /// <summary> Coroutine that ticks every frame and updates the audio player state </summary>
        private IEnumerator TickEveryFrame()
        {
            while (true)
            {
                Tick();
                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        #region Fade In/Out

        /// <summary> Internal flag to keep track if the automated fade in process has started or not. </summary>
        public bool autoFadeInStarted { get; protected set; }
        /// <summary> Internal flag to keep track if the automated fade out process has started or not. </summary>
        public bool autoFadeOutStarted { get; protected set; }

        /// <summary> Coroutine that fades in the audio player </summary>
        protected Coroutine fadeInCoroutine { get; set; }
        /// <summary> Coroutine that fades out the audio player </summary>
        protected Coroutine fadeOutCoroutine { get; set; }

        /// <summary>
        /// Start the fade in process by stopping both the fade in and fade out processes (if they are running)
        /// and then starting the fade in process.
        /// </summary>
        internal void StartFadeIn()
        {
            StopFadeOut();
            StopFadeIn();
            ProcessFadeIn();
        }

        /// <summary>
        /// Start the fade out process by stopping both the fade in and fade out processes (if they are running)
        /// and then starting the fade out process.
        /// </summary>
        internal void StartFadeOut()
        {
            StopFadeIn();
            StopFadeOut();
            ProcessFadeOut();
        }

        /// <summary> Stop the fade in process and update the isFadingIn flag to false </summary>
        protected void StopFadeIn()
        {
            isFadingIn = false;
            if (fadeInCoroutine == null) return;
            StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = null;
            //onFadeInFinish?.Invoke();
        }

        /// <summary> Stop the fade out process and update the isFadingOut flag to false </summary>
        protected void StopFadeOut()
        {
            isFadingOut = false;
            if (fadeOutCoroutine == null) return;
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
            //onFadeOutFinish?.Invoke();
        }

        /// <summary> Start the coroutine that fades in the audio player's volume </summary> 
        internal void ProcessFadeIn()
        {
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            isFadingIn = true;

            fadeInCoroutine =
                StartCoroutine
                (
                    ProcessFade
                    (
                        duration: adjustedCrossFadeDuration,
                        fromVolume: 0,
                        toVolume: referenceVolume,
                        onFadeStartCallback: () =>
                        {
                            onFadeInStart?.Invoke();
                        },
                        onFadeFinishCallback: () =>
                        {
                            isFadingIn = false;
                            onFadeInFinish?.Invoke();
                        }
                    )
                );
        }

        /// <summary> Start the coroutine that fades out the audio player's volume </summary>
        internal void ProcessFadeOut()
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            isFadingOut = true;

            fadeOutCoroutine =
                StartCoroutine
                (
                    ProcessFade
                    (
                        duration: adjustedCrossFadeDuration,
                        fromVolume: referenceVolume,
                        toVolume: 0,
                        onFadeStartCallback: () =>
                        {
                            onFadeOutStart?.Invoke();
                        },
                        onFadeFinishCallback: () =>
                        {
                            isFadingOut = false;
                            onFadeOutFinish?.Invoke();
                        }
                    )
                );
        }

        protected string frameCountInRed => $"<color=red>[{Time.frameCount}]</color>";

        /// <summary> Internal IEnumerator used to fade in/out the audio player's volume </summary>
        /// <param name="duration"></param>
        /// <param name="fromVolume"></param>
        /// <param name="toVolume"></param>
        /// <param name="onFadeStartCallback"></param>
        /// <param name="onFadeFinishCallback"></param>
        /// <returns></returns>
        protected IEnumerator ProcessFade(float duration, float fromVolume, float toVolume, UnityAction onFadeStartCallback, UnityAction onFadeFinishCallback)
        {
            onFadeStartCallback?.Invoke();
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                if (!isPlaying)
                {
                    yield return null;
                    continue;
                }

                elapsedTime += Time.unscaledDeltaTime;
                elapsedTime = Mathf.Clamp(elapsedTime, 0f, duration);
                float t = Mathf.Clamp01(elapsedTime / duration);
                volume = Mathf.Lerp(fromVolume, toVolume, t);
                yield return null;
            }

            volume = toVolume;
            onFadeFinishCallback?.Invoke();
        }

        #endregion

        /// <summary>
        /// Called every frame to update the audio player's state.
        /// This is where the audio player is recycled if it is not in use.
        /// This method is timescale independent.
        /// </summary>
        protected virtual void Tick()
        {
            switch (playState)
            {
                // --- Disabled ---
                // AudioPlayer is in the pool, Tick() should not be called -> Recycle()
                case PlayState.InPool:
                    Recycle();
                    return;

                // --- Idle ---
                // AudioPlayer is inUse but not playing anything
                case PlayState.Idle:
                    return;

                // --- Stopped ---
                // AudioPlayer stopped playing -> check if a Recycle() is needed
                case PlayState.Stopped:
                    // elapsedPlayTime = 0f;
                    playProgress = 0f;
                    UpdateLastPlayedTime();
                    if (recycleAfterUse)
                    {
                        Recycle();
                        return;
                    }
                    //AudioPlayer is inUse, not playing anything and set NOT to Recycle() -> set Idle state
                    SetPlayState(PlayState.Idle);
                    return;

                // --- Paused ---
                // AudioPlayer is paused -> just update the last played time and return
                case PlayState.Paused:
                    UpdateLastPlayedTime();
                    return;

                // --- Playing ---
                // AudioPlayer is playing -> perform all the necessary actions to keep it playing
                case PlayState.Playing:

                    //audio player is playing but the clip is null, stop the audio player 
                    if (clip == null)
                    {
                        UpdateLastPlayedTime();
                        SetPlayState(PlayState.Stopped);
                        return;
                    }

                    onUpdate?.Invoke();

                    playProgress = Mathf.Clamp01(time / clipLength);
                    if (playProgress.Approximately(1f))
                        playProgress = 1f;

                    // update the last played time
                    UpdateLastPlayedTime();

                    // if the audio player is set to follow a target, update its position
                    FollowTarget();

                    // --- CrossFade --- FadeOut ---
                    if (hasCrossFade & !loop & !autoFadeOutStarted)
                    {
                        // check if we need to start the fade out (if the clip is about to finish and getting close to the end of the CrossFade duration)
                        // bool startFadeOut = elapsedPlayTime >= clipLength - adjustedCrossFadeDuration - 0.2f;
                        bool startFadeOut = time > 0 && time >= clipLength - adjustedCrossFadeDuration - 0.2f;
                        if (startFadeOut)
                        {
                            autoFadeOutStarted = true; // set to true to avoid starting the fade out multiple times
                            StartFadeOut();            // start the fade out
                            onFadeOutFinish = Finish;  // set the onFinish callback to Finish()
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool finishedPlaying = Math.Abs(playProgress - 1) < Mathf.Epsilon | time >= clipLength;
            if (!finishedPlaying) return;

            if (loop)
            {
                volume = referenceVolume; // reset the volume (since it might have been faded out)
                FollowTarget();           // update the position of the audio player

                playState = PlayState.Stopped;
                StopFadeIn();
                StopFadeOut();

                Play(); // play again
                return; // return to avoid calling Finish()
            }

            if (!autoFadeOutStarted)
                Finish();
        }

        /// <summary>
        /// Starts fading out the volume of the audio player, if it is not currently playing, and then stops it when the fade out is finished.
        /// A fade out duration of 0 will stop the audio player immediately.
        /// </summary>
        public void FadeOutAndStop()
        {
            if (isPaused)
            {
                Stop();
                return;
            }

            if (!isPlaying)
                return;

            bool usedTemporaryCrossFade = false;
            bool previousCrossFade = crossFade;
            float previousCrossFadeDuration = crossFadeDuration;

            if (!hasCrossFade)
            {
                usedTemporaryCrossFade = true;
                crossFade = true;
                crossFadeDuration = SoundySettings.k_DefaultCrossFadeDuration;
            }

            StartFadeOut();

            onFadeOutFinish = () =>
            {
                onFadeOutFinish = null;
                if (usedTemporaryCrossFade)
                {
                    crossFade = previousCrossFade;
                    crossFadeDuration = previousCrossFadeDuration;
                }
                Stop();
            };
        }

        /// <summary>
        /// Starts playing and fading in the volume of the audio player, if it is not currently playing.
        /// A fade in duration of 0 will play the audio player immediately.
        /// </summary>
        public void FadeInAndPlay()
        {
            if (isPlaying)
                return;

            bool usedTemporaryCrossFade = false;
            bool previousCrossFade = crossFade;
            float previousCrossFadeDuration = crossFadeDuration;

            if (!hasCrossFade)
            {
                usedTemporaryCrossFade = true;
                crossFade = true;
                crossFadeDuration = SoundySettings.k_DefaultCrossFadeDuration;
            }

            StartFadeIn();

            if (usedTemporaryCrossFade)
            {
                onFadeInFinish = () =>
                {
                    onFadeInFinish = null;
                    crossFade = previousCrossFade;
                    crossFadeDuration = previousCrossFadeDuration;
                };
            }

            if (isPaused)
            {
                UnPause();
                return;
            }

            Play();
        }
    }

    public static class BaseAudioPlayerExtensions
    {
        /// <summary>
        /// Set if the audio player should fade in and out when playing and stopping.
        /// <para/> Note: This will only work if cross fade duration is greater than 0.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="crossFade"> Should the audio player cross fade? </param>
        public static T SetCrossFade<T>(this T audioPlayer, bool crossFade) where T : AudioPlayer
        {
            audioPlayer.crossFade = crossFade;
            return audioPlayer;
        }

        /// <summary>
        /// Starts fading in the volume of the audio player if it is playing.
        /// If the audio player is not playing, this method does nothing.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        public static T StartFadeIn<T>(this T audioPlayer) where T : AudioPlayer
        {
            if (!audioPlayer.isPlaying) return audioPlayer; // if the audio player is not playing, return
            audioPlayer.SetCrossFade(true);                 // set cross fade to true
            audioPlayer.StartFadeIn();                      // initialize the fade in process
            return audioPlayer;
        }

        /// <summary>
        /// Sets the fade in duration and starts fading in the volume of the audio player if it is playing.
        /// If the audio player is not playing, this method does nothing.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="duration"> Fade in duration </param>
        public static T StartFadeIn<T>(this T audioPlayer, float duration) where T : AudioPlayer
        {
            if (!audioPlayer.isPlaying) return audioPlayer; // if the audio player is not playing, return
            audioPlayer.crossFade = true;                   // set cross fade to true
            audioPlayer.crossFadeDuration = duration;       // set the fade in duration
            audioPlayer.StartFadeIn();                      // initialize the fade in process
            return audioPlayer;
        }

        /// <summary>
        /// Starts fading out the volume of the audio player if it is playing.
        /// If the audio player is not playing, this method does nothing.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        public static T StartFadeOut<T>(this T audioPlayer) where T : AudioPlayer
        {
            if (!audioPlayer.isPlaying) return audioPlayer; // if the audio player is not playing, return
            audioPlayer.SetCrossFade(true);                 // set cross fade to true
            audioPlayer.StartFadeOut();                     // initialize the fade out process
            return audioPlayer;
        }

        /// <summary>
        /// Sets the fade out duration and starts fading out the volume of the audio player if it is playing.
        /// If the audio player is not playing, this method does nothing.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="duration"> Fade out duration </param>
        public static T StartFadeOut<T>(this T audioPlayer, float duration) where T : AudioPlayer
        {
            if (!audioPlayer.isPlaying) return audioPlayer; // if the audio player is not playing, return
            audioPlayer.crossFade = true;                   // set cross fade to true
            audioPlayer.crossFadeDuration = duration;       // set the fade out duration
            audioPlayer.StartFadeOut();                     // initialize the fade out process
            return audioPlayer;
        }

        /// <summary> Set the cross fade duration for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="duration"> Fade in duration </param>
        public static T SetCrossFadeDuration<T>(this T audioPlayer, float duration) where T : AudioPlayer
        {
            audioPlayer.crossFadeDuration = duration;
            return audioPlayer;
        }

        /// <summary> Set the OnPlay callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onPlay"> OnPlay callback </param>
        public static T SetOnPlayCallback<T>(this T audioPlayer, UnityAction onPlay) where T : AudioPlayer
        {
            audioPlayer.onPlay = onPlay;
            return audioPlayer;
        }

        /// <summary> Set the OnStop callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onStop"> OnStop callback </param>
        public static T SetOnStopCallback<T>(this T audioPlayer, UnityAction onStop) where T : AudioPlayer
        {
            audioPlayer.onStop = onStop;
            return audioPlayer;
        }

        /// <summary> Set the OnUpdate callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onUpdate"> OnUpdate callback </param>
        public static T SetOnUpdateCallback<T>(this T audioPlayer, UnityAction onUpdate) where T : AudioPlayer
        {
            audioPlayer.onUpdate = onUpdate;
            return audioPlayer;
        }

        /// <summary> Set the OnPause callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onPause"> OnPause callback </param>
        public static T SetOnPauseCallback<T>(this T audioPlayer, UnityAction onPause) where T : AudioPlayer
        {
            audioPlayer.onPause = onPause;
            return audioPlayer;
        }

        /// <summary> Set the OnResume callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onResume"> OnResume callback </param>
        public static T SetOnResumeCallback<T>(this T audioPlayer, UnityAction onResume) where T : AudioPlayer
        {
            audioPlayer.onResume = onResume;
            return audioPlayer;
        }

        /// <summary> Set the OnRecycled callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onRecycled"> OnRecycled callback </param>
        public static T SetOnRecycledCallback<T>(this T audioPlayer, UnityAction onRecycled) where T : AudioPlayer
        {
            audioPlayer.onRecycle = onRecycled;
            return audioPlayer;
        }

        /// <summary> Set the OnDisposed callback for the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onDisposed"> OnDisposed callback </param>
        public static T SetOnDisposedCallback<T>(this T audioPlayer, UnityAction onDisposed) where T : AudioPlayer
        {
            audioPlayer.onDispose = onDisposed;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnPlay event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onPlay"> OnPlay callback </param>
        public static T AddOnPlayCallback<T>(this T audioPlayer, UnityAction onPlay) where T : AudioPlayer
        {
            audioPlayer.onPlay -= onPlay;
            audioPlayer.onPlay += onPlay;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnStop event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onStop"> OnStop callback </param>
        public static T AddOnStopCallback<T>(this T audioPlayer, UnityAction onStop) where T : AudioPlayer
        {
            audioPlayer.onStop -= onStop;
            audioPlayer.onStop += onStop;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnUpdate event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onUpdate"> OnUpdate callback </param>
        public static T AddOnUpdateCallback<T>(this T audioPlayer, UnityAction onUpdate) where T : AudioPlayer
        {
            audioPlayer.onUpdate -= onUpdate;
            audioPlayer.onUpdate += onUpdate;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnPause event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onPause"> OnPause callback </param>
        public static T AddOnPauseCallback<T>(this T audioPlayer, UnityAction onPause) where T : AudioPlayer
        {
            audioPlayer.onPause -= onPause;
            audioPlayer.onPause += onPause;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnResume event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onResume"> OnResume callback </param>
        public static T AddOnResumeCallback<T>(this T audioPlayer, UnityAction onResume) where T : AudioPlayer
        {
            audioPlayer.onResume -= onResume;
            audioPlayer.onResume += onResume;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnRecycled event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onRecycled"> OnRecycled callback </param>
        public static T AddOnRecycledCallback<T>(this T audioPlayer, UnityAction onRecycled) where T : AudioPlayer
        {
            audioPlayer.onRecycle -= onRecycled;
            audioPlayer.onRecycle += onRecycled;
            return audioPlayer;
        }

        /// <summary> Add a callback to the OnDisposed event. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="onDisposed"> OnDisposed callback </param>
        public static T AddOnDisposedCallback<T>(this T audioPlayer, UnityAction onDisposed) where T : AudioPlayer
        {
            audioPlayer.onDispose -= onDisposed;
            audioPlayer.onDispose += onDisposed;
            return audioPlayer;
        }

        /// <summary> Clear all callbacks from the audio player. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        public static T ClearCallbacks<T>(this T audioPlayer) where T : AudioPlayer
        {
            audioPlayer.onPlay = null;
            audioPlayer.onStop = null;
            audioPlayer.onUpdate = null;
            audioPlayer.onPause = null;
            audioPlayer.onResume = null;
            audioPlayer.onRecycle = null;
            audioPlayer.onDispose = null;

            audioPlayer.onFadeInStart = null;
            audioPlayer.onFadeInFinish = null;
            audioPlayer.onFadeOutStart = null;
            audioPlayer.onFadeOutFinish = null;
            return audioPlayer;
        }

        /// <summary> Set the audio clip to play </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="clip"> The audio clip to set </param>
        public static T SetClip<T>(this T audioPlayer, AudioClip clip) where T : AudioPlayer
        {
            audioPlayer.clip = clip;
            return audioPlayer;
        }

        /// <summary> Set the Doppler scale for this audio player </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="value"> The Doppler scale to set </param>
        public static T SetDopplerLevel<T>(this T audioPlayer, float value) where T : AudioPlayer
        {
            audioPlayer.dopplerLevel = value;
            return audioPlayer;
        }

        /// <summary>
        /// Allow this audio player to play even though AudioListener.pause is set to true.
        /// This is useful for the menu element sounds or background music in pause menus.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="enabled"> True to ignore AudioListener.pause, false to not ignore it </param>
        public static T SetIgnoreListenerPause<T>(this T audioPlayer, bool enabled) where T : AudioPlayer
        {
            audioPlayer.ignoreListenerPause = enabled;
            return audioPlayer;
        }

        /// <summary> (Logarithmic rolloff) Set the distance a sounds stops attenuating at </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="maxDistance"> The distance to set </param>
        public static T SetMaxDistance<T>(this T audioPlayer, float maxDistance) where T : AudioPlayer
        {
            audioPlayer.maxDistance = maxDistance;
            return audioPlayer;
        }

        /// <summary> Set the minimum distance within the audio source will cease to grow louder in volume </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="minDistance"> The distance to set </param>
        public static T SetMinDistance<T>(this T audioPlayer, float minDistance) where T : AudioPlayer
        {
            audioPlayer.minDistance = minDistance;
            return audioPlayer;
        }

        /// <summary> Set the target group to which the AudioSource should route its signal </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="audioMixerGroup"> The AudioMixerGroup to set </param>
        public static T SetOutputAudioMixerGroup<T>(this T audioPlayer, AudioMixerGroup audioMixerGroup) where T : AudioPlayer
        {
            audioPlayer.outputAudioMixerGroup = audioMixerGroup;
            return audioPlayer;
        }

        /// <summary>
        /// Set the pan level (left / right bias) of the audio.
        /// This only applies to sounds that are Mono or Stereo.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="panStereo"> The pan level to set </param>
        public static T SetPanStereo<T>(this T audioPlayer, float panStereo) where T : AudioPlayer
        {
            audioPlayer.panStereo = panStereo;
            return audioPlayer;
        }

        /// <summary> Set the pitch of the audio </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="pitch"> The pitch to set </param>
        public static T SetPitch<T>(this T audioPlayer, float pitch) where T : AudioPlayer
        {
            audioPlayer.pitch = pitch;
            return audioPlayer;
        }

        /// <summary>
        /// Set the priority of the audio.
        /// Unity virtualizes AudioSources when there are more AudioSources playing than available hardware channels.
        /// The AudioSources with lowest priority (and audibility) are virtualized first.
        /// Priority is an integer between 0 and 255. Where, 0 = high priority, 255 = low priority.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="priority"> The priority to set </param>
        public static T SetPriority<T>(this T audioPlayer, int priority) where T : AudioPlayer
        {
            audioPlayer.priority = priority;
            return audioPlayer;
        }

        /// <summary> Set how much this audio player is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="spatialBlend"> The spatial blend to set (0.0 makes the sound full 2D, 1.0 makes it full 3D) </param>
        public static T SetSpatialBlend<T>(this T audioPlayer, float spatialBlend) where T : AudioPlayer
        {
            audioPlayer.spatialBlend = spatialBlend;
            return audioPlayer;
        }

        /// <summary> Set the amount by which the signal from this sound will be mixed into the global reverb associated with the Reverb Zones </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="reverbZoneMix"> The reverb zone mix to set </param>
        public static T SetReverbZoneMix<T>(this T audioPlayer, float reverbZoneMix) where T : AudioPlayer
        {
            audioPlayer.reverbZoneMix = reverbZoneMix;
            return audioPlayer;
        }

        /// <summary> Set the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space. </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="spread"> The spread angle (in degrees) to set </param>
        public static T SetSpread<T>(this T audioPlayer, float spread) where T : AudioPlayer
        {
            audioPlayer.spread = spread;
            return audioPlayer;
        }

        /// <summary> Set the volume of the audio </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="volume"> The volume to set </param>
        public static T SetVolume<T>(this T audioPlayer, float volume) where T : AudioPlayer
        {
            audioPlayer.volume = volume;
            return audioPlayer;
        }

        /// <summary> Set this audio player to loop or not loop the audio clip </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="enabled"> True to loop the audio clip, false to not loop it </param>
        public static T SetLoop<T>(this T audioPlayer, bool enabled) where T : AudioPlayer
        {
            audioPlayer.loop = enabled;
            return audioPlayer;
        }

        /// <summary> Mutes / unmute the audio </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="enabled"> True to mute the audio, false to unmute it </param>
        public static T SetMute<T>(this T audioPlayer, bool enabled) where T : AudioPlayer
        {
            audioPlayer.mute = enabled;
            return audioPlayer;
        }

        /// <summary> Pauses / unpauses the audio </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="enabled"> True to pause the audio, false to unpause it </param>
        public static T SetPause<T>(this T audioPlayer, bool enabled) where T : AudioPlayer
        {
            if (enabled) audioPlayer.Pause();
            else audioPlayer.UnPause();
            return audioPlayer;
        }

        /// <summary>
        /// Loads a sound object into the audio player.
        /// If the audio player is already playing a sound, it will be stopped and replaced with the new sound.
        /// If the new sound is null, the audio player will be stopped.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="libraryName"> The name of the library to load the sound from </param>
        /// <param name="soundName"> The name of the sound to load </param>
        /// <param name="audioMixerGroup"> The audio mixer group to use for the sound </param>
        public static T SetSound<T>(this T audioPlayer, string libraryName, string soundName, AudioMixerGroup audioMixerGroup = null) where T : AudioPlayer
        {
            audioPlayer.LoadSound(libraryName, soundName, audioMixerGroup);
            return audioPlayer;
        }

        /// <summary>
        /// Loads a sound object into the audio player.
        /// If the audio player is already playing a sound, it will be stopped and replaced with the new sound.
        /// If the new sound is null, the audio player will be stopped.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="soundObject"> The sound object to load </param>
        /// <param name="audioMixerGroup"> The audio mixer group to use for the sound </param>
        public static T SetSound<T>(this T audioPlayer, SoundObject soundObject, AudioMixerGroup audioMixerGroup = null) where T : AudioPlayer
        {
            audioPlayer.LoadSound(soundObject, audioMixerGroup);
            return audioPlayer;
        }

        /// <summary>
        /// Loads a sound id into the audio player.
        /// If the audio player is already playing a sound, it will be stopped and replaced with the new sound.
        /// If the new sound is null, the audio player will be stopped.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="soundId"> The sound id to load </param>
        public static T SetSound<T>(this T audioPlayer, SoundId soundId) where T : AudioPlayer
        {
            if (soundId == null)
            {
                audioPlayer.Stop();
                return audioPlayer;
            }

            SoundObject soundObject = soundId.GetSoundObject();
            if (soundObject == null)
            {
                audioPlayer.Stop();
                return audioPlayer;
            }

            AudioMixerGroup outputAudioMixerGroup = soundId.GetOutputAudioMixerGroup();

            audioPlayer.LoadSound(soundObject, outputAudioMixerGroup);
            return audioPlayer;
        }

        /// <summary>
        /// Loads a music object into the audio player.
        /// If the audio player is already playing a music, it will be stopped and replaced with the new music.
        /// If the new music is null, the audio player will be stopped.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="libraryName"> The name of the library to load the music from </param>
        /// <param name="musicName"> The name of the music to load </param>
        /// <param name="audioMixerGroup"> The audio mixer group to use for the music </param>
        public static T SetMusic<T>(this T audioPlayer, string libraryName, string musicName, AudioMixerGroup audioMixerGroup = null) where T : AudioPlayer
        {
            audioPlayer.LoadMusic(libraryName, musicName, audioMixerGroup);
            return audioPlayer;
        }

        /// <summary>
        /// Loads a music object into the audio player.
        /// If the audio player is already playing a music, it will be stopped and replaced with the new music.
        /// If the new music is null, the audio player will be stopped.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="musicObject"> The music object to load </param>
        /// <param name="audioMixerGroup"> The audio mixer group to use for the music </param>
        public static T SetMusic<T>(this T audioPlayer, MusicObject musicObject, AudioMixerGroup audioMixerGroup = null) where T : AudioPlayer
        {
            audioPlayer.LoadMusic(musicObject, audioMixerGroup);
            return audioPlayer;
        }

        /// <summary>
        /// Load a music id into the audio player.
        /// If the audio player is already playing a music, it will be stopped and replaced with the new music.
        /// If the new music is null, the audio player will be stopped.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="musicId"> The music id to load </param>
        public static T SetMusic<T>(this T audioPlayer, MusicId musicId) where T : AudioPlayer
        {
            if (musicId == null)
            {
                audioPlayer.Stop();
                return audioPlayer;
            }

            MusicObject musicObject = musicId.GetMusicObject();
            if (musicObject == null)
            {
                audioPlayer.Stop();
                return audioPlayer;
            }

            AudioMixerGroup outputAudioMixerGroup = musicId.GetOutputAudioMixerGroup();

            audioPlayer.LoadMusic(musicObject, outputAudioMixerGroup);
            return audioPlayer;
        }

        public static T SetRecycleAfterUse<T>(this T audioPlayer, bool enabled) where T : AudioPlayer
        {
            audioPlayer.recycleAfterUse = enabled;
            return audioPlayer;
        }

        /// <summary>
        /// Sets the audio player to follow a target transform while playing audio.
        /// If the target transform is null, the audio player will stop following the target.
        /// </summary>
        /// <param name="audioPlayer"> Target audio player </param>
        /// <param name="followTarget"> The target transform to follow </param>
        public static T SetFollowTarget<T>(this T audioPlayer, Transform followTarget) where T : AudioPlayer
        {
            audioPlayer.followTarget = followTarget;
            return audioPlayer;
        }
    }
}
