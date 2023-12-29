using UnityEngine;

namespace AutoSingleton
{
    public abstract class SelfSingleton : MonoBehaviour
    {
        protected virtual bool UseDontDestroyOnLoad()
        {
            return true;
        }

        protected virtual void OnDestroy()
        {

            Singleton.Release(this);
        }
    }


}
