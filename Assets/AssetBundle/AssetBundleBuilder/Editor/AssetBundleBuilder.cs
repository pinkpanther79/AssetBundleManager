namespace AssetBundle
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Collections.Generic;

    public static class AssetBundleBuilder
    {
        /// TODO : here variable edit
        private static string AssetBundlesOutputPath = "AssetBundles";
        private static string AssetBundleRootPath = Path.Combine(Application.dataPath, "AssetBundle/Sample/Resources");
        private static string SceneRootPath = Path.Combine(Application.dataPath, "AssetBundle/Sample/Scenes");
        private static string BuiltInfomationFileName = "OriginalAssetBundles.txt";
        private static string BundleSizeFileName = "BundleSizeInfos.json";

        [MenuItem("AssetBundles/Build AssetBundles For Android")]
        public static void BuildAssetBundlesForAndroid()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

                MakeAssetBundleDirectory();

                ApplyAssetLabels();

                MakeAssetBundleInfomationFile();

                BuildPipeline.BuildAssetBundles(AssetBundlesOutputPath, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.Android);

                MakeBundleSizeFile();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);

                MakeErrorFile(e.Message);
            }
        }

        [MenuItem("AssetBundles/Build AssetBundles For iOS")]
        public static void BuildAssetBundlesForiOS()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

                MakeAssetBundleDirectory();

                ApplyAssetLabels();

                MakeAssetBundleInfomationFile();
                
                BuildPipeline.BuildAssetBundles(AssetBundlesOutputPath, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.iOS);

                MakeBundleSizeFile();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);

                MakeErrorFile(e.Message);
            }
        }

        [MenuItem("AssetBundles/ClearAssetLabels")]
        private static void ClearAssetLabels()
        {
            string[] names = AssetDatabase.GetAllAssetBundleNames();
            foreach (var name in names)
            {
                AssetDatabase.RemoveAssetBundleName(name, true);
            }
        }

        [MenuItem("AssetBundles/CleanCache")]
        public static void CleanCache()
        {
            Cache cache = Caching.currentCacheForWriting;
            if (cache.ClearCache())
            {
                EditorUtility.DisplayDialog("Success", "Success to clear cache.", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed", "Failed to clear cache.", "ok");
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
                    assetImporter.assetBundleName = AssetBundleUtility.AssetNameExcludeVariant(directory.Name);

                    string variant = AssetBundleUtility.VariantName(directory.Name);
                    if (variant.IsValidText())
                    {
                        assetImporter.assetBundleVariant = variant;
                    }
                }
            }
        }

        public static void UpdateSceneAssetLabels()
        {
            DirectoryInfo sceneDirectory = new DirectoryInfo(SceneRootPath);
            FileInfo[] files = sceneDirectory.GetFiles("*.unity", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string path = file.FullName.Remove(0, file.FullName.IndexOf("Assets"));
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                assetImporter.assetBundleName = AssetBundleUtility.MakeSceneName(file.Name);
            }
        }

        private static void MakeAssetBundleDirectory()
        {
            AssetBundlesOutputPath = Path.Combine(AssetBundlesOutputPath, AssetBundleUtility.GetPlatformForAssetBundles(Application.platform));

            if (!Directory.Exists(AssetBundlesOutputPath))
            {
                Directory.CreateDirectory(AssetBundlesOutputPath);
            }
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

        private static void MakeAssetBundleInfomationFile()
        {
            FileInfo file = new FileInfo(BuiltInfomationFileName);

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
        
        private static void MakeBundleSizeFile()
        {
            BundleSizeInfo[] bundleSizeInfos = MakeSizeInfos();

            string json = JsonHelper.ArrayToJson(bundleSizeInfos, true);

            string filePath = MakeBundleSizeInfoFile();
            
            File.WriteAllText(filePath, json);
        }

        private static BundleSizeInfo[] MakeSizeInfos()
        {
            string[] files = Directory.GetFiles(AssetBundlesOutputPath);
            List<BundleSizeInfo> bundleSizeInfos = new List<BundleSizeInfo>();
            FileInfo fileInfo = null;
            foreach (var file in files)
            {
                fileInfo = new FileInfo(file);

                if (fileInfo.Name.Contains("manifest").IsFalse())
                {
                    bundleSizeInfos.Add(new BundleSizeInfo(fileInfo.Name, fileInfo.Length));
                }
            }

            return bundleSizeInfos.ToArray();
        }

        private static string MakeBundleSizeInfoFile()
        {
            string filePath = Path.Combine(AssetBundlesOutputPath, BundleSizeFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return filePath;
        }
    }
}