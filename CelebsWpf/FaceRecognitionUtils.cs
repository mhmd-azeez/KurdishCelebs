using FaceRecognitionDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace CelebsWpf
{
    public class Face
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public FaceEncoding Encoding { get; set; }
    }

    public class Result
    {
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public double Confidence { get; set; }
    }

    public static class FaceRecognitionUtils
    {
        public static List<Result> Search(string dataSetFolder, string path)
        {
            FaceRecognition.InternalEncoding = System.Text.Encoding.UTF8;
            var folder = Environment.CurrentDirectory;

            using (FaceRecognition fr = FaceRecognition.Create("models"))
            using (Image imageB = FaceRecognition.LoadImageFile(path, Mode.Greyscale))
            {
                var images = DeserializeFaces(dataSetFolder).Where(i => i.Name != dataSetFolder).ToList();

                var locationsB = fr.FaceLocations(imageB);
                FaceEncoding encodingB = fr.FaceEncodings(imageB, locationsB).First();

                var distances = FaceRecognition.FaceDistances(images.Select(i => i.Encoding), encodingB).ToList();

                return distances.Select((d, i) => new Result
                {
                    Confidence = 1 - d,
                    ImagePath = images[i].FullPath,
                    Name = images[i].Name
                }).OrderByDescending(i => i.Confidence).ToList();
            }
        }


        public static void EncodeDataSet(string folder)
        {
            using (FaceRecognition fr = FaceRecognition.Create("models"))
            {
                var images = GetImages(fr, folder);
                foreach (var image in images)
                {
                    var name = Path.GetFileName(image.FullPath);
                    var path = Path.Combine(Path.GetDirectoryName(image.FullPath), name + ".fe");

                    using (var file = File.OpenWrite(path))
                    {
                        file.SetLength(0);
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(file, image.Encoding);
                    }
                }
            }
        }

        public static IEnumerable<Face> DeserializeFaces(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.fe", SearchOption.AllDirectories);
            var serializer = new BinaryFormatter();

            foreach (var file in files)
            {
                var parts = file.Split('\\');
                var dir = parts[parts.Length - 2];

                using (var stream = File.OpenRead(file))
                {
                    yield return new Face
                    {
                        Encoding = (FaceEncoding)serializer.Deserialize(stream),
                        FullPath = file.Replace(".fe", ""),
                        Name = dir
                    };
                }
            }
        }

        public static IEnumerable<Face> GetImages(FaceRecognition fr, string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.jpg", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var parts = file.Split('\\');
                var dir = parts[parts.Length - 2];
                var fileDirectory = Path.GetDirectoryName(file);
                var fileName = Path.GetFileName(file);
                var faceEncodingFile = Path.Combine(fileDirectory, fileName + ".fe");
                if (File.Exists(faceEncodingFile))
                {
                    continue;
                }

                using (var image = FaceRecognition.LoadImageFile(file, Mode.Greyscale))
                {
                    var locations = fr.FaceLocations(image);
                    var face = fr.FaceEncodings(image, locations).FirstOrDefault();

                    if (face is null) continue;

                    yield return new Face
                    {
                        FullPath = file,
                        Encoding = face,
                        Name = dir,
                    };
                }
            }
        }
    }
}
