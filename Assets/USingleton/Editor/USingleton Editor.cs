using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UnityEngine.Singleton
{
    [CustomEditor(typeof(USingletonSettings))]
    public class USingletonEditor : Editor
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

             PropertyField managerListField = _root.Q<PropertyField>("managerList-field");
             managerListField.SetEnabled(false);

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
            Assert.IsNotNull(USingletonSettings.CurrentSettings, "USingletonSettings is null");

            // 기존 리스트 삭제
            USingletonSettings.CurrentSettings.ClearManagerList();
            
            // Singleton 어트리뷰트를 가지고 있는 클래스를 모두 가져옵니다.
            Type[] allUSingletonTypes = SingletonManager.GetAllSingletonAttributeTypes();
            
            // all Search
            foreach (Type type in allUSingletonTypes)
            {
                SingletonAttribute attribute = type.GetCustomAttribute<SingletonAttribute>();
                if (attribute != null)
                {
                    var path =
                        // Resources 폴더에 생성
                        // 파일이 존재하는지 체크
                        "Assets/Resources/Managers/" + type.Name + ".prefab";

                    // Resources 폴더가 없으면 생성합니다.
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");

                    // Resources/Managers 폴더가 없으면 생성합니다.
                    if (!AssetDatabase.IsValidFolder("Assets/Resources/Managers"))
                        AssetDatabase.CreateFolder("Assets/Resources", "Managers");

                    // 파일이 존재하는지 체크
                    bool isExist = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (!isExist)
                    {
                        // 없으면 생성
                        GameObject prefab = new GameObject(type.Name);
                        prefab.AddComponent(type);

                        // 프리팹 생성
                        PrefabUtility.SaveAsPrefabAsset(prefab, path);
                        
                        // 프리팹을 만들기 위해 씬에 배치한 임시 프리팹을 삭제한다.
                        DestroyImmediate(prefab);
                    }
                    
                    // 리스트에 추가
                    var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    USingletonSettings.CurrentSettings.AddManager(targetPrefab);
                }
            }

            // 새로고침
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/USingleton/Settings")]
        private static void OpenSettings()
        {
            var settings = USingletonSettings.CurrentSettings;
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
