// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.Common.Utils;
using Doozy.Runtime.Soundy.Ids;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.Soundy
{
    /// <summary>
    /// Audio component that can play a one or more Soundy music tracks by using a MusicPlaylist that contains one or more MusicIds.
    /// It uses AudioPlayers to play the music..
    /// </summary>
    [AddComponentMenu("Doozy/Soundy/Music Player")]
    public class MusicPlayer : MonoBehaviour
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Doozy/Soundy/Music Player", false, 8)]
        private static void CreateComponent(UnityEditor.MenuCommand menuCommand)
        {
            GameObjectUtils.AddToScene<MusicPlayer>("Music Player", false, true);
        }
        #endif

        /// <summary>
        /// Create a new MusicPlayer GameObject and add a MusicPlayer component to it.
        /// </summary>
        /// <returns> Newly created MusicPlayer component </returns>
        public static MusicPlayer Get() =>
            new GameObject("Music Player").AddComponent<MusicPlayer>();
        
        [SerializeField] private bool DontDestroyOnSceneChange;
        /// <summary>
        /// Don't destroy this game object when loading a new scene.
        /// This is useful for music players that need to persist between scene changes.
        /// </summary>
        public bool dontDestroyOnSceneChange
        {
            get => DontDestroyOnSceneChange;
            set => DontDestroyOnSceneChange = value;
        }

        [SerializeField] private MusicPlaylist Playlist;
        /// <summary> Music Playlist that this MusicPlayer is referencing </summary>
        public MusicPlaylist playlist => Playlist ?? (Playlist = new MusicPlaylist());

        [SerializeField] private bool AutoAdvance = true;
        /// <summary> Automatically advance to the next music track when the current one finishes playing </summary>
        public bool autoAdvance
        {
            get => AutoAdvance;
            set => AutoAdvance = value;
        }

        [SerializeField] private bool CrossFade = true;
        /// <summary>
        /// Cross fade between music tracks when auto advance is enabled and fade in and fade out durations are greater than 0.
        /// <para/> Note: Cross fading is not supported when the music player is playing a single music track
        /// </summary>
        public bool crossFade
        {
            get => CrossFade;
            set => CrossFade = value;
        }

        [SerializeField] private float CrossFadeDuration = SoundySettings.k_DefaultCrossFadeDuration;
        /// <summary> Cross fade duration (in seconds) </summary>
        public float crossFadeDuration
        {
            get => CrossFadeDuration;
            set => CrossFadeDuration = Mathf.Max(0, value);
        }

        /// <summary>
        /// Check if the music player has cross fade enabled and the cross fade duration is greater than 0.
        /// </summary>
        public bool hasCrossFade => crossFade && crossFadeDuration > 0;

        [SerializeField] private bool PlayOnStart = false;
        /// <summary> Play the music on Start </summary>
        public bool playOnStart
        {
            get => PlayOnStart;
            set => PlayOnStart = value;
        }

        [SerializeField] private bool PlayOnEnable = false;
        /// <summary> Play the music on Enable </summary>
        public bool playOnEnable
        {
            get => PlayOnEnable;
            set => PlayOnEnable = value;
        }

        [SerializeField] private bool PlayOnDisable = false;
        /// <summary> Play the music on Disable </summary>
        public bool playOnDisable
        {
            get => PlayOnDisable;
            set => PlayOnDisable = value;
        }

        [SerializeField] private bool StopOnDisable = true;
        /// <summary> Stop the music on Disable </summary>
        public bool stopOnDisable
        {
            get => StopOnDisable;
            set => StopOnDisable = value;
        }

        [SerializeField] private bool StopOnDestroy = true;
        /// <summary> Stop the music on Destroy </summary>
        public bool stopOnDestroy
        {
            get => StopOnDestroy;
            set => StopOnDestroy = value;
        }

        [SerializeField] private bool PauseOnDisable = false;
        /// <summary> Pause the music on Disable, if it was playing </summary>
        public bool pauseOnDisable
        {
            get => PauseOnDisable;
            set => PauseOnDisable = value;
        }

        [SerializeField] private bool UnPauseOnEnable = false;
        /// <summary> Resume the music on Enable, if it was paused </summary>
        public bool unPauseOnEnable
        {
            get => UnPauseOnEnable;
            set => UnPauseOnEnable = value;
        }

        [SerializeField] private bool ResetPlaylistOnEnable = false;
        /// <summary> Reset the playlist on Enable </summary>
        public bool resetPlaylistOnEnable
        {
            get => ResetPlaylistOnEnable;
            set => ResetPlaylistOnEnable = value;
        }

        [SerializeField] private bool ResetPlaylistOnDisable = false;
        /// <summary> Reset the playlist on Disable </summary>
        public bool resetPlaylistOnDisable
        {
            get => ResetPlaylistOnDisable;
            set => ResetPlaylistOnDisable = value;
        }

        [SerializeField] private Transform FollowTarget;
        /// <summary> Transform that the music played by this MusicPlayer will follow (if set) </summary>
        public Transform followTarget
        {
            get => FollowTarget;
            set => FollowTarget = value;
        }

        /// <summary> Check if the playlist can play (has at least one music track that can play) </summary>
        public bool canPlay => playlist.canPlay;

        /// <summary>
        /// The currently active Audio Player used for playing music tracks.
        /// This can be either Audio Player A or Audio Player B.
        /// Access this property to get a reference to the currently active Audio Player.
        /// It is not recommended to adjust the currently active Audio Player settings directly.
        /// </summary>
        public AudioPlayer audioPlayer { get; private set; }

        /// <summary> Check if the music player is playing. </summary>
        public bool isPlaying => audioPlayer != null && audioPlayer.isPlaying;

        /// <summary> Check if the music player is paused. </summary>
        public bool isPaused => audioPlayer != null && audioPlayer.isPaused;

        /// <summary> Check if the music player is stopped. </summary>
        public bool isStopped => audioPlayer != null && audioPlayer.isStopped;

        /// <summary> Check if the music player is fading in. </summary>
        public bool isFadingIn => audioPlayer != null && audioPlayer.isFadingIn;

        /// <summary> Check if the music player is fading out. </summary>
        public bool isFadingOut => audioPlayer != null && audioPlayer.isFadingOut;

        /// <summary> Check if the music player is muted </summary>
        public bool isMuted { get; private set; }

        private void Awake()
        {
            SoundyService.Initialize();

            this.ResetPlaylist();

            if (dontDestroyOnSceneChange)
                DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        private void OnEnable()
        {
            // playlist.Validate();

            if (unPauseOnEnable && isPaused)
                UnPause();

            if (resetPlaylistOnEnable)
                this.ResetPlaylist();

            if (playOnEnable && !isPlaying)
                Play();
        }

        private void OnDisable()
        {
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            #endif

            if (pauseOnDisable && isPlaying)
                Pause();

            if (resetPlaylistOnDisable)
                this.ResetPlaylist();

            if (stopOnDisable && isPlaying && !isPaused && !isFadingOut)
                Stop();

            if (playOnDisable && !isPlaying)
                Play();
        }

        private void OnDestroy()
        {
            if (stopOnDestroy && (isPlaying || isPaused || isFadingOut))
                Stop();
        }

        /// <summary>
        /// Play the next song in the playlist (if there is one).
        /// If the playlist is empty, nothing happens.
        /// </summary>
        public void Play()
        {
            PrepareForPlaying();
            if (!canPlay) return;
            MusicId next = playlist.LoadNext();
            if (next == null)
            {
                Debug.Log($"Cannot load a null music track.");
                return;
            }
            Play(next);
        }

        /// <summary>
        /// Play the next song in the playlist (if there is one).
        /// If the playlist is empty, nothing happens.
        /// This is the same as calling Play() (it's here for convenience).
        /// </summary>
        public void PlayNext() =>
            Play();

        /// <summary>
        /// Play the previous song in the playlist (if there is one).
        /// If the playlist is empty, nothing happens.
        /// </summary>
        public void PlayPrevious()
        {
            PrepareForPlaying();
            if (!canPlay) return;
            MusicId previous = playlist.LoadPrevious();
            if (previous == null)
            {
                Debug.Log($"No previous music track found. You have reached the beginning of the playlist.");
                audioPlayer.Recycle();
                audioPlayer = null;
                return;
            }
            Play(previous);
        }

        /// <summary>
        /// Play the song at the given index in the playlist (if there is one).
        /// If the index is out of range, it will be clamped to the playlist's range.
        /// If the playlist is empty, nothing happens.
        /// </summary>
        /// <param name="index"> Index of the song in the playlist </param>
        public void Play(int index)
        {
            PrepareForPlaying();
            if (!canPlay) return;
            Play(playlist.GetMusicId(index));
        }

        /// <summary>
        /// Play the song with the given name in the playlist (if there is one).
        /// If the song is not found, nothing happens.
        /// </summary>
        /// <param name="libraryName"> Name of the library where the song is located </param>
        /// <param name="musicName"> Name of the song </param>
        public void Play(string libraryName, string musicName)
        {
            PrepareForPlaying();
            if (!canPlay) return;
            Play(playlist.GetMusicId(libraryName, musicName));
        }

        /// <summary>
        /// Stop the music, if it is playing.
        /// If the music is already stopped, nothing happens.
        /// </summary>
        public void Stop()
        {
            if (audioPlayer == null || !audioPlayer.inUse)
            {
                audioPlayer = null;
                return;
            }
            audioPlayer.Stop();
        }

        /// <summary>
        /// Fade out the music and then stop it, if it is playing.
        /// If the music is already stopped, nothing happens.
        /// If the fade out duration is 0, the music stops immediately.
        /// </summary>
        public void FadeOutAndStop()
        {
            if (audioPlayer == null || !audioPlayer.inUse)
            {
                audioPlayer = null;
                return;
            }
            audioPlayer.FadeOutAndStop();
        }

        /// <summary> Pause the currently playing music (if there is one) </summary>
        public void Pause()
        {
            if (!isPlaying)
                return;
            
            audioPlayer.Pause();
        }

        /// <summary> Resume playing the currently paused music (if there is one) </summary>
        public void UnPause()
        {
            if (!isPaused)
                return;
            
            audioPlayer.UnPause();
        }
        
        /// <summary> Mute the music, if it is playing. </summary>
        public void Mute()
        {
            isMuted = true;
            if (audioPlayer == null || !audioPlayer.inUse)
            {
                audioPlayer = null;
                return;
            }
            
            audioPlayer.Mute();
        }
        
        /// <summary> Unmute the music, if it is playing. </summary>
        public void UnMute()
        {
            isMuted = false;
            if (audioPlayer == null || !audioPlayer.inUse)
            {
                audioPlayer = null;
                return;
            }
            
            audioPlayer.UnMute();
        }

        /// <summary>
        /// Check if the music is playing and if it is, fade it out and stop it.
        /// Then, prepare the other AudioPlayer for playing to be ready for the next Play() call.
        /// This is useful when you want to play a new song and you want to stop the current one (if it is playing) while achieving a cross-fade effect.
        /// </summary>
        private void PrepareForPlaying()
        {
            if (audioPlayer == null) //first time initialization
            {
                audioPlayer = SoundyService.GetMusicPlayer();
                return;
            }

            if (!audioPlayer.inUse) //this audio player is in the pool, yet we have a reference to it -> get a new one
            {
                audioPlayer = SoundyService.GetMusicPlayer();
                return;
            }

            if (!audioPlayer.isFadingOut && (audioPlayer.isPlaying || audioPlayer.isPaused))
            {
                audioPlayer.ResetCallbacks();
                audioPlayer.FadeOutAndStop();
                audioPlayer = null;
            }

            // Get a new audio player for the next song
            audioPlayer = SoundyService.GetMusicPlayer();
        }

        /// <summary> Play the given music id </summary>
        /// <param name="musicId"> MusicId to play </param>
        private void Play(MusicId musicId)
        {
            if (musicId == null)
                return;

            if (audioPlayer == null)
                return;

            audioPlayer.loop = playlist.loopSong;

            if (autoAdvance && !playlist.loopSong)
            {
                if (hasCrossFade)
                {
                    audioPlayer.onFadeOutStart = () =>
                    {
                        audioPlayer.onFadeOutStart = null;
                        PlayNext();
                    };
                }
                else
                {
                    audioPlayer.onFinish = () =>
                    {
                        audioPlayer.onFinish = null;
                        PlayNext();
                    };
                }
            }

            audioPlayer
                .SetMusic(musicId)
                .SetCrossFade(crossFade)
                .SetCrossFadeDuration(crossFadeDuration)
                .SetLoop(playlist.loopSong)
                .SetFollowTarget(followTarget);

            if (playlist.loopSong && hasCrossFade)
            {
                audioPlayer
                    .SetMute(isMuted) //if the music player is muted, we need to mute the audio player as well
                    .FadeInAndPlay();

                return;
            }

            audioPlayer
                .SetMute(isMuted) //if the music player is muted, we need to mute the audio player as well
                .Play();
        }

        /// <summary>
        /// Set the dontDestroyOnLoad flag and, if true, call DontDestroyOnLoad() on this GameObject.
        /// </summary>
        /// <param name="dontDestroyOnLoad"> Whether to call DontDestroyOnLoad() on this GameObject </param>
        internal void SetDontDestroyOnLoad(bool dontDestroyOnLoad)
        {
            dontDestroyOnSceneChange = dontDestroyOnLoad;
            if (!dontDestroyOnLoad) return;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static class MusicPlayerExtensions
    {
         #region Playlist Management

        /// <summary>
        /// Reset the playlist to its initial state (first song).
        /// This does not stop the music if it is playing.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T ResetPlaylist<T>(this T musicPlayer) where T : MusicPlayer
        {
            // musicPlayer.playlist.Validate(); //this is disabled as it would remove null ids and also any id with 'None' as library name or music name
            musicPlayer.playlist.RemoveNulls();
            musicPlayer.playlist.Reset();
            return musicPlayer;
        }

        /// <summary>
        /// Add a music to the playlist.
        /// If the provided music is already in the playlist, it will be added again.
        /// If the provided music is null, nothing happens.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="musicId"> Music to add to the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T AddToPlaylist<T>(this T musicPlayer, MusicId musicId) where T : MusicPlayer
        {
            if (musicId == null) return musicPlayer;
            musicPlayer.playlist.Add(musicId);
            return musicPlayer;
        }

        /// <summary>
        /// Add a music with the given library name and music name to the playlist.
        /// If the provided music is already in the playlist, a new music with the same library name and music name will be added.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="libraryName"> Name of the library containing the music to add to the playlist </param>
        /// <param name="musicName"> Name of the music to add to the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T AddToPlaylist<T>(this T musicPlayer, string libraryName, string musicName) where T : MusicPlayer
        {
            musicPlayer.playlist.Add(libraryName, musicName);
            return musicPlayer;
        }

        /// <summary>
        /// Insert a music to the playlist at the given index.
        /// If the index is out of range, the music will be added at the beginning or at the end of the playlist (depending on the provided index).
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="index"> Index at which the music should be inserted </param>
        /// <param name="musicId"> Music to insert into the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T InsertIntoPlaylist<T>(this T musicPlayer, int index, MusicId musicId) where T : MusicPlayer
        {
            musicPlayer.playlist.Insert(index, musicId);
            return musicPlayer;
        }

        /// <summary>
        /// Insert a music with the given library name and music name to the playlist at the given index.
        /// If the index is out of range, the music will be added at the beginning or at the end of the playlist (depending on the provided index).
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="index"> Index at which the music should be inserted </param>
        /// <param name="libraryName"> Name of the library containing the music to insert into the playlist </param>
        /// <param name="musicName"> Name of the music to insert into the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T InsertIntoPlaylist<T>(this T musicPlayer, int index, string libraryName, string musicName) where T : MusicPlayer
        {
            musicPlayer.playlist.Insert(index, libraryName, musicName);
            return musicPlayer;
        }

        /// <summary>
        /// Remove the music id from the playlist.
        /// If the music id is not in the playlist, nothing happens.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="musicId"> Music to remove from the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T RemoveFromPlaylist<T>(this T musicPlayer, MusicId musicId) where T : MusicPlayer
        {
            musicPlayer.playlist.Remove(musicId);
            return musicPlayer;
        }

        /// <summary>
        /// Remove the first music with the given library name and music name from the playlist.
        /// If one does not exist, nothing happens.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="libraryName"> Name of the library containing the music to remove from the playlist </param>
        /// <param name="musicName"> Name of the music to remove from the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T RemoveFromPlaylist<T>(this T musicPlayer, string libraryName, string musicName) where T : MusicPlayer
        {
            musicPlayer.playlist.Remove(libraryName, musicName);
            return musicPlayer;
        }

        /// <summary>
        /// Remove the music at the given index from the playlist.
        /// If the index is out of range, nothing will happen.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="index"> Index of the music to remove from the playlist </param>
        /// <returns> This MusicPlayer (useful for chaining) </returns>
        public static T RemoveAtFromPlaylist<T>(this T musicPlayer, int index) where T : MusicPlayer
        {
            musicPlayer.playlist.RemoveAt(index);
            return musicPlayer;
        }

        #endregion

        /// <summary>
        /// Set the music player to not destroy on scene change.
        /// This allows the music player to persist between scene changes.
        /// This method also calls DontDestroyOnLoad(gameObject).
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to not destroy on scene change, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetDonDestroyOnSceneChange<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.SetDontDestroyOnLoad(value);
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to automatically advance to the next music track when the current one finishes playing.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to automatically advance to the next music track, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetAutoAdvance<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.autoAdvance = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to cross fade between music tracks when auto advance is enabled and fade in and fade out durations are greater than 0.
        /// <para/> Note: Cross fading is not supported when the music player is playing a single music track
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to cross fade between music tracks, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetCrossFade<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.crossFade = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the cross fade duration (in seconds) for the music player.
        /// <para/> Note: Cross fade duration must be greater than 0 for cross fading to work
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> Cross fade duration (in seconds) </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetCrossFadeDuration<T>(this T musicPlayer, float value) where T : MusicPlayer
        {
            musicPlayer.crossFadeDuration = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to play the first music track in the playlist when the music player starts.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to play the first music track in the playlist, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetPlayOnStart<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.playOnStart = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to play the first music track in the playlist when the music player is enabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to play the first music track in the playlist, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetPlayOnEnable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.playOnEnable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to play the first music track in the playlist when the music player is disabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to play the first music track in the playlist, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetPlayOnDisable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.playOnDisable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to stop playing music when the music player is disabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to stop playing music, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetStopOnDisable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.stopOnDisable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to stop playing music when the music player is destroyed.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to stop playing music, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetStopOnDestroy<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.stopOnDestroy = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to pause playing music when the music player is disabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to pause playing music, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetPauseOnDisable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.pauseOnDisable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to resume playing music when the music player is enabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to resume playing music, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetResumeOnEnable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.unPauseOnEnable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to reset the playlist when the music player is enabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to reset the playlist, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetResetPlaylistOnEnable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.resetPlaylistOnEnable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set the music player to reset the playlist when the music player is disabled.
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> True to reset the playlist, false otherwise </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetResetPlaylistOnDisable<T>(this T musicPlayer, bool value) where T : MusicPlayer
        {
            musicPlayer.resetPlaylistOnDisable = value;
            return musicPlayer;
        }

        /// <summary>
        /// Set a target that the music will follow while playing
        /// </summary>
        /// <param name="musicPlayer"> Target MusicPlayer </param>
        /// <param name="value"> Target transform </param>
        /// <returns> This music player (useful for chaining) </returns>
        public static T SetFollowTarget<T>(this T musicPlayer, Transform value) where T : MusicPlayer
        {
            musicPlayer.followTarget = value;
            if (musicPlayer.audioPlayer != null)
                musicPlayer.audioPlayer.SetFollowTarget(value);
            return musicPlayer;
        }
    }
}
