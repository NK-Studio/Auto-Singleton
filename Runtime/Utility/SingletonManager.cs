using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace UnityEngine.Singleton
{
    /// <summary>
    /// 이 클래스는 SingletonAttribute를 기반으로 싱글톤 개체의 생성 및 릴리스를 관리합니다.
    /// </summary>
    public static class SingletonManager
    {
        internal static readonly Dictionary<Type, MonoBehaviour> Managers = new();

        /// <summary>
        /// Singleton Attribute를 사용하고 추상 클래스가 아닌 모든 클래스를 검색합니다.
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
                    types.AddRange(assemblyTypes.Where(t =>
                        t.GetCustomAttribute<SingletonAttribute>() != null && !t.IsAbstract));
            }

            return types.ToArray();
        }

        /// <summary>
        /// 이 메서드는 USingletonSettings에 제공된 설정을 기반으로 모든 관리자 개체를 자동으로 생성합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void AutoCreateAll()
        {
            // 매니저 초기화
            Managers.Clear();

            // 제외 목록
            string[] exclusionList = USingletonSettings.CurrentSettings.ExcludedManagers;

            if (USingletonSettings.CurrentSettings.ShowDebugLog)
                Debug.Log("Initializing all managers...");

            // 매니저 목록을 가져와서 생성
            foreach (GameObject manager in USingletonSettings.CurrentSettings.GetManagerList())
            {
                // 제외 목록에 있는지 확인
                if (exclusionList != null && exclusionList.ToList().Contains(manager.name))
                {
                    if (USingletonSettings.CurrentSettings.ShowDebugLog)
                        Debug.Log(
                            $"매니저 : {manager.name}가 USingleton Settings에 excludedManagers리스트에 있습니다. 생성을 무시합니다.");
                    continue;
                }

                // 매니저 타입 찾기
                Type type = FindManagerType(manager.name);

                if (HasManager(type))
                    continue;

                // Resources 폴더에서 매니저 프리팹을 로드
                GameObject managerPrefab = Resources.Load<GameObject>($"Managers/{manager.name}");

                // 해당 오브젝트를 생성
                GameObject gameObject = Object.Instantiate(managerPrefab);
                gameObject.name = manager.name;

                // 씬이 변경되어도 삭제되지 않도록 설정
                Object.DontDestroyOnLoad(gameObject);

                // 해당 오브젝트에 싱글톤 컴포넌트를 추가
                MonoBehaviour comp = (MonoBehaviour)gameObject.GetComponent(manager.name);

                // 매니저에 추가
                if (type != null)
                    Managers.Add(type, comp);

                // 디버그 메세지 출력
                if (USingletonSettings.CurrentSettings.ShowDebugLog)
                    Debug.Log($" -> {type.Name} 생성 완료");
            }
        }

        /// <summary>
        /// Releases the specified singleton instance.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        public static void Release<T>() where T : MonoBehaviour
        {
            if (!Managers.ContainsKey(typeof(T)))
                return;
            
            Managers.Remove(typeof(T));
        }

        /// <summary>
        /// Returns the manager type through managerTypeName.
        /// </summary>
        /// <param name="managerTypeName">찾고자 하는 매니저의 타입 이름입니다.</param>
        /// <returns>찾은 매니저의 타입을 반환합니다. 찾지 못하면 null을 반환합니다.</returns>
        private static Type FindManagerType(string managerTypeName)
        {
            Type[] managerList = GetAllSingletonAttributeTypes();

            foreach (var type in managerList)
            {
                if (type.Name == managerTypeName)
                    return type;
            }

            return null;
        }

        /// <summary>
        /// Adds a manager of the specified type to the Managers dictionary.
        /// </summary>
        /// <param name="manager">The manager instance to be added.</param>
        /// <param name="isDontDestroyOnLoad"></param>
        public static void Add<T>(MonoBehaviour manager, bool isDontDestroyOnLoad = true) where T : MonoBehaviour
        {
            if (Managers.ContainsKey(typeof(T)))
                return;

            Managers.Add(typeof(T), manager);

            if (isDontDestroyOnLoad)
                Object.DontDestroyOnLoad(manager.gameObject);
        }

        public static bool HasManager(Type type)
        {
            return Managers.ContainsKey(type);
        }

        /// <summary>
        /// Checks if a manager of the specified type exists in the Managers dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the MonoBehaviour instance to check for.</typeparam>
        /// <returns>Returns true if a manager of the specified type exists in the Managers dictionary, otherwise false.</returns>
        public static bool HasManager<T>() where T : MonoBehaviour
        {
            return Managers.ContainsKey(typeof(T));
        }
    }

    /// <summary>
    /// Singleton 클래스는 지정된 유형의 인스턴스 존재 여부를 검색하고 확인하기 위한 정적 메서드를 제공합니다. 이는 특정 인스턴스가 하나만 보장되도록 설계되었습니다.
    /// 애플리케이션에 유형이 존재합니다.
    /// </summary>
    public static class Singleton
    {
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Returns an instance of the specified generic type T from the Managers dictionary if it exists. Otherwise, returns null.
        /// </summary>
        /// <typeparam name="T">The type of the MonoBehaviour instance to search for.</typeparam>
        /// <returns>An instance of the specified generic type T from the Managers dictionary if it exists. Otherwise, returns null.</returns>
        public static T Instance<T>() where T : MonoBehaviour
        {
            // If there is a manager
            if (SingletonManager.Managers.ContainsKey(typeof(T)))
                return (T)SingletonManager.Managers[typeof(T)];

            if (USingletonSettings.CurrentSettings.ShowDebugLog)
            {
                Debug.LogError(
                    $"Manager: '{typeof(T)}' has not been accessed. Please check if this manager has been excluded in the Auto Singleton Settings.");
            }

            return null;
        }
    }
}