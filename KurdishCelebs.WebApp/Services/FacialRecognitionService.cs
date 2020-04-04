using KurdishCelebs.Shared;
using KurdishCelebs.WebApp.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace KurdishCelebs.WebApp.Services
{
    public class FacialRecognitionService
    {
        private List<Face> _faces;

        public void Initialize()
        {
            FaceRecognitionUtils.Initialize(PathHelper.ModelsFolder());
            FaceRecognitionUtils.EncodeDataSet(PathHelper.ImagesFolder(), PathHelper.ModelsFolder());
            _faces = FaceRecognitionUtils.DeserializeFaces(PathHelper.ImagesFolder()).ToList();
        }

        internal SearchResult Search(string filePath)
        {
            return FaceRecognitionUtils.Search(PathHelper.ImagesFolder(), filePath, _faces);
        }
    }
}
