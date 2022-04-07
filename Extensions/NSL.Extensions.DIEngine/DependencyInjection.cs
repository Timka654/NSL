using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NSL.Extensions.DIEngine
{
    public class DependencyInjection
    {
        private Dictionary<Type, object> diObjectMap = new Dictionary<Type, object>();

        private List<(Type type, object obj)> waitBuildObjectMap = new List<(Type type, object obj)>();

        /// <summary>
        /// Подключить уже инициализированный обьект
        /// </summary>
        /// <param name="obj">Обьект</param>
        /// <param name="process">Обработка обьекта</param>
        /// <param name="waitBuild">Обработка обьекта во время следующего вызова Build</param>
        public void Attach(object obj, bool process = false, bool waitBuild = false)
        {
            if (process)
            {
                if (waitBuild)
                {
                    waitBuildObjectMap.Add(new (obj.GetType(), obj));
                    return;
                }
                ProcessObject(obj);
            }
            else
            {
                diObjectMap.Add(obj.GetType(), obj);
            }
        }

        /// <summary>
        /// Инициализировать и подключить обьект
        /// </summary>
        /// <typeparam name="T">Тип который нужно инициализировать</typeparam>
        /// <param name="waitBuild">Обработка обьекта во время следующего вызова Build</param>
        public void Attach<T>(bool waitBuild = false)
        {
            object obj = Activator.CreateInstance<T>();
            if (waitBuild)
            {
                waitBuildObjectMap.Add(new (typeof(T), obj));
                return;
            }

            ProcessObject(obj);
        }

        /// <summary>
        /// Получить в результате выполнения функции и подключить обьект
        /// </summary>
        /// <typeparam name="T">Тип который будет инициализирован</typeparam>
        /// <param name="initialAction"></param>
        /// <param name="waitBuild">Обработка обьекта во время следующего вызова Build</param>
        public void Attach<T>(Func<DependencyInjection, T> initialAction, bool waitBuild = false)
        {
            var obj = initialAction(this);
            if (waitBuild)
            {
                waitBuildObjectMap.Add(new (typeof(T), obj));
                return;
            }
            ProcessObject(obj);
        }

        /// <summary>
        /// Запустить процесс инициализации для всех обьектов помеченных как waitBuild
        /// </summary>
        /// <returns>Кол-во обработанных обьектов</returns>
        public int Build()
        {
            int cnt = waitBuildObjectMap.Count;

            foreach (var item in waitBuildObjectMap)
            {
                ProcessObject(item.obj);
            }

            waitBuildObjectMap.Clear();

            return cnt;
        }

        private void ProcessObject(object obj)
        {
            var t = obj.GetType();

            var members = t
                .GetMembers(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
                .Where(x => x is PropertyInfo || x is FieldInfo)
                .Select(x => new { membr = x, attr = x.GetCustomAttribute<DependencyMemberAttribute>() })
                .Where(x => x.attr != null);

            object setObj = null;
            Type setT = null;
            foreach (var item in members)
            {
                if (item.membr is PropertyInfo p)
                {
                    setObj = GetValue(setT = item.attr.Type ?? p.PropertyType);
                    p.SetValue(obj, setObj);
                }
                else if (item.membr is FieldInfo f)
                {
                    setObj = GetValue(setT = item.attr.Type ?? f.FieldType);
                    f.SetValue(obj, setObj);
                }
                else
                    throw new Exception($"Аттрибут DependencyMemberAttribute не может применяться для {item.membr.GetType()}");
            }

            diObjectMap.Add(t, obj);
            if (typeof(IDependencyObject).IsAssignableFrom(t))
            {
                ((IDependencyObject)obj).OnLoaded(this);
            }
        }

        /// <summary>
        /// Получить обьект по типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>()
        {
            if (!diObjectMap.TryGetValue(typeof(T), out var val))
                throw new Exception($"Не удалось найти значение для типа { typeof(T)}");
            return (T)val;
        }

        /// <summary>
        /// Получить обьект по типу
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public object GetValue(Type t)
        {
            if (!diObjectMap.TryGetValue(t, out var val))
                throw new Exception($"Не удалось найти значение для типа {t}");
            return val;
        }
    }
}
