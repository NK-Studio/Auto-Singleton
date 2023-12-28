using System;
using System.Collections.Generic;
using System.Reflection;
using AutoSingleton;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

#if  AUTO_SINGLETON_USE_ADDRESSABLE
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

[CustomEditor(typeof(AutoSingletonSettings))]
public class AutoSingletonEditor : Editor
{
    public VisualTreeAsset visualTreeAsset;

    private VisualElement _root;

    private SerializedProperty _excludedManagers;

    private const string AddressableDefine = "AUTO_SINGLETON_USE_ADDRESSABLE";

    public override VisualElement CreateInspectorGUI()
    {
        _root = visualTreeAsset.CloneTree();

        var listContainer = _root.Q<IMGUIContainer>("List-Container");

        _excludedManagers = serializedObject.FindProperty("excludedManagers");

        listContainer.onGUIHandler = () => {
            EditorGUILayout.PropertyField(_excludedManagers);
            serializedObject.ApplyModifiedProperties();
        };

        return _root;
    }

    [MenuItem("Tools/Auto Singleton/Refresh")]
    private static void ForceRefresh()
    {
        Type[] allManagerTypes = Singleton.GetAllSingletonTypes();

        foreach (Type type in allManagerTypes)
        {
            var attribute = type.GetCustomAttribute<SingletonAttribute>();

            if (attribute != null)
            {
                string path = string.Empty;

                if (!attribute.UseAddressable)
                {
                    // 파일이 존재하는지 체크
                    path = "/Resources/" + type.Name + ".prefab";

                    // Resources 폴더가 없으면 생성합니다.
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                }
#if  AUTO_SINGLETON_USE_ADDRESSABLE
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

                if (!System.IO.File.Exists(Application.dataPath + path))
                {
                    // 없으면 생성
                    GameObject prefab = new GameObject(type.Name);
                    prefab.AddComponent(type);

                    // 프리팹 생성
                    PrefabUtility.SaveAsPrefabAsset(prefab, $"Assets{path}");

#if  AUTO_SINGLETON_USE_ADDRESSABLE
                    // 추가적인 어드레서블 처리
                    if (attribute.UseAddressable)
                    {
                        // Addressable 주소 설정
                        AddressableAssetGroup targetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Manager");

                        if (!targetGroup)
                        {
                            targetGroup = targetGroup.Settings.CreateGroup("Manager", false, false, false, null,
                                typeof(BundledAssetGroupSchema));
                        }

                        //생성한 프리팹을 다시 로드해서 가져옵니다.
                        Object target = AssetDatabase.LoadAssetAtPath<Object>($"Assets{path}");

                        string assetPath = AssetDatabase.GetAssetPath(target);
                        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);

                        var e = targetGroup.Settings.CreateOrMoveEntry(assetGUID, targetGroup, false, false);
                        e.address = type.Name;
                        var entriesAdded = new List<AddressableAssetEntry> { e };

                        targetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
                        targetGroup.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

                    }
#endif

                    // 프리팹을 만들기 위해 씬에 배치한 임시 프리팹을 삭제한다.
                    DestroyImmediate(prefab);
                }
            }
        }

        // 새로고침
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Auto Singleton/Settings")]
    private static void OpenSettings()
    {
        var settings = AutoSingletonSettings.CurrentSettings;
        Selection.activeObject = settings;
    }

    [MenuItem("Tools/Auto Singleton/Use Addressable")]
    private static void UseAddressable()
    {
        var menuPath = "Tools/Auto Singleton/Use Addressable";
        var checkFlag = Menu.GetChecked(menuPath);
        var nextFlag = !checkFlag;
        Menu.SetChecked(menuPath, nextFlag);

        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        
        if (nextFlag)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            defines += $";{AddressableDefine}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }
        else
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            defines = defines.Replace(AddressableDefine, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }
    }
}
