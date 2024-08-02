// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Doozy.Runtime.Soundy
{
    public static class AudioSourceExtensions
    {
        /// <summary> Get the percentage of the clip that has been played </summary>
        /// <param name="source"> The AudioSource </param>
        /// <returns> The percentage of the clip that has been played </returns>
        public static float PlayedPercentage(this AudioSource source) =>
            source.time / source.clip.length;
        
        /// <summary> Get the percentage of the clip that has not been played </summary>
        /// <param name="source"> The AudioSource </param>
        /// <returns> The percentage of the clip that has not been played </returns>
        public static float NotPlayedPercentage(this AudioSource source) =>
            1 - source.PlayedPercentage();
        
        /// <summary> Get the percentage of the clip that has been played </summary>
        /// <param name="source"> The AudioSource </param>
        /// <param name="time"> The time to check </param>
        /// <returns> The percentage of the clip that has been played </returns>
        public static float PlayedPercentage(this AudioSource source, float time) =>
            time / source.clip.length;
        
        /// <summary> Get the percentage of the clip that has not been played </summary>
        /// <param name="source"> The AudioSource </param>
        /// <param name="time"> The time to check </param>
        /// <returns> The percentage of the clip that has not been played </returns>
        public static float NotPlayedPercentage(this AudioSource source, float time) =>
            1 - source.PlayedPercentage(time);
    }
}
