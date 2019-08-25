using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlLockFinder.Tests.Util
{
    public static class ObjectGeneratorExtensions
    {
        public static T WithId<T>(this T obj, int val)
        {
            var id = typeof(T).GetProperties().FirstOrDefault(p => p.Name == "Id");

            if (id != null)
            {
                id.SetValue(obj, val);
            }

            return obj;
        }

        public static List<T> WithUnique<T>(this List<T> collection, string propertyName = "Id")
        {
            var allUnique = false;
            while (!allUnique)
            {
                var id = typeof(T).GetProperties().FirstOrDefault(p => p.Name == propertyName);

                if (id == null)
                    return collection;

                foreach (var item in collection)
                {
                    var duplicate = collection.FirstOrDefault(x => !x.Equals(item) && id.GetValue(x).Equals(id.GetValue(item)));

                    if (duplicate != null)
                    {
                        SetValueOnProperty(duplicate, id, true);
                    }

                    allUnique = collection.All(x => id.GetValue(x) != id.GetValue(item));
                }
            }

            return collection;
        }

        internal static void SetValueOnProperty<T>(T obj, PropertyInfo propertyInfo, bool useNullOnUnknownType)
        {
            object value = GetRandomValue(propertyInfo.PropertyType);

            try
            {
                if (!useNullOnUnknownType && value == null)
                {
                    return;
                }
                propertyInfo.SetValue(obj, value);
            }
            catch (ArgumentException)
            {
                // ignore and don't set property
            }
        }

        internal static object GetRandomValue(Type propertyType)
        {
            if (propertyType == typeof(string))
            {
                return Generator.GetRandomString(7);
            }

            if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
            {
                return Guid.NewGuid();
            }

            if (propertyType == typeof(DateTime))
            {
                return Generator.GetRandomDateTime();
            }

            if (propertyType == typeof(DateTime?))
            {
                return new DateTime?(Generator.GetRandomDateTime());
            }

            if (propertyType == typeof(int) || propertyType == typeof(int?) || propertyType == typeof(Int64) || propertyType == typeof(Int64?) || propertyType == typeof(Int16) ||
                propertyType == typeof(Int16?))
            {
                return Generator.GetRandomNumber(Int16.MaxValue);
            }

            if (propertyType == typeof(double) || propertyType == typeof(double?))
            {
                return (double)Generator.GetRandomNumber(1000) / 100;
            }

            if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
            {
                return (decimal)Generator.GetRandomNumber(1000) / 100;
            }

            if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
            {
                return (decimal)Generator.GetRandomNumber(1000) / 100;
            }

            if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType.GenericTypeArguments.Any())
            {
                return typeof(List<>).MakeGenericType(propertyType.GenericTypeArguments.First()).GetConstructors().First().Invoke(null);
            }

            // note: expand with more types possibly
            return null;
        }
    }
}