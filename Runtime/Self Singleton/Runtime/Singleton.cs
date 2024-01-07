using UnityEngine;

namespace USingleton.SelfSingleton
{
    public abstract class Singleton: MonoBehaviour  
    {
        /// <summary>
        /// 새로운 씬이 로드될 때 게임 오브젝트가 파괴되는 것을 방지합니다.
        /// </summary>
        /// <returns>
        /// 기본적으로 true를 반환하며, false를 지정할 시 새로운 씬이 로드될 때 게임 오브젝트가 파괴됩니다.
        /// </returns>
        protected virtual bool DontDestroyOnLoad()
        {
            return true;
        }
        
        protected virtual void Awake()
        {
            SingletonManager.Create(this, DontDestroyOnLoad());
            name = GetType().Name;
        }
        
        protected virtual void OnDestroy()
        {
            SingletonManager.Release(this);
        }
    }
}