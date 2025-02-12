﻿using MultiFunPlayer.Common;
using System.Globalization;
using System.Windows.Data;

namespace MultiFunPlayer.UI.Converters;

public class DeviceAxisValueToPercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is float x && float.IsFinite(x) ? MathUtils.Clamp(MathF.Round(x, 2) * 100, 0, 100) : 0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is float x && float.IsFinite(x) ? MathUtils.Clamp01(x / 100) : float.NaN;
}
