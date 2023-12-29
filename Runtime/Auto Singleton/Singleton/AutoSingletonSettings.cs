using System;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace USingleton.AutoSingleton
{
    public class AutoSingletonSettings : ScriptableObject
    {
        public string[] ExcludedManagers => excludedManagers;

        public bool ShowDebugCustomManager => showDebugCustomManager;

        [SerializeField, TypeDropDown(typeof(SingletonAttribute))]
        protected string[] excludedManagers;

        [SerializeField]
        protected bool showDebugCustomManager;

        private const string KAssetName = "Auto Singleton Settings";

        public static AutoSingletonSettings CurrentSettings =>
            HasSettingAsset ? Resources.Load<AutoSingletonSettings>(KAssetName) : DefaultCreator;
        
        public static bool HasSettingAsset => Resources.Load<AutoSingletonSettings>(KAssetName) != null;

        /// <summary>
        /// Represents the default settings for the AutoSingleton class.
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
    }
}