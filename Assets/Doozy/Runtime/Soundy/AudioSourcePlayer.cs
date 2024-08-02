// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;
using UnityEngine.Audio;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global

namespace Doozy.Runtime.Soundy
{
    /// <summary> Audio Player that uses an AudioSource to play the audio </summary>
    public class AudioSourcePlayer : AudioPlayer
    {
        /// <summary> Audio clip that will be played </summary>
        public override AudioClip clip
        {
            get => source.clip;
            set => source.clip = value;
        }

        /// <summary> Audio mixer group that the audio will be routed through </summary>
        public override AudioMixerGroup outputAudioMixerGroup
        {
            get => source.outputAudioMixerGroup;
            set => source.outputAudioMixerGroup = value;
        }

        /// <summary> Volume to for the audio clip </summary>
        public override float volume
        {
            get => source.volume;
            set => source.volume = value;
        }

        /// <summary> Pitch to play the audio clip at </summary>
        public override float pitch
        {
            get => source.pitch;
            set => source.pitch = value;
        }

        /// <summary> Mute flag for the audio clip </summary>
        public override bool mute
        {
            get => source.mute;
            set => source.mute = value;
        }

        /// <summary> Doppler scale for the audio clip </summary>
        public override float dopplerLevel
        {
            get => source.dopplerLevel;
            set => source.dopplerLevel = value;
        }

        /// <summary> Allows AudioSource to play even though AudioListener.pause is set to true </summary>
        public override bool ignoreListenerPause
        {
            get => source.ignoreListenerPause;
            set => source.ignoreListenerPause = value;
        }

        /// <summary> (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at </summary>
        public override float maxDistance
        {
            get => source.maxDistance;
            set => source.maxDistance = value;
        }

        /// <summary> Within the Min distance the AudioSource will cease to grow louder in volume </summary>
        public override float minDistance
        {
            get => source.minDistance;
            set => source.minDistance = value;
        }

        /// <summary> Pan the playing audio in a stereo way (left or right) </summary>
        public override float panStereo
        {
            get => source.panStereo;
            set => source.panStereo = value;
        }

        /// <summary> Priority of the AudioSource </summary>
        public override int priority
        {
            get => source.priority;
            set => source.priority = value;
        }

        /// <summary> How much the audio is affected by 3D spatialisation calculations </summary>
        public override float spatialBlend
        {
            get => source.spatialBlend;
            set => source.spatialBlend = value;
        }

        /// <summary> Spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space </summary>
        public override float spread
        {
            get => source.spread;
            set => source.spread = value;
        }

        /// <summary> Playback position in seconds </summary>
        public override float time
        {
            get => source.time;
            set => source.time = value;
        }

        /// <summary> Playback position in PCM samples </summary>
        public override int timeSamples
        {
            get => source.timeSamples;
            set => source.timeSamples = value;
        }

        /// <summary> AudioSource component reference that this player uses to play the audio </summary>
        [SerializeField] private AudioSource Source;
        /// <summary>
        /// Returns the AudioSource component reference that this player uses to play the audio.
        /// If the reference is null, it will try to get it from the GameObject.
        /// If the reference is still null, it will add an AudioSource component to the GameObject.
        /// This never returns null.
        /// </summary>
        public AudioSource source
        {
            get
            {
                if (Source != null) return Source;
                Source = gameObject.GetComponent<AudioSource>();
                if (Source != null) return Source;
                Source = gameObject.AddComponent<AudioSource>();
                return Source;
            }
        }

        /// <summary>
        /// Check if the audio player is paused by checking if it is in use, if it is playing and if the audio source is not playing.
        /// This is a special case to handle the fact that the audio source is not playing when the game is paused.
        /// </summary>
        protected override bool autoPaused => inUse && isPlaying && !source.isPlaying;

        private void Reset()
        {
            ResetAudioPlayer();
        }

        private void OnValidate()
        {
            if (source == null)
                Source = gameObject.GetComponent<AudioSource>();
        }

        protected override void Awake()
        {
            base.Awake();
            source.playOnAwake = false;
        }

        /// <summary> Play the currently set audio clip </summary>
        public override void Play()
        {
            base.Play();
            source.Play();
        }

        /// <summary> Stop the currently playing audio </summary>
        public override void Stop()
        {
            base.Stop();
            source.Stop();
        }

        /// <summary> Reset the audio player to its default values </summary>
        public override void ResetAudioPlayer()
        {
            source.clip = null;
            source.outputAudioMixerGroup = null;

            source.volume = SoundySettings.k_DefaultVolume;
            source.pitch = SoundySettings.k_DefaultPitch;
            source.priority = SoundySettings.k_DefaultPriority;
            source.panStereo = SoundySettings.k_DefaultPanStereo;
            source.spatialBlend = SoundySettings.k_DefaultSpatialBlend;
            source.reverbZoneMix = SoundySettings.k_DefaultReverbZoneMix;
            source.dopplerLevel = SoundySettings.k_DefaultDopplerLevel;
            source.spread = SoundySettings.k_DefaultSpread;
            source.minDistance = SoundySettings.k_DefaultMinDistance;
            source.maxDistance = SoundySettings.k_DefaultMaxDistance;
            source.loop = SoundySettings.k_DefaultLoop;
            source.ignoreListenerPause = SoundySettings.k_DefaultIgnoreListenerPause;
        }

        /// <summary> Pause the audio player </summary>
        public override void Pause()
        {
            base.Pause();
            source.Pause();
        }
        
        /// <summary> UnPause the audio player </summary>
        public override void UnPause()
        {
            base.UnPause();
            source.UnPause();
        }
    }
}
