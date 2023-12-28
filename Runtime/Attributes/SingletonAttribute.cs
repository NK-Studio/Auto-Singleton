using System;

namespace AutoSingleton
{
    /// <summary>
    /// The SingletonAttribute is an attribute class that can be applied to classes to indicate that only a single instance of the class should exist in the application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SingletonAttribute : Attribute
    {
        public string PrefabName { get; }
        public bool UseAddressable { get; }

        /// <summary>
        /// Convert to manager.
        /// </summary>
        /// <param name="prefabName">Name to use when converted to Prefab</param>
        /// <param name="useAddressable">Should I use addressable method?</param>
        public SingletonAttribute(string prefabName, bool useAddressable = false)
        {
            PrefabName = prefabName;
#if !AUTO_SINGLETON_USE_ADDRESSABLE
            UseAddressable = false;
#else
            UseAddressable = useAddressable;
#endif
        }
    }
}