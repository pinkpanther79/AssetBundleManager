namespace AssetBundle
{
    using UnityEngine;

    public enum eVariantType
    {
        hd = 0,
        sd,
    }

    public class AssetBundleUtility
    {
        public static bool UseAssetBundle = false;
        
        private static eVariantType m_VariantType = eVariantType.hd;
        public static eVariantType VariantType
        {
            get
            {
                return m_VariantType;
            }

            set
            {
                m_VariantType = value;

                PlayerPrefs.SetInt("VariantType", System.Convert.ToInt32(value));
            }
        }

        public static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    return "Android";
            }
        }
    }
}
