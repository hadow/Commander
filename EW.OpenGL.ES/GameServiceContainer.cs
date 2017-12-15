using System;
using System.Collections.Generic;

/// <summary>
/// ÓÎÏ··þÎñÈÝÆ÷
/// </summary>
public class GameServiceContainer:IServiceProvider
{
    Dictionary<Type, object> services;

    public GameServiceContainer()
    {
        services = new Dictionary<Type, object>();
    }

    public void AddService(Type type,object provider)
    {
        services.Add(type, provider);
    }

    public void AddService<T>(T provider)
    {
        AddService(typeof(T), provider);
    }


    public void RemoveService(Type type)
    {
        if (type == null)
            throw new ArgumentException("type");
        services.Remove(type);
    }
    public object GetService(Type type)
    {
        if (type == null)
            throw new ArgumentNullException("type");

        object service;
        if (services.TryGetValue(type, out service))
            return service;
        return null;
    }

    public T GetService<T>() where T :class
    {
        var service = GetService(typeof(T));
        if (service == null)
            return null;
        return (T)service;
    }
}