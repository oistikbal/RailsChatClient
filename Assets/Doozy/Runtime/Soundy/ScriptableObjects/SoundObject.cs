// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using UnityEngine;

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Audio Object used to play sounds. It holds a list of SoundData that can be played using an AudioPlayer.
    /// This gets referenced by a SoundLibrary and can be played using an AudioPlayer.
    /// </summary>
    [Serializable]
    public class SoundObject : AudioObject
    {
        /// <summary> Sound Library that holds this sound object </summary>
        public SoundLibrary soundLibrary => (SoundLibrary)library;
        
        /// <summary>
        /// Get the current time in seconds.
        /// This value is different in the editor and in the game (to allow using it when not in play mode).
        /// </summary>
        public float currentTime
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.isPlaying
                    ? Time.realtimeSinceStartup
                    : (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
                return Time.realtimeSinceStartup;
#endif
            }
        }

        /// <summary> Checks if this sound object can be played (if it has at least one sound data that can be played) </summary>
        public override bool canPlay
        {
            get
            {
                if (Data == null || Data.Count == 0) return false;
                for (int i = 0; i < Data.Count; i++)
                    if (Data[i] != null && Data[i].canPlay)
                        return true;
                return false;
            }
        }

        /// <summary> Order in which the sounds will be played (Random, Sequential) </summary>
        public PlayMode PlayMode = PlayMode.Sequential;

        /// <summary> If PlayMode is set to Sequential, this will reset the sequence after a set idle time (in seconds) </summary>
        public bool AutoResetSequence = false;

        /// <summary>
        /// If PlayMode is set to Sequential, this is the idle time (in seconds) after which the sequence will be reset.
        /// This value is used only if AutoResetSequence is set to TRUE.
        /// Note that the time is measured from the time a sound starts playing, not from the time it ends.
        /// </summary>
        public float AutoResetSequenceTime = 2f;

        /// <summary> List of all audio data available for this sound </summary>
        [SerializeField] private List<SoundData> Data;
        /// <summary> List of all audio data available for this sound </summary>
        public List<SoundData> data => Data ?? (Data = new List<SoundData>());

        /// <summary> Next sound to play </summary>
        public SoundData nextData { get; private set; }

        /// <summary> Keeps track of the last played index </summary>
        public int lastPlayedIndex { get; private set; } = -1;

        /// <summary> Keeps track of the last played time. This is used to reset the sequence if AutoResetSequence is set to TRUE. </summary>
        public float lastPlayedTime { get; private set; }

        /// <summary> Keeps track of the played sounds </summary>
        private readonly List<SoundData> m_PlayedSounds = new List<SoundData>();

        /// <summary> Keeps a reference to the last played sound </summary>
        public SoundData lastPlayedSound { get; private set; }

        /// <summary> Add a new sound data to the list of available sounds to play. </summary>
        /// <param name="save"> Save the changes to the asset file </param>
        public SoundObject AddNew(bool save = true) =>
            this.Do("Add New", () => data.Add(new SoundData()), save);

        /// <summary>
        /// Add as many new sound data to the list of available sounds to play as the number of clips provided.
        /// This is a helper method that allows you to add multiple sounds at once.
        /// </summary>
        /// <param name="clips"> Array of audio clips </param>
        /// <returns> Returns this sound object </returns>
        public SoundObject AddNew(params AudioClip[] clips)
        {
            AddNewNoSave(clips);

            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif

            OnUpdate?.Invoke();

            return this;
        }

        /// <summary>
        /// Add as many new sound data to the list of available sounds to play as the number of clips provided.
        /// This is a helper method that allows you to add multiple sounds at once.
        /// AssetDatabase.SaveAssetIfDirty is not called.
        /// </summary>
        /// <param name="clips"> Array of audio clips </param>
        /// <returns> Returns this sound object </returns>
        internal SoundObject AddNewNoSave(params AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return this;

            bool setAudioClipNameAsSoundName = audioName.IsNullOrEmpty() || audioName.Equals(SoundySettings.k_DefaultAudioName);

            foreach (AudioClip clip in clips)
            {
                if (clip == null) continue;
                var soundData = new SoundData
                {
                    Clip = clip
                };
                data.Add(soundData);

                if (!setAudioClipNameAsSoundName) continue;
                string newName = clip.name.CleanName();
                if (newName.IsNullOrEmpty()) continue;
                audioName = newName;
                name = audioName;
                setAudioClipNameAsSoundName = false;
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif

            return this;
        }

        /// <summary> Play a sound using the provided player. </summary>
        /// <param name="player"> Audio player that will play the sound </param>
        public void Play(AudioPlayer player)
        {
            if (data.Count == 0)
            {
                Debug.LogWarning($"{nameof(SoundObject)} [{audioName}] has no sounds assigned to it. Please add at least one sound to it.");
                return;
            }

            SoundData soundData = GetNextSound();
            if (soundData == null)
            {
                Debug.LogWarning($"{nameof(SoundObject)} [{audioName}] has no sounds assigned to it. Please add at least one sound to it.");
                return;
            }

            player
                .SetClip(soundData.Clip)
                .SetVolume(GetVolume())
                .SetPitch(GetPitch())
                .SetPriority(priority)
                .SetPanStereo(panStereo)
                .SetSpatialBlend(spatialBlend)
                .SetReverbZoneMix(reverbZoneMix)
                .SetDopplerLevel(dopplerLevel)
                .SetSpread(spread)
                .SetMinDistance(minDistance)
                .SetMaxDistance(maxDistance)
                .SetLoop(loop)
                .SetIgnoreListenerPause(ignoreListenerPause)
                .Play();
        }

        /// <summary> Validate the sound object and remove any invalid data </summary>
        public override void Validate()
        {
            base.Validate();
            Data ??= new List<SoundData>();
            for (int i = 0; i < Data.Count; i++)
            {
                SoundData soundData = Data[i];
                if (soundData == null)
                {
                    Data.RemoveAt(i);
                    i--;
                    continue;
                }
                soundData.Validate();
                if (soundData.Clip != null)
                    continue;

                Data.RemoveAt(i);
                i--;
            }
        }

        /// <summary> Get the next sound to play based on the selected PlayMode. </summary>
        /// <returns> Returns the next sound to play </returns>
        private SoundData GetNextSound()
        {
            if (Data.Count == 0)
                return null;

            if (Data.Count == 1)
                return Data[0];

            SoundData soundData;

            switch (PlayMode)
            {
                case PlayMode.Sequential:
                    soundData = GetSequentialSound();
                    break;
                case PlayMode.Random:
                    soundData = GetRandomSound();
                    break;
                case PlayMode.RandomNoRepeat:
                    soundData = GetRandomNoRepeatSound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lastPlayedSound = soundData;
            return soundData;
        }

        /// <summary> Get the next sound to play when the PlayMode is set to Sequential </summary>
        /// <returns> Returns the next sound to play </returns>
        private SoundData GetSequentialSound()
        {
            if (data.Count == 0) return null;
            lastPlayedIndex++;
            lastPlayedIndex = lastPlayedIndex < 0 || lastPlayedIndex >= Data.Count ? 0 : lastPlayedIndex;
            bool resetSequence = AutoResetSequence && currentTime - lastPlayedTime > AutoResetSequenceTime;
            lastPlayedIndex = resetSequence ? 0 : lastPlayedIndex;
            lastPlayedTime = currentTime;
            return Data[lastPlayedIndex];
        }

        /// <summary>
        /// Get the next sound to play when the PlayMode is set to Random.
        /// This method takes into account the weight of each sound.
        /// </summary>
        /// <returns> Returns the next sound to play </returns>
        private SoundData GetRandomSound()
        {
            int totalWeight = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                SoundData sound = Data[i];
                totalWeight += sound.Weight;
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);
            int weight = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                SoundData sound = Data[i];
                weight += sound.Weight;
                if (randomWeight <= weight)
                    return sound;
            }

            return null;
        }

        /// <summary>
        /// Get the next sound to play when the PlayMode is set to RandomNoRepeat.
        /// This method takes into account the weight of each sound.
        /// </summary>
        /// <returns> Returns the next sound to play </returns>
        private SoundData GetRandomNoRepeatSound()
        {
            if (m_PlayedSounds.Count == Data.Count)
                m_PlayedSounds.Clear();

            int totalWeight = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                SoundData sound = Data[i];
                if (m_PlayedSounds.Contains(sound))
                    continue;
                totalWeight += sound.Weight;
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);
            int weight = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                SoundData sound = Data[i];
                if (m_PlayedSounds.Contains(sound))
                    continue;
                weight += sound.Weight;
                if (randomWeight <= weight)
                {
                    m_PlayedSounds.Add(sound);
                    return sound;
                }
            }

            return null;
        }

        /// <summary> Load the next clip to play and get the AudioClip from it. </summary>
        /// <returns> Returns the AudioClip from the next sound data. </returns>
        public AudioClip LoadNext()
        {
            if (!canPlay) return null;
            nextData = GetNextSound();
            return nextData?.Clip;
        }

        /// <summary> Get the volume of the sound object for the next clip. </summary>
        /// <returns> Returns the volume of the sound object for the next clip adjusted by the volume of the sound data. </returns>
        public float GetVolume() =>
            nextData != null ? volume * nextData.Volume : volume;

        /// <summary> Get the pitch of the sound object for the next clip. </summary>
        /// <returns> Returns the pitch of the sound object for the next clip. </returns>
        public float GetPitch() =>
            nextData?.Pitch ?? SoundySettings.k_DefaultPitch;

        /// <summary> Clear all the sound data from the sound object. </summary>
        /// <param name="save"> Save the sound object </param>
        /// <returns> Returns the sound object. </returns>
        public SoundObject ClearData(bool save = true) =>
            this.Do("Clear data", () => data.Clear(), save);

        /// <summary> Sort the sound data by the name of the audio clip in ascending order. </summary>
        /// <param name="save"> Save the sound object </param>
        /// <returns> Returns the sound object. </returns>
        public SoundObject SortByAudioClipNameAz(bool save = true) =>
            this.Do("Sort by audio clip name", () => data.Sort((a, b) => string.Compare(a.Clip.name, b.Clip.name, StringComparison.Ordinal)), save);

        /// <summary> Sort the sound data by the name of the audio clip in descending order. </summary>
        /// <param name="save"> Save the sound object </param>
        /// <returns> Returns the sound object. </returns>
        public SoundObject SortByAudioClipNameZa(bool save = true) =>
            this.Do("Sort by audio clip name", () => data.Sort((a, b) => string.Compare(b.Clip.name, a.Clip.name, StringComparison.Ordinal)), save);
    }
}