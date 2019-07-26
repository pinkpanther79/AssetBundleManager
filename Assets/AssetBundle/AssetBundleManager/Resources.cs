namespace AssetBundle
{
    using UnityEngine;

    public static class Resources
    {
        public static void Load(string path, System.Action<Object> callback, bool dependencies = true)
        {
            if (Utilities.UseAssetBundles)
            {
                AssetBundleManager.Instance.LoadAsset(Utilities.FindBundleName(path), Utilities.FindAssetName(path), (obj) =>
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
            if (Utilities.UseAssetBundles)
            {
                AssetBundleManager.Instance.LoadAssetAsync(Utilities.FindBundleName(path), Utilities.FindAssetName(path), (request) =>
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
