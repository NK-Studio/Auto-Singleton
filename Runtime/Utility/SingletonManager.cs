using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using USingleton.AutoSingleton;
using Object = UnityEngine.Object;

#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
#endif

namespace USingleton
{
    public static class SingletonManager
    {
        internal static readonly Dictionary<Type, MonoBehaviour> Managers = new();
        internal static readonly Type[] KAllManagerTypes = GetAllSingletonAttributeTypes();

        /// <summary>
        /// Retrieves all the types that are decorated with the SingletonAttribute and are not abstract.
        /// </summary>
        /// <returns>An array of Type objects representing the singleton types.</returns>
        public static Type[] GetAllSingletonAttributeTypes()
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
                    types.AddRange(assemblyTypes.Where(t => typeof(SelfSingleton.Singleton).IsAssignableFrom(t) && !t.IsAbstract));
            }

            return types.ToArray();
        }
        
        /// <summary>
        /// This method automatically creates all the manager objects based on the settings provided in AutoSingletonSettings.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreateAll()
        {
            Managers.Clear();

            string[] exclusionList = AutoSingletonSettings.CurrentSettings.ExcludedManagers;

            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                Debug.Log("모든 매니저 초기화 중 ...");

#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE && UNITY_EDITOR
            AddressableAssetGroup targetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Manager");
#endif

            foreach (Type type in KAllManagerTypes)
            {
                // 제외 목록에 있는지 확인
                if (exclusionList != null && exclusionList.ToList().Contains(type.Name))
                {
                    if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                        Debug.Log(
                            $"매니저 : {type.Name}가 Auto Singleton Settings에 excludedManagers리스트에 있습니다. 생성을 무시합니다.");
                    continue;
                }

                // 해당 매니저의 어트리뷰트의 내용을 가져온다.
                var attribute = type.GetCustomAttribute<SingletonAttribute>();
 
                if (attribute != null)
                {
                    // Resources 폴더 방식
                    if (!attribute.UseAddressable)
                    {
                        GameObject prefab = Resources.Load<GameObject>($"Managers/{attribute.PrefabName}");

                        // 리소스 폴더에 존재할 경우
                        if (prefab != null)
                        {
                            // 해당 오브젝트를 생성
                            GameObject gameObject = Object.Instantiate(prefab);
                            gameObject.name = type.Name;

                            // 씬이 변경되어도 삭제되지 않도록 설정
                            Object.DontDestroyOnLoad(gameObject);

                            // 해당 오브젝트에 싱글톤 컴포넌트를 추가
                            MonoBehaviour comp = (MonoBehaviour)gameObject.GetComponent(type);

                            // 매니저에 추가
                            Managers.Add(type, comp);

                            // 디버그 메세지 출력
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                                Debug.Log($" -> {type.Name} 생성 완료");
                        }
                        else
                        {
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                                Debug.LogError(
                                    $"{type}를 생성 할 수 없습니다. '{attribute.PrefabName}'프리팹을 찾을 수 없습니다.");
                        }
                    }
#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE
                    // Addressable 방식
                    else
                    {
#if UNITY_EDITOR
                        if (!targetGroup)
                        {
                            Debug.LogError("Manager 그룹이 존재하지 않습니다.");
                            return;
                        }
#endif
                        // Addressable 주소 설정
                        AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(attribute.PrefabName);
                        handle.WaitForCompletion();

                        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Count > 0)
                        {
                            // 객체 생성
                            AsyncOperationHandle<GameObject> managerObject = Addressables.InstantiateAsync(attribute.PrefabName);
                            managerObject.WaitForCompletion();

                            // 해당 오브젝트에 이름 부여
                            managerObject.Result.name = type.Name;

                            // 씬이 변경되어도 삭제되지 않도록 설정
                            Object.DontDestroyOnLoad(managerObject.Result);

                            // 매니저에 추가
                            MonoBehaviour comp = (MonoBehaviour)managerObject.Result.GetComponent(type);
                            Managers.Add(type, comp);

                            // 디버그 메세지 출력
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                                Debug.Log($" -> {type.Name} 생성 완료");
                        }
                        else
                        {
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                                Debug.LogError(
                                    $"{type}를 생성 할 수 없습니다. '{attribute.PrefabName}'프리팹을 찾을 수 없습니다.");
                        }

                        // 핸들 해제
                        Addressables.Release(handle);
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Adds the specified self singleton instance to the managers dictionary and optionally marks it to not be destroyed on scene load.
        /// </summary>
        /// <param name="singleton">The self singleton instance to add.</param>
        /// <param name="useDontDestroyOnLoad">Flag indicating whether to mark the singleton to not be destroyed on scene load.</param>
        /// <returns>Nothing.</returns>
        public static void Create(SelfSingleton.Singleton singleton, bool useDontDestroyOnLoad)
        {
            if (!Managers.ContainsKey(singleton.GetType()))
            {
                Managers.Add(singleton.GetType(), singleton);

                if (useDontDestroyOnLoad)
                    Object.DontDestroyOnLoad(singleton.gameObject);
            }
        }

        /// <summary>
        /// Releases the specified singleton instance.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="singleton">The singleton instance to release.</param>
        public static void Release<T>(T singleton) where T : MonoBehaviour
        {
            Managers.Remove(singleton.GetType());
        }
    }
}

namespace USingleton
{
    public static class Singleton
    {
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Returns an instance of the specified generic type T if it exists in the Managers dictionary. Otherwise, returns null.
        /// </summary>
        /// <typeparam name="T">The type of the MonoBehaviour instance to retrieve</typeparam>
        /// <returns>An instance of the specified generic type T if found in the Managers dictionary. Otherwise, returns null.</returns>
        public static T Instance<T>() where T : MonoBehaviour
        {
            // If there is a manager
            if (SingletonManager.Managers.ContainsKey(typeof(T)))
                return (T)SingletonManager.Managers[typeof(T)];
            
            // If there is no manager
            if (typeof(T).BaseType == typeof(USingleton.SelfSingleton.Singleton))
            {
                T autoCreateInstance = Resources.Load<T>("Managers/" + typeof(T).Name);
                if (autoCreateInstance)
                    return Object.Instantiate(autoCreateInstance);;
            }
            
            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                Debug.LogError($"매니저 : '{typeof(T)}'가 액세스 되지 않았습니다. Auto Singleton Settings에서 해당 매니저가 제외됬는지 확인해주세요.");

            return null;
        }

        /// <summary>
        /// Determines whether an instance of the specified type exists in the SingletonManager.
        /// </summary>
        /// <typeparam name="T">The type of the MonoBehaviour instance to check.</typeparam>
        /// <returns>true if an instance of the specified type exists in the SingletonManager, otherwise false.</returns>
        public static bool HasInstance<T>() where T : MonoBehaviour
        {
            return SingletonManager.Managers.ContainsKey(typeof(T));
        }
    }
}
