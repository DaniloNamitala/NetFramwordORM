using DataSource.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace DataSource
{
    public static class Extensions
    {
        public static List<T> ToList<T>(this DataTable table)
        {
            List<T> list = new List<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (DataRow row in table.Rows)
            {
                T obj = (T) Activator.CreateInstance(typeof(T));
                foreach (var prop in properties) {
                    string col = prop.GetCustomAttribute<ColumnAttribute>().Name;
                    if (!(row[col] is DBNull))
                        prop.SetValue(obj, row[col]);
                }
                list.Add(obj);
            }
            return list;
        }
    }
}
