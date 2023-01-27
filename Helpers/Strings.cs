using System;
using System.ComponentModel;

namespace TinkoffMonitor.Helpers
{
	public class Strings
	{
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static T Get<T>(object value)
        {
            if (value != null && value != System.DBNull.Value)
            {
                return TryParse<T>(value);
            }

            return default(T);
        }

        public static T Get<T>(object[] row, int index)
        {
            if (row[index] != null && row[index] != System.DBNull.Value)
            {
                return TryParse<T>(row[index]);
            }

            return default(T);
        }

        public static T TryParse<T>(object obj)
        {
            T result = default(T);
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    string str = obj.ToString();
                    result = (T)converter.ConvertFromString(str);
                }
                catch (Exception)
                {
                    result = default(T);
                }
            }

            return result;
        }
    }
}

