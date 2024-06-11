using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace UnityEngine.USingleton
{
    [CustomPropertyDrawer(typeof(TypeDropDownAttribute))]
    public class TypeDropDownPropertyDrawer : PropertyDrawer
    {
        private Dictionary<string, List<string>> assignableTypeNames;

        private Type type;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (type == null)
                type = ((TypeDropDownAttribute)attribute).BaseType;

            CacheType(type);
            string typeName = type.FullName;

            int index = assignableTypeNames[typeName].IndexOf(property.stringValue);

            EditorGUI.BeginChangeCheck();

            int newVal = EditorGUI.Popup(position, index, assignableTypeNames[typeName].ToArray());

            if (EditorGUI.EndChangeCheck() && index != newVal)
                property.stringValue = assignableTypeNames[typeName][newVal];
        }

        private void CacheType(Type baseType)
        {
            if (assignableTypeNames != null)
                return;
            
            assignableTypeNames = new Dictionary<string, List<string>>();

            string key = baseType.FullName;

            if (string.IsNullOrWhiteSpace(key))
                return;
            
            if (!assignableTypeNames.ContainsKey(key))
                assignableTypeNames.Add(key, new List<string>());

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Type> singletonChildrenClass = from type in assembly.GetTypes()
                    where type.GetCustomAttributes(typeof(SingletonAttribute), true).Length > 0
                    select type;

                foreach (Type singletonChildClass in singletonChildrenClass)
                {
                    // 키에 해당하는 리스트가 있으면 불러오고, 없으면 새로 생성
                    if (!assignableTypeNames.TryGetValue(key, out List<string> typeList))
                    {
                        typeList = new List<string>();
                        assignableTypeNames[key] = typeList;
                    }
                        
                    // 리스트에 클래스 이름 추가
                    typeList.Add(singletonChildClass.Name);
                }
            }
        }
    }
}
