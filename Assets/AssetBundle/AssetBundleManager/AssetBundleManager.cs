﻿namespace AssetBundle
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    
    public class AssetBundleManager : Singleton<AssetBundleManager>, IDisposable
    {
        private Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();
        private AssetBundleManifest m_AssetManifest = null;
        private BundleSizeInfo[] m_BundleSizeInfos = null;
        public string BaseUri{ get; private set; } = string.Empty;

        private bool Initialized
        {
            get
            {
                return m_AssetManifest.IsNotNull();
            }
        }
        
        private const long m_CacheSize = 4L * 1024L * 1024L * 1024L;
        
        public void Initialize(string uri, Action<bool> callback)
        {
            MakeBaseUri(uri);

            InternalInitialize(callback);
        }

        private void MakeBaseUri(string baseUri)
        {
            /// TODO : make custom base uri
            BaseUri = string.Format("{0}/{1}/assetbundles", baseUri, AssetBundleUtility.GetPlatformForAssetBundles(Application.platform));
        }

        public string[] NeedDownloadList()
        {
            List<string> needDownladBundles = new List<string>();

            string[] bundleNames = m_AssetManifest.GetAllAssetBundles();
            foreach (var name in bundleNames)
            {
                if (FindLoadedBundle(RemapVariantName(name)).IsNull())
                {
                    string url = string.Format("{0}/{1}", BaseUri, name);

                    bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(name));

                    if (isCaching.IsFalse())
                    {
                        needDownladBundles.Add(name);
                    }
                }
            }

            return needDownladBundles.ToArray();
        }

        public double CapacityDownloadBundle(string bundleName)
        {
            return DownloadCapacity(bundleName);
        }

        public double CapacityDownloadBundles(string[] bundleName)
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
            BundleSizeInfo[] infos = Array.FindAll(m_BundleSizeInfos, (info) =>
            {
                return info.BundleName.Contains(string.Format(".{0}", variantType.ToString()));
            });

            double TotalLength = 0;
            foreach (var info in infos)
            {
                TotalLength += info.BundleSize;
            }

            return Math.Round(Convert.ToDouble((TotalLength / 1024) / 1024));
        }

        public void DownloadBundle(string bundleName, Action<AssetBundle> callback)
        {
            bundleName = RemapVariantName(bundleName);
            
            if (FindLoadedBundle(bundleName).IsNull())
            {
                Hash128 hash = m_AssetManifest.GetAssetBundleHash(bundleName);
                AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, bundleName, hash, (bundle) =>
                {
                    if (bundle.IsNotNull())
                    {
                        InsertBundle(bundleName, bundle);

                        callback(bundle);
                    }
                });

                StartCoroutine(downloader.Download());
            }
            else
            {
                callback(FindLoadedBundle(bundleName));
            }
        }

        public void DownloadBundleWithDependencies(string bundleName, Action<AssetBundle> callback)
        {
            bundleName = RemapVariantName(bundleName);

            if (FindLoadedBundle(bundleName).IsNull())
            {
                Hash128 hash = m_AssetManifest.GetAssetBundleHash(bundleName);
                AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, bundleName, hash, (bundle) =>
                {
                    if (bundle.IsNotNull())
                    {
                        InsertBundle(bundleName, bundle);

                        string[] dependencies = m_AssetManifest.GetAllDependencies(bundleName);
                        StartCoroutine(InteralDownloadBundles(dependencies, (success) =>
                        {
                            callback(bundle);
                        }));
                    }
                });

                StartCoroutine(downloader.Download());
            }
            else
            {
                callback(FindLoadedBundle(bundleName));
            }
        }

        public void DownloadBundles(string[] bundleNames, Action<bool> callback)
        {
            if (bundleNames.NotEmpty())
            {
                StartCoroutine(InteralDownloadBundles(bundleNames, callback));
            }
            else
            {
                Debug.LogError("bundleNames is Empty!!");
            }
        }

        private IEnumerator InternalInitialize(Action<bool> callback)
        {
            if (Application.isMobilePlatform)
            {
                CheckCacheSize();
                
                ManifestDownloader manifestDownloader = new ManifestDownloader(BaseUri, (manifest) =>
                {
                    m_AssetManifest = manifest;
                });

                yield return StartCoroutine(manifestDownloader.Download());

                BundleSizeDownloader sizeDownloader = new BundleSizeDownloader(BaseUri, (infos) =>
                {
                    m_BundleSizeInfos = infos; 
                });

                yield return StartCoroutine(sizeDownloader.Download());
            }

            callback(m_AssetManifest.IsNotNull() && m_BundleSizeInfos.IsNotNull());
        }
        
        private double DownloadCapacity(string bundleName)
        {
            string findBundleName = RemapVariantName(bundleName);
            string url = string.Format("{0}/{1}", BaseUri, findBundleName);

            bool isCaching = Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(findBundleName));

            if (isCaching.IsFalse())
            {
                BundleSizeInfo sizeinfo = Array.Find(m_BundleSizeInfos, (info) =>
                {
                    return info.BundleName.Equals(bundleName);
                });

                if (sizeinfo.IsNotNull())
                {
                    return Math.Round(Convert.ToDouble((sizeinfo.BundleSize / 1024) / 1024));
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
        
        private IEnumerator InteralDownloadBundles(string[] assetBundleNames, Action<bool> callback)
        {
            if (m_AssetManifest.IsNotNull())
            {
                if (assetBundleNames.NotEmpty())
                {
                    foreach (var bundleName in assetBundleNames)
                    {
                        string remapName = RemapVariantName(bundleName);
                        if (FindLoadedBundle(remapName).IsNull())
                        {
                            Hash128 hash = m_AssetManifest.GetAssetBundleHash(remapName);
                            AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, remapName, hash, (bundle) =>
                            {
                                if (bundle.IsNotNull())
                                {
                                    InsertBundle(remapName, bundle);
                                }
                            });

                            yield return StartCoroutine(downloader.Download());
                        }
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
        
        private AssetBundle FindLoadedBundle(string bundleName)
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
                
            if (Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
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

        public void Dispose()
        {
            foreach (var bundle in m_AssetBundles.Values)
            {
                if (bundle != null)
                {
                    bundle.Unload(true);
                }
            }

            m_AssetBundles.Clear();
        }
    }
}
