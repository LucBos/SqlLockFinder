using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlLockFinder.Tests.Util
{
    public class ObjectGenerator
    {
        public static List<T> GenerateList<T>(int nrOfItems = -1, Func<object> constructor = null, Action<T> manipulate = null, string[] ignoreProperties = null)
        {
            var list = new List<T>();
            if (nrOfItems == -1)
            {
                nrOfItems = Generator.GetRandomNumber(10);
            }
            for (int i = 0; i < nrOfItems; i++)
            {
                var generated = (T)Generate(typeof(T), constructor, true, ignoreProperties);
                manipulate?.Invoke(generated);
                list.Add(generated);
            }

            return list;
        }


        public static T Generate<T>(Func<object> constructor = null, bool useNullOnUnknownType = true, string[] ignoreProperties = null)
        {
            return (T)Generate(typeof(T), constructor, useNullOnUnknownType, ignoreProperties);
        }

        public static T Generate<T>(Action<T> manipulate)
        {
            var instance = (T)Generate(typeof(T));
            manipulate(instance);
            return instance;
        }

        public static object Generate(Type type, Func<object> constructor = null, bool useNullOnUnknownType = true, string[] ignoreProperties = null)
        {
            if (constructor == null)
            {
                constructor = () => ConstructNewType(type);
            }

            var obj = constructor.Invoke();
            var allProperties = obj.GetType().GetProperties();
            if (ignoreProperties != null)
            {
                allProperties = allProperties.Where(p => !ignoreProperties.Contains(p.Name)).ToArray();
            }

            foreach (var propertyInfo in allProperties)
            {
                ObjectGeneratorExtensions.SetValueOnProperty(obj, propertyInfo, useNullOnUnknownType);
            }

            return obj;
        }

        private static object ConstructNewType(Type type)
        {
            var constructors = type.GetConstructors();
            var constructorWithMostParameters = constructors.FirstOrDefault(x => x.GetParameters().Count() == constructors.Min(y => y.GetParameters().Count()));
            if (constructorWithMostParameters == null)
            {
                throw new Exception($"Could not find constructor to construct type {type.Name}");
            }
            var constructorParameters = constructorWithMostParameters.GetParameters();
            var parameters = new object[constructorParameters.Count()];

            for (int i = 0; i < parameters.Count(); i++)
            {
                parameters[i] = ObjectGeneratorExtensions.GetRandomValue(constructorParameters[i].ParameterType);
            }

            return constructorWithMostParameters.Invoke(parameters);
        }

        public static List<string> GenerateStringList(int count, int stringLength)
        {
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                list.Add(Generator.GetRandomString(stringLength));
            }
            return list;
        }
    }
}