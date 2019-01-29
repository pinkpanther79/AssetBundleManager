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
        
        public static bool UseAssetBundles = false;
        public static char VariantDelimiter = '-';
        
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
        
        public static string FindBundleName(string resourcePath)
        {
            resourcePath = resourcePath.ToLower();

            string[] token = resourcePath.Split('/');

            if (token.Length >= 2)
            {
                return token[token.Length - 2];
            }
            else
            {
                return token[token.Length - 1];
            }
        }

        public static string FindAssetName(string resourcePath)
        {
            resourcePath = resourcePath.ToLower();

            string[] token = resourcePath.Split('/');

            return token[token.Length - 1];
        }

        public static string AssetNameExcludeVariant(string directoryName)
        {
            return directoryName.Split(VariantDelimiter)[0];
        }

        public static string VariantName(string directoryName)
        {
            int index = directoryName.IndexOf(VariantDelimiter);

            if (index > 0)
            {
                return directoryName.Substring(index + 1).ToLower();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string MakeSceneName(string fileName)
        {
            fileName = fileName.ToLower();

            return string.Format("scene-{0}", fileName.Split('.')[0]);
        }
    }
}
