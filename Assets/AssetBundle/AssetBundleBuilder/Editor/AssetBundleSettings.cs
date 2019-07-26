namespace AssetBundle
{
    using UnityEngine;
    using UnityEditor;

    public static class AssetBundleSettings
    {
        private const string UseAssetBundleMenuName = "AssetBundles/Settings/UseAssetBundle";

        static AssetBundleSettings()
        {
            Utilities.UseAssetBundles = EditorPrefs.GetBool(UseAssetBundleMenuName, true);

            EditorApplication.delayCall += () => 
            {
                PerformAction(Utilities.UseAssetBundles);
            };
        }

        [MenuItem(UseAssetBundleMenuName)]
        private static void ToggleAction()
        {
            PerformAction(!Utilities.UseAssetBundles);
        }

        public static void PerformAction(bool enabled)
        {
            Menu.SetChecked(UseAssetBundleMenuName, enabled);
         
            EditorPrefs.SetBool(UseAssetBundleMenuName, enabled);

            Utilities.UseAssetBundles = enabled;
        }
    }
}