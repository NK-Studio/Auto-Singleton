using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Singleton
{
    /// <summary>
    /// USingleton 기능에 대한 설정을 나타냅니다.
    /// </summary>
    public class USingletonSettings : ScriptableObject
    {
        [SerializeField] private List<GameObject> managerList = new();
        
        public string[] ExcludedManagers => excludedManagers;

        /// <summary>
        /// 자동으로 생성하지 않을 싱글턴 매니저의 이름을 등록합니다.
        /// </summary>
        [SerializeField, TypeDropDown(typeof(SingletonAttribute))]
        protected string[] excludedManagers;
        
        /// <summary>
        /// 싱글턴 객체를 생성할 때 디버그 모드를 활성화할 수 있는 기능을 제공합니다.
        /// </summary>
        [SerializeField]
        protected bool showDebugLog;
        public bool ShowDebugLog => showDebugLog;
        private const string KAssetName = "USingleton Settings";

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
        public static USingletonSettings CurrentSettings =>
            HasSettingAsset ? Resources.Load<USingletonSettings>(KAssetName) : DefaultCreator;

        /// <summary>
        /// 설정 에셋이 존재하는지 여부를 나타내는 값을 가져옵니다.
        /// </summary>
        /// <remarks>
        /// 설정 에셋은 'Auto Singleton Settings' 이름을 사용하여 리소스 폴더에서 로드됩니다.
        /// 에셋이 발견되면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </remarks>
        private static bool HasSettingAsset => Resources.Load<USingletonSettings>(KAssetName) != null;

        /// <summary>
        /// Represents the default settings for the USingleton class.
        /// </summary>
        private static USingletonSettings DefaultCreator
        {
            get
            {
                if (_defaultCreator == null)
                    _defaultCreator = CreateDefaultSettings();
                return _defaultCreator;
            }
        }

        /// <summary>
        /// Returns the list of managers.
        /// </summary>
        /// <returns>The list of GameObjects representing the managers.</returns>
        internal List<GameObject> GetManagerList()
        {
            return managerList;
        }

        /// <summary>
        /// Manager List에 추가합니다.
        /// </summary>
        /// <param name="manager"></param>
        public void AddManager(GameObject manager)
        {
            managerList?.Add(manager);
        }

        /// <summary>
        /// 매니저 리스트를 초기화 합니다.
        /// </summary>
        public void ClearManagerList()
        {
            managerList?.Clear();
        }
        
        private static USingletonSettings _defaultCreator;

        private static USingletonSettings CreateDefaultSettings()
        {
            USingletonSettings defaultAsset = CreateInstance<USingletonSettings>();

#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(defaultAsset, $"Assets/Resources/{KAssetName}.asset");
            AssetDatabase.SaveAssets();
#endif

            defaultAsset.excludedManagers = Array.Empty<string>();
            return defaultAsset;
        }
    }
}
