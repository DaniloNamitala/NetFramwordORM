using DataSource.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataSource
{
    public class DatabaseConnection
    {
        private readonly string ConnectionString = "Server=localhost;Database=LabDB;user id=nepre;Password=N3pre@2025;MultipleActiveResultSets=False;TrustServerCertificate=True";
        private SqlConnection _connection = null;

        public DatabaseConnection()
        {
            _connection = new SqlConnection(ConnectionString);
            _connection.Open();
        }

        public bool Insert<T>(ref T entity)
        {
            string table = typeof(T).GetCustomAttribute<TableAttribute>().Name;
            PropertyInfo[] properties = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<DatabaseGeneratedAttribute>() == null).ToArray();
            PropertyInfo keyProp = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null).FirstOrDefault();
            string output = "";
            if (keyProp != null)
                output = $"OUTPUT INSERTED.{keyProp.GetCustomAttribute<ColumnAttribute>().Name}";

            string columns = string.Join(",", properties.Select(p => p.GetCustomAttribute<ColumnAttribute>().Name));
            string values = string.Join(",", properties.Select(p => $"@{p.GetCustomAttribute<ColumnAttribute>().Name}"));

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = $"INSERT INTO {table}({columns}) {output} VALUES({values})";

                foreach (PropertyInfo prop in properties)
                {
                    string key = $"@{prop.GetCustomAttribute<ColumnAttribute>().Name}";
                    object value = prop.GetValue(entity, null);
                    if (value == null)
                        value = DBNull.Value;
                    cmd.Parameters.AddWithValue(key, value);
                }
                try
                {
                    object keyValue = cmd.ExecuteScalar();
                    if (keyValue != null)
                        keyProp.SetValue(entity, keyValue);
                }
                catch (Exception ex)
                {
                    throw;
                }

                return true;
            }
        }

        public bool Update<T>(T entity)
        {
            string table = typeof(T).GetCustomAttribute<TableAttribute>().Name;
            PropertyInfo[] properties = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() == null).ToArray();
            PropertyInfo keyProp = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null).FirstOrDefault();

            if (keyProp == null)
                throw new Exception("Atributo Key não definido");

            string setters = string.Join(",", properties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>().Name} = @{p.GetCustomAttribute<ColumnAttribute>().Name}"));
            string key = keyProp.GetCustomAttribute<ColumnAttribute>().Name;

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = $"UPDATE {table} SET {setters} WHERE {key} = @key";

                object value = keyProp.GetValue(entity);
                cmd.Parameters.AddWithValue("@key", value);
                foreach (PropertyInfo prop in properties)
                {
                    string _key = $"@{prop.GetCustomAttribute<ColumnAttribute>().Name}";
                    value = prop.GetValue(entity, null);
                    if (value == null)
                        value = DBNull.Value;
                    cmd.Parameters.AddWithValue(_key, value);
                }

                try
                {
                    int lines = cmd.ExecuteNonQuery();
                    if (lines > 0)
                        return true;
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            return false;
        }

        public bool Delete<T>(T entity)
        {
            PropertyInfo keyProp = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null).FirstOrDefault();
            if (keyProp == null)
                throw new Exception("Atributo Key não definido");

            string table = typeof(T).GetCustomAttribute<TableAttribute>().Name;
            string key = keyProp.GetCustomAttribute<ColumnAttribute>().Name;
            object value = keyProp.GetValue(entity);

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM {table} WHERE {key} = @key";
                cmd.Parameters.AddWithValue("@key", value);

                try
                {
                    int lines = cmd.ExecuteNonQuery();
                    if (lines >= 0)
                        return true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return false;
        }


        public List<T> Select<T>(Expression<Func<T, bool>> expression = null)
        {
            string where = "";
            if (expression != null)
            {
                var translator = new ExpressionToSqlVisitor();
                where = "WHERE " + translator.Translate(expression.Body);
            }

            string tableName = typeof(T).GetCustomAttribute<TableAttribute>().Name;
            using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {tableName} {where}", _connection))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);

                return table.ToList<T>();
            }
        }

        private static DatabaseConnection _instance = null;
        public static DatabaseConnection Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DatabaseConnection();
                return _instance;
            }
        }
    }
}
