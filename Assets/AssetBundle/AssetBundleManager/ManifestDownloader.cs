namespace AssetBundle
{
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections;

    public class ManifestDownloader : Downloader<AssetBundleManifest>
    {
        public ManifestDownloader(string baseUri, System.Action<AssetBundleManifest> OnComplete)
        {
            m_OnComplete = OnComplete;

            MakeUrl(baseUri);
        }

        protected override void MakeUrl(string baseUri)
        {
            m_Url = string.Format("{0}/{1}?{2}", baseUri, AssetBundleUtility.GetPlatformForAssetBundles(Application.platform), Random.Range(0, 99999));
        }

        public override IEnumerator Download()
        {
            AssetBundleManifest manifest = null;
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(m_Url))
            {
                yield return www.SendWebRequest();

                bool isSuccess = !www.isNetworkError && www.responseCode == 200;
                if (isSuccess)
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

                    if (bundle.IsNotNull())
                    {
                        manifest = bundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                    }
                }

                m_OnComplete(manifest);
            }
        }
    }
}