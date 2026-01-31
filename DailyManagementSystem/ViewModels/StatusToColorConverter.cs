using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DailyManagementSystem.ViewModels
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "delivered" => Brush.Parse("#A5D6A7"), // Green
                    "pending" => Brush.Parse("#FFB74D"),   // Orange/Yellow
                    _ => Brush.Parse("#757575")            // Gray
                };
            }
            return Brush.Parse("#757575");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
