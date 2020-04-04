using FaceRecognitionDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace KurdishCelebs.Shared
{
    public class NoFaceFoundException : Exception
    {

    }

    public class Face
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public FaceEncoding Encoding { get; set; }
    }

    public class SearchResult
    {
        public Location FaceLocation { get; set; }
        public List<Result> Matches { get; set; }
    }

    public class Result
    {
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public double Confidence { get; set; }
    }

    public static class FaceRecognitionUtils
    {
        private static FaceRecognition _faceRecognition;

        public static void Initialize(string modelsFolder)
        {
            _faceRecognition = FaceRecognition.Create(modelsFolder);
        }

        public static SearchResult Search(string imagesFolder, string path, List<Face> images = null)
        {
            FaceRecognition.InternalEncoding = System.Text.Encoding.UTF8;

            using (Image imageB = FaceRecognition.LoadImageFile(path, Mode.Greyscale))
            {
                if (images == null)
                    images = DeserializeFaces(imagesFolder).Where(i => i.Name != imagesFolder).ToList();

                var faceLocations = _faceRecognition.FaceLocations(imageB)
                        .OrderByDescending(l => (l.Right - l.Left) * (l.Bottom - l.Top));
                if (faceLocations.Any() == false)
                    throw new NoFaceFoundException();

                var faceLocation = faceLocations.First();
                var faceEncoding = _faceRecognition.FaceEncodings(imageB, new[] { faceLocation }).First();

                var distances = FaceRecognition.FaceDistances(images.Select(i => i.Encoding), faceEncoding).ToList();

                faceEncoding.Dispose();

                var results = distances.Select((d, i) => new Result
                {
                    Confidence = 1 - d,
                    ImagePath = images[i].FullPath,
                    Name = images[i].Name
                }).OrderByDescending(i => i.Confidence).ToList();

                return new SearchResult
                {
                    FaceLocation = faceLocation,
                    Matches = results
                };
            }
        }


        public static void EncodeDataSet(string imagesFolder, string modelsFolder)
        {
            var images = GetImages(imagesFolder);
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

        public static IEnumerable<Face> DeserializeFaces(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.fe", SearchOption.AllDirectories);
            var serializer = new BinaryFormatter();

            foreach (var file in files)
            {
                var parts = file.Split(new char[] { '\\', '/' });
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

        public static IEnumerable<Face> GetImages(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.jpg", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var parts = file.Split(new char[] { '\\', '/' });
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
                    var locations = _faceRecognition.FaceLocations(image);
                    var face = _faceRecognition.FaceEncodings(image, locations).FirstOrDefault();

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
