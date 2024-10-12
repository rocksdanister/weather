using Avalonia.Data.Converters;
using Drizzle.Models.Enums;
using FluentAvalonia.UI.Controls;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class NotificationToInfoBarSeverityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return InfoBarSeverity.Informational;

        return (AppNotification)value switch
        {
            AppNotification.information => InfoBarSeverity.Informational,
            AppNotification.warning => InfoBarSeverity.Warning,
            AppNotification.success => InfoBarSeverity.Success,
            AppNotification.error => InfoBarSeverity.Error,
            _ => (object)InfoBarSeverity.Informational,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
