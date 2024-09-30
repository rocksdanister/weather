using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

// Ref: https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Uwp.UI/Converters/StringFormatConverter.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
public class StringFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        // Retrieve the format string and use it to format the value.
        string formatString = parameter as string;
        if (string.IsNullOrEmpty(formatString))
        {
            // If the format string is null or empty, simply call ToString()
            // on the value.
            return value.ToString();
        }

        try
        {
            return string.Format(culture, formatString, value);
        }
        catch
        {
            return value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
