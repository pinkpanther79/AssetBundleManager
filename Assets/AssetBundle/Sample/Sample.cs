﻿namespace AssetBundle
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
        private string ResourcePath = string.Empty;

        [SerializeField]
        private string SceneName = string.Empty;

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

        public void OnClickLoadAsset()
        {
            double downloadSize = AssetBundleManager.Instance.DownloadCapacity(Utilities.FindBundleName(ResourcePath));

            InsertLogText(string.Format("Download Size : {0}", downloadSize));

            Resources.Load(ResourcePath, (obj) =>
            {
            InsertLogText(string.Format("OnLoadAsset {0} : {1}", ResourcePath, obj.IsNotNull()));

                if (obj.IsNotNull())
                {
                    /// TODO : insert after actions
                }
                else
                {
                    /// TODO : insert retry actions
                }
            });
        }

        public void OnClickLoadAsyncAsset()
        {
            double downloadSize = AssetBundleManager.Instance.DownloadCapacity(Utilities.FindBundleName(ResourcePath));
            
            InsertLogText(string.Format("Download Size : {0}", downloadSize));

            Resources.LoadAsync(ResourcePath, (request) =>
            {
                InsertLogText(string.Format("OnLoadAsyncAsset {0} : {1}", ResourcePath, request.IsNotNull()));

                if (request.IsNotNull())
                {
                    /// TODO : insert after actions
                }
                else
                {
                    /// TODO : insert retry actions
                }
            });
        }

        public void OnClickLoadScene()
        {
            AssetBundleManager.Instance.LoadScene(Utilities.MakeSceneName(SceneName), (success) =>
            {
                InsertLogText(string.Format("OnClickLoadScene {0} : {1}", SceneName, success));

                if (success)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
                }
            });
        }

        public void OnClickLoadSceneAsync()
        {
            AssetBundleManager.Instance.LoadScene(Utilities.MakeSceneName(SceneName), (success) =>
            {
                InsertLogText(string.Format("OnClickLoadScene {0} : {1}", SceneName, success));

                if (success)
                {
                    AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName);
                }
            });
        }

        public void OnClickDownloadAllBundles()
        {
            string[] downloadList = AssetBundleManager.Instance.DownloadList();

            double downloadSize = AssetBundleManager.Instance.DownloadCapacity(downloadList);

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