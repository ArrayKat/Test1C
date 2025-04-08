using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Test1C.Converter
{
    internal class ImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                // Пытаемся загрузить изображение
                var imagePath = $"avares://Test1C/Assets/{value}";
                return new Bitmap(AssetLoader.Open(new Uri(imagePath)));
            }
            catch
            {
                // В случае ошибки возвращаем null вместо изображения-заглушки
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
