namespace AssetBundle
{
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections;

    public class BundleSizeDownloader : Downloader<BundleSizeInfo[]>
    {
        private readonly string m_BundleSizeFileName = "BundleSizeInfos.json";

        public BundleSizeDownloader(string baseUri, System.Action<BundleSizeInfo[]> OnComplete)
        {
            m_OnComplete = OnComplete;

            MakeUrl(baseUri);
        }

        protected override void MakeUrl(string baseUri)
        {
            m_Url = string.Format("{0}/{1}?{2}", baseUri, m_BundleSizeFileName, Random.Range(0, 99999));
        }

        public override IEnumerator Download()
        {
            BundleSizeInfo[] infos = null;
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(m_Url))
            {
                yield return www.SendWebRequest();

                bool isSuccess = !www.isNetworkError && www.responseCode == 200;
                if (isSuccess)
                {
                    infos = JsonHelper.GetJsonArray<BundleSizeInfo>(www.downloadHandler.text);
                }

                m_OnComplete(infos);
            }
        }
    }
}