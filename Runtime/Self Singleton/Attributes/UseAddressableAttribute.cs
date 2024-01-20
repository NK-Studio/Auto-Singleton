using System;

namespace USingleton.SelfSingleton
{
    /// <summary>
    /// SingletonAttribute는 클래스의 단일 인스턴스만 애플리케이션에 존재해야 할 경우 사용됩니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UseAddressableAttribute : Attribute
    {
        /// <summary>
        /// SingletonAttribute는 클래스의 단일 인스턴스만 애플리케이션에 존재해야 할 경우 사용됩니다.
        /// </summary>
        /// <param name="addressableName">Addressable Asset System에서 사용할 이름입니다.</param>
        public UseAddressableAttribute(string addressableName)
        {
            AddressableName = addressableName;
        }

        /// <summary>
        /// Addressable Asset System에서 사용할 이름입니다.
        /// </summary>
        public string AddressableName { get; }
    }
}