﻿// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using UnityEngine;

namespace Doozy.Runtime.Bindy.Converters
{
    /// <summary>
    /// Converts a string value to a Vector2 value.
    /// The expected string format is "x,y".
    /// <para/> Note that it throws an ArgumentException if the source or target types are not of the expected type.
    /// </summary>
    public class StringToVector2Converter : IValueConverter
    {
        /// <summary>
        /// Flag that determines whether the converter should be registered to the converter registry refreshing the list of available converters.
        /// This is useful for special converters that are not registered to the converter registry by default.
        /// </summary>
        public bool registerToConverterRegistry => true;
        
        /// <summary>
        /// The source type to convert from.
        /// </summary>
        public Type sourceType => typeof(string);

        /// <summary>
        /// The target type to convert to.
        /// </summary>
        public Type targetType => typeof(Vector2);

        /// <summary>
        /// Determines whether the converter can convert between the specified source and target types.
        /// </summary>
        /// <param name="source">The source type to convert from.</param>
        /// <param name="target">The target type to convert to.</param>
        /// <returns>True if the conversion is supported, otherwise false.</returns>
        public bool CanConvert(Type source, Type target) =>
            source == typeof(string) && target == typeof(Vector2);

        /// <summary>
        /// Converts the specified value to the target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="target">The target type to convert to.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object value, Type target)
        {
            if (value == null)
                return null;

            if (target != typeof(Vector2))
                throw new ArgumentException($"Invalid target type: {target}. Expected: {typeof(Vector2)}.");

            if (value is not string strValue)
                throw new ArgumentException($"Invalid source type: {value.GetType()}. Expected: {typeof(string)}.");
            
            string[] values = strValue.Split(',');
            if (values.Length != 2 || !float.TryParse(values[0], out float x) || !float.TryParse(values[1], out float y))
                throw new ArgumentException($"Invalid source string: {strValue}. Expected format: 'x,y'.");

            return new Vector2(x, y);
        }
    }
}

