namespace AssetBundle
{
    using System;
    using UnityEngine;

    public class JsonHelper : MonoBehaviour
    {
        public static T[] GetJsonArray<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);

            return wrapper.array;
        }

        public static string ArrayToJson<T>(T[] array, bool prettyPrint = false)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.array = array;

            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}