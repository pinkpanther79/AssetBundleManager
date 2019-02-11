namespace AssetBundle
{
    using UnityEngine;
    using UnityEditor;

    public static class AssetBundleSettings
    {
        private const string UseAssetBundleMenuName = "AssetBundles/Settings/UseAssetBundle";

        static AssetBundleSettings()
        {
            AssetBundleUtility.UseAssetBundles = EditorPrefs.GetBool(UseAssetBundleMenuName, true);

            EditorApplication.delayCall += () => 
            {
                PerformAction(AssetBundleUtility.UseAssetBundles);
            };
        }

        [MenuItem(UseAssetBundleMenuName)]
        private static void ToggleAction()
        {
            PerformAction(!AssetBundleUtility.UseAssetBundles);
        }

        public static void PerformAction(bool enabled)
        {
            Menu.SetChecked(UseAssetBundleMenuName, enabled);
         
            EditorPrefs.SetBool(UseAssetBundleMenuName, enabled);

            AssetBundleUtility.UseAssetBundles = enabled;
        }
    }
}