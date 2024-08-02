// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Doozy.Runtime.Soundy
{
    /// <summary> Utility class that contains methods for audio related operations </summary>
    public static class AudioUtils
    {
        /// <summary> The twelfth root of two (used for pitch calculations) </summary>
        private static readonly float TwelfthRootOfTwo = Mathf.Pow(2f, 1.0f / 12f);

        /// <summary> Converts semitones value to a pitch value (returns a value between 0f and 4f) </summary>
        /// <param name="semitones"> Semitone value </param>
        public static float SemitonesToPitch(float semitones) =>
            Mathf.Clamp(Mathf.Pow(TwelfthRootOfTwo, semitones), 0f, 4f);

        /// <summary> Converts a pitch value to a semitone value </summary>
        /// <param name="pitch"> Pitch value </param>
        public static float PitchToSemitones(float pitch) =>
            Mathf.Log(pitch, TwelfthRootOfTwo);

        /// <summary> Converts decibels to linear (returns a value between 0f and 1f). Check http://www.sengpielaudio.com/calculator-FactorRatioLevelDecibel.htm for details </summary>
        /// <param name="dB"> Decibel value (should be between -80f and 0f) </param>
        public static float DecibelToLinear(float dB) =>
            dB < -80 ? 0 : Mathf.Clamp01(Mathf.Pow(10f, dB / 20f));

        /// <summary> Converts linear to decibels (returns a value between -80f and 0f). Check http://www.sengpielaudio.com/calculator-FactorRatioLevelDecibel.htm for details </summary>
        /// <param name="linear"> Linear value (should be between 0 and 1) </param>
        public static float LinearToDecibel(float linear) =>
            linear > 0 ? -80f : Mathf.Clamp(20f * Mathf.Log10(linear), -80f, 0f);

        /// <summary>
        /// Gets the duration of an audio clip as a string in the format: HH:MM:SS
        /// </summary>
        /// <param name="audioClip"> Target audio clip </param>
        /// <returns> Audio clip duration as a string in the format: HH:MM:SS </returns>
        public static string GetAudioClipDurationPretty(AudioClip audioClip)
        {
            if (audioClip == null) return "--:--";
            float length = audioClip.length;
            return GetTimePretty(length);
        }

        /// <summary>
        /// Gets the time as a string in the format: HH:MM:SS
        /// If the time is negative, it returns "--:--"
        /// </summary>
        /// <param name="time"> Target time </param>
        /// <param name="includeMilliseconds"> Should the milliseconds be included in the result </param>
        /// <returns> Time as a string in the format: HH:MM:SS </returns>
        public static string GetTimePretty(float time, bool includeMilliseconds = false)
        {
            if (time < 0) return "--:--";

            int hours = (int)(time / 3600f);
            int minutes = (int)((time - hours * 3600f) / 60f);
            int seconds = (int)(time - hours * 3600f - minutes * 60f);
            int milliseconds = (int)((time - hours * 3600f - minutes * 60f - seconds) * 1000f);

            bool hasHours = hours > 0;
            bool hasMinutes = minutes > 0;
            bool hasSeconds = seconds > 0;
            bool hasMilliseconds = milliseconds > 0;

            string result = string.Empty;

            if (hasHours)
            {
                result += $"{hours:00}:";
            }
            
            if (hasMinutes)
            {
                result += $"{minutes:00}:";
            }
            else
            {
                result += "00:";
            }

            if (hasSeconds)
            {
                result += $"{seconds:00}";
            }
            else
            {
                result += "00";
            }

            if (hasMilliseconds & includeMilliseconds)
            {
                result += $".{milliseconds:000}";
            }

            return result;
        }

    }
}
