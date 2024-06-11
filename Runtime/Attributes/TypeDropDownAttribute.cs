using System;

namespace UnityEngine.USingleton
{
    /// <summary>
    /// 인스펙터에서 속성에 대한 드롭다운 유형 선택을 지정하는 데 사용되는 사용자 정의 속성을 나타냅니다.
    /// </summary>
    public class TypeDropDownAttribute : PropertyAttribute
    {
        public readonly Type BaseType;

        public TypeDropDownAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
