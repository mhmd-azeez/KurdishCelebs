using System.IO;
using System.Reflection;

namespace KurdishCelebs.WebApp.Helpers
{
    public static class PathHelper
    {
        public static string BinPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public const string ResultFolder = "results";

        public static string ModelsFolder()
        {
            return Path.Combine(BinPath(), Shared.Constants.ModelsFolder);
        }

        public static string ImagesFolder()
        {
            return Path.Combine(BinPath(), Shared.Constants.ImagesFolder);
        }
    }
}
