using UnityEngine;

namespace USingleton.SelfSingleton
{
    public abstract class Singleton: MonoBehaviour  
    {
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