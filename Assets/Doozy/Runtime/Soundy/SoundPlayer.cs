// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.Common.Utils;
using Doozy.Runtime.Soundy.Ids;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;
using UnityEngine.Audio;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.Soundy
{
    /// <summary>
    /// Audio component that can play a Soundy sound by using a SoundId..
    /// It uses an AudioPlayer component to play the sound.
    /// </summary>
    [AddComponentMenu("Doozy/Soundy/Sound Player")]
    public class SoundPlayer : MonoBehaviour
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Doozy/Soundy/Sound Player", false, 8)]
        private static void CreateComponent(UnityEditor.MenuCommand menuCommand)
        {
            GameObjectUtils.AddToScene<SoundPlayer>("Sound Player", false, true);
        }
        #endif
        
        /// <summary>
        /// Create a new SoundPlayer GameObject and add a SoundPlayer component to it.
        /// </summary>
        /// <returns> Newly created SoundPlayer component </returns>
        public static SoundPlayer Get() =>
            new GameObject("Sound Player").AddComponent<SoundPlayer>();

        [SerializeField] private SoundId Id;
        /// <summary> Sound Id (library name + sound name) that this SoundPlayer is referencing </summary>
        public SoundId id
        {
            get => Id;
            set => Id = value;
        }

        [SerializeField] private bool PlayOnStart = false;
        /// <summary> Play the sound on Start </summary>
        public bool playOnStart
        {
            get => PlayOnStart;
            set => PlayOnStart = value;
        }

        [SerializeField] private bool PlayOnEnable = false;
        /// <summary> Play the sound on Enable </summary>
        public bool playOnEnable
        {
            get => PlayOnEnable;
            set => PlayOnEnable = value;
        }

        [SerializeField] private bool PlayOnDisable = false;
        /// <summary> Play the sound on Disable </summary>
        public bool playOnDisable
        {
            get => PlayOnDisable;
            set => PlayOnDisable = value;
        }

        [SerializeField] private bool StopOnDisable = true;
        /// <summary> Stop the sound on Disable </summary>
        public bool stopOnDisable
        {
            get => StopOnDisable;
            set => StopOnDisable = value;
        }

        [SerializeField] private bool StopOnDestroy = true;
        /// <summary> Stop the sound on Destroy </summary>
        public bool stopOnDestroy
        {
            get => StopOnDestroy;
            set => StopOnDestroy = value;
        }

        [SerializeField] private Transform FollowTarget;
        /// <summary> Transform that the sound played by this SoundPlayer will follow (if set) </summary>
        public Transform followTarget
        {
            get => FollowTarget;
            set => FollowTarget = value;
        }
        
        /// <summary> When playing, this is the audio player that is used to play the sound (is null when not playing) </summary>
        public AudioPlayer audioPlayer { get; private set; }
        
        /// <summary> SoundLibrary the SoundId is referencing (can be null) </summary>
        public SoundLibrary soundLibrary { get; private set; }
        
        /// <summary> SoundObject the SoundId is referencing (can be null) </summary>
        public SoundObject soundObject { get; private set; }
        
        /// <summary> AudioMixerGroup used by this SoundPlayer when playing a sound (can be null) </summary>
        public AudioMixerGroup outputAudioMixerGroup { get; private set; }

        private void Awake()
        {
            SoundyService.Initialize();
            SetSound(id);
        }

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            if (stopOnDisable)
            {
                Stop();
                return;
            }
            
            if (playOnDisable)
                Play();
        }
        
        private void OnDestroy()
        {
            if (stopOnDestroy)
                Stop();
        }

        /// <summary>
        /// Clear the sound player and id and set everything to null.
        /// After this method is called, the sound player will not be able to play any sound until a new sound is set.
        /// </summary>
        /// <returns> Self (useful for chaining) </returns>
        public SoundPlayer ResetPlayer()
        {
            id.Reset();
            soundLibrary = null;
            outputAudioMixerGroup = null;
            soundObject = null;
            followTarget = null;
            return this;
        }
        
        /// <summary> Set a new sound by providing the sound id </summary>
        /// <param name="newSoundId"> Sound Id (library name + sound name) </param>
        /// <returns> Self (useful for chaining) </returns>
        public SoundPlayer SetSound(SoundId newSoundId) =>
            SetSound(newSoundId.libraryName, newSoundId.audioName);

        /// <summary> Set a new sound by providing the sound library name and the sound name </summary>
        /// <param name="newLibraryName"> Sound Library Name where this sound is located </param>
        /// <param name="newSoundName"> Sound Name from the Sound Library </param>
        /// <returns> Self (useful for chaining) </returns>
        public SoundPlayer SetSound(string newLibraryName, string newSoundName)
        {
            id.Set(newLibraryName, newSoundName);
            soundLibrary = id.GetSoundLibrary();
            outputAudioMixerGroup = id.GetOutputAudioMixerGroup();
            soundObject = id.GetSoundObject();
            return this;
        }
        
        /// <summary> Set a target that the sound will follow while playing </summary>
        /// <param name="newFollowTarget"> Transform that the sound will follow </param>
        /// <returns> Self (useful for chaining) </returns>
        public SoundPlayer SetFollowTarget(Transform newFollowTarget)
        {
            followTarget = newFollowTarget;
            if(audioPlayer != null)
                audioPlayer.SetFollowTarget(followTarget);
            return this;
        }

        /// <summary>
        /// Play the loaded sound object.
        /// If the sound object is null or cannot play, nothing happens.
        /// </summary>
        public void Play()
        {
            if (audioPlayer != null && audioPlayer.isPlaying)
            {
                bool recycleAfterUse = audioPlayer.recycleAfterUse;
                audioPlayer.SetRecycleAfterUse(false);
                audioPlayer.Stop();
                soundObject.LoadNext();
                audioPlayer.SetSound(soundObject);
                audioPlayer.Play();
                audioPlayer.SetRecycleAfterUse(recycleAfterUse);
                audioPlayer.SetFollowTarget(followTarget);
                return;
            }

            if (soundObject == null)
                SetSound(id);

            if (soundObject == null)
                return;

            audioPlayer = SoundyService.GetSoundPlayer();
            
            if (audioPlayer == null)
                return;

            audioPlayer
                .SetSound(soundObject)
                .SetOnRecycledCallback(() => audioPlayer = null)
                .SetFollowTarget(followTarget)
                .Play();
        }
        
        /// <summary>
        /// Stop the loaded sound object, if it is playing.
        /// If the sound object is null or cannot stop, nothing happens.
        /// </summary>
        public void Stop()
        {
            if (audioPlayer == null)
                return;

            audioPlayer.Stop();
        }
    }
}
