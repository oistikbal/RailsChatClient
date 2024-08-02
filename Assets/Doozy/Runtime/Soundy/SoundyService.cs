// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections;
using Doozy.Runtime.Common;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Common.Utils;
using Doozy.Runtime.Global;
using Doozy.Runtime.Soundy.Ids;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;
using UnityEngine.Audio;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PartialTypeWithSinglePart

namespace Doozy.Runtime.Soundy
{
    /// <summary>
    /// Specialized class that manages the audio players that play sounds and music and handles the audio players pool.
    /// </summary>
    public partial class SoundyService : SingletonBehaviour<SoundyService>
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Doozy/Soundy/Soundy Service", false, 8)]
        private static void CreateComponent(UnityEditor.MenuCommand menuCommand)
        {
            GameObjectUtils.AddToScene<SoundyService>("Soundy Service", true, true);
        }
        #endif

        /// <summary> Reference to the SoundySettings asset that contains all the settings for the Soundy system </summary>
        public static SoundySettings settings => SoundySettings.instance;

        /// <summary> Coroutine that checks if there are any idle audio players that can be destroyed </summary>
        private Coroutine idleCheckCoroutine { get; set; }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
            soundPlayers.PreheatPool(settings.PreheatSoundPlayers);
            musicPlayers.PreheatPool(settings.PreheatMusicPlayers);
        }

        private void OnEnable()
        {
            if (!settings.DestroyIdleAudioPlayers) return;
            StartIdleCheck();
        }

        private void OnDisable()
        {
            StopIdleCheck();
        }

        /// <summary> Initialize the AudioEngine </summary>
        public static void Initialize()
        {
            instance.soundPlayers.Initialize(instance.soundPlayersTransform);
            instance.musicPlayers.Initialize(instance.musicPlayersTransform);
        }

        /// <summary> Start the coroutine that checks if there are any idle audio players that can be destroyed </summary>
        private void StartIdleCheck()
        {
            StopIdleCheck();
            idleCheckCoroutine = StartCoroutine(IdleCheck());
        }

        /// <summary> Stop the coroutine that checks if there are any idle audio players that can be destroyed </summary>
        private void StopIdleCheck()
        {
            if (idleCheckCoroutine == null) return;
            StopCoroutine(idleCheckCoroutine);
            idleCheckCoroutine = null;
        }

        /// <summary> Check if there are any idle audio players that can be destroyed </summary>
        private IEnumerator IdleCheck()
        {
            var wait = new WaitForSecondsRealtime(settings.IdleCheckInterval < 0 ? 0 : settings.IdleCheckInterval);
            while (settings.DestroyIdleAudioPlayers)
            {
                yield return wait;
                soundPlayers.RemoveNullPlayers();
                soundPlayers.RecycleIdlePlayers();
                soundPlayers.RemoveIdlePlayers(settings.MinSoundPlayersToKeepAlive);

                musicPlayers.RemoveNullPlayers();
                musicPlayers.RecycleIdlePlayers();
                musicPlayers.RemoveIdlePlayers(settings.MinMusicPlayersToKeepAlive);
            }

            idleCheckCoroutine = null;
        }

        #region Sound

        private Transform m_SoundPlayersTransform;
        /// <summary> Transform that holds all the audio players that play sounds </summary>
        private Transform soundPlayersTransform
        {
            get
            {
                if (m_SoundPlayersTransform != null) return m_SoundPlayersTransform;
                m_SoundPlayersTransform = new GameObject("Sound Channels").transform;
                m_SoundPlayersTransform.SetParent(transform);
                return m_SoundPlayersTransform;
            }
        }

        private AudioPlayerPool m_SoundPlayers;
        /// <summary> Pool of audio players that play sounds </summary>
        public AudioPlayerPool soundPlayers => m_SoundPlayers ?? (m_SoundPlayers = new AudioPlayerPool());

        /// <summary>
        /// Get a sound library by its name.
        /// <para/> This method searches for the first SoundLibrary with the given name.
        /// And returns the SoundLibrary reference if found, otherwise returns null.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> SoundLibrary reference if found, otherwise returns null </returns>
        public static SoundLibrary GetSoundLibrary(string libraryName)
        {
            if (libraryName.IsNullOrEmpty()) return null;
            if (libraryName.Equals(SoundySettings.k_DefaultLibraryName)) return null;
            if (libraryName.Equals(SoundySettings.k_None)) return null;

            #if UNITY_EDITOR
            // this Registry is only available in the Editor
            return SoundLibraryRegistry.GetLibrary(libraryName);
            #endif
            
            // the Registry is not available in the build, so we use the Database instead
            #pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return SoundLibraryDatabase.GetLibrary(libraryName);
            #pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        /// Check if a sound library with a given name exists.
        /// This will return false if the library name is null or empty, or if the library name is the default library name or the none library name.
        /// Does not check for multiple libraries with the same name.
        /// Does not check for music libraries.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> True if the library exists, false otherwise </returns>
        public static bool SoundLibraryExists(string libraryName) =>
            GetSoundLibrary(libraryName) != null;

        /// <summary>
        /// Get a sound object by its name and its library name.
        /// <para/> This method searches for the first SoundLibrary with the given name.
        /// Then it searches for the SoundObject with the given name in that library.
        /// And returns the SoundObject reference if found, otherwise returns null.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        /// <returns> SoundObject reference if found, otherwise returns null </returns>
        public static SoundObject GetSoundObject(string libraryName, string soundName)
        {
            SoundLibrary library = GetSoundLibrary(libraryName);
            return library == null ? null : library.GetSoundObject(soundName);
        }

        /// <summary>
        /// Check if a sound object with a given name exists in a given library.
        /// This will return false if the library name is null or empty, or if the library name is the default library name or the none library name.
        /// Does not check for multiple libraries with the same name.
        /// Does not check for music libraries.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        /// <returns> True if the sound object exists, false otherwise </returns>
        public static bool SoundObjectExists(string libraryName, string soundName) =>
            GetSoundObject(libraryName, soundName) != null;

        /// <summary>
        /// Get an audio player used for playing sounds.
        /// This method will return an audio player from the pool of AudioPlayers that play sounds.
        /// If no audio player is available, a new one will be created and returned.
        /// <para/> Note that the audio player pool for sounds is separate from the audio player pool for music.
        /// </summary>
        /// <returns> An audio player used for playing sounds </returns>
        public static AudioPlayer GetSoundPlayer() =>
            instance == null
                ? null
                : instance.soundPlayers.GetAudioPlayer();

        #region Play Sounds

        /// <summary>
        /// Play a sound from a given library and with a given name and return the audio player that is playing it.
        /// You can also specify an output audio mixer group for the audio player (if null, the audio mixer group set in the library will be used).
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        /// <param name="outputAudioMixerGroup"> Output audio mixer group </param>
        /// <returns> Audio player that is playing the sound. If the sound could not be played, returns null </returns>
        public static AudioPlayer PlaySound(string libraryName, string soundName, AudioMixerGroup outputAudioMixerGroup = null)
        {
            var soundLibrary = GetSoundLibrary(libraryName);
            if (soundLibrary == null) return null;
            var soundObject = soundLibrary.GetSoundObject(soundName);
            if (soundObject == null) return null;
            outputAudioMixerGroup ??= soundLibrary.OutputAudioMixerGroup;
            var player = GetSoundPlayer();
            if (player == null) return null;

            player
                .SetSound(soundObject, outputAudioMixerGroup)
                .Play();

            return player;
        }

        /// <summary>
        /// Play a sound from a given library and with a given name and return the audio player that is playing it.
        /// You can also specify an output audio mixer group for the audio player (if null, the audio mixer group set in the library will be used).
        /// </summary>
        /// <param name="soundObject"> Sound object </param>
        /// <param name="outputAudioMixerGroup"> Output audio mixer group </param>
        /// <returns> Audio player that is playing the sound. If the sound could not be played, returns null </returns>
        public static AudioPlayer PlaySound(SoundObject soundObject, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (soundObject == null) return null;
            if (soundObject.library == null) return null;
            outputAudioMixerGroup ??= soundObject.library.OutputAudioMixerGroup;
            var player = GetSoundPlayer();
            if (player == null) return null;

            player
                .SetSound(soundObject, outputAudioMixerGroup)
                .Play();

            return player;
        }

        /// <summary>
        /// Play a sound from a given library and with a given name and return the audio player that is playing it.
        /// You can also specify an output audio mixer group for the audio player (if null, the audio mixer group set in the library will be used).
        /// </summary>
        /// <param name="soundId"> Sound id </param>
        /// <param name="outputAudioMixerGroup"> Output audio mixer group </param>
        /// <returns> Audio player that is playing the sound. If the sound could not be played, returns null </returns>
        public static AudioPlayer PlaySound(SoundId soundId, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (soundId == null || !soundId.isValid) return null;
            var soundObject = soundId.GetSoundObject();
            if (soundObject == null) return null;
            if (soundObject.library == null) return null;
            outputAudioMixerGroup ??= soundObject.library.OutputAudioMixerGroup;
            var player = GetSoundPlayer();
            if (player == null) return null;

            player
                .SetSound(soundObject, outputAudioMixerGroup)
                .Play();

            return player;
        }

        #endregion // Play Sounds

        #region Stop Sounds

        #region Single Sound Stop

        /// <summary> Stop all sounds that are currently playing (including looping sounds) the given sound object. </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        public static void StopSound(SoundObject soundObject)
        {
            if (soundObject == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject != soundObject) continue;
                player.Stop();
            }
        }

        /// <summary> Stop all sounds that are currently playing (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void StopSound(string libraryName, string soundName) =>
            StopSound(GetSoundObject(libraryName, soundName));

        /// <summary> Stop all sounds that are currently playing (including looping sounds) the given sound id. </summary>
        /// <param name="soundId"> SoundId reference </param>
        public static void StopSound(SoundId soundId)
        {
            if (soundId == null || !soundId.isValid) return;
            StopSound(soundId.GetSoundObject());
        }

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds) the given sound object. </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        public static void FadeOutAndStopSound(SoundObject soundObject)
        {
            if (soundObject == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject != soundObject) continue;
                player.FadeOutAndStop();
            }
        }

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void FadeOutAndStopSound(string libraryName, string soundName) =>
            FadeOutAndStopSound(GetSoundObject(libraryName, soundName));

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds) the given sound id. </summary>
        /// <param name="soundId"> SoundId reference </param>
        public static void FadeOutAndStopSound(SoundId soundId)
        {
            if (soundId == null || !soundId.isValid) return;
            FadeOutAndStopSound(soundId.GetSoundObject());
        }

        #endregion // Single Sound Stop

        #region Sound Library Stop

        /// <summary> Stop all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibrary"> Sound library </param>
        public static void StopSoundLibrary(SoundLibrary soundLibrary)
        {
            if (soundLibrary == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject.library != soundLibrary) continue;
                player.Stop();
            }
        }

        /// <summary> Stop all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void StopSoundLibrary(string libraryName) =>
            StopSoundLibrary(GetSoundLibrary(libraryName));

        /// <summary> Stop all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibraryId"> Sound library id </param>
        public static void StopSoundLibrary(SoundLibraryId soundLibraryId)
        {
            if (soundLibraryId == null) return;
            StopSoundLibrary(soundLibraryId.GetSoundLibrary());
        }

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibrary"> Sound library </param>
        public static void FadeOutAndStopSoundLibrary(SoundLibrary soundLibrary)
        {
            if (soundLibrary == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject.library != soundLibrary) continue;
                player.FadeOutAndStop();
            }
        }

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void FadeOutAndStopSoundLibrary(string libraryName) =>
            FadeOutAndStopSoundLibrary(GetSoundLibrary(libraryName));

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="soundLibraryId"> Sound library id </param>
        public static void FadeOutAndStopSoundLibrary(SoundLibraryId soundLibraryId)
        {
            if (soundLibraryId == null) return;
            FadeOutAndStopSoundLibrary(soundLibraryId.GetSoundLibrary());
        }

        #endregion // Sound Library Stop

        #region All Sounds Stop

        /// <summary> Stop all sounds that are currently playing (including looping sounds). </summary>
        public static void StopAllSounds()
        {
            if (instance == null) return;
            instance.soundPlayers.StopAllActivePlayers();
        }

        /// <summary> Fade out and stop all sounds that are currently playing (including looping sounds). </summary>
        public static void FadeOutAndStopAllSounds()
        {
            if (instance == null) return;
            instance.soundPlayers.FadeOutAndStopAllActivePlayers();
        }

        #endregion // All Sounds Stop

        #endregion // Stop Sounds

        #region Pause Sounds

        #region Single Sound Pause

        /// <summary> Pause all sounds that are currently playing (including looping sounds) the given sound object. </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        public static void PauseSound(SoundObject soundObject)
        {
            if (soundObject == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject != soundObject) continue;
                player.Pause();
            }
        }

        /// <summary> Pause all sounds that are currently playing (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void PauseSound(string libraryName, string soundName) =>
            PauseSound(GetSoundObject(libraryName, soundName));

        /// <summary> Pause all sounds that are currently playing (including looping sounds) the given sound id. </summary>
        /// <param name="soundId"> SoundId reference </param>
        public static void PauseSound(SoundId soundId)
        {
            if (soundId == null || !soundId.isValid) return;
            PauseSound(soundId.GetSoundObject());
        }

        /// <summary> Unpause all sounds that are currently paused (including looping sounds) the given sound object. </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        public static void UnPauseSound(SoundObject soundObject)
        {
            if (soundObject == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject != soundObject) continue;
                player.UnPause();
            }
        }

        /// <summary> Unpause all sounds that are currently paused (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void UnPauseSound(string libraryName, string soundName) =>
            UnPauseSound(GetSoundObject(libraryName, soundName));

        /// <summary> Unpause all sounds that are currently paused (including looping sounds) the given sound id. </summary>
        /// <param name="soundId"> SoundId reference </param>
        public static void UnPauseSound(SoundId soundId)
        {
            if (soundId == null || !soundId.isValid) return;
            UnPauseSound(soundId.GetSoundObject());
        }

        #endregion // Single Sound Pause

        #region Sound Library Pause

        /// <summary> Pause all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibrary"> Sound library </param>
        public static void PauseSoundLibrary(SoundLibrary soundLibrary)
        {
            if (soundLibrary == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject.library != soundLibrary) continue;
                player.Pause();
            }
        }

        /// <summary> Pause all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void PauseSoundLibrary(string libraryName) =>
            PauseSoundLibrary(GetSoundLibrary(libraryName));

        /// <summary> Pause all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibraryId"> Sound library id </param>
        public static void PauseSoundLibrary(SoundLibraryId soundLibraryId)
        {
            if (soundLibraryId == null) return;
            PauseSoundLibrary(soundLibraryId.GetSoundLibrary());
        }

        /// <summary> Unpause all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibrary"> Sound library </param>
        public static void UnPauseSoundLibrary(SoundLibrary soundLibrary)
        {
            if (soundLibrary == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject.library != soundLibrary) continue;
                player.UnPause();
            }
        }

        /// <summary> Unpause all sounds that are currently playing (including looping sounds) and are paused from a given library. </summary>
        /// <param name="libraryName"></param>
        public static void UnPauseSoundLibrary(string libraryName) =>
            UnPauseSoundLibrary(GetSoundLibrary(libraryName));

        /// <summary> Unpause all sounds that are currently playing (including looping sounds) and are paused from a given library. </summary>
        /// <param name="soundLibraryId"> Sound library id </param>
        public static void UnPauseSoundLibrary(SoundLibraryId soundLibraryId)
        {
            if (soundLibraryId == null) return;
            UnPauseSoundLibrary(soundLibraryId.GetSoundLibrary());
        }

        #endregion // Sound Library Pause

        #region All Sounds Pause

        /// <summary> Pause all sounds that are currently playing (including looping sounds). </summary>
        public static void PauseAllSounds()
        {
            if (instance == null) return;
            instance.soundPlayers.PauseAllActivePlayers();
        }

        /// <summary> Unpause all sounds that are currently playing (including looping sounds) and are paused. </summary>
        public static void UnPauseAllSounds()
        {
            if (instance == null) return;
            instance.soundPlayers.UnPauseAllActivePlayers();
        }

        #endregion // All Sounds Pause

        #endregion // Pause Sounds

        #region Mute Sounds

        #region Single Sound Mute

        /// <summary> Mute all sounds that are currently playing (including looping sounds) the given sound object. </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        public static void MuteSound(SoundObject soundObject)
        {
            if (soundObject == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject != soundObject) continue;
                player.Mute();
            }
        }

        /// <summary> Mute all sounds that are currently playing (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void MuteSound(string libraryName, string soundName) =>
            MuteSound(GetSoundObject(libraryName, soundName));

        /// <summary> Mute all sounds that are currently playing (including looping sounds) the given sound id. </summary>
        /// <param name="soundId"> SoundId reference </param>
        public static void MuteSound(SoundId soundId)
        {
            if (soundId == null || !soundId.isValid) return;
            MuteSound(soundId.GetSoundObject());
        }

        /// <summary> Unmute all sounds that are currently muted (including looping sounds) the given sound object. </summary>
        /// <param name="soundObject"> SoundObject reference </param>
        public static void UnMuteSound(SoundObject soundObject)
        {
            if (soundObject == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject != soundObject) continue;
                player.UnMute();
            }
        }

        /// <summary> Unmute all sounds that are currently muted (including looping sounds) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void UnMuteSound(string libraryName, string soundName) =>
            UnMuteSound(GetSoundObject(libraryName, soundName));

        /// <summary> Unmute all sounds that are currently muted (including looping sounds) the given sound id. </summary>
        /// <param name="soundId"> SoundId reference </param>
        public static void UnMuteSound(SoundId soundId)
        {
            if (soundId == null || !soundId.isValid) return;
            UnMuteSound(soundId.GetSoundObject());
        }

        #endregion // Single Sound Mute

        #region Sound Library Mute

        /// <summary> Mute all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibrary"> Sound library </param>
        public static void MuteSoundLibrary(SoundLibrary soundLibrary)
        {
            if (soundLibrary == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject.library != soundLibrary) continue;
                player.Mute();
            }
        }

        /// <summary> Mute all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void MuteSoundLibrary(string libraryName) =>
            MuteSoundLibrary(GetSoundLibrary(libraryName));

        /// <summary> Mute all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibraryId"> Sound library id </param>
        public static void MuteSoundLibrary(SoundLibraryId soundLibraryId)
        {
            if (soundLibraryId == null) return;
            MuteSoundLibrary(soundLibraryId.GetSoundLibrary());
        }

        /// <summary> Unmute all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibrary"> Sound library </param>
        public static void UnMuteSoundLibrary(SoundLibrary soundLibrary)
        {
            if (soundLibrary == null) return;
            foreach (AudioPlayer player in instance.soundPlayers.activePlayers)
            {
                if (!player.isPlayingSound) continue;
                if (player.loadedSoundObject.library != soundLibrary) continue;
                player.UnMute();
            }
        }

        /// <summary> Unmute all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void UnMuteSoundLibrary(string libraryName) =>
            UnMuteSoundLibrary(GetSoundLibrary(libraryName));

        /// <summary> Unmute all sounds that are currently playing (including looping sounds) from a given library. </summary>
        /// <param name="soundLibraryId"> Sound library id </param>
        public static void UnMuteSoundLibrary(SoundLibraryId soundLibraryId)
        {
            if (soundLibraryId == null) return;
            UnMuteSoundLibrary(soundLibraryId.GetSoundLibrary());
        }

        #endregion // Sound Library Mute

        #region All Sounds Mute

        /// <summary> Mute all sounds that are currently playing (including looping sounds). </summary>
        public static void MuteAllSounds()
        {
            if (instance == null) return;
            instance.soundPlayers.MuteAllActivePlayers();
        }

        /// <summary> Unmute all sounds that are currently playing (including looping sounds). </summary>
        public static void UnMuteAllSounds()
        {
            if (instance == null) return;
            instance.soundPlayers.UnmuteAllActivePlayers();
        }

        #endregion // All Sounds Mute

        #endregion // Mute Sounds

        #endregion

        #region Music

        private Transform m_MusicPlayersTransform;
        /// <summary> Transform that holds all the audio players that play music </summary>
        private Transform musicPlayersTransform
        {
            get
            {
                if (m_MusicPlayersTransform != null) return m_MusicPlayersTransform;
                m_MusicPlayersTransform = new GameObject("Music Channels").transform;
                m_MusicPlayersTransform.SetParent(transform);
                return m_MusicPlayersTransform;
            }
        }

        private AudioPlayerPool m_MusicPlayers;
        /// <summary> Pool of audio players that play music </summary>
        public AudioPlayerPool musicPlayers => m_MusicPlayers ?? (m_MusicPlayers = new AudioPlayerPool());

        /// <summary>
        /// Get a music library by its name.
        /// <para/> This method searches for the first MusicLibrary with the given name.
        /// And returns the MusicLibrary reference if found, otherwise returns null.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> MusicLibrary reference if found, otherwise returns null </returns>
        public static MusicLibrary GetMusicLibrary(string libraryName)
        {
            if (libraryName.IsNullOrEmpty()) return null;
            if (libraryName.Equals(SoundySettings.k_DefaultLibraryName)) return null;
            if (libraryName.Equals(SoundySettings.k_None)) return null;

            #if UNITY_EDITOR
            // this Registry is only available in the Editor
            return MusicLibraryRegistry.GetLibrary(libraryName);
            #endif
            
            // the Registry is not available in the build, so we use the Database instead
            #pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return MusicLibraryDatabase.GetLibrary(libraryName);
            #pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        /// Check if a music library with a given name exists.
        /// This will return false if the library name is null or empty, or if the library name is the default library name or the none library name.
        /// Does not check for multiple libraries with the same name.
        /// Does not check for sound libraries.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> True if the library exists, false otherwise </returns>
        public static bool MusicLibraryExists(string libraryName) =>
            GetMusicLibrary(libraryName) != null;

        /// <summary>
        /// Get a music object by its name and its library name.
        /// <para/> This method searches for the first MusicLibrary with the given name.
        /// Then it searches for the MusicObject with the given name in that library.
        /// And returns the MusicObject reference if found, otherwise returns null.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <returns> MusicObject reference if found, otherwise returns null </returns>
        public static MusicObject GetMusicObject(string libraryName, string musicName)
        {
            MusicLibrary library = GetMusicLibrary(libraryName);
            return library == null ? null : library.GetMusicObject(musicName);
        }

        /// <summary>
        /// Check if a music object with a given name exists in a given library.
        /// This will return false if the library name is null or empty, or if the library name is the default library name or the none library name.
        /// Does not check for multiple libraries with the same name.
        /// Does not check for sound libraries.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <returns> True if the music object exists, false otherwise </returns>
        public static bool MusicObjectExists(string libraryName, string musicName) =>
            GetMusicObject(libraryName, musicName) != null;

        /// <summary>
        /// Get an audio player used for playing music.
        /// This method will return an audio player from the pool of AudioPlayers that play music.
        /// If no audio player is available, a new one will be created and returned.
        /// <para/> Note that the audio player pool for music is separate from the audio player pool for sounds.
        /// </summary>
        /// <returns> An audio player used for playing music </returns>
        public static AudioPlayer GetMusicPlayer() =>
            instance == null
                ? null
                : SoundyService.instance.musicPlayers.GetAudioPlayer();

        #region Play Music

        /// <summary>
        /// Play a music from a given library and with a given name and return the audio player that is playing it.
        /// You can also specify an output audio mixer group for the audio player (if null, the audio mixer group set in the library will be used).
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <param name="outputAudioMixerGroup"> Output audio mixer group </param>
        /// <returns> Audio player that is playing the music. If the music could not be played, returns null </returns>
        public static AudioPlayer PlayMusic(string libraryName, string musicName, AudioMixerGroup outputAudioMixerGroup = null)
        {
            var musicLibrary = GetMusicLibrary(libraryName);
            if (musicLibrary == null) return null;
            var musicObject = musicLibrary.GetMusicObject(musicName);
            if (musicObject == null) return null;
            outputAudioMixerGroup ??= musicLibrary.OutputAudioMixerGroup;
            var player = GetMusicPlayer();
            if (player == null) return null;

            player
                .SetMusic(musicObject, outputAudioMixerGroup)
                .Play();

            return player;
        }

        /// <summary>
        /// Play a music from a given library and with a given name and return the audio player that is playing it.
        /// You can also specify an output audio mixer group for the audio player (if null, the audio mixer group set in the library will be used).
        /// </summary>
        /// <param name="musicObject"> Music object </param>
        /// <param name="outputAudioMixerGroup"> Output audio mixer group </param>
        /// <returns> Audio player that is playing the music. If the music could not be played, returns null </returns>
        public static AudioPlayer PlayMusic(MusicObject musicObject, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (musicObject == null) return null;
            if (musicObject.library == null) return null;
            outputAudioMixerGroup ??= musicObject.library.OutputAudioMixerGroup;
            var player = GetMusicPlayer();
            if (player == null) return null;

            player
                .SetMusic(musicObject, outputAudioMixerGroup)
                .Play();

            return player;
        }

        /// <summary>
        /// Play a music from a given library and with a given name and return the audio player that is playing it.
        /// You can also specify an output audio mixer group for the audio player (if null, the audio mixer group set in the library will be used).
        /// </summary>
        /// <param name="musicId"> Music id </param>
        /// <param name="outputAudioMixerGroup"> Output audio mixer group </param>
        /// <returns> Audio player that is playing the sound. If the sound could not be played, returns null </returns>
        public static AudioPlayer PlayMusic(MusicId musicId, AudioMixerGroup outputAudioMixerGroup = null)
        {
            if (musicId == null || !musicId.isValid) return null;
            var musicObject = musicId.GetMusicObject();
            if (musicObject == null) return null;
            if (musicObject.library == null) return null;
            outputAudioMixerGroup ??= musicObject.library.OutputAudioMixerGroup;
            var player = GetMusicPlayer();
            if (player == null) return null;

            player
                .SetMusic(musicObject, outputAudioMixerGroup)
                .Play();

            return player;
        }

        #endregion // Play Music

        #region Stop Music

        #region Single Music Stop

        /// <summary>
        /// Stop all music that is currently playing (including looping music) the given music object.
        /// This will not stop music that is scheduled to be played in the future.
        /// </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        public static void StopMusic(MusicObject musicObject)
        {
            if (musicObject == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject != musicObject) continue;
                player.Stop();
            }
        }

        /// <summary>
        /// Stop all music that is currently playing (including looping music) from a given library and with a given name.
        /// This will not stop music that is scheduled to be played in the future.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void StopMusic(string libraryName, string soundName) =>
            StopMusic(GetMusicObject(libraryName, soundName));

        /// <summary>
        /// Stop all music that is currently playing (including looping music) the given music id.
        /// This will not stop music that is scheduled to be played in the future.
        /// </summary>
        /// <param name="musicId"> MusicId reference </param>
        public static void StopMusic(MusicId musicId)
        {
            if (musicId == null || !musicId.isValid) return;
            StopMusic(musicId.GetMusicObject());
        }

        /// <summary> Stop all music that is currently playing (including looping music) from a given library and with a given name. </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        public static void FadeOutAndStopMusic(MusicObject musicObject)
        {
            if (musicObject == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject != musicObject) continue;
                player.FadeOutAndStop();
            }
        }

        /// <summary> Stop all music that is currently playing (including looping music) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void FadeOutAndStopMusic(string libraryName, string soundName) =>
            FadeOutAndStopMusic(GetMusicObject(libraryName, soundName));

        /// <summary> Stop all music that is currently playing (including looping music) the given music id. </summary>
        /// <param name="musicId"> MusicId reference </param>
        public static void FadeOutAndStopMusic(MusicId musicId)
        {
            if (musicId == null || !musicId.isValid) return;
            FadeOutAndStopMusic(musicId.GetMusicObject());
        }

        #endregion // Single Music Stop

        #region Music Library Stop

        /// <summary> Stop all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibrary"> Music library </param>
        public static void StopMusicLibrary(MusicLibrary musicLibrary)
        {
            if (musicLibrary == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject.library == musicLibrary) continue;
                player.Stop();
            }
        }

        /// <summary> Stop all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void StopMusicLibrary(string libraryName) =>
            StopMusicLibrary(GetMusicLibrary(libraryName));

        /// <summary> Stop all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibraryId"> Music library id </param>
        public static void StopMusicLibrary(MusicLibraryId musicLibraryId)
        {
            if (musicLibraryId == null) return;
            StopMusicLibrary(musicLibraryId.GetMusicLibrary());
        }

        /// <summary> Fade out and stop all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibrary"> Music library </param>
        public static void FadeOutAndStopMusicLibrary(MusicLibrary musicLibrary)
        {
            if (musicLibrary == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject.library == musicLibrary) continue;
                player.FadeOutAndStop();
            }
        }

        /// <summary> Fade out and stop all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void FadeOutAndStopMusicLibrary(string libraryName) =>
            StopMusicLibrary(GetMusicLibrary(libraryName));

        /// <summary> Fade out and stop all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibraryId"> Music library id </param>
        public static void FadeOutAndStopMusicLibrary(MusicLibraryId musicLibraryId)
        {
            if (musicLibraryId == null) return;
            FadeOutAndStopMusicLibrary(musicLibraryId.GetMusicLibrary());
        }

        #endregion

        #region All Music Stop

        /// <summary> Stop all music that is currently playing (including looping music). </summary>
        public static void StopAllMusic()
        {
            if (instance == null) return;
            instance.musicPlayers.StopAllActivePlayers();
        }

        /// <summary> Fade out and stop all music that is currently playing (including looping music). </summary>
        public static void FadeOutAndStopAllMusic()
        {
            if (instance == null) return;
            instance.musicPlayers.FadeOutAndStopAllActivePlayers();
        }

        #endregion // All Music Stop

        #endregion // Stop Music

        #region Pause Music

        #region Single Music Pause

        /// <summary> Pause all music that is currently playing (including looping music) the given music object. </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        public static void PauseMusic(MusicObject musicObject)
        {
            if (musicObject == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject != musicObject) continue;
                player.Pause();
            }
        }

        /// <summary> Pause all music that is currently playing (including looping music) the given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void PauseMusic(string libraryName, string soundName) =>
            PauseMusic(GetMusicObject(libraryName, soundName));

        /// <summary> Pause all music that is currently playing (including looping music) the given music id. </summary>
        /// <param name="musicId"> MusicId reference </param>
        public static void PauseMusic(MusicId musicId)
        {
            if (musicId == null || !musicId.isValid) return;
            PauseMusic(musicId.GetMusicObject());
        }

        /// <summary> Unpause all music that is currently playing (including looping music) the given music object. </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        public static void UnPauseMusic(MusicObject musicObject)
        {
            if (musicObject == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject != musicObject) continue;
                player.UnPause();
            }
        }

        /// <summary> Unpause all music that is currently playing (including looping music) the given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void UnPauseMusic(string libraryName, string soundName) =>
            UnPauseMusic(GetMusicObject(libraryName, soundName));

        /// <summary> Unpause all music that is currently playing (including looping music) the given music id. </summary>
        /// <param name="musicId"> MusicId reference </param>
        public static void UnPauseMusic(MusicId musicId)
        {
            if (musicId == null || !musicId.isValid) return;
            UnPauseMusic(musicId.GetMusicObject());
        }

        #endregion // Single Music Pause

        #region Music Library Pause

        /// <summary> Pause all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibrary"> Music library </param>
        public static void PauseMusicLibrary(MusicLibrary musicLibrary)
        {
            if (musicLibrary == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject.library != musicLibrary) continue;
                player.Pause();
            }
        }

        /// <summary> Pause all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void PauseMusicLibrary(string libraryName) =>
            PauseMusicLibrary(GetMusicLibrary(libraryName));

        /// <summary> Pause all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibraryId"> Music library id </param>
        public static void PauseMusicLibrary(MusicLibraryId musicLibraryId)
        {
            if (musicLibraryId == null) return;
            PauseMusicLibrary(musicLibraryId.GetMusicLibrary());
        }

        /// <summary> Unpause all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibrary"> Music library </param>
        public static void UnPauseMusicLibrary(MusicLibrary musicLibrary)
        {
            if (musicLibrary == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject.library != musicLibrary) continue;
                player.UnPause();
            }
        }

        /// <summary> Unpause all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void UnPauseMusicLibrary(string libraryName) =>
            UnPauseMusicLibrary(GetMusicLibrary(libraryName));

        /// <summary> Unpause all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibraryId"> Music library id </param>
        public static void UnPauseMusicLibrary(MusicLibraryId musicLibraryId)
        {
            if (musicLibraryId == null) return;
            UnPauseMusicLibrary(musicLibraryId.GetMusicLibrary());
        }

        #endregion // Music Library Pause

        #region All Music Pause

        /// <summary> Pause all music that is currently playing (including looping music). </summary>
        public static void PauseAllMusic()
        {
            if (instance == null) return;
            instance.musicPlayers.PauseAllActivePlayers();
        }

        /// <summary> Unpause all music that is currently playing (including looping music) and are paused. </summary>
        public static void UnPauseAllMusic()
        {
            if (instance == null) return;
            instance.musicPlayers.UnPauseAllActivePlayers();
        }

        #endregion // All Music Pause

        #endregion // Pause Music

        #region Mute Music

        #region Single Music Mute

        /// <summary> Mute all music that is currently playing (including looping music) the given music object. </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        public static void MuteMusic(MusicObject musicObject)
        {
            if (musicObject == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject != musicObject) continue;
                player.Mute();
            }
        }

        /// <summary> Mute all music that is currently playing (including looping music) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void MuteMusic(string libraryName, string soundName) =>
            MuteMusic(GetMusicObject(libraryName, soundName));

        /// <summary> Mute all music that is currently playing (including looping music) the given music id. </summary>
        /// <param name="musicId"> MusicId reference </param>
        public static void MuteMusic(MusicId musicId)
        {
            if (musicId == null || !musicId.isValid) return;
            MuteMusic(musicId.GetMusicObject());
        }

        /// <summary> Unmute all music that is currently playing (including looping music) the given music object. </summary>
        /// <param name="musicObject"> MusicObject reference </param>
        public static void UnMuteMusic(MusicObject musicObject)
        {
            if (musicObject == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject != musicObject) continue;
                player.UnMute();
            }
        }

        /// <summary> Unmute all music that is currently playing (including looping music) from a given library and with a given name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        public static void UnMuteMusic(string libraryName, string soundName) =>
            UnMuteMusic(GetMusicObject(libraryName, soundName));

        /// <summary> Unmute all music that is currently playing (including looping music) the given music id. </summary>
        /// <param name="musicId"> MusicId reference </param>
        public static void UnMuteMusic(MusicId musicId)
        {
            if (musicId == null || !musicId.isValid) return;
            UnMuteMusic(musicId.GetMusicObject());
        }

        #endregion

        #region Music Library Mute

        /// <summary> Mute all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibrary"> Music library </param>
        public static void MuteMusicLibrary(MusicLibrary musicLibrary)
        {
            if (musicLibrary == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject.library != musicLibrary) continue;
                player.Mute();
            }
        }

        /// <summary> Mute all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void MuteMusicLibrary(string libraryName) =>
            MuteMusicLibrary(GetMusicLibrary(libraryName));

        /// <summary> Mute all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibraryId"> Music library id </param>
        public static void MuteMusicLibrary(MusicLibraryId musicLibraryId)
        {
            if (musicLibraryId == null) return;
            MuteMusicLibrary(musicLibraryId.GetMusicLibrary());
        }

        /// <summary> Unmute all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibrary"> Music library </param>
        public static void UnMuteMusicLibrary(MusicLibrary musicLibrary)
        {
            if (musicLibrary == null) return;
            foreach (AudioPlayer player in instance.musicPlayers.activePlayers)
            {
                if (!player.isPlayingMusic) continue;
                if (player.loadedMusicObject.library != musicLibrary) continue;
                player.UnMute();
            }
        }

        /// <summary> Unmute all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="libraryName"> Library name </param>
        public static void UnMuteMusicLibrary(string libraryName) =>
            UnMuteMusicLibrary(GetMusicLibrary(libraryName));

        /// <summary> Unmute all music that is currently playing (including looping music) from a given library. </summary>
        /// <param name="musicLibraryId"> Music library id </param>
        public static void UnMuteMusicLibrary(MusicLibraryId musicLibraryId)
        {
            if (musicLibraryId == null) return;
            UnMuteMusicLibrary(musicLibraryId.GetMusicLibrary());
        }

        #endregion // Music Library Mute

        #region All Music Mute

        /// <summary> Mute all music that is currently playing (including looping music). </summary>
        public static void MuteAllMusic()
        {
            if (instance == null) return;
            instance.musicPlayers.MuteAllActivePlayers();
        }

        /// <summary> Unmute all music that is currently playing (including looping music). </summary>
        public static void UnMuteAllMusic()
        {
            if (instance == null) return;
            instance.musicPlayers.UnmuteAllActivePlayers();
        }

        #endregion // All Music Mute

        #endregion // Mute Music

        #endregion // Music

        #region All Audio Methods (Sounds and Music)

        /// <summary>
        /// Stop all sounds and music that are currently playing (including looping sounds and music).
        /// This will not stop sounds and music that are scheduled to be played in the future.
        /// </summary>
        public static void StopAll()
        {
            StopAllSounds();
            StopAllMusic();
        }

        /// <summary>
        /// Mute all sounds and music that are currently playing (including looping sounds and music).
        /// This will not mute sounds and music that are scheduled to be played in the future.
        /// </summary>
        public static void MuteAll()
        {
            MuteAllSounds();
            MuteAllMusic();
        }

        /// <summary>
        /// Unmute all sounds and music that are currently playing (including looping sounds and music).
        /// This will not unmute sounds and music that are scheduled to be played in the future.
        /// </summary>
        public static void UnMuteAll()
        {
            UnMuteAllSounds();
            UnMuteAllMusic();
        }

        /// <summary>
        /// Pause all sounds and music that are currently playing (including looping sounds and music).
        /// This will not pause sounds and music that are scheduled to be played in the future.
        /// </summary>
        public static void PauseAll()
        {
            PauseAllSounds();
            PauseAllMusic();
        }

        /// <summary>
        /// Unpause all sounds and music that are currently playing (including looping sounds and music).
        /// This will not unpause sounds and music that are scheduled to be played in the future.
        /// </summary>
        public static void UnPauseAll()
        {
            UnPauseAllSounds();
            UnPauseAllMusic();
        }
        
        #endregion // All Audio Methods (Sounds and Music)

        #region Idle Players Removal (Sounds and Music)

        /// <summary> Remove all idle audio players that are not in use and are used for playing sounds. </summary>
        /// <param name="minPlayersToKeepAlive"> Minimum number of players to keep alive </param>
        public static void RemoveIdleSoundPlayers(int minPlayersToKeepAlive)
        {
            if (instance == null) return;
            instance.soundPlayers.RemoveIdlePlayers(minPlayersToKeepAlive);
        }

        /// <summary> Remove all idle audio players that are not in use and are used for playing music. </summary>
        /// <param name="minPlayersToKeepAlive"> Minimum number of players to keep alive </param>
        public static void RemoveIdleMusicPlayers(int minPlayersToKeepAlive)
        {
            if (instance == null) return;
            instance.musicPlayers.RemoveIdlePlayers(minPlayersToKeepAlive);
        }

        /// <summary> Remove all idle audio players that are not in use. </summary>
        /// <param name="minSoundPlayersToKeepAlive"> Minimum number of sound players to keep alive </param>
        /// <param name="minMusicPlayersToKeepAlive"> Minimum number of music players to keep alive </param>
        public static void RemoveIdlePlayers(int minSoundPlayersToKeepAlive, int minMusicPlayersToKeepAlive)
        {
            RemoveIdleSoundPlayers(minSoundPlayersToKeepAlive);
            RemoveIdleMusicPlayers(minMusicPlayersToKeepAlive);
        }

        #endregion // Idle Players Removal (Sounds and Music)

        // class AudioPlayerPool

    } // class SoundyService

} // namespace Doozy.Runtime.Soundy
