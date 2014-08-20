using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CountryByIp
{

    public interface IDbManager
    {
        IEnumerable<T> GetColumn<T>(string sql, params object[] parameters);
        int NonQuery(string sql, params object[] parameters);
        T GetOne<T>(string sql, params object[] parameters);
        SqlCommand CreateCommand(SqlConnection conn, string commandText, params object[] parameters);
        IEnumerable<T> GetMany<T>(string sql, params object[] parameters) where T : new();
        IEnumerable<T> GetMany<T>(Func<SqlDataReader, T> serialize, string sql, params object[] parameters);
        IEnumerable<IDictionary<string, object>> GetRowsAsDictionaries(string sql, params object[] parameters);
        SqlConnection OpenConn();
        Task<int> NonQueryAsync(string sql, params object[] parameters);
    }
    internal class DbManager : IDbManager
    {
        private readonly IDbManager _dbManager;
        private readonly string _connectionString;

        public DbManager(IConfig config)
        {
            _dbManager = this;
            _connectionString = config.DbConnectionString;
        }

        SqlConnection IDbManager.OpenConn()
        {
            var c = new SqlConnection(_connectionString);
            c.Open();
            return c;
        }
        SqlCommand IDbManager.CreateCommand(SqlConnection conn, string commandText, params object[] parameters)
        {
            if (parameters != null)
            {
                for (var i = 0; i < parameters.Length; ++i)
                {
                    commandText = commandText.Replace("{" + i + "}", "@Var" + i);
                }
            }
            var command = new SqlCommand(commandText, conn);
            if (parameters != null && parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; ++i)
                {
                    var value = parameters[i];
                    if (value == null) value = DBNull.Value;
                    command.Parameters.Add(new SqlParameter("@Var" + i, value));
                }
            }
            return command;
        }
        T IDbManager.GetOne<T>(string sql, params object[] parameters)
        {
            var result = default(T);
            using (var c = _dbManager.OpenConn())
            {
                SqlCommand command = _dbManager.CreateCommand(c, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = GetValue<T>(reader, 0);
                    }
                }
            }
            return result;
        }
        int IDbManager.NonQuery(string sql, params object[] parameters)
        {
            var result = 0;
            using (var c = _dbManager.OpenConn())
            {
                var command = _dbManager.CreateCommand(c, sql, parameters);
                result = command.ExecuteNonQuery();
            }
            return result;
        }
        async Task<int> IDbManager.NonQueryAsync(string sql, params object[] parameters)
        {
            using (var c = _dbManager.OpenConn())
            {
                var command = _dbManager.CreateCommand(c, sql, parameters);
                return await command.ExecuteNonQueryAsync();
            }
        }
        IEnumerable<T> IDbManager.GetMany<T>(Func<SqlDataReader, T> serialize, string sql, params object[] parameters)
        {
            using (var c = _dbManager.OpenConn())
            {
                var command = _dbManager.CreateCommand(c, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return serialize(reader);
                    }
                }
            }
        }
        IEnumerable<T> IDbManager.GetColumn<T>(string sql, params object[] parameters)
        {
            using (var c = _dbManager.OpenConn())
            {
                var command = _dbManager.CreateCommand(c, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return GetValue<T>(reader, 0);
                    }
                }
            }
        }

        private static T GetValue<T>(SqlDataReader reader, int i)
        {
            var value = reader.GetValue(i);

            T result;
            if (typeof(T) == typeof(bool))
            {
                result = (T)(object)((int)value == 1);
            }
            else
            {
                result = (T)value;
            }
            return result;
        }
        private static object GetValue(SqlDataReader reader, int i, Type t)
        {
            var value = reader.GetValue(i);

            object result;
            if (t == typeof(bool) && value is int)
            {
                result = ((int)value == 1);
            }
            else
            {
                result = value;
            }
            return result;
        }
        private static readonly IDictionary<Type, IDictionary<string, ActionAndType>> _types = new Dictionary<Type, IDictionary<string, ActionAndType>>();
        IEnumerable<T> IDbManager.GetMany<T>(string sql, params object[] parameters)
        {
            var type = typeof(T);
            IDictionary<string, ActionAndType> setters;
            if (!_types.TryGetValue(type, out setters))
            {
                _types[type] = setters =
                    type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(p => p.CanWrite && p.GetAccessors().All(a => !a.IsPrivate))
                        .Select(p => new KeyValuePair<PropertyInfo, Action<object, object>>(p, p.CreateSetter()))
                        .ToDictionary(p => p.Key.Name, p => new ActionAndType { Action = p.Value, Type = p.Key.PropertyType }, StringComparer.InvariantCultureIgnoreCase);

                type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => !f.IsPrivate && !f.IsInitOnly)
                    .Aggregate(setters, (d, e) =>
                    {
                        d.Add(e.Name, new ActionAndType { Action = e.CreateSetter(), Type = e.FieldType });
                        return d;
                    });
            }

            using (var c = _dbManager.OpenConn())
            {
                var command = _dbManager.CreateCommand(c, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    var fields = new HashSet<string>();
                    for (var i = 0; i < reader.FieldCount; ++i)
                        fields.Add(reader.GetName(i));

                    while (reader.Read())
                    {

                        var obj = new T();
                        foreach (var setter in setters)
                        {
                            if (!fields.Contains(setter.Key))
                            {
                                continue;
                            }
                            var index = reader.GetOrdinal(setter.Key);
                            setter.Value.Action(obj, GetValue(reader, index, setter.Value.Type));

                        }
                        yield return obj;
                    }
                }
            }
        }

        IEnumerable<IDictionary<string, object>> IDbManager.GetRowsAsDictionaries(string sql, params object[] parameters)
        {
            using (var c = _dbManager.OpenConn())
            {
                var command = _dbManager.CreateCommand(c, sql, parameters);
                using (var reader = command.ExecuteReader())
                {
                    var fields = new HashSet<string>();
                    for (var i = 0; i < reader.FieldCount; ++i)
                        fields.Add(reader.GetName(i));

                    while (reader.Read())
                    {
                        var dict = new Dictionary<string, object>();
                        var count = reader.FieldCount;
                        for (var i = 0; i < count; ++i)
                        {
                            dict[reader.GetName(i)] = reader.GetValue(i);
                        }
                        yield return dict;
                    }
                }
            }
        }
    }
    internal class ActionAndType
    {
        internal Action<object, object> Action;
        internal Type Type;
    }
    internal static class DbExtension
    {

        internal static Action<object, object> CreateSetter(this PropertyInfo pi)
        {
            var instance = Expression.Parameter(typeof(object), "i");
            var value = Expression.Parameter(typeof(object));

            var convertedParam = Expression.Convert(instance, pi.DeclaringType);
            var propExp = Expression.Property(convertedParam, pi.Name);
            var assignExp = Expression.Assign(propExp, Expression.Convert(value, pi.PropertyType));

            return Expression.Lambda<Action<object, object>>(assignExp, instance, value).Compile();
        }
        internal static Action<object, object> CreateSetter(this FieldInfo fi)
        {
            var instance = Expression.Parameter(typeof(object), "i");
            var value = Expression.Parameter(typeof(object));

            var convertedParam = Expression.Convert(instance, fi.DeclaringType);
            var propExp = Expression.Field(convertedParam, fi.Name);
            var assignExp = Expression.Assign(propExp, Expression.Convert(value, fi.FieldType));

            return Expression.Lambda<Action<object, object>>(assignExp, instance, value).Compile();
        }
    }
}
