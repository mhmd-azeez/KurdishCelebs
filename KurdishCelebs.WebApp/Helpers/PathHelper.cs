using System;
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
            var modelsFolder = Environment.GetEnvironmentVariable("KURDCELEBS_MODELS_DIR", EnvironmentVariableTarget.Machine)
                ?? Shared.Constants.ModelsFolder;

            return Path.Combine(BinPath(), modelsFolder);
        }

        public static string ImagesFolder()
        {
            var modelsFolder = Environment.GetEnvironmentVariable("KURDCELEBS_IMAGES_DIR", EnvironmentVariableTarget.Machine)
                ?? Shared.Constants.ImagesFolder;

            return Path.Combine(BinPath(), Shared.Constants.ImagesFolder);
        }
    }
}
