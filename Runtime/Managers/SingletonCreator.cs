using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

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
    public static class SingletonCreator
    {
        /// <summary>
        /// This method automatically creates all the manager objects based on the settings provided in AutoSingletonSettings.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreateAll()
        {
            Singleton.Managers.Clear();

            string[] exclusionList = AutoSingletonSettings.CurrentSettings.ExcludedManagers;

            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                Debug.Log("모든 매니저 초기화 중 ...");

#if AUTO_SINGLETON_USE_ADDRESSABLE && UNITY_EDITOR
            AddressableAssetGroup targetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Manager");
#endif

            foreach (Type type in Singleton.KAllManagerTypes)
            {
                // 제외 목록에 있는지 확인
                if (exclusionList != null && exclusionList.ToList().Contains(type.Name))
                {
                    if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
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
                        GameObject prefab = Resources.Load<GameObject>(attribute.PrefabName);

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
                            Singleton.Managers.Add(type, comp);

                            // 디버그 메세지 출력
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                                Debug.Log($" -> {type.Name} 생성 완료");
                        }
                        else
                        {
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                                Debug.LogError(
                                    $"{type}를 생성 할 수 없습니다. '{attribute.PrefabName}'프리팹을 찾을 수 없습니다.");
                        }
                    }
#if AUTO_SINGLETON_USE_ADDRESSABLE
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
                            Singleton.Managers.Add(type, comp);

                            // 디버그 메세지 출력
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                                Debug.Log($" -> {type.Name} 생성 완료");
                        }
                        else
                        {
                            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
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
    }
}
