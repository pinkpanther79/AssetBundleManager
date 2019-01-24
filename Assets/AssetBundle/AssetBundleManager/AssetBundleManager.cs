namespace AssetBundle
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class AssetBundleManager : Singleton<AssetBundleManager>, IDisposable
    {
        private Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();
        
        private AssetBundleManifest m_AssetManifest = null;
        private BundleSizeInfo[] m_BundleSizeInfos = null;
        public string BaseUri { get; private set; } = string.Empty;

        private bool Initialized
        {
            get
            {
                return m_AssetManifest.IsNotNull() && m_BundleSizeInfos.IsNotNull();
            }
        }

        private const long m_CacheSize = 4L * 1024L * 1024L * 1024L;
        
        public void Initialize(string uri, Action<bool> callback)
        {
            if (Initialized)
            {
                BaseUri = string.Empty;
                m_AssetManifest = null;
                m_BundleSizeInfos = null;
            }

            MakeBaseUri(uri);

            StartCoroutine(InternalInitialize(callback));
        }

        private void MakeBaseUri(string baseUri)
        {
            /// TODO : make custom base uri
            BaseUri = string.Format("{0}/{1}/assetbundles", baseUri, AssetBundleUtility.GetPlatformForAssetBundles(Application.platform));
        }

        public string[] DownloadList()
        {
            List<string> needDownladBundles = new List<string>();

            if (Initialized)
            {
                string[] bundleNames = m_AssetManifest.GetAllAssetBundles();
                foreach (var name in bundleNames)
                {
                    string remapName = RemapVariantName(name);
                    if (DownloadedBundle(remapName).IsNull())
                    {
                        string url = string.Format("{0}/{1}", BaseUri, remapName);

                        if (BundleCached(url, remapName).IsFalse())
                        {
                            needDownladBundles.Add(name);
                        }
                    }
                }
            }

            return needDownladBundles.ToArray();
        }

        public double CapacityDownloadBundle(string bundleName)
        {
            double size = 0;

            if (Initialized)
            {
                size = DownloadCapacity(RemapVariantName(bundleName));
            }

            return size;
        }

        public double CapacityDownloadBundles(string[] bundleName)
        {
            double totalSize = 0;

            if (Initialized)
            {
                foreach (var name in bundleName)
                {
                    totalSize = DownloadCapacity(RemapVariantName(name));
                }
            }

            return totalSize;
        }

        public double CapacityVariantBundles(AssetBundleUtility.eVariantType variantType)
        {
            double totalSize = 0;

            if (Initialized)
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

                totalSize = Math.Round(Convert.ToDouble((TotalLength / 1024) / 1024));
            }

            return totalSize;
        }

        public void DownloadBundle(string bundleName, Action<AssetBundle> callback)
        {
            if (Initialized)
            {
                string remapName = RemapVariantName(bundleName);

                if (DownloadedBundle(remapName).IsNull())
                {
                    Hash128 hash = m_AssetManifest.GetAssetBundleHash(remapName);
                    AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, remapName, hash, (bundle) =>
                    {
                        InsertBundle(remapName, bundle);

                        callback(bundle);
                    });

                    StartCoroutine(downloader.Download());
                }
                else
                {
                    callback(DownloadedBundle(remapName));
                }
            }
        }

        public void DownloadBundleWithDependencies(string bundleName, Action<AssetBundle> callback)
        {
            if (Initialized)
            {
                string remapName = RemapVariantName(bundleName);

                if (DownloadedBundle(remapName).IsNull())
                {
                    Hash128 hash = m_AssetManifest.GetAssetBundleHash(remapName);
                    AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, remapName, hash, (bundle) =>
                    {
                        if (bundle.IsNotNull())
                        {
                            InsertBundle(remapName, bundle);

                            string[] dependencies = m_AssetManifest.GetAllDependencies(remapName);
                            DependenciesDownload(new DependenciesContainer(remapName, dependencies, callback));
                        }
                        else
                        {
                            callback(null);
                        }
                    });

                    StartCoroutine(downloader.Download());
                }
                else
                {
                    string[] dependencies = m_AssetManifest.GetAllDependencies(remapName);
                    DependenciesDownload(new DependenciesContainer(remapName, dependencies, callback));
                }
            }
        }

        public void DownloadAssetBundles(string[] bundleNames, Action<bool> callback)
        {
            if (Initialized && bundleNames.NotEmpty())
            {
                InternalDownloadAssetBundles(new BundlesDownloadContainer(bundleNames, callback));
            }
        }

        private void InternalDownloadAssetBundles(BundlesDownloadContainer container)
        {
            if (container.Bundles.Empty())
            {
                container.OnComplete(true);
            }
            else
            {
                string targetBundleName = RemapVariantName(container.Bundles.Pop());
                if (DownloadedBundle(RemapVariantName(targetBundleName)).IsNull())
                {
                    Hash128 hash = m_AssetManifest.GetAssetBundleHash(targetBundleName);
                    AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, targetBundleName, hash, (bundle) =>
                    {
                        if (bundle.IsNotNull())
                        {
                            InsertBundle(targetBundleName, bundle);

                            InternalDownloadAssetBundles(container);
                        }
                        else
                        {
                            container.OnComplete(false);
                        }
                    });

                    StartCoroutine(downloader.Download());
                }
                else
                {
                    InternalDownloadAssetBundles(container);
                }
            }
        }

        private void DependenciesDownload(DependenciesContainer container)
        {
            if (container.Bundles.Empty())
            {
                container.OnComplete(DownloadedBundle(container.BundleName));
            }
            else
            {
                string targetBundleName = RemapVariantName(container.Bundles.Pop());
                if (DownloadedBundle(targetBundleName).IsNull())
                {
                    Hash128 hash = m_AssetManifest.GetAssetBundleHash(targetBundleName);
                    AssetBundleDownloader downloader = new AssetBundleDownloader(BaseUri, targetBundleName, hash, (bundle) =>
                    {
                        if (bundle.IsNotNull())
                        {
                            InsertBundle(targetBundleName, bundle);

                            DependenciesDownload(container);
                        }
                        else
                        {
                            container.OnComplete(null);
                        }
                    });

                    StartCoroutine(downloader.Download());
                }
                else
                {
                    DependenciesDownload(container);
                }
            }
        }
        
        private IEnumerator InternalInitialize(Action<bool> callback)
        {
            CacheSize();

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

            callback(Initialized);
        }
        
        private double DownloadCapacity(string bundleName)
        {
            string url = string.Format("{0}/{1}", BaseUri, bundleName);
            
            if (BundleCached(url, bundleName).IsFalse())
            {
                BundleSizeInfo sizeinfo = Array.Find(m_BundleSizeInfos, (info) =>
                {
                    return info.BundleName.Equals(bundleName);
                });

                if (sizeinfo.IsNotNull())
                {
                    return Math.Round(Convert.ToDouble(sizeinfo.BundleSize / 1024));
                }
            }

            return 0;
        }
        
        private void CacheSize()
        {
            string cacheSize = PlayerPrefs.GetString("maximumAvailableDiskSpace");
            if (cacheSize.IsNullOrEmpty() && cacheSize.Equals(m_CacheSize.ToString()).IsFalse())
            {
                Cache cache = Caching.currentCacheForWriting;
                cache.maximumAvailableStorageSpace = m_CacheSize;

                PlayerPrefs.SetString("maximumAvailableDiskSpace", m_CacheSize.ToString());
            }
        }
        
        private void CacheMarkAsUsed(string url, string bundleName)
        {
            if (BundleCached(url, bundleName))
            {
                Caching.MarkAsUsed(url, m_AssetManifest.GetAssetBundleHash(bundleName));
            }
        }
        
        private AssetBundle DownloadedBundle(string bundleName)
        {
            m_AssetBundles.TryGetValue(bundleName, out AssetBundle bundle);

            return bundle;
        }

        private void InsertBundle(string bundleName, AssetBundle bundle)
        {
            if (bundle.IsNotNull())
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

        private bool BundleCached(string url, string bundleName)
        {
            return Caching.IsVersionCached(url, m_AssetManifest.GetAssetBundleHash(bundleName));
        }

        private string RemapVariantName(string bundleName)
        {
            string[] bundlesWithVariant = m_AssetManifest.GetAllAssetBundlesWithVariant();

            if (Array.FindIndex(bundlesWithVariant, (find) =>
            {
                return find.Split('.')[0].Equals(bundleName);
            }) < 0)
            {
                return bundleName;
            }

            string[] split = bundleName.Split('.');

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

            return bundleName;
        }
        
        public void Dispose()
        {
            foreach (var bundle in m_AssetBundles.Values)
            {
                if (bundle.IsNotNull())
                {
                    bundle.Unload(true);
                }
            }

            m_AssetBundles.Clear();
        }
    }
}