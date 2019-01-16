namespace AssetBundle
{
    using UnityEngine;
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T m_Instance;
        public static T Instance
        {
            get
            {
                return m_Instance;
            }
        }

        protected virtual void Awake()
        {
            if (m_Instance.IsNull())
            {
                m_Instance = this as T;
           
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}