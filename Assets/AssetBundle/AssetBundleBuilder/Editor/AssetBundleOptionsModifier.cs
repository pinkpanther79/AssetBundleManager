namespace AssetBundle
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    public sealed class AssetBundleOptionsModifier : EditorWindow
    {
        public enum AssetBundleOptionType
        {
            CompressOption = 0,
            ForceRebuildAssetBundle,
            StrictMode,
            ClearFolder,
            CopyToStreamingAssets,
            Max
        }

        private static readonly string AssetBundleOptionsFileName = "AssetBundleOptions";
        private static readonly int WindowHeightUnitSize = 100;
        private static readonly int WindowWidthSize = 600;

        private static AssetBundlesBuildOptions Options = null;
        private static EditorWindow Window = null;
        
        private static Rect WindowPostion
        {
            get
            {
                return new Rect(100, 100, WindowWidthSize, WindowHeightSize);
            }
        }

        private static int WindowHeightSize
        {
            get
            {
                return System.Convert.ToInt32(AssetBundleOptionType.Max) * WindowHeightUnitSize;
            }
        }

        [MenuItem("AssetBundles/Modifier/AssetBundleOptions Modifier")]
        public static void Modifiy()
        {
            if (OpenAssetBundleOptions())
            {
                Window = GetWindow(typeof(AssetBundleOptionsModifier));
                Window.position = WindowPostion;
                Window.ShowPopup();
            }
        }

        private static void Save()
        {
            WriteInfosFile();
        }

        private static bool OpenAssetBundleOptions()
        {
            TextAsset jsonText = UnityEngine.Resources.Load(AssetBundleOptionsFileName) as TextAsset;

            if (jsonText == null)
            {
                Options = new AssetBundlesBuildOptions();

                WriteInfosFile();

                AssetDatabase.Refresh();

                jsonText = UnityEngine.Resources.Load(AssetBundleOptionsFileName) as TextAsset;
            }
            
            Options = JsonUtility.FromJson<AssetBundlesBuildOptions>(jsonText.text);

            if (Options == null)
            {
                EditorUtility.DisplayDialog("Error", "There is a problem with AssetBundleOptions.", "ok");

                return false;
            }

            return true;
        }

        private static void WriteInfos(AssetBundleOptionType type, object value)
        {
            switch (type)
            {
                case AssetBundleOptionType.CompressOption:
                    {
                        Options.CompressOption = (CompressOptions)value;
                        break;
                    }
                case AssetBundleOptionType.ForceRebuildAssetBundle:
                    {
                        Options.ForceRebuildAssetBundle = (bool)value;
                        break;
                    }
                case AssetBundleOptionType.StrictMode:
                    {
                        Options.StrictMode = (bool)value;
                        break;
                    }
                case AssetBundleOptionType.ClearFolder:
                    {
                        Options.ClearFolder = (bool)value;
                        break;
                    }
                case AssetBundleOptionType.CopyToStreamingAssets:
                    {
                        Options.CopyToStreamingAssets = (bool)value;
                        break;
                    }
            }
        }

        private static void WriteInfosFile()
        {
            string jsonString = JsonUtility.ToJson(Options);

            Debug.Log(jsonString);

            File.WriteAllText(Application.dataPath + "Resources" + "/" + AssetBundleOptionsFileName + ".json", jsonString);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        internal void OnGUI()
        {
            if (Options != null)
            {
                InformationsGUI();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                CompressOptionGUI();

                EditorGUILayout.Space();

                ForceRebuildAssetBundleGUI();

                EditorGUILayout.Space();

                StrictModeBundleGUI();

                EditorGUILayout.Space();

                ClearFolderGUI();

                EditorGUILayout.Space();

                CopyToStreamingAssetsGUI();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (GUILayout.Button("Save"))
                {
                    Save();
                }
            }
            else
            {
                if (GUILayout.Button("Update Infomations"))
                {
                    OpenAssetBundleOptions();
                }
            }
        }

        private void InformationsGUI()
        {
            EditorGUILayout.HelpBox(string.Format("CompressOption : {0}", Options.CompressOption.ToString()), MessageType.Info);
            EditorGUILayout.HelpBox(string.Format("ForceRebuildAssetBundle : {0}", Options.ForceRebuildAssetBundle.ToString()), MessageType.Info);
            EditorGUILayout.HelpBox(string.Format("StrictMode : {0}", Options.StrictMode.ToString()), MessageType.Info);
            EditorGUILayout.HelpBox(string.Format("ClearFolder : {0}", Options.ClearFolder.ToString()), MessageType.Info);
            EditorGUILayout.HelpBox(string.Format("CopyToStreamingAssets : {0}", Options.CopyToStreamingAssets.ToString()), MessageType.Info);
        }
        
        private void CompressOptionGUI()
        {
            EditorGUILayout.LabelField("CompressOption Setting", new GUIStyle("BoldLabel"));

            var oldOption = Options.CompressOption;
            CompressOptions curOption = (CompressOptions)EditorGUILayout.EnumPopup("CompressOptions", Options.CompressOption);

            if (oldOption != curOption)
            {
                WriteInfos(AssetBundleOptionType.CompressOption, curOption);
            }
        }

        private void ForceRebuildAssetBundleGUI()
        {
            EditorGUILayout.LabelField("ForceRebuildAssetBundle Setting", new GUIStyle("BoldLabel"));

            var oldOption = Options.ForceRebuildAssetBundle;
            bool curOption = EditorGUILayout.Toggle("ForceRebuildAssetBundle", Options.ForceRebuildAssetBundle);

            if (oldOption != curOption)
            {
                WriteInfos(AssetBundleOptionType.ForceRebuildAssetBundle, curOption);
            }
        }

        private void StrictModeBundleGUI()
        {
            EditorGUILayout.LabelField("StrictMode Setting", new GUIStyle("BoldLabel"));

            var oldOption = Options.StrictMode;
            bool curOption = EditorGUILayout.Toggle("StrictMode", Options.StrictMode);

            if (oldOption != curOption)
            {
                WriteInfos(AssetBundleOptionType.StrictMode, curOption);
            }
        }

        private void ClearFolderGUI()
        {
            EditorGUILayout.LabelField("ClearFolder Setting", new GUIStyle("BoldLabel"));

            var oldOption = Options.ClearFolder;
            bool curOption = EditorGUILayout.Toggle("ClearFolder", Options.ClearFolder);

            if (oldOption != curOption)
            {
                WriteInfos(AssetBundleOptionType.ClearFolder, curOption);
            }
        }

        private void CopyToStreamingAssetsGUI()
        {
            EditorGUILayout.LabelField("CopyToStreamingAssets Setting", new GUIStyle("BoldLabel"));

            var oldOption = Options.CopyToStreamingAssets;
            bool curOption = EditorGUILayout.Toggle("CopyToStreamingAssets", Options.CopyToStreamingAssets);

            if (oldOption != curOption)
            {
                WriteInfos(AssetBundleOptionType.CopyToStreamingAssets, curOption);
            }
        }
    }
}