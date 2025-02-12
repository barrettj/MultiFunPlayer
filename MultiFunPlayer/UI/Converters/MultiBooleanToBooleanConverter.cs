﻿using System.Globalization;
using System.Windows.Data;

namespace MultiFunPlayer.UI.Converters;

public class MultiBooleanToBooleanAndConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.OfType<IConvertible>().All(System.Convert.ToBoolean);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class MultiBooleanToBooleanOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.OfType<IConvertible>().Any(System.Convert.ToBoolean);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
