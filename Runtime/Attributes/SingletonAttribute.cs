using System;

namespace UnityEngine.USingleton
{
    /// <summary>
    /// SingletonAttribute는 클래스의 단일 인스턴스만 애플리케이션에 존재해야 할 경우 사용됩니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SingletonAttribute : Attribute
    {
        /// <summary>
        /// 매니저로 전환합니다.
        /// </summary>
        public SingletonAttribute()
        {
        }
    }
}
