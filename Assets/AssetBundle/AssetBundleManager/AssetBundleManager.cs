﻿namespace AssetBundle
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        private Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();
        private AssetBundleManifest m_AssetManifest = null;
        private BundleSizeInfo[] m_BundleSizeInfos = null;

        private string m_BaseUri = string.Empty;
        public string BaseUri
        {
            set
            {
                m_BaseUri = value;
            }
        }

        private bool Initialized
        {
            get
            {
                return m_AssetManifest.IsNotNull();
            }
        }
        
        private const long m_CacheSize = 4L * 1024L * 1024L * 1024L;
        
        public void Initialize(System.Action<bool> callback)
        {
            StartCoroutine(StartInitialize(callback));
        }

        public string[] NeedDownloadList()
        {
            List<string> needDownladBundles = new List<string>();

            string[] bundleNames = m_AssetManifest.GetAllAssetBundles();
            for (int i = 0; i < bundleNames.Length; i++)
            {
                if (GetLoadedBundle(RemapVariantName(bundleNames[i])).IsNull())
                {
                    string url = string.Format("{0}/{1}", m_BaseUri, bundleNames[i]);

                    bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(bundleNames[i]));

                    if (isCaching.IsFalse())
                    {
                        needDownladBundles.Add(bundleNames[i]);
                    }
                }
            }

            return needDownladBundles.ToArray();
        }

        public double CapacityDownloadBundle(string[] bundleName)
        {
            double totalSize = 0;
            foreach (var name in bundleName)
            {
                totalSize = DownloadCapacity(name);
            }

            return totalSize;
        }

        public double CapacityVariantBundles(AssetBundleUtility.eVariantType variantType)
        {
            BundleSizeInfo[] infos = System.Array.FindAll(m_BundleSizeInfos, (info) =>
            {
                return info.BundleName.Contains(string.Format(".{0}", variantType.ToString()));
            });

            double TotalLength = 0;
            foreach (var info in infos)
            {
                TotalLength += info.BundleSize;
            }

            return System.Math.Round(System.Convert.ToDouble((TotalLength / 1024) / 1024));
        }

        public void DownloadBundles(string[] bundleNames, System.Action callback)
        {
            if (bundleNames.NotEmpty())
            {
                StartCoroutine(StartDownloadBundles(bundleNames, callback));
            }
            else
            {
                Debug.LogError("bundleNames is Empty!!");
            }
        }

        public void DownloadBundle(string bundleName, System.Action<AssetBundle> callback)
        {
            bundleName = RemapVariantName(bundleName);
            
            if (GetLoadedBundle(bundleName).IsNull())
            {
                StartCoroutine(StartDownloadBundle(bundleName, callback));
            }
            else
            {
                callback(GetLoadedBundle(bundleName));
            }
        }

        private IEnumerator StartInitialize(System.Action<bool> callback)
        {
            if (Application.isMobilePlatform)
            {
                CheckCacheSize();

                yield return StartCoroutine(MakeBundleManifest());

                yield return StartCoroutine(MakeBundleSizeInfos());
            }

            callback(m_AssetManifest.IsNotNull() && m_BundleSizeInfos.IsNotNull());
        }

        private IEnumerator StartDownloadBundles(string[] bundleNames, System.Action callback)
        {
            foreach (var bundleName in bundleNames)
            {
                yield return DownloadBundle(RemapVariantName(bundleName));
            }

            callback();
        }

        private IEnumerator StartDownloadBundle(string bundleName, System.Action<AssetBundle> callback)
        {
            string url = string.Format("{0}/{1}", m_BaseUri, bundleName);

            bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(bundleName));

            if (isCaching)
            {
                Caching.MarkAsUsed(url, m_AssetManifest.GetAssetBundleHash(bundleName));
            }

            AssetBundle bundle = null;

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

            callback(bundle);
        }

        private double DownloadCapacity(string bundleName)
        {
            string findBundleName = RemapVariantName(bundleName);
            string url = string.Format("{0}/{1}", m_BaseUri, findBundleName);

            bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(findBundleName));

            if (isCaching.IsFalse())
            {
                BundleSizeInfo sizeinfo = System.Array.Find(m_BundleSizeInfos, (info) =>
                {
                    return info.BundleName.Equals(bundleName);
                });

                if (sizeinfo.IsNotNull())
                {
                    return System.Math.Round(System.Convert.ToDouble((sizeinfo.BundleSize / 1024) / 1024));
                }
            }

            return 0;
        }
        
        private void CheckCacheSize()
        {
            string cacheSize = PlayerPrefs.GetString("maximumAvailableDiskSpace");
            if (cacheSize.IsNullOrEmpty() && cacheSize.Equals(m_CacheSize.ToString()).IsFalse())
            {
                Cache cache = Caching.currentCacheForWriting;
                cache.maximumAvailableStorageSpace = m_CacheSize;

                PlayerPrefs.SetString("maximumAvailableDiskSpace", m_CacheSize.ToString());
            }
        }

        private IEnumerator MakeBundleManifest()
        {
            string url = string.Format("{0}/{1}?{2}", m_BaseUri, AssetBundleUtility.GetPlatformForAssetBundles(Application.platform), Random.Range(0, 99999));

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

        private IEnumerator MakeBundleSizeInfos()
        {
            string url = string.Format("{0}/{1}?{2}", m_BaseUri, AssetBundleUtility.BundleSizeFileName, Random.Range(0, 99999));

            UnityWebRequest www = new UnityWebRequest(url);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            bool isSuccess = !www.isNetworkError;
            if (isSuccess && www.responseCode == 200)
            {
                m_BundleSizeInfos = JsonHelper.GetJsonArray<BundleSizeInfo>(www.downloadHandler.text);
            }
        }

        private IEnumerator DownloadBundle(string bundleName)
        {
            if (GetLoadedBundle(bundleName).IsNull())
            {
                string url = string.Format("{0}/{1}", m_BaseUri, bundleName);

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
        
        private string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = m_AssetManifest.GetAllAssetBundlesWithVariant();
                
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
            {
                return assetBundleName;
            }

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
                
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] == split[0])
                {
                    int found = AssetBundleUtility.VariantType.ToString().IndexOf(curSplit[1]);
                    if (found != -1 && found < bestFit)
                    {
                        bestFit = found;
                        bestFitIndex = i;
                    }
                }
            }

            if (bestFitIndex != -1)
            {
                return bundlesWithVariant[bestFitIndex];
            }

            return assetBundleName;
        }
    }
}
