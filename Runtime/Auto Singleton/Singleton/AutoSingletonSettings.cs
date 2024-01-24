using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace USingleton.AutoSingleton
{
    /// <summary>
    /// AutoSingleton 기능에 대한 설정을 나타냅니다.
    /// </summary>
    public class AutoSingletonSettings : ScriptableObject
    {
        public string[] ExcludedManagers => excludedManagers;

        /// <summary>
        /// 자동으로 생성하지 않을 싱글턴 매니저의 이름을 등록합니다.
        /// </summary>
        [SerializeField, TypeDropDown(typeof(SingletonAttribute))]
        protected string[] excludedManagers;

        /// <summary>
        /// 어드레서블 에셋 시스템을 활용하여 싱글턴을 활성화할 수 있는 기능을 제공합니다.
        /// </summary>
        [SerializeField]
        private bool useAddressable;

        /// <summary>
        /// 싱글턴 객체를 생성할 때 디버그 모드를 활성화할 수 있는 기능을 제공합니다.
        /// </summary>
        [SerializeField]
        protected bool showDebugLog;
        public bool ShowDebugLog => showDebugLog;
        private const string KAssetName = "Auto Singleton Settings";

        private const string AddressableDefine = "USINGLETON_USE_ADDRESSABLE";

        /// <summary>
        /// 현재 설정을 가져옵니다.
        /// </summary>
        /// <remarks>
        /// 설정 에셋을 사용할 수 있으면 이를 로드하고 반환합니다.
        /// 그렇지 않으면 설정을 생성하기 위한 기본 생성자를 반환합니다.
        /// </remarks>
        /// <value>
        /// 현재 설정입니다.
        /// </value>
        public static AutoSingletonSettings CurrentSettings =>
            HasSettingAsset ? Resources.Load<AutoSingletonSettings>(KAssetName) : DefaultCreator;

        /// <summary>
        /// 설정 에셋이 존재하는지 여부를 나타내는 값을 가져옵니다.
        /// </summary>
        /// <remarks>
        /// 설정 에셋은 'Auto Singleton Settings' 이름을 사용하여 리소스 폴더에서 로드됩니다.
        /// 에셋이 발견되면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </remarks>
        public static bool HasSettingAsset => Resources.Load<AutoSingletonSettings>(KAssetName) != null;

        /// <summary>
        /// AutoSingleton 클래스의 기본 설정을 나타냅니다.
        /// </summary>
        public static AutoSingletonSettings DefaultCreator
        {
            get
            {
                if (_defaultCreator == null)
                    _defaultCreator = CreateDefaultSettings();
                return _defaultCreator;
            }
        }

        private static AutoSingletonSettings _defaultCreator;

        private static AutoSingletonSettings CreateDefaultSettings()
        {
            AutoSingletonSettings defaultAsset = CreateInstance<AutoSingletonSettings>();

#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(defaultAsset, "Assets/Resources/Auto Singleton Settings.asset");
            AssetDatabase.SaveAssets();
#endif

            defaultAsset.excludedManagers = Array.Empty<string>();
            return defaultAsset;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateDefineSymbols();
        }
#endif

        /// <summary>
        /// Updates the scripting define symbols for the selected build target group.
        /// If 'useAddressable' is true, it adds the 'AddressableDefine' symbol to the existing symbols.
        /// If 'useAddressable' is false, it removes the 'AddressableDefine' symbol from the existing symbols.
        /// </summary>
        /// <remarks>
        /// This method only takes effect when running in Unity Editor.
        /// </remarks>
        private void UpdateDefineSymbols()
        {
#if UNITY_EDITOR
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (useAddressable)
            {
                if (defines.Contains(AddressableDefine))
                    return;

                defines += $";{AddressableDefine}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            }
            else
            {
                if (!defines.Contains(AddressableDefine))
                    return;

                defines = defines.Replace(AddressableDefine, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            }
#endif
        }
    }
}
