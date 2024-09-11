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
                            // Convert to the underlying type if it's a nullable type
                            value = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType));
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
