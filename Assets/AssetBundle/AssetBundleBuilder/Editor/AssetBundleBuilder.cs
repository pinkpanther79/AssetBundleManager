namespace AssetBundle
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Collections.Generic;

    public enum CompressOptions
    {
        Uncompressed = 0,
        StandardCompression,
        ChunkBasedCompression,
    }

    public class AssetBundlesBuildOptions
    {
        public CompressOptions CompressOption = CompressOptions.Uncompressed;
        public bool ForceRebuildAssetBundle = true;
        public bool StrictMode = true;
        public bool ClearFolder = true;
        public bool CopyToStreamingAssets = false;
    }

    public static class AssetBundleBuilder
    {
        private static string AssetBundlesOutputPath
        {
            get
            {
                return Path.Combine(AssetBundlesOutputRootName, Utilities.GetPlatformForAssetBundles(Application.platform));
            }
        }

        /// TODO : here variable edit
        private static readonly string AssetBundlesOutputRootName = "AssetBundles";
        private static readonly string AssetBundleOptionsFileName = "AssetBundleOptions";
        private static readonly string AssetBundleRootPath = Path.Combine(Application.dataPath, "AssetBundle/Sample/Resources");
        private static readonly string SceneRootPath = Path.Combine(Application.dataPath, "AssetBundle/Sample/Scenes");
        private static readonly string BuiltInfomationFileName = "OriginalAssetBundles.txt";
        private static readonly string BundleSizeFileName = "BundleSizeInfos.json";

        [MenuItem("AssetBundles/Build AssetBundles For Android")]
        public static void BuildAssetBundlesForAndroid()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

                AssetBundlesBuildOptions options = GenerateBuildOptions();

                ArrangeAssetBundleDirectory(options.ClearFolder);

                ApplyAssetLabels();

                GenerateAssetBundleInfomationFile();

                BuildPipeline.BuildAssetBundles(AssetBundlesOutputPath, AssetBundleOptions(options), BuildTarget.Android);

                MakeBundleSizeFile();

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                if (options.CopyToStreamingAssets)
                {
                    EditorUtilities.DirectoryCopy(AssetBundlesOutputPath, Application.streamingAssetsPath);
                }
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

                AssetBundlesBuildOptions options = GenerateBuildOptions();

                ArrangeAssetBundleDirectory(options.ClearFolder);

                ApplyAssetLabels();

                GenerateAssetBundleInfomationFile();
                
                BuildPipeline.BuildAssetBundles(AssetBundlesOutputPath, AssetBundleOptions(options), BuildTarget.iOS);

                MakeBundleSizeFile();

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                if (options.CopyToStreamingAssets)
                {
                    EditorUtilities.DirectoryCopy(AssetBundlesOutputPath, Application.streamingAssetsPath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);

                MakeErrorFile(e.Message);
            }
        }

        private static BuildAssetBundleOptions AssetBundleOptions(AssetBundlesBuildOptions options)
        {
            BuildAssetBundleOptions resultOptions = ConvertCompressOption(options.CompressOption);

            if (options.StrictMode)
            {
                resultOptions = resultOptions | BuildAssetBundleOptions.StrictMode;
            }

            if (options.ForceRebuildAssetBundle)
            {
                resultOptions = resultOptions | BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            return resultOptions;
        }

        private static void ApplyAssetLabels()
        {
            UpdateResourceAssetLabels();
            UpdateSceneAssetLabels();

            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public static void UpdateResourceAssetLabels()
        {
            DirectoryInfo[] directories = EditorUtilities.FindDirectoryInfos(AssetBundleRootPath);
            foreach (var directory in directories)
            {
                AssetImporter assetImporter = EditorUtilities.FindAssetImporter(directory.FullName);
                if (assetImporter.IsNotNull())
                {
                    assetImporter.assetBundleName = Utilities.AssetNameExcludeVariant(directory.Name);

                    string variant = Utilities.VariantName(directory.Name);
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
                assetImporter.assetBundleName = Utilities.MakeSceneName(file.Name);
            }
        }

        private static void ArrangeAssetBundleDirectory(bool clearFolder)
        {
            EditorUtilities.ClearFolder(Application.streamingAssetsPath);

            if (Directory.Exists(AssetBundlesOutputPath))
            {
                if (clearFolder)
                {
                    EditorUtilities.ClearFolder(AssetBundlesOutputPath);
                }
            }
            else
            {
                Directory.CreateDirectory(AssetBundlesOutputPath);
            }
        }

        private static void GenerateAssetBundleInfomationFile()
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

        private static AssetBundlesBuildOptions GenerateBuildOptions()
        {
            TextAsset jsonText = UnityEngine.Resources.Load(AssetBundleOptionsFileName) as TextAsset;
            if (jsonText != null)
            {
                AssetBundlesBuildOptions GeneratedOptions = JsonUtility.FromJson<AssetBundlesBuildOptions>(jsonText.text);

                if (GeneratedOptions != null)
                {
                    return GeneratedOptions;
                }
                else
                {
                    throw new System.Exception("BuildPlayer Exception : AssetBundlesBuildOptions is Null");
                }
            }
            else
            {
                throw new System.Exception("BuildPlayer Exception : Make a AssetBundlesBuildOptions first");
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

        private static BuildAssetBundleOptions ConvertCompressOption(CompressOptions compressOption)
        {
            if (compressOption == CompressOptions.Uncompressed)
            {
                return BuildAssetBundleOptions.UncompressedAssetBundle;
            }
            else if (compressOption == CompressOptions.ChunkBasedCompression)
            {
                return BuildAssetBundleOptions.ChunkBasedCompression;
            }
            else
            {
                return BuildAssetBundleOptions.None;
            }
        }
    }
}