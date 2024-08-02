// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Common.Extensions;
using UnityEngine;
using UnityEngine.Events;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.Soundy.ScriptableObjects.Internal
{
    /// <summary> Base class for all audio scriptable objects </summary>
    public abstract class AudioObject : ScriptableObject
    {
        /// <summary> Internal use only. Used by the Soundy system to know when this object has been updated and trigger editor updates. </summary>
        public UnityAction OnUpdate;
        
        [SerializeField] private AudioLibrary Library;
        /// <summary> Audio Library that holds this audio object </summary>
        public AudioLibrary library
        {
            get => Library;
            internal set => Library = value;
        }
        
        /// <summary> Name of this sound, used to identify it in the Soundy system </summary>
        [SerializeField] private string AudioName = SoundySettings.k_DefaultAudioName;
        /// <summary> Name of this sound, used to identify it in the Soundy system </summary>
        public string audioName
        {
            get => AudioName;
            set
            {
                if (value.IsNullOrEmpty())
                    value = SoundySettings.k_DefaultAudioName;
                AudioName = value.CleanName();
            }
        }

        /// <summary> Volume multiplier for this sound </summary>
        [SerializeField] private float Volume = SoundySettings.k_DefaultVolume;
        /// <summary> Volume multiplier for this sound (0 = lowest volume, 1 = highest volume) </summary>
        public float volume
        {
            get => Volume;
            set => Volume = Mathf.Clamp(value, SoundySettings.k_MinVolume, SoundySettings.k_MaxVolume);
        }

        /// <summary> Priority of this sound (0 = highest priority, 255 = lowest priority) </summary>
        [SerializeField] private int Priority = SoundySettings.k_DefaultPriority;
        /// <summary> Priority of this sound (0 = highest priority, 255 = lowest priority) </summary>
        public int priority
        {
            get => Priority;
            set => Priority = Mathf.Clamp(value, SoundySettings.k_MinPriority, SoundySettings.k_MaxPriority);
        }

        /// <summary> Pans the sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo. </summary>
        [SerializeField] private float PanStereo = SoundySettings.k_DefaultPanStereo;
        /// <summary> Pans the sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo. </summary>
        public float panStereo
        {
            get => PanStereo;
            set => PanStereo = Mathf.Clamp(value, SoundySettings.k_MinPanStereo, SoundySettings.k_MaxPanStereo);
        }

        /// <summary>
        /// Spatial Blend factor for this sound that sets how much this sound is affected by 3D spatialisation calculations
        /// (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
        /// </summary>
        [SerializeField] private float SpatialBlend = SoundySettings.k_DefaultSpatialBlend;
        /// <summary>
        /// Spatial Blend factor for this sound that sets how much this sound is affected by 3D spatialisation calculations
        /// (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
        /// </summary>
        public float spatialBlend
        {
            get => SpatialBlend;
            set => SpatialBlend = Mathf.Clamp(value, SoundySettings.k_MinSpatialBlend, SoundySettings.k_MaxSpatialBlend);
        }

        /// <summary> The amount by which the signal from this sound will be mixed into the global reverb associated with the Reverb Zones. </summary>
        [SerializeField] private float ReverbZoneMix = SoundySettings.k_DefaultReverbZoneMix;
        /// <summary> The amount by which the signal from this sound will be mixed into the global reverb associated with the Reverb Zones. </summary>
        public float reverbZoneMix
        {
            get => ReverbZoneMix;
            set => ReverbZoneMix = Mathf.Clamp(value, SoundySettings.k_MinReverbZoneMix, SoundySettings.k_MaxReverbZoneMix);
        }

        /// <summary> Set the Doppler scale for this sound. This is the amount of effect the listener's velocity has on the pitch of the sound. </summary>
        [SerializeField] private float DopplerLevel = SoundySettings.k_DefaultDopplerLevel;
        /// <summary> Set the Doppler scale for this sound. This is the amount of effect the listener's velocity has on the pitch of the sound. </summary>
        public float dopplerLevel
        {
            get => DopplerLevel;
            set => DopplerLevel = Mathf.Clamp(value, SoundySettings.k_MinDopplerLevel, SoundySettings.k_MaxDopplerLevel);
        }

        /// <summary> Set the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space. </summary>
        [SerializeField] private int Spread = SoundySettings.k_DefaultSpread;
        /// <summary> Set the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space. </summary>
        public int spread
        {
            get => Spread;
            set => Spread = Mathf.Clamp(value, SoundySettings.k_MinSpread, SoundySettings.k_MaxSpread);
        }

        /// <summary> Within the Min distance the sound will cease to grow louder in volume. </summary>
        [SerializeField] private float MinDistance = SoundySettings.k_DefaultMinDistance;
        /// <summary> Within the Min distance the sound will cease to grow louder in volume. </summary>
        public float minDistance
        {
            get => MinDistance;
            set => MinDistance = Mathf.Clamp(value, SoundySettings.k_MinMinDistance, SoundySettings.k_MaxMinDistance);
        }

        /// <summary> (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at. </summary>
        [SerializeField] private float MaxDistance = SoundySettings.k_DefaultMaxDistance;
        /// <summary> (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at. </summary>
        public float maxDistance
        {
            get => MaxDistance;
            set => MaxDistance = Mathf.Clamp(value, SoundySettings.k_MinMaxDistance, SoundySettings.k_MaxMaxDistance);
        }

        /// <summary> Play in a loop </summary>
        [SerializeField] private bool Loop = SoundySettings.k_DefaultLoop;
        /// <summary> Play in a loop </summary>
        public bool loop
        {
            get => Loop;
            set => Loop = value;
        }
        
        /// <summary> If TRUE, the sound will ignore the AudioListener.pause state and will play even if the game/application is paused </summary>
        [SerializeField] private bool IgnoreListenerPause = SoundySettings.k_DefaultIgnoreListenerPause;
        /// <summary> If TRUE, the sound will ignore the AudioListener.pause state and will play even if the game/application is paused </summary>
        public bool ignoreListenerPause
        {
            get => IgnoreListenerPause;
            set => IgnoreListenerPause = value;
        }

        /// <summary> Check if this sound can be played </summary>
        public abstract bool canPlay { get; }

        protected AudioObject()
        {
            audioName = SoundySettings.k_DefaultAudioName;
        }
        
        public virtual void Validate()
        {
            string initialName = audioName;
            audioName = audioName;
            bool updateAssetName = initialName != audioName | name != audioName;
            if (!updateAssetName) return;
            name = AudioName;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }
    }

    public static class AudioObjectExtensions
    {
        /// <summary> Perform an action on this music object and record it in the undo system. </summary>
        /// <param name="target"> The audio object to perform the action on. </param>
        /// <param name="undoName"> The name of the undo action. </param>
        /// <param name="action"> The action to perform. </param>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the sound object. </returns>
        public static T Do<T>(this T target, string undoName, Action action, bool save = true) where T : AudioObject
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(target, undoName);
            #endif

            action.Invoke();

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
            if (save) UnityEditor.AssetDatabase.SaveAssetIfDirty(target);
            #endif

            target.OnUpdate?.Invoke();
            return target;
        }
    }
}
