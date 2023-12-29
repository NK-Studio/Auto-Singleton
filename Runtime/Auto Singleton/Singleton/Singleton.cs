using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

#if AUTO_SINGLETON_USE_ADDRESSABLE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
#endif

namespace USingleton.AutoSingleton
{
    /// <summary>
    /// Generic access class for singleton instances.
    /// </summary>
    public static class Singleton
    {

    }
}
