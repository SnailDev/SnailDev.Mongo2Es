using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Repository.Container
{
    /// <summary>
    /// 容器
    /// </summary>
    public static class RepositoryContainer
    {
        private static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 
        /// </summary>
        static RepositoryContainer()
        {
            Instances = new ConcurrentDictionary<string, Lazy<object>>();
        }

        /// <summary>
        /// 
        /// </summary>
        //public static ConcurrentDictionary<string, Lazy<object>> Instances { get; private set; }
        private static ConcurrentDictionary<string, Lazy<object>> Instances;

        #region 注册
        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void Register<T>(T service)
            where T : NESTBaseRepository
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(() => service);

            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
            where T : NESTBaseRepository, new()
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(() => new T());

            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        public static void Register<T>(Func<object> function)
            where T : NESTBaseRepository
        {
            var t = typeof(T);
            var lazy = new Lazy<object>(function);

            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <param name="t">typeof(T)</param>
        public static void Register(Type t)
        {
            var instance = Activator.CreateInstance(t, true);
            var lazy = new Lazy<object>(() => instance);
            Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
        }

        /// <summary>
        /// 注册实例（添加或替换原有对象）
        /// </summary>
        /// <param name="assemblyNames"></param>
        public static void RegisterAll(params string[] assemblyNames)
        {
            LoadAll((t, repo) =>
            {
                var lazy = new Lazy<object>(() => repo);
                Instances.AddOrUpdate(GetKey(t), lazy, (x, y) => lazy);
            }, assemblyNames);
        }
        #endregion

        /// <summary>
        /// 取出指定对象
        /// </summary>
        /// <typeparam name="T">指定对象类型</typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
            where T : NESTBaseRepository
        {
            var t = typeof(T);
            var k = GetKey(t);

            if (Instances.TryGetValue(k, out Lazy<object> repository))
            {
                return (T)repository.Value;
            }
            else
            {
                throw new Exception($"this repository({k}) is not register");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="assemblyNames"></param>
        private static void LoadAll(Action<Type, NESTBaseRepository> a, params string[] assemblyNames)
        {
            foreach (var assemblyName in assemblyNames)
            {
                var classList = GetAllClassByInterface(typeof(NESTBaseRepository), assemblyName);
                foreach (var c in classList)
                {
                    NESTBaseRepository repos = null;
                    try
                    {
                        var method = c.GetMethod("CreateInstance", bindingFlags);
                        if (method != null)
                        {
                            repos = (NESTBaseRepository)method.Invoke(null, null);
                        }
                    }
                    catch { }
                    if (repos == null)
                    {
                        var instance = Activator.CreateInstance(c, true);
                        repos = (NESTBaseRepository)instance;
                    }
                    if (repos == null)
                    {
                        throw new Exception(string.Format("this repository({0}.{1}) is not create", c.Namespace, c.Name));
                    }
                    a?.Invoke(c, repos);
                }
            }
        }

        /// <summary>
        /// 取得某个接口下所有实现这个接口的类
        /// </summary>
        /// <param name="t">指定接口</param>
        /// <param name="assemblyName">指定程序集名称</param>
        /// <returns></returns>
        private static List<Type> GetAllClassByInterface(Type t, string assemblyName)
        {
            if (!t.IsInterface || string.IsNullOrWhiteSpace(assemblyName))
            {
                return null;
            }

            //获取指定包下面所有的class
            List<Type> allClassList = GetClasses(assemblyName);
            if (allClassList == null || !allClassList.Any())
            {
                return null;
            }
            List<Type> returnClassList = new List<Type>();
            foreach (var type in allClassList)
            {
                if (type.GetInterface(t.Name, true) != null && !type.IsAbstract && !type.IsInterface)
                {
                    returnClassList.Add(type);
                }
            }
            return returnClassList;
        }

        /// <summary>
        /// 从指定程序集下获取所有的Class
        /// </summary>
        /// <param name="assemblyName">指定程序集名称</param>
        /// <returns></returns>
        private static List<Type> GetClasses(string assemblyName)
        {
            List<Type> classes = null;
            //获取指定程序集下所有的类
            var assembly = Assembly.Load(assemblyName);
            //取出所有类型集合
            var typeArray = assembly.GetTypes();
            if (typeArray != null)
            {
                classes = typeArray.ToList();
            }
            return classes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static string GetKey(Type t)
        {
            return t.FullName;
        }

    }
}
