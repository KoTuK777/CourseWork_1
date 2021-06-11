using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Examples
{
    public class ObjectComparer : IEqualityComparer<object>
    {
        public new bool Equals([AllowNull] object obj1, [AllowNull] object obj2)
        {

            if (ReferenceEquals(obj1, null) != ReferenceEquals(obj2, null))
                return false;

            if (obj1.GetType() != obj2.GetType())
                return false;

            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is string)
                return obj1.Equals(obj2);

            Type type = obj1.GetType();

            if (DoesImplementIEnumerable(type))
                return CompareIEnumerables(obj1, obj2);

            if (type.IsClass || type.IsInterface)
                return CompareProperties(obj1, obj2);

            if (type.IsPrimitive || type.IsEnum)
                return obj1.Equals(obj2);

            if (IsNullable(type))
                return CompareNullable(obj1, obj2);

            if (type.IsValueType)
                return CompareProperties(obj1, obj2);

            return obj1.Equals(obj2);
        }

        public int GetHashCode([DisallowNull] object obj)
        {
            return obj.GetHashCode();
        }

        private bool DoesImplementIEnumerable(Type type)
        {
            return type.GetInterface("IEnumerable`1") is not null;
        }

        private bool CompareIEnumerables(object obj1, object obj2)
        {
            Type elemType = obj1.GetType()
                .GetInterface("IEnumerable`1")
                .GetGenericArguments()
                .First();

            object[] arguments;

            if (elemType.IsPrimitive)
            {
                arguments = new[] { obj1, obj2 };
            }
            else
            {
                arguments = new[] { obj1, obj2, this };
            }

            MethodInfo sequenceEqualMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(mi => mi.Name == "SequenceEqual" && mi.GetParameters().Length == arguments.Length)
                .MakeGenericMethod(elemType);

            return (bool)sequenceEqualMethod.Invoke(null, arguments);
        }

        private bool CompareProperties(object obj1, object obj2)
        {
            PropertyInfo[] props = obj1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            IEnumerable<PropertyInfo> nonIndexers = props.Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (PropertyInfo propInfo in nonIndexers)
            {
                object a = propInfo.GetValue(obj1, null);
                object b = propInfo.GetValue(obj2, null);

                if (!this.Equals(a, b))
                    return false;
            }

            return true;
        }

        private bool CompareNullable(object obj1, object obj2)
        {
            Type type = obj1.GetType();
            Type underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType.IsPrimitive)
                return obj1.Equals(obj2);

            PropertyInfo valueProp = type.GetProperty("Value");
            var a = valueProp.GetValue(obj1, null);
            var b = valueProp.GetValue(obj2, null);

            return CompareProperties(a, b);
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
