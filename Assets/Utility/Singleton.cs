namespace AssetBundle
{
    using UnityEngine;
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance.IsNull())
            {
                Instance = this as T;
           
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}