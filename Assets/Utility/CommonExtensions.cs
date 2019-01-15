namespace AssetBundle
{
    using System.Collections;

    public static class CommonExtensions
    {
        public static bool IsNull(this UnityEngine.Object value)
        {
            return (value == false);
        }

        public static bool IsNotNull(this UnityEngine.Object value)
        {
            return (value == true);
        }

        public static bool IsNull(this object value)
        {
            return (value == null);
        }

        public static bool IsNotNull(this object value)
        {
            return (value != null);
        }

        public static bool IsTrue(this bool value)
        {
            return (value == true);
        }

        public static bool IsFalse(this bool value)
        {
            return (value == false);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return (string.IsNullOrEmpty(value));
        }

        public static bool IsValidText(this string value)
        {
            return (string.IsNullOrEmpty(value) == false);
        }

        public static bool NotEmpty(this System.Array container)
        {
            return (container != null && container.Length > 0);
        }

        public static bool NotEmpty(this ICollection container)
        {
            return (container != null && container.Count > 0);
        }

        public static bool Empty(this System.Array container)
        {
            return (container == null || container.Length <= 0);
        }

        public static bool Empty(this ICollection container)
        {
            return (container == null || container.Count <= 0);
        }
    }
}