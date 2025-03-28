﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace NSL.Extensions.DBEngine
{
    /// <summary>
    /// Класс помогающий исполнять запросы к базе данных
    /// </summary>
    public class DBCommand
    {
        public delegate void DbExceptionEventHandle(DBCommand command, Exception ex);

        public delegate void DbPerformanceEventHandle(string filename, string methodname, TimeSpan time);

        public event DbExceptionEventHandle DbExceptionEvent;

        public event DbPerformanceEventHandle DbPerformanceEvent;

        /// <summary>
        /// Зависит от типа базы данных, содержит в себе основной функционал для запросов
        /// </summary>
        DbCommand cmd;

        /// <summary>
        /// Параметры для запросов, хранимых процедур
        /// </summary>
        List<DbParameter> ParameterList = new List<DbParameter>();
        //Этот лист нужен исключительно для возможности удаления параметра из списка, т.к DbCommand возвращает object

        /// <summary>
        /// Текст запроса
        /// </summary>
        public string Query { get { return cmd.CommandText; } set { cmd.CommandText = value; } }

        /// <summary>
        /// Тип запроса, обычный запрос/хранимая процедура
        /// </summary>
        public System.Data.CommandType CommandType { get { return cmd.CommandType; } set { cmd.CommandType = value; } }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="connection">Подключение, зависит от типа базы данных</param>
        /// <param name="query">Текст запроса</param>
        /// <param name="commandType">Тип запроса (запрос/хранимая процедура)</param>
        public DBCommand(DbConnection connection, string query, System.Data.CommandType commandType) : this(connection, query)
        {
            CommandType = commandType;
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="connection">Подключение, зависит от типа базы данных</param>
        /// <param name="query">Текст запроса</param>
        public DBCommand(DbConnection connection, string query) : this(connection)
        {
            Query = query;
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="connection">Подключение, зависит от типа базы данных</param>
        public DBCommand(DbConnection connection)
        {
            cmd = connection.CreateCommand();
        }

        public DBCommand GetStorageCommand(string storageName)
        {
            Query = storageName;
            CommandType = CommandType.StoredProcedure;

            return this;
        }

        /// <summary>
        /// Выполнить запрос безрезультатно
        /// </summary>
        public DBCommand Execute([System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                cmd.ExecuteNonQuery();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос, и получить кол-во строк, к примеру при (Insert = кол-во добавленных строк, Update = кол-во измененных строк)
        /// </summary>
        /// <returns>кол-во строк</returns>
        public DBCommand ExecuteGetCount(out int count, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                count = cmd.ExecuteNonQuery();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос, и получить кол-во строк, к примеру при (Insert = кол-во добавленных строк, Update = кол-во измененных строк)
        /// </summary>
        /// <returns>кол-во строк</returns>
        public int ExecuteGetCount([System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var r = cmd.ExecuteNonQuery();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
                return r;
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Выполнить запрос, и получить значение первой строки, первого столбца
        /// </summary>
        /// <returns>значение первой строки, первого столбца</returns>
        public DBCommand ExecuteGetFirstValue(out object result, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                result = cmd.ExecuteScalar();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос, и получить значение первой строки, первого столбца
        /// </summary>
        /// <returns>значение первой строки, первого столбца</returns>
        public DBCommand ExecuteGetFirstValue<T>(out T result, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();

                var v = cmd.ExecuteScalar();
                if (v != DBNull.Value && v != null)
                    result = (T)v;
                else
                    result = default;
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос, и получить значение первой строки, первого столбца
        /// </summary>
        /// <returns>значение первой строки, первого столбца</returns>
        public DBCommand ExecuteGetFirstValue(Action<object> getter, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                getter?.Invoke(cmd.ExecuteScalar());
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос, и получить значение первой строки, первого столбца
        /// </summary>
        /// <returns>значение первой строки, первого столбца</returns>
        public object ExecuteGetFirstValue([System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var r = cmd.ExecuteScalar();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
                return r;
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Выполнить запрос и получить результат
        /// </summary>
        /// <param name="action"></param>
        public DBCommand ExecuteAndRead(Action<DbDataReader> action, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    action.Invoke(reader);
                }
                reader.Close();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                CloseConnection();
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос и получить результат
        /// </summary>
        /// <param name="action"></param>
        public DBCommand ExecuteAndRead(Action<DbDataReader, byte> action, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var reader = cmd.ExecuteReader();
                byte i = 0;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        i = 0;
                        action.Invoke(reader, i);
                    }
                }
                reader.Close();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                CloseConnection();
                throw;
            }
            return this;
        }

        /// <summary>
        /// Выполнить запрос и получить результат
        /// </summary>
        /// <returns>ридер для чтения данных с потока/returns>
        public DbDataReader ExecuteGetReader([System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                var r = cmd.ExecuteReader();
                sw.Stop();
                DbPerformanceEvent?.Invoke(sourceFilePath, memberName, sw.Elapsed);

                return r;
            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Добавление параметров к запросу
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <param name="type">Тип параметра</param>
        /// <param name="length">Размер типа</param>
        /// <param name="value">Значение параметра</param>
        public DBCommand AddInputParameter(string name, System.Data.DbType type = System.Data.DbType.Object, int length = 0, object value = null)
        {
            var param = cmd.CreateParameter();

            param.ParameterName = name;
            param.DbType = type;
            param.Direction = ParameterDirection.Input;
            param.Size = length;
            param.Value = value;

            cmd.Parameters.Add(param);
            ParameterList.Add(param);
            return this;
        }

        /// <summary>
        /// Добавление параметров к запросу
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <param name="type">Тип параметра</param>
        /// <param name="length">Размер типа</param>
        /// <param name="value">Значение параметра</param>
        public DBCommand AddOutputParameter(string name, System.Data.DbType type = System.Data.DbType.Object, int length = 0, object value = null)
        {
            var param = cmd.CreateParameter();

            param.ParameterName = name;
            param.DbType = type;
            param.Direction = ParameterDirection.Output;
            param.Size = length;
            param.Value = value;

            cmd.Parameters.Add(param);
            ParameterList.Add(param);
            return this;
        }

        /// <summary>
        /// Добавление параметров к запросу
        /// </summary>
        /// <param name="param">Параметр запроса</param>
        public DBCommand AddParameter(DbParameter param)
        {
            cmd.Parameters.Add(param);
            ParameterList.Add(param);
            return this;
        }

        /// <summary>
        /// Создать параметр
        /// </summary>
        /// <returns>параметр</returns>
        public DbParameter CreateParameter()
        {
            return cmd.CreateParameter();
        }

        /// <summary>
        /// Удалить параметр
        /// </summary>
        /// <param name="name">имя параметра</param>
        public void RemoveParameter(string name)
        {
            var param = ParameterList.Find(x => x.ParameterName == name);
            if (param == null)
                return;
            cmd.Parameters.Remove(param);
            ParameterList.Remove(param);
        }

        /// <summary>
        /// Получить параметр
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <returns>Параметр заполса</returns>
        public DbParameter GetParameter(string name)
        {
            return ParameterList.Find(x => x.ParameterName == name);
        }

        /// <summary>
        /// Очистить все параметры
        /// </summary>
        public void ClearParameters()
        {
            cmd.Parameters.Clear();
            ParameterList.Clear();
        }

        public DBCommand CloseConnection()
        {
            cmd.Connection.Close();
            cmd.Dispose();
            return this;
        }

        public DbConnection GetConnection() => cmd.Connection;
    }
}
