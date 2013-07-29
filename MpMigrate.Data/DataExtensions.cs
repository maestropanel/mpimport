namespace MpMigrate.Data
{
    using System;
    using System.Data;

    public static class DataExtensions
    {
        public static T GetColumnValue<T>(object record, string columnName)
        {            
            try
            {
                if (record == null)
                {
                    return default(T);
                }
                else
                {
                    return (T)record;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("{0}, {1}, {2}", columnName,
                                record != null ? record.GetType().ToString() : "Null-Object"
                                , ex.Message), ex);
            }
        }

        public static T GetColumnValue<T>(this IDataRecord record, string columnName)
        {
            return GetColumnValue<T>(record, columnName, default(T));
        }

        public static T GetColumnValue<T>(this IDataRecord record, string columnName, T defaultValue)
        {
            object value = null;

            try
            {
                value = record[columnName];
                           
                if (value == null || value == DBNull.Value)
                {
                    return defaultValue;
                }
                else
                {                    
                    return (T)value;
                }
            }
            catch (Exception ex)
            {

                throw new Exception(String.Format("{0}, {1}, {2}", columnName, 
                                value != null ? value.GetType().ToString() : "Null-Object"
                                , ex.Message), ex);
            }
        }
    }
}
