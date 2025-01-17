﻿#if UNITY_EDITOR
using UnityEngine;

namespace PSS
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance = new T();
        public static T Get { get { return _instance; } }
    }
}
#endif