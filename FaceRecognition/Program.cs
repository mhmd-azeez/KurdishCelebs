using FaceRecognitionDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace FaceRecognitionApp
{
    class PersonImage
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public FaceEncoding Face { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //EncodeDataSet("images");

            FaceRecognition.InternalEncoding = System.Text.Encoding.UTF8;
            var folder = Environment.CurrentDirectory;

            using (FaceRecognition fr = FaceRecognition.Create("models"))
            using (Image imageB = FaceRecognition.LoadImageFile(Path.Combine(folder, @"images\10.jpg")))
            {
                var images = DeserializeFaces("images").Where(i => i.Name != "images").ToList();

                var locationsB = fr.FaceLocations(imageB);
                FaceEncoding encodingB = fr.FaceEncodings(imageB, locationsB).First();

                var distances = FaceRecognition.FaceDistances(images.Select(i => i.Face), encodingB).ToList();

                for (int i = 0; i < distances.Count; i++)
                {
                    Console.WriteLine($"{images[i].Name}: {1 - distances[i]:p}");
                }

                var mostLikely = distances.Select((d, i) => new
                {
                    Distance = d,
                    Name = images[i].Name
                }).OrderBy(i => i.Distance).First();

                Console.WriteLine($"Match: {mostLikely.Name} : {1 - mostLikely.Distance:p}");
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
                        formatter.Serialize(file, image.Face);
                    }
                }
            }
        }

        public static IEnumerable<PersonImage> DeserializeFaces(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.fe", SearchOption.AllDirectories);
            var serializer = new BinaryFormatter();

            foreach (var file in files)
            {
                var dir = file.Split('\\')[^2];

                using (var stream = File.OpenRead(file))
                {
                    yield return new PersonImage
                    {
                        Face = (FaceEncoding)serializer.Deserialize(stream),
                        FullPath = file,
                        Name = dir
                    };
                }
            }
        }

        public static IEnumerable<PersonImage> GetImages(FaceRecognition fr, string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.jpg", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var dir = file.Split('\\')[^2];

                using (var image = FaceRecognition.LoadImageFile(file, Mode.Greyscale))
                {
                    var locations = fr.FaceLocations(image);
                    var face = fr.FaceEncodings(image, locations).FirstOrDefault();

                    if (face is null) continue;

                    yield return new PersonImage
                    {
                        FullPath = file,
                        Face = face,
                        Name = dir,
                    };
                }
            }
        }
    }
}