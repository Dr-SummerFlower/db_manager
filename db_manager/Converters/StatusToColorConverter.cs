using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace db_manager.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value as string;
        return status switch
        {
            "运行中" => Brushes.Green,       // 绿色 - 运行中
            "已停止" => Brushes.Red,         // 红色 - 已停止
            "启动中..." => Brushes.Yellow,  // 黄色 - 启动中
            "停止中..." => Brushes.Yellow,  // 黄色 - 停止中
            "操作失败" => Brushes.Orange,    // 橙色 - 操作失败
            _ => Brushes.Gray               // 灰色 - 默认/未知状态
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}