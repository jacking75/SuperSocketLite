using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SuperSocket.Common;

/// <summary>
/// Assembly Util Class
/// </summary>
public static class AssemblyUtil
{
    /// <summary>
    /// Creates the instance from type name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static T CreateInstance<T>(string type)
    {
        return CreateInstance<T>(type, new object[0]);
    }

    /// <summary>
    /// Creates the instance from type name and parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public static T CreateInstance<T>(string type, object[] parameters)
    {
        Type instanceType = null;
        var result = default(T);

        instanceType = Type.GetType(type, true);

        if (instanceType == null)
            throw new Exception(string.Format("The type '{0}' was not found!", type));

        object instance = Activator.CreateInstance(instanceType, parameters);
        result = (T)instance;
        return result;
    }

    


    /// <summary>
    /// Gets the implement types from assembly.
    /// </summary>
    /// <typeparam name="TBaseType">The type of the base type.</typeparam>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
    {
        return assembly.GetExportedTypes().Where(t =>
            t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
    }

    /// <summary>
    /// Gets the implemented objects by interface.
    /// </summary>
    /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
        where TBaseInterface : class
    {
        return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface));
    }

    /// <summary>
    /// Gets the implemented objects by interface.
    /// </summary>
    /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
    /// <param name="assembly">The assembly.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns></returns>
    public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType)
        where TBaseInterface : class
    {
        Type[] arrType = assembly.GetExportedTypes();

        var result = new List<TBaseInterface>();

        for (int i = 0; i < arrType.Length; i++)
        {
            var currentImplementType = arrType[i];

            if (currentImplementType.IsAbstract)
                continue;

            if (!targetType.IsAssignableFrom(currentImplementType))
                continue;

            result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
        }

        return result;
    }

  

    /// <summary>
    /// Copies the properties of one object to another object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    public static T CopyPropertiesTo<T>(this T source, T target)
    {
        return source.CopyPropertiesTo(p => true, target);
    }

    /// <summary>
    /// Copies the properties of one object to another object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="predict">The properties predict.</param>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    public static T CopyPropertiesTo<T>(this T source, Predicate<PropertyInfo> predict, T target)
    {
        PropertyInfo[] properties = source.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

        Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);

        PropertyInfo[] targetProperties = target.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
            .Where(p => predict(p)).ToArray();

        for (int i = 0; i < targetProperties.Length; i++)
        {
            var p = targetProperties[i];
            PropertyInfo sourceProperty;

            if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
            {
                if (sourceProperty.PropertyType != p.PropertyType)
                    continue;

                // 2023.11.28 최흥배  닷넷5부터 지원하지 않으므로 주석 처리 한다
                //if (!sourceProperty.PropertyType.IsSerializable)
                //    continue;

                p.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
        }

        return target;
    }

    
}
