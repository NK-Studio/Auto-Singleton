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
    /// <summary>
    /// 이 클래스는 SingletonAttribute를 기반으로 싱글톤 개체의 생성 및 릴리스를 관리합니다.
    /// </summary>
    public static class SingletonManager
    {
        internal static readonly Dictionary<Type, MonoBehaviour> Managers = new();
        internal static readonly Type[] KAllManagerTypes = GetAllSingletonAttributeTypes();

        /// <summary>
        /// SingletonAttribute로 장식되고 추상이 아닌 모든 유형을 검색합니다.
        /// </summary>
        /// <returns>싱글톤 유형을 나타내는 Type 객체의 배열입니다.</returns>
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
        /// Singleton 클래스를 상속받은 모든 유형을 검색합니다.
        /// </summary>
        /// <returns>싱글톤 유형을 나타내는 Type 객체의 배열입니다.</returns>
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
        /// 이 메서드는 AutoSingletonSettings에 제공된 설정을 기반으로 모든 관리자 개체를 자동으로 생성합니다.
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
        /// 지정된 자체 싱글톤 인스턴스를 미리 매니저에 추가하고 선택적으로 씬 로드 시 삭제되지 않도록 표시합니다.
        /// </summary>
        /// <param name="singleton">추가할 자체 싱글톤 인스턴스입니다.</param>
        /// <param name="useDontDestroyOnLoad">씬 로드 시 싱글톤이 삭제되지 않도록 표시할지 여부를 나타내는 플래그입니다.</param>
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
        /// 지정된 싱글톤 인스턴스를 해제합니다.
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
    /// <summary>
    /// Singleton 클래스는 지정된 유형의 인스턴스 존재 여부를 검색하고 확인하기 위한 정적 메서드를 제공합니다. 이는 특정 인스턴스가 하나만 보장되도록 설계되었습니다.
    /// 애플리케이션에 유형이 존재합니다.
    /// </summary>
    public static class Singleton
    {
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Managers 사전에 있는 경우 지정된 일반 유형 T의 인스턴스를 반환합니다. 그렇지 않으면 null을 반환합니다.
        /// </summary>
        /// <typeparam name="T">검색할 MonoBehaviour 인스턴스의 유형</typeparam>
        /// <returns>Managers 사전에 있는 경우 지정된 일반 유형 T의 인스턴스입니다. 그렇지 않으면 null을 반환합니다.</returns>
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
                    return Object.Instantiate(autoCreateInstance);
            }

            if (AutoSingletonSettings.CurrentSettings.ShowDebugLog)
                Debug.LogError($"매니저 : '{typeof(T)}'가 액세스 되지 않았습니다. Auto Singleton Settings에서 해당 매니저가 제외됬는지 확인해주세요.");

            return null;
        }

        /// <summary>
        /// SingletonManager에 지정된 유형의 인스턴스가 있는지 여부를 확인합니다.
        /// </summary>
        /// <typeparam name="T">확인할 MonoBehaviour 인스턴스의 유형입니다.</typeparam>
        /// <returns>지정된 유형의 인스턴스가 SingletonManager에 있으면 true이고, 그렇지 않으면 false입니다.</returns>
        public static bool HasInstance<T>() where T : MonoBehaviour
        {
            return SingletonManager.Managers.ContainsKey(typeof(T));
        }

        /// <summary>
        /// SingletonManager에 지정된 유형의 인스턴스가 있는지 여부를 확인합니다.
        /// </summary>
        /// <param name="singleton">확인할 MonoBehaviour 인스턴스의 유형입니다.</param>
        /// <returns>지정된 유형의 인스턴스가 SingletonManager에 있으면 true이고, 그렇지 않으면 false입니다.</returns>
        public static bool HasInstance(SelfSingleton.Singleton singleton)
        {
            return SingletonManager.Managers.ContainsKey(singleton.GetType());
        }
    }
}
