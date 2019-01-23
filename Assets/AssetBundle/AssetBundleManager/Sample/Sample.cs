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
            InsertLogText(string.Format("Download {0} : {1}", AssetBundleName, bundle.IsNotNull()));

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

        public void OnDownloadAllBundles()
        {
            string[] downloadList = AssetBundleManager.Instance.NeedDownloadList();

            double downloadSize = AssetBundleManager.Instance.CapacityDownloadBundles(downloadList);

            InsertLogText(string.Format("Download Size : {0}", downloadSize));

            AssetBundleManager.Instance.DownloadBundles(downloadList, (success) =>
            {
                InsertLogText(string.Format("DownloadAllBundles {0}", success));

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

        public void OnClickBundleLoad()
        {

        }

        public void OnClickBundleUnload()
        {
            
        }

        private void InsertLogText(string text)
        {
            LogText.text = LogText.text.Insert(LogText.text.Length, string.Format("{0}\n", text));
        }
    }
}