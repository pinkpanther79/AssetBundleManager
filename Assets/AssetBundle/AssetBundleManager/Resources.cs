namespace AssetBundle
{
    using UnityEngine;

    public static class Resources
    {
        public static void Load(string path, System.Action<Object> callback, bool dependencies = true)
        {
            if (AssetBundleUtility.UseAssetBundles)
            {
                AssetBundleManager.Instance.LoadAsset(string.Empty, string.Empty, (obj) =>
                {
                    callback(obj);
                });
            }
            else
            {
                callback(UnityEngine.Resources.Load(path));
            }
        }

        public static void LoadAsync(string path, System.Action<AsyncOperation> callback, bool dependencies = true)
        {
            if (AssetBundleUtility.UseAssetBundles)
            {
                AssetBundleManager.Instance.LoadAssetAsync(string.Empty, string.Empty, (request) =>
                {
                    callback(request);
                });
            }
            else
            {
                callback(UnityEngine.Resources.LoadAsync(path));
            }
        }
    }
}
