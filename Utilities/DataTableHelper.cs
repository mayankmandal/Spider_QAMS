using System.Data;
using System.Reflection;

namespace Spider_QAMS.Utilities
{
    public static class DataTableHelper
    {
        public static List<T> MapDataTableToList<T>(DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T obj = new T();
                foreach(DataColumn column in dataTable.Columns)
                {
                    PropertyInfo prop = typeof(T).GetProperty(column.ColumnName);
                    if (prop != null && row[column] != DBNull.Value)
                    {
                        object value = row[column];

                        // Check if the property is a nullable type
                        if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                        {
                            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                            // Explicitly handle boolean (bit) conversion
                            if (underlyingType == typeof(bool))
                            {
                                value = Convert.ToInt32(value) == 1; // Convert 1 to true, 0 to false
                            }
                            else
                            {
                                // Convert to the underlying type if it's a nullable type
                                value = Convert.ChangeType(value, underlyingType);
                            }
                        }
                        else if (prop.PropertyType == typeof(bool))
                        {
                            // Explicitly handle boolean (bit) conversion
                            value = Convert.ToInt32(value) == 1;
                        }
                        else
                        {
                            // Convert to the actual type
                            value = Convert.ChangeType(value, prop.PropertyType);
                        }
                        prop.SetValue(obj, value, null);
                    }
                }
                dataList.Add(obj);
            }
            return dataList;
        }
    }
}
