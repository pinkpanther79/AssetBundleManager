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
        
        private void Start()
        {
            AssetBundleManager.Instance.BaseUri = BaseUri;

            InsertLogText(string.Format("Set DownloadURL {0}", BaseUri));
        }

        public void OnClickInit()
        {
            StartCoroutine(AssetBundleManager.Instance.Initialize((success) =>
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
            }));
        }

        public void OnDownloadBundles()
        {
            AssetBundleManager.Instance.NeedDownloadList((list) =>
            {
                InsertLogText(string.Format("Need Download List Count : {0}", list.Count));

                List<string> downloadList = list;
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