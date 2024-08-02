// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Common.Extensions;
using UnityEngine;

namespace Doozy.Runtime.Soundy
{
    public partial class SoundyService
    {
        /// <summary> Specialized pool of audio players </summary>
        public class AudioPlayerPool
        {
            private List<AudioPlayer> m_Pool;
            /// <summary> List of audio players that are currently in the pool </summary>
            private List<AudioPlayer> pool => m_Pool ?? (m_Pool = new List<AudioPlayer>());

            private List<AudioPlayer> m_ActivePlayers;
            /// <summary> List of audio players that are currently active (playing a sound) </summary>
            internal List<AudioPlayer> activePlayers => m_ActivePlayers ?? (m_ActivePlayers = new List<AudioPlayer>());

            private Type m_AudioPlayerType;
            /// <summary>
            /// Audio player type that will be used to create new audio players.
            /// This is set automatically when the pool is initialized and it is based on the audio player type set in the SoundySettings asset.
            /// By setting a different audio player type in the SoundySettings asset, you can change the type of audio player that will be used, thus use a custom audio player.
            /// </summary>
            private Type audioPlayerType => m_AudioPlayerType ?? (m_AudioPlayerType = GetAudioPlayerType());

            /// <summary> Transform that holds all the audio players that are currently in the pool </summary>
            private Transform poolTransform { get; set; }

            /// <summary> Internal flag that keeps track if the pool has been initialized </summary>
            private bool initialized { get; set; }

            /// <summary> Number of audio players that are currently out of the pool and in use. </summary>
            public int activePlayersCount => activePlayers.Count;

            /// <summary> Number of audio players that are currently in the pool and are not in use, thus ready to be used. </summary>
            public int inPoolPlayersCount => pool.Count(player => player.inPool);

            /// <summary> Number of audio players that are currently is use and idle (not playing a sound) </summary>
            public int isIdlePlayersCount => pool.Count(player => player.isIdle);

            /// <summary> Number of audio players that are currently is use and playing a sound </summary>
            public int isPlayingPlayersCount => pool.Count(player => player.isPlaying);

            /// <summary> Number of audio players that are currently is use and paused (not playing a sound, but paused) </summary>
            public int isPausedPlayersCount => pool.Count(player => player.isPaused);

            /// <summary> Number of audio players that are currently is use and stopped (not playing a sound, but stopped) </summary>
            public int isStoppedPlayersCount => pool.Count(player => player.isStopped);

            /// <summary> Initializes the pool with the specified parent transform </summary>
            /// <param name="parent"> Parent transform </param>
            public void Initialize(Transform parent)
            {
                if (initialized) return;
                poolTransform = parent;
                initialized = true;
            }

            /// <summary> Preheats the pool with the specified number of audio players </summary>
            /// <param name="count"> Number of audio players to preheat the pool with </param>
            public void PreheatPool(int count)
            {
                for (int i = 0; i < count; i++)
                    CreateAudioPlayer();
            }

            /// <summary> Creates a new audio player and adds it to the pool </summary>
            /// <returns> The newly created audio player </returns>
            private AudioPlayer CreateAudioPlayer()
            {
                var go = new GameObject(nameof(AudioPlayer));
                var player = go.AddComponent(audioPlayerType) as AudioPlayer;
                // ReSharper disable once PossibleNullReferenceException
                player.pool = this;
                go.transform.SetParent(poolTransform);
                go.transform.ResetTransformation();
                go.SetActive(false);
                pool.Add(player);
                player.SetPlayState(PlayState.InPool);
                return player;
            }

            /// <summary> Get an audio player from the pool. If there are no available audio players, a new one will be created. </summary>
            /// <returns> An audio player from the pool </returns>
            public AudioPlayer GetAudioPlayer()
            {
                bool foundNull = false;
                for (int i = 0; i < pool.Count; i++)
                {
                    AudioPlayer player = pool[i];
                    if (player == null)
                    {
                        foundNull = true;
                        continue;
                    }

                    if (player.gameObject.activeInHierarchy && player.inUse)
                        continue; // Player is already in use so we skip it

                    player.ResetAudioPlayer();
                    player.ResetCallbacks();
                    player.gameObject.SetActive(true);
                    activePlayers.Add(player);
                    player.SetPlayState(PlayState.Idle);
                    return player;
                }

                if (foundNull)
                {
                    for (int i = pool.Count - 1; i >= 0; i--)
                    {
                        if (pool[i] != null) continue;
                        pool.RemoveAt(i);
                        break;
                    }
                }

                AudioPlayer newPlayer = CreateAudioPlayer();
                newPlayer.gameObject.SetActive(true);
                activePlayers.Add(newPlayer);
                newPlayer.SetPlayState(PlayState.Idle);
                return newPlayer;
            }

            /// <summary> Return a given audio player to the pool </summary>
            /// <param name="player"> AudioPlayer to return to the pool </param>
            internal void Recycle(AudioPlayer player)
            {
                if (player == null) return;
                if (!pool.Contains(player)) return;
                player.ResetAudioPlayer();
                player.ResetCallbacks();
                player.UpdateLastPlayedTime();
                player.gameObject.SetActive(false);
                player.gameObject.transform.SetParent(poolTransform);
                player.gameObject.transform.ResetTransformation();
                activePlayers.Remove(player);
                player.SetPlayState(PlayState.InPool);
                player.ClearCallbacks();
            }

            /// <summary>
            /// Return all active audio players to the pool.
            /// This method also removes any null players from the pool's active players list.
            /// </summary>
            /// <param name="ignoreRecycleAfterUse"> If true, all players will be recycled, even if they have the recycleAfterUse flag set to false </param>
            public void RecycleAll(bool ignoreRecycleAfterUse = false)
            {
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null)
                    {
                        activePlayers.RemoveAt(i);
                        continue;
                    }

                    switch (player.recycleAfterUse)
                    {
                        case true:
                            player.Recycle();
                            continue;
                        case false when ignoreRecycleAfterUse:
                            player.Recycle();
                            break;
                    }
                }
            }

            /// <summary>
            /// Recycle all idle audio players that have the recycleAfterUse flag set to true and are not currently playing a sound.
            /// </summary>
            public void RecycleIdlePlayers()
            {
                bool foundNull = false;
                for (int i = pool.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = pool[i];
                    if (player == null)
                    {
                        foundNull = true;
                        continue;
                    }

                    if (player.isIdle && player.recycleAfterUse)
                        player.Recycle();
                }

                if (!foundNull) return;
                for (int i = pool.Count - 1; i >= 0; i--)
                {
                    if (pool[i] != null) continue;
                    pool.RemoveAt(i);
                    break;
                }
            }

            /// <summary> Remove all null players from the pool and active players list </summary>
            public void RemoveNullPlayers()
            {
                for (int i = pool.Count - 1; i >= 0; i--)
                {
                    if (pool[i] != null) continue;
                    pool.RemoveAt(i);
                    break;
                }

                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    if (activePlayers[i] != null) continue;
                    activePlayers.RemoveAt(i);
                    break;
                }
            }

            /// <summary>
            /// Remove all idle players from the pool and active players list, but keep the specified number of players alive.
            /// </summary>
            /// <param name="minPlayersToKeepAlive"></param>
            public void RemoveIdlePlayers(int minPlayersToKeepAlive)
            {
                minPlayersToKeepAlive = Mathf.Max(0, minPlayersToKeepAlive);
                if (pool.Count <= minPlayersToKeepAlive) return;
                for (int i = pool.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = pool[i];
                    if (player.inUse) continue;
                    if (pool.Count <= minPlayersToKeepAlive) break;
                    Dispose(player);
                }
            }

            /// <summary> Get the type of the AudioPlayer that is used to play the audio clips </summary>
            /// <returns> Type of the AudioPlayer that is used to play the audio clips </returns>
            private static Type GetAudioPlayerType() =>
                Type.GetType(settings.AudioPlayerFullQualifiedTypeName);

            /// <summary>
            /// Dispose the specified player and remove it from the pool and active players list.
            /// This will destroy the game object of the player.
            /// </summary>
            /// <param name="player"> Player to dispose </param>
            public void Dispose(AudioPlayer player)
            {
                if (player == null) return;
                if (pool.Contains(player)) pool.Remove(player);
                if (activePlayers.Contains(player)) activePlayers.Remove(player);
                player.ClearCallbacks();
                if (player.gameObject == null)
                {
                    Destroy(player);
                    return;
                }
                Destroy(player.gameObject);
            }

            /// <summary> Stop all active players </summary>
            public void StopAllActivePlayers()
            {
                if (activePlayers == null) return;
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null) continue;
                    player.Stop();
                }
            }
            
            /// <summary> Fade out and stop all active players </summary>
            public void FadeOutAndStopAllActivePlayers()
            {
                if(activePlayers == null) return;
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null) continue;
                    player.FadeOutAndStop();
                }
            }

            /// <summary> Mute all active players </summary>
            public void MuteAllActivePlayers()
            {
                if (activePlayers == null) return;
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null) continue;
                    player.Mute();
                }
            }

            /// <summary> Unmute all active players </summary>
            public void UnmuteAllActivePlayers()
            {
                if (activePlayers == null) return;
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null) continue;
                    player.UnMute();
                }
            }

            /// <summary> Pause all active players </summary>
            public void PauseAllActivePlayers()
            {
                if (activePlayers == null) return;
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null) continue;
                    player.Pause();
                }
            }

            /// <summary> Unpause all active players </summary>
            public void UnPauseAllActivePlayers()
            {
                if (activePlayers == null) return;
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    AudioPlayer player = activePlayers[i];
                    if (player == null) continue;
                    player.UnPause();
                }
            }
        }
    }
}
