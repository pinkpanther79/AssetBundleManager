namespace AssetBundle
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class AssetBundleManager : MonoBehaviour
    {
        public static AssetBundleManager Instance;
        public static bool UseAssetBundle = false;
        public static bool Initialized = false;

        private static Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();

        private AssetBundleManifest m_AssetManifest = null;

        private string m_DownloadURL = string.Empty;
        public string DownloadURL
        {
            set
            {
                m_DownloadURL = value;
            }
        }

        private static System.Action m_FailedDownload = null;
        public static System.Action onFailedDownload
        {
            set
            {
                m_FailedDownload = value;
            }
        }

        private static System.Action m_SuccessDownload = null;
        public static System.Action SuccessDownload
        {
            set
            {
                m_SuccessDownload = value;
            }
        }

        private long m_CacheSize = 4L * 1024L * 1024L * 1024L;

        internal void OnAwake()
        {
            Instance = this;
        }

        public IEnumerator Initialize(System.Action<bool> callback)
        {
            Debug.LogFormat("Start AssetBundle Manager : Initialize {0}", Time.frameCount);

            if (UseAssetBundle || Application.isMobilePlatform)
            {
                CheckCacheSize();

                yield return StartCoroutine(MakeBundleManifest());
            }

            callback(m_AssetManifest.IsNotNull());

            Debug.LogFormat("End AssetBundle Manager : Initialize {0}", Time.frameCount);
        }

        public void NeedDownloadList(System.Action<List<string>> callback)
        {
            Debug.Assert(m_AssetManifest.IsNotNull(), "Manifest is Null!! Need Initialize");

            List<string> needDownladBundles = new List<string>();

            string[] bundleNames = m_AssetManifest.GetAllAssetBundles();
            for (int i = 0; i < bundleNames.Length; i++)
            {
                if (GetLoadedBundle(bundleNames[i]).IsNull())
                {
                    string url = string.Format("{0}/{1}", m_DownloadURL, bundleNames[i]);

                    bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(bundleNames[i]));

                    if (isCaching.IsFalse())
                    {
                        needDownladBundles.Add(bundleNames[i]);
                    }
                }
            }

            callback(needDownladBundles);
        }

        public IEnumerator DownloadBundles(string[] bundleNames, System.Action<bool> callback)
        {
            if (bundleNames.NotEmpty())
            {
                foreach (var bundleName in bundleNames)
                {
                    yield return DownloadBundle(bundleName);
                }
            }
            else
            {
                Debug.Log("bundleNames is Empty!!");
            }
        }

        public IEnumerator DownloadBundle(string bundleName, System.Action<AssetBundle> callback)
        {
            AssetBundle bundle = null;

            if (GetLoadedBundle(bundleName).IsNull())
            {
                string url = string.Format("{0}/{1}", m_DownloadURL, bundleName);

                bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(bundleName));

                if (isCaching)
                {
                    Caching.MarkAsUsed(url, m_AssetManifest.GetAssetBundleHash(bundleName));
                }

                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url, m_AssetManifest.GetAssetBundleHash(bundleName), 0))
                {
                    yield return www.SendWebRequest();

                    bool isSuccess = !www.isNetworkError;
                    if (isSuccess && www.responseCode == 200)
                    {
                        bundle = DownloadHandlerAssetBundle.GetContent(www);

                        InsertBundle(bundleName, bundle);

                        yield return LoadDependencies(bundleName);
                    }
                }
            }
            else
            {
                bundle = GetLoadedBundle(bundleName);
            }

            callback(bundle);
        }

        private void CheckCacheSize()
        {
            if (PlayerPrefs.GetString("maximumAvailableDiskSpace").IsNullOrEmpty())
            {
                Cache cache = Caching.currentCacheForWriting;
                cache.maximumAvailableStorageSpace = m_CacheSize;

                PlayerPrefs.SetString("maximumAvailableDiskSpace", m_CacheSize.ToString());
            }
        }

        private IEnumerator MakeBundleManifest()
        {
            string url = string.Format("{0}/{1}?{2}", m_DownloadURL, AssetBundleUtility.GetPlatformForAssetBundles(Application.platform), Random.Range(0, 99999));

            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                yield return www.SendWebRequest();

                bool isSuccess = !www.isNetworkError;
                if (isSuccess && www.responseCode == 200)
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

                    if (bundle.IsNotNull())
                    {
                        m_AssetManifest = bundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                    }
                }
            }
        }

        private IEnumerator DownloadBundle(string bundleName)
        {
            if (GetLoadedBundle(bundleName).IsNull())
            {
                string url = string.Format("{0}/{1}", m_DownloadURL, bundleName);

                CacheMarkAsUsed(url, bundleName);

                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url, m_AssetManifest.GetAssetBundleHash(bundleName), 0))
                {
                    yield return www.SendWebRequest();

                    bool isSuccess = !www.isNetworkError;
                    if (isSuccess && www.responseCode == 200)
                    {
                        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

                        InsertBundle(bundleName, bundle);
                    }
                }
            }
        }

        private IEnumerator LoadDependencies(string assetBundleName)
        {
            if (m_AssetManifest.IsNotNull())
            {
                string[] dependencies = m_AssetManifest.GetAllDependencies(assetBundleName);
                if (dependencies.NotEmpty())
                {
                    foreach (var asset in dependencies)
                    {
                        yield return StartCoroutine(DownloadBundle(asset));
                    }
                }
            }
        }

        private void CacheMarkAsUsed(string url, string bundleName)
        {
            bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(bundleName));
            
            if (isCaching)
            {
                Caching.MarkAsUsed(url, m_AssetManifest.GetAssetBundleHash(bundleName));
            }
        }
        
        private AssetBundle GetLoadedBundle(string bundleName)
        {
            m_AssetBundles.TryGetValue(bundleName, out AssetBundle bundle);

            return bundle;
        }

        private void InsertBundle(string bundleName, AssetBundle bundle)
        {
            Debug.LogFormat("m_AssetBundles.Add({0}) : {1}", bundleName, m_AssetManifest.GetAssetBundleHash(bundleName));

            if (m_AssetBundles.ContainsKey(bundleName))
            {
                if (m_AssetBundles[bundleName].IsNotNull())
                {
                    m_AssetBundles[bundleName].Unload(true);
                }

                m_AssetBundles.Remove(bundleName);
            }

            m_AssetBundles.Add(bundleName, bundle);
        }
    }
}
