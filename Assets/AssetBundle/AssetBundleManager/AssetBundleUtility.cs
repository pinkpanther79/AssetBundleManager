namespace AssetBundle
{
    using UnityEngine;

    [System.Serializable]
    public class BundleSizeInfo
    {
        public string BundleName = string.Empty;
        public long BundleSize = 0;

        public BundleSizeInfo(string name, long size)
        {
            BundleName = name;
            BundleSize = size;
        }
    }

    public class AssetBundleUtility
    {
        public enum eVariantType
        {
            hd = 0,
            sd,
        }

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
