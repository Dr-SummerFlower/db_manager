using System;
using System.Globalization;
using Avalonia.Data.Converters;
using db_manager.Models;

namespace db_manager.Converters;

public class EnumToDisplayNameConverter : IValueConverter
{
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value == null) return null;

    return value switch
    {
      ButtonState.Start => "启动",
      ButtonState.Stop => "停止",
      ButtonState.Loading => "加载中",
      _ => value.ToString()
    };
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
