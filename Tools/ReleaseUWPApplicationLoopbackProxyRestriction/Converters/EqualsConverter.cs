#region FileHeader

// // Project:  ReleaseUWPApplicationLoopbackProxyRestriction
// // File:  EqualsConverter.cs
// // CreateTime:  2023-01-03 8:38
// // LastUpdateTime:  2023-01-03 8:51

#endregion

#region Nmaespaces

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace ReleaseUWPApplicationLoopbackProxyRestriction.Converters
{
    internal class EqualsConverter : IValueConverter
    {
        private static EqualsConverter _instance;
        private static EqualsConverter _invertInstance;
        private bool _inverse;

        private EqualsConverter()
        {
        }

        public static EqualsConverter Instance
        {
            get
            {
                if (_instance == null) _instance = new EqualsConverter();

                return _instance;
            }
        }

        public static EqualsConverter InvertInstance
        {
            get
            {
                if (_invertInstance == null)
                    _invertInstance = new EqualsConverter
                    {
                        _inverse = true
                    };

                return _invertInstance;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return _inverse ? parameter != null : parameter == null;

            return _inverse ? value.Equals(parameter) : !value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}