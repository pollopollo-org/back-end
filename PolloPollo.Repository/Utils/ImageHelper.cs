namespace PolloPollo.Services.Utils
{
    public class ImageHelper
    {
        public static string GetRelativeStaticFolderImagePath(string thumbnail)
        {
            return !string.IsNullOrEmpty(thumbnail)
                ? $"{ImageFolderEnum.@static.ToString()}/{thumbnail}"
                : null;
        }
    }
}
