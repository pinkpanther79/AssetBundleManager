namespace AssetBundle
{
    using UnityEngine;
    using UnityEngine.UI;

    public class Sample : MonoBehaviour
    {
        [SerializeField]
        private RawImage DownloadedImage = null;

        private Texture Texture = null;

        private string ResourcePath = "SampleTexture";

        public void OnClickResoureLoad()
        {
            Texture = UnityEngine.Resources.Load(ResourcePath) as Texture;

            if (Texture != null)
            {
                DownloadedImage.texture = Texture;
            }
        }

        public void OnClickResourceUnload()
        {
            UnityEngine.Resources.UnloadAsset(Texture);
        }
    }
}