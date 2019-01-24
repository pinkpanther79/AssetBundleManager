namespace AssetBundle
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;

    public class Sample : MonoBehaviour
    {
        [SerializeField]
        private Text LogText = null;

        [SerializeField]
        private string BaseUri = string.Empty;

        [SerializeField]
        private string AssetBundleName = string.Empty;

        private void Start()
        {
            InsertLogText(string.Format("Set DownloadURL {0}", BaseUri));
        }

        public void OnClickInit()
        {
            AssetBundleManager.Instance.Initialize(BaseUri, (success) =>
            {
                InsertLogText(string.Format("AssetBundleManager Initialize {0}", success));

                if (success)
                {
                    /// TODO : insert after actions
                }
                else
                {
                    /// TODO : insert retry actions
                }
            });
        }

        public void OnDownloadBundle()
        {
            double downloadSize = AssetBundleManager.Instance.CapacityDownloadBundle(AssetBundleName);

            InsertLogText(string.Format("Download Size : {0}", downloadSize));

            AssetBundleManager.Instance.DownloadBundle(AssetBundleName, (bundle) =>
            {
            InsertLogText(string.Format("DownloadBundle {0} : {1}", AssetBundleName, bundle.IsNotNull()));

                if (bundle.IsNotNull())
                {
                    /// TODO : insert after actions
                }
                else
                {
                    /// TODO : insert retry actions
                }
            });
        }

        public void OnDownloadWithDependencies()
        {
            double downloadSize = AssetBundleManager.Instance.CapacityDownloadBundle(AssetBundleName);
            
            InsertLogText(string.Format("Download Size : {0}", downloadSize));

            AssetBundleManager.Instance.DownloadBundleWithDependencies(AssetBundleName, (success) =>
            {
                InsertLogText(string.Format("DownloadWithDependencies {0}", success));

                if (success)
                {
                    /// TODO : insert after actions
                }
                else
                {
                    /// TODO : insert retry actions
                }
            });
        }

        public void OnClickDownloadAllBundles()
        {
            string[] downloadList = AssetBundleManager.Instance.DownloadList();

            double downloadSize = AssetBundleManager.Instance.CapacityDownloadBundles(downloadList);

            InsertLogText(string.Format("Download Size : {0}", downloadSize));

            AssetBundleManager.Instance.DownloadAssetBundles(downloadList, (success) =>
            {
                InsertLogText(string.Format("ClickDownloadAllBundles {0}", success));

                if (success)
                {
                    /// TODO : insert after actions
                }
                else
                {
                    /// TODO : insert retry actions
                }
            });
        }
        
        private void InsertLogText(string text)
        {
            LogText.text = LogText.text.Insert(LogText.text.Length, string.Format("{0}\n", text));
        }
    }
}