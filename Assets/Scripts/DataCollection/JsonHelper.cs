using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

struct JsonDateTime
{
    public long dateTime;
    public static implicit operator System.DateTime(JsonDateTime jdt)
    {
        return System.DateTime.FromFileTimeUtc(jdt.dateTime);
    }
    public static implicit operator JsonDateTime(System.DateTime dt)
    {
        JsonDateTime jdt = new JsonDateTime();
        jdt.dateTime = dt.ToFileTimeUtc();
        return jdt;
    }
}