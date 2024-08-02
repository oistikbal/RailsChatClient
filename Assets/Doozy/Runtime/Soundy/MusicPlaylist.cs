// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.Ids;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;

namespace Doozy.Runtime.Soundy
{
    /// <summary>
    /// Playlist of Music Ids (library name + music name) that can be played by a MusicPlayer
    /// </summary>
    [Serializable]
    public class MusicPlaylist
    {
        [SerializeField] private List<MusicId> Ids;
        /// <summary> List of Music Ids (library name + music name) that this playlist is referencing </summary>
        public List<MusicId> ids
        {
            get => Ids ?? (Ids = new List<MusicId>());
            private set => Ids = value;
        }

        /// <summary> List of Music Ids (library name + music name) that have been played </summary>
        private List<MusicId> m_PlayedIds;
        /// <summary>
        /// List of Music Ids (library name + music name) that have been played.
        /// This is use to keep track of the played songs and be used like a history.
        /// </summary>
        public List<MusicId> playedIds => m_PlayedIds ?? (m_PlayedIds = new List<MusicId>());

        [SerializeField] private PlayMode PlayMode;
        /// <summary> Play mode for this MusicPlayer </summary>
        public PlayMode playMode
        {
            get => PlayMode;
            set => PlayMode = value;
        }

        [SerializeField] private bool LoopPlaylist;
        /// <summary> Loop the playlist (works only if Play Mode is Sequential) </summary>
        public bool loopPlaylist
        {
            get => LoopPlaylist;
            set => LoopPlaylist = value;
        }

        [SerializeField] private bool LoopSong;
        /// <summary>
        /// Loop the current song over and over (if enabled)
        /// If enabled, LoadNext will start playing the current song again, instead of loading the next one
        /// </summary>
        public bool loopSong
        {
            get => LoopSong;
            set => LoopSong = value;
        }

        /// <summary> Check if the playlist can play (if it has at least one valid music id) </summary>
        public bool canPlay => ids.Count != 0 && ids.Any(id => id != null && id.isValid);

        /// <summary> Check if the playlist is empty (if it has no valid music ids) </summary>
        public bool isEmpty => ids == null || ids.Count == 0;

        /// <summary> Previous music id that was played </summary>
        public MusicId previousMusicId
        {
            get => history.Count > 0 ? history.Peek() : null;
            private set
            {
                //do not add null or invalid music ids to the history
                if (value == null || !value.isValid) return;
                //add the music id to the history
                history.Push(value);
            }
        }
        /// <summary> Current music id that is playing </summary>
        public MusicId currentMusicId { get; private set; }
        /// <summary> Index of the current music id that is playing </summary>
        public int currentMusicIdIndex => currentMusicId != null ? ids.IndexOf(currentMusicId) : -1;
        /// <summary> Index of the previous music id that was played </summary>
        public int previousMusicIdIndex => previousMusicId != null ? ids.IndexOf(previousMusicId) : -1;
        /// <summary> Number of music ids in the playlist </summary>
        public int count => ids.Count;
        /// <summary> Number of valid music ids in the playlist (not null and valid) </summary>
        public int validIdsCount => ids.Count(id => id != null && id.isValid);

        private Stack<MusicId> history { get; } = new Stack<MusicId>();

        /// <summary>
        /// Reset the playlist to its default values.
        /// This will reset the previous and current music ids and their indexes.
        /// </summary>
        public void Reset()
        {
            currentMusicId = null; //reset the current music id
            history.Clear();       //clear the history
            playedIds.Clear();     //clear the played ids
        }

        /// <summary>
        /// Validate the playlist by removing all null music ids and all invalid music ids.
        /// A music id is considered invalid if it is null, empty or has the 'None' library name or music name.
        /// </summary>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist Validate() =>
            this
                .RemoveNulls()
                .RemoveInvalids();

        /// <summary>
        /// Remove all null music ids from the playlist.
        /// Note that this keeps any ids that have the 'None' library name and/or 'None' music name (these do not work, but are not null).
        /// </summary>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist RemoveNulls()
        {
            for (int i = ids.Count - 1; i >= 0; i--)
                if (ids[i] == null)
                    ids.RemoveAt(i);

            return this;
        }
        
        /// <summary>
        /// Remove all invalid music ids from the playlist.
        /// A music id is considered invalid if it is null, empty or has the 'None' library name or music name.
        /// </summary>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist RemoveInvalids()
        {
            for (int i = ids.Count - 1; i >= 0; i--)
                if (ids[i] == null || !ids[i].isValid)
                    ids.RemoveAt(i);

            return this;
        }
        
        /// <summary>
        /// Set the current music id to the music id at the given index.
        /// If the index is invalid, the current music id is returned (it can be null). 
        /// </summary>
        /// <param name="index"> Index of the music id in the playlist </param>
        /// <returns> Current music id (it can be null if it was not set or is invalid) </returns>
        public MusicId SetNext(int index)
        {
            if (index < 0 || index >= ids.Count) return currentMusicId;
            previousMusicId = currentMusicId;
            currentMusicId = ids[index];
            return currentMusicId;
        }

        /// <summary>
        /// Set the current music id to the music id at the given library name and music name.
        /// If a music id with the given library name and music name is not found, the current music id is returned (it can be null).
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <returns> Current music id (it can be null if it was not set or is invalid) </returns>
        public MusicId SetNext(string libraryName, string musicName)
        {
            int index = ids.FindIndex(id => id.libraryName.Equals(libraryName) && id.audioName.Equals(musicName));
            return SetNext(index);
        }

        /// <summary> Check if the playlist contains a music id with the given library name and music name. </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <returns> TRUE if the playlist contains a music id with the given library name and music name </returns>
        public bool Contains(string libraryName, string musicName) =>
            ids.Exists(id => id.libraryName.CleanName().Equals(libraryName) && id.audioName.CleanName().Equals(musicName));

        /// <summary>
        /// Get the music id at the given library name and music name.
        /// If a music id with the given library name and music name is not found, null is returned.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <returns> Music id (it can be null if it was not found) </returns>
        public MusicId GetMusicId(string libraryName, string musicName) =>
            ids.Find(id => id.libraryName.CleanName().Equals(libraryName) && id.audioName.CleanName().Equals(musicName));

        /// <summary>
        /// Get the music id at the given index.
        /// If the index is invalid, null is returned.
        /// </summary>
        /// <param name="index"> Index of the music id in the playlist </param>
        /// <returns> Music id (it can be null if it was not found) </returns>
        public MusicId GetMusicId(int index) =>
            index < 0 || index >= ids.Count ? null : ids[index];

        /// <summary>
        /// Loads the next music id in the playlist, based on the current play mode.
        /// After the next music id is loaded, it is set as the current music id.
        /// </summary>
        /// <returns> Next music id in the playlist (based on the current play mode) </returns>
        public MusicId LoadNext()
        {
            if (loopSong && currentMusicId != null)
                return currentMusicId;

            MusicId next;

            switch (playMode)
            {
                case PlayMode.Sequential:
                    next = GetSequentialNext();
                    break;
                case PlayMode.Random:
                    next = GetRandomNext();
                    break;
                case PlayMode.RandomNoRepeat:
                    next = GetRandomNoRepeatNext();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (next == null)
            {
                Debug.Log("No music id found in the playlist");
                return null;
            }

            if (!next.isValid)
            {
                Debug.Log($"The {next} music id is not valid");
                return null;
            }

            playedIds.Add(next); //since the next music id is valid, add it to the played ids
            return next;
        }

        /// <summary>
        /// Loads the previous music id in the playlist, if there is one.
        /// After the previous music id is loaded, it is set as the current music id.
        /// </summary>
        /// <returns> Previous music id in the playlist (if there is one) </returns>
        public MusicId LoadPrevious()
        {
            if (previousMusicId == null)
            {
                currentMusicId = null;
                return null;
            }
            currentMusicId = history.Count > 0 ? history.Pop() : null;
            if (playedIds.Count > 0)
                while (playedIds.Count > history.Count)
                    playedIds.RemoveAt(playedIds.Count - 1);
            return currentMusicId;
        }

        /// <summary> Get the next music id in the playlist when the play mode is set to Sequential </summary>
        /// <returns> Next music id in the playlist </returns>
        private MusicId GetSequentialNext()
        {
            previousMusicId = currentMusicId;

            if (!canPlay) //there isn't any valid music id in the playlist (either because it is empty or all music ids are invalid)
            {
                currentMusicId = null;
                return currentMusicId;
            }

            int nextIndex = currentMusicIdIndex + 1;
            if (nextIndex >= ids.Count) //end of the playlist
            {
                if (loopPlaylist)
                {
                    nextIndex = 0;
                }
                else
                {
                    currentMusicId = null;
                    return currentMusicId;
                }
            }

            currentMusicId = ids[nextIndex];

            while (currentMusicId == null || !currentMusicId.isValid)
            {
                if (nextIndex >= ids.Count - 1) //end of the playlist
                {
                    if (loopPlaylist)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        currentMusicId = null;
                        return currentMusicId;
                    }
                }
                else
                {
                    nextIndex++;
                }
                
                currentMusicId = ids[nextIndex];
            }

            return currentMusicId;
        }

        /// <summary> Get the next music id in the playlist when the play mode is set to Random </summary>
        /// <returns> Next music id in the playlist </returns>
        private MusicId GetRandomNext()
        {
            previousMusicId = currentMusicId;

            if (!canPlay) //there isn't any valid music id in the playlist (either because it is empty or all music ids are invalid)
            {
                currentMusicId = null;
                return currentMusicId;
            }

            int randomIndex = UnityEngine.Random.Range(0, ids.Count);
            currentMusicId = ids[randomIndex];
            while (currentMusicId == null || !currentMusicId.isValid)
            {
                randomIndex = UnityEngine.Random.Range(0, ids.Count);
                currentMusicId = ids[randomIndex];
            }

            return currentMusicId;
        }

        /// <summary> Get the next music id in the playlist when the play mode is set to RandomNoRepeat </summary>
        /// <returns> Next music id in the playlist </returns>
        private MusicId GetRandomNoRepeatNext()
        {
            previousMusicId = currentMusicId;

            if (!canPlay) //there isn't any valid music id in the playlist (either because it is empty or all music ids are invalid)
            {
                currentMusicId = null;
                return currentMusicId;
            }

            int validIdsCount = ids.Count(id => id != null && id.isValid);
            if (playedIds.Count >= validIdsCount) playedIds.Clear();
            int randomIndex = UnityEngine.Random.Range(0, ids.Count);
            currentMusicId = ids[randomIndex];
            while (currentMusicId == null || !currentMusicId.isValid || playedIds.Contains(currentMusicId))
            {
                randomIndex = UnityEngine.Random.Range(0, ids.Count);
                currentMusicId = ids[randomIndex];
            }

            return currentMusicId;
        }

        /// <summary> Set the playlist play mode </summary>
        /// <param name="value"> Play mode </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist SetPlayMode(PlayMode value)
        {
            playMode = value;
            return this;
        }

        /// <summary> Set the playlist to loop or not </summary>
        /// <param name="value"> If TRUE, the playlist will loop when it reaches the end </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist SetLoop(bool value)
        {
            loopPlaylist = value;
            return this;
        }

        /// <summary> Set the playlist to loop the current song or not </summary>
        /// <param name="value"> If TRUE, the playlist will loop the current song </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist SetLoopCurrentSong(bool value)
        {
            loopSong = value;
            return this;
        }

        /// <summary> Clear the playlist and reset the current music id and index </summary>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist Clear()
        {
            ids.Clear();
            Reset();
            return this;
        }

        /// <summary>
        /// Add a music id to the playlist.
        /// If a music id already exists in the playlist, it will not be added again.
        /// </summary>
        /// <param name="musicId"> Music id to add </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist Add(MusicId musicId)
        {
            if (musicId == null) throw new ArgumentNullException(nameof(musicId));
            if (ids.Contains(musicId)) return this;
            ids.Add(musicId);
            return this;
        }

        /// <summary>
        /// Add a music id to the playlist.
        /// If a music id with the same library name and music name already exists in the playlist, a new music id with the same library name and music name will be added.
        /// </summary>
        /// <param name="libraryName"> Name of the library where the music is located </param>
        /// <param name="musicName"> Name of the music </param>
        /// <returns> The music id that was added, or if the music id already exists, the existing music id with the same library name and music name </returns>
        public MusicId Add(string libraryName, string musicName)
        {
            var musicId = new MusicId(libraryName, musicName);
            ids.Add(musicId);
            return musicId;
        }

        /// <summary>
        /// Add a music id to the playlist with the default library name and default audio name.
        /// </summary>
        /// <returns> The music id that was added, or if the music id already exists, the existing music id with the default library name and default audio name </returns>
        public MusicId AddNew() =>
            Add(SoundySettings.k_None, SoundySettings.k_None);

        /// <summary>
        /// Insert a music id to the playlist at the specified index.
        /// If the index is out of range, the music id will be added to the beginning or the end of the playlist (depending on the provided index).
        /// </summary>
        /// <param name="index"> Index to insert the music id at </param>
        /// <param name="musicId"> Music id to insert </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist Insert(int index, MusicId musicId)
        {
            if (musicId == null) throw new ArgumentNullException(nameof(musicId));
            //validate index
            index = Mathf.Clamp(index, 0, ids.Count);
            ids.Insert(index, musicId);
            return this;
        }

        /// <summary>
        /// Insert a music id with the specified library name and music name to the playlist at the specified index.
        /// If the index is out of range, the music id will be added to the beginning or the end of the playlist (depending on the provided index).
        /// </summary>
        /// <param name="index"> Index to insert the music id at </param>
        /// <param name="libraryName"> Name of the library where the music is located </param>
        /// <param name="musicName"> Name of the music </param>
        /// <returns> The music id that was inserted, or if the music id already exists, the existing music id with the same library name and music name </returns>
        public MusicId Insert(int index, string libraryName, string musicName)
        {
            var musicId = new MusicId(libraryName, musicName);
            Insert(index, musicId);
            return musicId;
        }

        /// <summary>
        /// Removes the given music id from the playlist.
        /// If the music id does not exist in the playlist, nothing will happen.
        /// </summary>
        /// <param name="musicId"> Music id to remove </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist Remove(MusicId musicId)
        {
            ids.Remove(musicId);
            return this;
        }

        /// <summary>
        /// Removes the first music id with the given library name and music name from the playlist.
        /// If a music id with the given library name and music name does not exist in the playlist, nothing will happen.
        /// </summary>
        /// <param name="libraryName"> Name of the library where the music is located </param>
        /// <param name="musicName"> Name of the music </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist Remove(string libraryName, string musicName)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                MusicId musicId = ids[i];
                if (musicId == null) continue;
                if (musicId.libraryName != libraryName || musicId.audioName != musicName) continue;
                ids.Remove(musicId);
                break;
            }

            return this;
        }

        /// <summary>
        /// Removes the music id at the given index from the playlist.
        /// If the index is out of range, nothing will happen.
        /// </summary>
        /// <param name="index"> Index of the music id to remove </param>
        /// <returns> This playlist (for method chaining) </returns>
        public MusicPlaylist RemoveAt(int index)
        {
            //check index
            if (index < 0 || index >= ids.Count) return this;
            ids.RemoveAt(index);
            return this;
        }

        /// <summary>
        /// Get the index of the given music id.
        /// If the music id does not exist in the playlist, -1 will be returned.
        /// </summary>
        /// <param name="musicId"> Music id to get the index of </param>
        /// <returns> The index of the given music id, or -1 if the music id does not exist in the playlist </returns>
        public int IndexOf(MusicId musicId) =>
            ids.IndexOf(musicId);

        /// <summary>
        /// Get the index of the first music id with the given library name and music name.
        /// If a music id with the given library name and music name does not exist in the playlist, -1 will be returned.
        /// </summary>
        /// <param name="libraryName"> Name of the library where the music is located </param>
        /// <param name="musicName"> Name of the music </param>
        /// <returns> The index of the first music id with the given library name and music name, or -1 if a music id with the given library name and music name does not exist in the playlist </returns>
        public int IndexOf(string libraryName, string musicName)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                MusicId musicId = ids[i];
                if (musicId == null) continue;
                if (musicId.libraryName != libraryName || musicId.audioName != musicName) continue;
                return i;
            }

            return -1;
        }
    }
}
