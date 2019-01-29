namespace AssetBundle
{
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections;

    public class AssetBundleDownloader : Downloader<AssetBundle>
    {
        private string m_BundleName;
        private Hash128 m_Hash;
        
        public AssetBundleDownloader(string baseUri, string bundleName, Hash128 hash, System.Action<AssetBundle> OnComplete)
        {
            m_BundleName = bundleName;
            m_Hash = hash;
            m_OnComplete = OnComplete;

            MakeUrl(baseUri);
        }

        protected override void MakeUrl(string baseUri)
        {
            m_Url = string.Format("{0}/{1}", baseUri, m_BundleName);
        }

        public override IEnumerator Download()
        {
            AssetBundle bundle = null;
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(m_Url, m_Hash, 0))
            {
                yield return www.SendWebRequest();

                bool isSuccess = !www.isNetworkError;
                if (isSuccess)
                {
                    bundle = DownloadHandlerAssetBundle.GetContent(www);
                }

                m_OnComplete(bundle);
            }
        }
    }
}