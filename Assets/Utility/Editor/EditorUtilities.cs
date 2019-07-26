namespace AssetBundle
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class EditorUtilities
    {
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

        [MenuItem("CI/AssetBundles/CleanPlayerPrefs")]
        public static void CleanPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();

            EditorUtility.DisplayDialog("Notice", "PlayerPrefs was deleted.", "Confirm");
        }

        [MenuItem("CI/AssetBundles/CleanPersistentData")]
        public static void CleanPersistentData()
        {
            DirectoryInfo dataDir = new DirectoryInfo(Application.persistentDataPath);

            dataDir.Delete(true);

            EditorUtility.DisplayDialog("Notice", "PersistentData was deleted.", "Confirm");
        }

        public static void ClearFolder(string folderPath)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(folderPath);

            foreach (FileInfo file in rootDirectory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in rootDirectory.GetDirectories())
            {
                if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    ClearFolder(directory.FullName);

                    directory.Delete();
                }
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                File.Copy(filePath, newFilePath, true);
            }
        }

        public static DirectoryInfo[] FindDirectoryInfos(string original)
        {
            DirectoryInfo info = new DirectoryInfo(original);

            DirectoryInfo[] infos = info.GetDirectories();

            return infos;
        }

        public static AssetImporter FindAssetImporter(string dirFullName)
        {
            string path = dirFullName.Remove(0, dirFullName.IndexOf("Assets"));

            return AssetImporter.GetAtPath(path);
        }
    }
}
