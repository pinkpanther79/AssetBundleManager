namespace AssetBundle
{
    using UnityEngine;

    public static class Resources
    {
        public static void Load(string path, System.Action<Object> callback, bool dependencies = true)
        {
            if (AssetBundleUtility.UseAssetBundles)
            {
                AssetBundleManager.Instance.LoadAsset(AssetBundleUtility.FindBundleName(path), AssetBundleUtility.FindAssetName(path), (obj) =>
                {
                    callback(obj);
                }, dependencies);
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
                AssetBundleManager.Instance.LoadAssetAsync(AssetBundleUtility.FindBundleName(path), AssetBundleUtility.FindAssetName(path), (request) =>
                {
                    callback(request);
                }, dependencies);
            }
            else
            {
                callback(UnityEngine.Resources.LoadAsync(path));
            }
        }
    }
}
