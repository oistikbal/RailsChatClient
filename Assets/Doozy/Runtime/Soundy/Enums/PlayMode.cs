// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Soundy
{
    /// <summary> Describes the order in which a group of audio clips will be played </summary>
    public enum PlayMode
    {
        /// <summary>
        /// The audio clips will be played in a sequential order (in the order they were added)
        /// </summary>
        Sequential = 0,
        
        /// <summary>
        /// The audio clips will be played in a random order respecting the weight of each audio clip 
        /// </summary>
        Random = 1,
        
        /// <summary>
        /// The audio clips will be played in a random order, but each audio clip will be played only once (no repeats).
        /// This mode is useful when you want to play a random sound, but you don't want to play the same sound twice in a row.
        /// Note that this mode is more expensive than the Random mode, because it needs to keep track of the played audio clips.
        /// </summary>
        RandomNoRepeat = 2
    }
}