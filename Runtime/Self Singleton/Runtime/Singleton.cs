using UnityEngine;

namespace USingleton.SelfSingleton
{
    public abstract class Singleton: MonoBehaviour  
    {
        protected virtual bool UseDontDestroyOnLoad()
        {
            return true;
        }
        
        protected virtual void Awake()
        {
            SingletonManager.Create(this, UseDontDestroyOnLoad());
            name = GetType().Name;
        }
        
        protected virtual void OnDestroy()
        {
            SingletonManager.Release(this);
        }
    }
}