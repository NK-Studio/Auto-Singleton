using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

#if USE_ADDRESSABLES && USINGLETON_USE_ADDRESSABLE
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
    /// 싱글톤 인스턴스에 대한 일반 액세스 클래스입니다.
    /// </summary>
    public static class Singleton
    {

    }
}
