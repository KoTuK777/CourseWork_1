using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapper
{
    public class Mapper
    {
        public List<TypePair> Types { get; set; } = new List<TypePair>();

        public Mapper Add<T, K>()
            where T : class, new()
            where K : class, new()
        {
            var sourceType = typeof(T);
            var destinationType = typeof(K);

            var typePair = new TypePair(sourceType, destinationType);

            Types.Add(typePair);

            return this;
        }

        public TResult Map<TResult>(object source)
            where TResult : class, new()
        {
            var sourceType = source.GetType();
            var destinationType = typeof(TResult);

            var typePair = new TypePair(sourceType, destinationType);

            var isPairExist = Types.Any(x => x.Equals(typePair));

            if (!isPairExist)
                throw new Exception("This type pair does not exist in Types");

            return this.CopyProperties(source, typeof(TResult)) as TResult;
        }

        private object CopyProperties(object source, Type resultType)
        {
            var sourceType = source.GetType();

            var result = Activator.CreateInstance(resultType);

            const BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly;

            var resultFields = resultType
                .GetFields(bindingFlags);

            var resultFieldsDictionary = resultFields
                .ToDictionary(x => x.Name);

            var sourceFields = sourceType
                .GetFields(bindingFlags);

            foreach (var sourceField in sourceFields)
            {
                if (resultFieldsDictionary.TryGetValue(sourceField.Name, out FieldInfo resultField))
                {
                    if (resultField.FieldType != sourceField.FieldType)
                    {
                        var typePair = new TypePair(sourceField.FieldType, resultField.FieldType);

                        if (Types.Any(x => x.Equals(typePair)))
                        {
                            var value = this.CopyProperties(sourceField.GetValue(source), resultField.FieldType);
                            resultField.SetValue(result, value);
                        }

                        continue;
                    }

                    var fieldType = resultField.FieldType;

                    if (DoesImplementIEnumerable(fieldType) && fieldType != typeof(string))
                    {
                        var value = CopyIEnumerables(sourceField.GetValue(source));

                        resultField.SetValue(result, MapCollections(value, fieldType));

                        continue;
                    }

                    if (fieldType.IsValueType || fieldType == typeof(string))
                    {
                        var value = sourceField.GetValue(source);
                        resultField.SetValue(result, value);

                        continue;
                    }

                    CopyProperties(sourceField.GetValue(source), resultField.GetValue(result));
                }
            }

            return result;
        }

        private void CopyProperties(object source, object destination)
        {
            var type = source?.GetType();

            if (type != destination?.GetType())
                return;

            const BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly;

            var fields = type
                .GetFields(bindingFlags);

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                if (fieldType.IsValueType || fieldType == typeof(string))
                {
                    var value = field.GetValue(source);
                    field.SetValue(destination, value);
                }
                else
                {
                    var destinationSubObj = Activator.CreateInstance(fieldType);
                    var sourceSubObj = field.GetValue(source);

                    CopyProperties(sourceSubObj, destinationSubObj);

                    field.SetValue(destination, destinationSubObj);
                }
            }
        }

        public object CopyIEnumerables(object source)
        {
            var sourceElemType = source.GetType()
                .GetInterface("IEnumerable`1")
                .GetGenericArguments()
                .First();

            var toArray = typeof(Enumerable).GetMethods()
                .Single(method => method.Name == "ToArray" && method.IsStatic);

            var sourceArray = toArray.MakeGenericMethod(sourceElemType)
                .Invoke(null, new object[] { source }) as Array;

            var resArray = Array.CreateInstance(sourceElemType, sourceArray.Length);

            int index = 0;
            foreach (var sourceItem in sourceArray)
            {
                var value = CopyProperties(sourceItem, sourceElemType);
                resArray.SetValue(value, index);

                index += 1;
            }

            var castArray = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "Cast")
                .MakeGenericMethod(sourceElemType);

            var res = castArray.Invoke(null, new object[] { resArray });

            return res;
        }

        public object MapCollections(object array, Type type)
        {
            var elemType = array.GetType()
                .GetInterface("IEnumerable`1")
                .GetGenericArguments()
                .First();

            var toList = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "ToList")
                .MakeGenericMethod(elemType);

            var toArray = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "ToArray")
                .MakeGenericMethod(elemType);

            if (type.IsArray)
            {
                return toArray.Invoke(null, new object[] { array });
            }

            var a = type.GetInterfaces();


            if (type.GetInterface("IList`1") is not null)
            {
                return toList.Invoke(null, new object[] { array });
            }

            return array;
        }

        private bool DoesImplementIEnumerable(Type type)
        {
            return type.GetInterface("IEnumerable`1") is not null;
        }
    }
}
