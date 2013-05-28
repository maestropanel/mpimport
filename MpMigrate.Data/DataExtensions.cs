namespace MpMigrate.Data
{
    using System;
    using System.Data;

    public static class DataExtensions
    {
        public static T GetColumnValue<T>(this IDataRecord record, string columnName)
        {
            return GetColumnValue<T>(record, columnName, default(T));
        }

        public static T GetColumnValue<T>(this IDataRecord record, string columnName, T defaultValue)
        {
            object value = record[columnName];
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            else
            {
                return (T)value;
            }
        }
    }
}
