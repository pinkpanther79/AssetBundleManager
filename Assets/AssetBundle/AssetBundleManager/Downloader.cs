namespace AssetBundle
{
    using System.Collections;

    public abstract class Downloader<T>
    {
        protected string m_Url;
        protected System.Action<T> m_OnComplete;

        protected abstract void MakeUrl(string baseUri);
        public abstract IEnumerator Download();
    }
}