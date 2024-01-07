using System;
using System.Collections.Generic;
using System.Reflection;
using AutoSingleton;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using USingleton;
using USingleton.AutoSingleton;
using Object = UnityEngine.Object;
using Singleton = USingleton.SelfSingleton.Singleton;

#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

namespace AutoSingleton
{
    [CustomEditor(typeof(AutoSingletonSettings))]
    public class AutoSingletonEditor : Editor
    {
        private VisualTreeAsset visualTreeAsset;
        private StyleSheet styleSheet;

        private VisualElement _root;

        private SerializedProperty _excludedManagers;

        public override VisualElement CreateInspectorGUI()
        {
            // Load
            string uxmlPath = AssetDatabase.GUIDToAssetPath("18e07ed21749147c69cb3a3e3195a9a1");
            visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            string ussPath = AssetDatabase.GUIDToAssetPath("2174863b709254c95ac78e9e9c5f06a1");
            styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);

            // Apply
            _root = visualTreeAsset.CloneTree();
            _root.styleSheets.Add(styleSheet);

            var listContainer = _root.Q<IMGUIContainer>("List-Container");

            _excludedManagers = serializedObject.FindProperty("excludedManagers");

            PropertyField addressableToggle = _root.Q<PropertyField>("propertyField-addressable");

            addressableToggle.tooltip = Application.systemLanguage switch {
                SystemLanguage.Korean => "Resources 폴더가 아닌 Addressable Asset System을 활용하여 싱글턴을 활성화할 수 있는 기능을 제공합니다.",
                _ => "Provides the ability to activate singletons using the Addressable Asset System instead of the Resources folder."
            };

#if !USE_ADDRESSABLES
            var addresableGroupBox = _root.Q<GroupBox>("groupBox-addressable");
            addresableGroupBox.style.display = DisplayStyle.None;
#endif

            listContainer.onGUIHandler = () => {
                EditorGUILayout.PropertyField(_excludedManagers);
                serializedObject.ApplyModifiedProperties();
            };

            listContainer.tooltip = Application.systemLanguage switch {
                SystemLanguage.Korean => "자동으로 생성하지 않을 싱글턴 매니저의 이름을 등록합니다.",
                _ => "Register the name of the singleton manager that will not be automatically created."
            };

            PropertyField showDebugLogToggle = _root.Q<PropertyField>("propertyField-ShowDebugLog");
            showDebugLogToggle.tooltip = Application.systemLanguage switch {
                SystemLanguage.Korean => "싱글턴 객체를 생성할 때 생성되었는지 로그를 출력합니다.",
                _ => "Prints a log when a singleton object is created."
            };

            return _root;
        }

        [MenuItem("Tools/USingleton/Refresh")]
        private static void ForceRefresh()
        {
            // Auto Singleton Create
            Type[] allAutoSingletonTypes = SingletonManager.GetAllSingletonAttributeTypes();
            foreach (Type type in allAutoSingletonTypes)
            {
                var attribute = type.GetCustomAttribute<SingletonAttribute>();
                if (attribute != null)
                {
                    string path = string.Empty;

                    // Resources 폴더에 생성
                    if (!attribute.UseAddressable)
                    {
                        // 파일이 존재하는지 체크
                        path = "Assets/Resources/Managers/" + type.Name + ".prefab";

                        // Resources 폴더가 없으면 생성합니다.
                        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                            AssetDatabase.CreateFolder("Assets", "Resources");

                        // Resources/Managers 폴더가 없으면 생성합니다.
                        if (!AssetDatabase.IsValidFolder("Assets/Resources/Managers"))
                            AssetDatabase.CreateFolder("Assets/Resources", "Managers");
                    }
#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE
                    else
                    {
                        // 파일이 존재하는지 체크
                        path = "/Managers/Prefabs/" + type.Name + ".prefab";

                        // Resources 폴더가 없으면 생성합니다.
                        if (!AssetDatabase.IsValidFolder("Assets/Managers"))
                            AssetDatabase.CreateFolder("Assets", "Managers");

                        if (!AssetDatabase.IsValidFolder("Assets/Managers/Prefabs"))
                            AssetDatabase.CreateFolder("Assets/Managers", "Prefabs");
                    }
#endif

                    // 파일이 존재하는지 체크
                    if (!AssetDatabase.LoadAssetAtPath<GameObject>(path))
                    {
                        // 없으면 생성
                        GameObject prefab = new GameObject(type.Name);
                        prefab.AddComponent(type);

                        // 프리팹 생성
                        PrefabUtility.SaveAsPrefabAsset(prefab, path);

#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE
                        // 추가적인 어드레서블 처리
                        if (attribute.UseAddressable)
                        {
                            // Addressable 주소 설정
                            AddressableAssetSettings targetSetting = AddressableAssetSettingsDefaultObject.GetSettings(true);

                            AddressableAssetGroup targetGroup = targetSetting.FindGroup("Manager");

                            if (!targetGroup)
                            {
                                targetGroup = targetSetting.CreateGroup("Manager", false, false, false, null,
                                    typeof(BundledAssetGroupSchema));
                            }

                            //생성한 프리팹을 다시 로드해서 가져옵니다.
                            Object target = AssetDatabase.LoadAssetAtPath<Object>($"Assets{path}");

                            string assetPath = AssetDatabase.GetAssetPath(target);
                            string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);

                            var e = targetSetting.CreateOrMoveEntry(assetGUID, targetGroup, false, false);
                            e.address = type.Name;
                            var entriesAdded = new List<AddressableAssetEntry> { e };

                            targetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
                            targetSetting.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

                        }
#endif

                        // 프리팹을 만들기 위해 씬에 배치한 임시 프리팹을 삭제한다.
                        DestroyImmediate(prefab);
                    }
                }
            }

            // Self Singleton Create
            Type[] allSelfSingletonTypes = SingletonManager.GetAllSingletonTypes();
            foreach (Type type in allSelfSingletonTypes)
            {
                // Resources 폴더에 생성
                string path = "Assets/Resources/Managers/" + type.Name + ".prefab";

                // Resources 폴더가 없으면 생성합니다.
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                // Resources/Managers 폴더가 없으면 생성합니다.
                if (!AssetDatabase.IsValidFolder("Assets/Resources/Managers"))
                    AssetDatabase.CreateFolder("Assets/Resources", "Managers");

                // 파일이 존재하는지 체크
                if (!AssetDatabase.LoadAssetAtPath<GameObject>(path))
                {
                    // 없으면 생성
                    GameObject prefab = new(type.Name);
                    prefab.AddComponent(type);

                    // 프리팹 생성
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);

                    // 프리팹을 만들기 위해 씬에 배치한 임시 프리팹을 삭제한다.
                    DestroyImmediate(prefab);
                }
            }

            // 새로고침
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/USingleton/Settings")]
        private static void OpenSettings()
        {
            var settings = AutoSingletonSettings.CurrentSettings;
            Selection.activeObject = settings;
        }

        [MenuItem("Tools/USingleton/About", false, 2000)]
        private static void About()
        {
            var path = AssetDatabase.GUIDToAssetPath("dd38d53d7bf7b40fa960de2e03525ea4");
            var packageJson = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            PackageInfo info = JsonUtility.FromJson<PackageInfo>(packageJson.text);

            Debug.Log($"USingleton v{info.version}");
        }

        [Serializable]
        internal class PackageInfo
        {
            public string name;
            public string displayName;
            public string version;
            public string unity;
            public string description;
            public List<string> keywords;
            public string type;
        }

    }
}
