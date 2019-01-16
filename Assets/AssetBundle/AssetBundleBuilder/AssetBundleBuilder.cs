namespace AssetBundle
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    
    public static class AssetBundleBuilder
    {
        private static string AssetBundlesOutputPath = "AssetBundles";
        private static string AssetBundleRootPath = Path.Combine(Application.dataPath, "Sample", "Assets");
        private static string SceneRootPath = Path.Combine(Application.dataPath, "Sample", "Scenes");
        private static char VariantDelimiter = '-';

        [MenuItem("Assets/AssetBundles/Build AssetBundles For Android")]
        public static void BuildAssetBundlesForAndroid()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                
                string outputPath = CreateAssetBundleDirectory();

                ApplyAssetLabels();

                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);

                MakeErrorFile(e.Message);
            }
        }

        [MenuItem("Assets/AssetBundles/Build AssetBundles For iOS")]
        public static void BuildAssetBundlesForiOS()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

                string outputPath = CreateAssetBundleDirectory();

                ApplyAssetLabels();

                MakeAssetBundleInfomationFile();
                
                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);

                MakeErrorFile(e.Message);
            }
        }

        private static void ApplyAssetLabels()
        {
            UpdateResourceAssetLabels();

            UpdateSceneAssetLabels();

            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public static void UpdateResourceAssetLabels()
        {
            DirectoryInfo[] directories = FindDirectoryInfos(AssetBundleRootPath);
            foreach (var directory in directories)
            {
                AssetImporter assetImporter = FindAssetImporter(directory.FullName);
                if (assetImporter.IsNotNull())
                {
                    assetImporter.assetBundleName = FindAssetName(directory.Name);

                    string variant = FindVariantName(directory.Name);
                    if (variant.IsValidText())
                    {
                        assetImporter.assetBundleVariant = variant;
                    }
                }
            }
        }

        public static void UpdateSceneAssetLabels()
        {
            DirectoryInfo[] directories = FindDirectoryInfos(SceneRootPath);
            foreach (var directory in directories)
            {
                AssetImporter assetImporter = FindAssetImporter(directory.FullName);
                if (assetImporter.IsNotNull())
                {
                    assetImporter.assetBundleName = FindAssetName(directory.Name);

                    string variant = FindVariantName(directory.Name);
                    if (variant.IsValidText())
                    {
                        assetImporter.assetBundleVariant = variant;
                    }
                }
            }
        }

        private static string CreateAssetBundleDirectory()
        {
            string outputPath = Path.Combine(AssetBundlesOutputPath, AssetBundleUtility.GetPlatformForAssetBundles(RuntimePlatform.Android));
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }

        private static DirectoryInfo[] FindDirectoryInfos(string original)
        {
            DirectoryInfo info = new DirectoryInfo(original);

            DirectoryInfo[] infos = info.GetDirectories();

            return infos;
        }

        private static AssetImporter FindAssetImporter(string dirFullName)
        {
            string path = dirFullName.Remove(0, dirFullName.IndexOf("Assets"));

            return AssetImporter.GetAtPath(path);
        }

        public static string FindAssetName(string dirName)
        {
            return dirName.Split(VariantDelimiter)[0];
        }

        public static string FindVariantName(string dirName)
        {
            int index = dirName.IndexOf(VariantDelimiter);

            if (index > 0)
            {
                return dirName.Substring(index + 1).ToLower();
            }
            else
            {
                return string.Empty;
            }
        }

        private static void MakeAssetBundleInfomationFile()
        {
            FileInfo file = new FileInfo("OriginalAssetBundles.txt");

            using (FileStream fs = file.Create())
            {
                using (TextWriter tw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    foreach (var text in AssetDatabase.GetAllAssetBundleNames())
                    {
                        tw.WriteLine(text);
                    }
                }
            }
        }

        private static void MakeErrorFile(string errorMessage)
        {
            FileInfo file = new FileInfo("BuildError.txt");

            using (FileStream fs = file.Create())
            {
                using (TextWriter tw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    tw.Write(string.Format("{0} - {1}", System.DateTime.Now, errorMessage));
                }
            }
        }
    }
}