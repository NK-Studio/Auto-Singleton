using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

#if AUTO_SINGLETON_USE_ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
#endif

namespace AutoSingleton
{
    /// <summary>
    /// Generic access class for singleton instances.
    /// </summary>
    public static class Singleton
    {
        internal static readonly Dictionary<Type, MonoBehaviour> Managers = new();
        internal static readonly Type[] KAllManagerTypes = GetAllSingletonTypes();
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Returns an instance of the specified generic type T if it exists in the Managers dictionary. Otherwise, returns null.
        /// </summary>
        /// <typeparam name="T">The type of the MonoBehaviour instance to retrieve</typeparam>
        /// <returns>An instance of the specified generic type T if found in the Managers dictionary. Otherwise, returns null.</returns>
        public static T Instance<T>() where T : MonoBehaviour
        {
            if (Managers.ContainsKey(typeof(T)))
                return (T)Managers[typeof(T)];

            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                Debug.LogError($"매니저 : '{typeof(T)}'가 액세스 되지 않았습니다. Auto Singleton Settings에서 해당 매니저가 제외됬는지 확인해주세요.");

            return null;
        }

        /// <summary>
        /// Checks if a manager of a specified type exists.
        /// </summary>
        /// <typeparam name="T">The type of manager to check.</typeparam>
        /// <returns>Returns true if a manager of the specified type exists; otherwise, false.</returns>
        public static bool Has<T>() where T : class
            => Managers.ContainsKey(typeof(T));

        /// <summary>
        /// Retrieves all the types that are decorated with the SingletonAttribute and are not abstract.
        /// </summary>
        /// <returns>An array of Type objects representing the singleton types.</returns>
        public static Type[] GetAllSingletonTypes()
        {
            List<Type> types = new();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"Unable to load type from assembly : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                    types.AddRange(assemblyTypes.Where(t => t.GetCustomAttribute<SingletonAttribute>() != null && !t.IsAbstract));
            }

            return types.ToArray();
        }
    }
}
