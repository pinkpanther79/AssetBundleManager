namespace AssetBundle
{
    using System.Collections.Generic;
    using System;
    using UnityEngine;

    public abstract class DownloadContainer<T>
    {
        public Stack<string> Bundles;
        public Action<T> OnComplete;
    }

    public class BundlesDownloadContainer : DownloadContainer<bool>
    {
        public BundlesDownloadContainer(string[] dependencies, Action<bool> onComplete)
        {
            Stack<string> stack = new Stack<string>(dependencies);
            
            Bundles = stack;
            OnComplete = onComplete;
        }

        public BundlesDownloadContainer(Stack<string> dependencies, Action<bool> onComplete)
        {
            Bundles = dependencies;
            OnComplete = onComplete;
        }
    }

    public class DependenciesContainer : DownloadContainer<AssetBundle>
    {
        public string BundleName;

        public DependenciesContainer(string bundleName, string[] dependencies, Action<AssetBundle> onComplete)
        {
            Stack<string> changedDependencies = new Stack<string>(dependencies);

            BundleName = bundleName;
            Bundles = changedDependencies;
            OnComplete = onComplete;
        }

        public DependenciesContainer(string bundleName, Stack<string> dependencies, Action<AssetBundle> onComplete)
        {
            BundleName = bundleName;
            Bundles = dependencies;
            OnComplete = onComplete;
        }
    }
}