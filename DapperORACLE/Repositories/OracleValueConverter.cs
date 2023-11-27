using System.Reflection;
using System.Text.RegularExpressions;

namespace DapperORACLE.Repositories
{
    internal static class OracleValueConverter
    {
        private static string[] OracleStringTypes { get; } = new string[2] { "Oracle.DataAccess.Types.OracleString", "Oracle.ManagedDataAccess.Types.OracleString" };


        public static T Convert<T>(object val)
        {
            if (val == null)
            {
                return default(T);
            }

            if (val == DBNull.Value)
            {
                if (default(T) != null)
                {
                    throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                }

                return default(T);
            }

            Type type = val.GetType();
            if (typeof(T) == typeof(string) && OracleStringTypes.Contains<string>(type.FullName))
            {
                val = val.ToString();
                if ((string)val == "null")
                {
                    return default(T);
                }

                return (T)val;
            }

            if (Regex.IsMatch(type.FullName, "Oracle\\.\\w+\\.Types\\.Oracle\\w+"))
            {
                PropertyInfo property = type.GetProperty("IsNull");
                if (property != null && property.CanRead && (bool)property.GetValue(val))
                {
                    if (default(T) != null)
                    {
                        throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                    }

                    return default(T);
                }

                PropertyInfo property2 = type.GetProperty("Value");
                if (property2 != null && property2.CanRead)
                {
                    return (T)property2.GetValue(val);
                }
            }

            return (T)val;
        }
    }
}
