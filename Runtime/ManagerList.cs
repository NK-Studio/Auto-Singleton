using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace USingleton
{
    [CreateAssetMenu(fileName = "ManagerList", menuName = "ManagerList" )]
    public class ManagerList : ScriptableObject
    {
        private static ManagerList _instance;
        public static ManagerList Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                
                _instance = Resources.Load<ManagerList>("ManagerList");

#if UNITY_EDITOR
                if (_instance == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    _instance = AssetDatabase.LoadAssetAtPath<ManagerList>("Assets/Resources/ManagerList.asset");

                    if (_instance == null)
                    {
                        _instance = CreateInstance<ManagerList>();
                        AssetDatabase.CreateAsset(_instance, "Assets/Resources/ManagerList.asset");
                    }
                }

#endif
                
                return _instance;
            }
        }
        
        public List<GameObject> managers;

        /// <summary>
        /// 인자로 들어온 매니저를 리스트에 추가합니다.
        /// </summary>
        /// <param name="manager">추가할 매니저 프리팹</param>
        public void AddManager(GameObject manager)
        {
            managers.Add(manager);
        }
    }
}
