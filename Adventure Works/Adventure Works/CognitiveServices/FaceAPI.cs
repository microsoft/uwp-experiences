using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Adventure_Works.CognitiveServices
{
    public class FaceAPI
    {
        private FaceServiceClient _client;

        private const string _groupId = "adventure_works_group3";

        private Person[] _personList;

        public Person[] KnownPeople
        {
            get
            {
                if (_personList == null)
                    return new Person[] { };
                else
                    return _personList;
            }
        }


        private static FaceAPI _instance;

        public static FaceAPI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FaceAPI();
                }

                return _instance;
            }
        }

        private FaceAPI()
        {
            _client = new FaceServiceClient(Keys.FaceServiceKey);
        }

        public async Task<IEnumerable<PhotoFace>> FindPeople(IRandomAccessStream stream)
        {
            Face[] faces = null;
            IdentifyResult[] results = null;
            List<PhotoFace> photoFaces = new List<PhotoFace>();

            try
            {
                // find all faces
                faces = await _client.DetectAsync(stream.AsStream());

                // no faces found
                if (faces.Count() == 0)
                {
                    return photoFaces;
                }

                if (await CheckIfGroupExistsAsync())
                {
                    results = await _client.IdentifyAsync(_groupId, faces.Select(f => f.FaceId).ToArray());
                }

                for (var i = 0; i < faces.Length; i++)
                {
                    var face = faces[i];

                    var photoFace = new PhotoFace()
                    {
                        Rect = face.FaceRectangle,
                        Identified = false
                    };

                    if (results != null)
                    {
                        var result = results[i];
                        if (result.Candidates.Length > 0)
                        {
                            photoFace.PersonId = result.Candidates[0].PersonId;
                            photoFace.Name = _personList.Where(p => p.PersonId == result.Candidates[0].PersonId).FirstOrDefault()?.Name;
                            photoFace.Identified = true;
                        }
                    }

                    photoFaces.Add(photoFace);
                }
            }
            catch (FaceAPIException ex)
            {
            
            }

            return photoFaces;
        }

        public async Task<bool> DeletePersonAsync(Guid personId)
        {
            try
            {
                await _client.DeletePersonAsync(_groupId, personId);
                await _client.TrainPersonGroupAsync(_groupId);

                var trained = false;

                while (!trained)
                {
                    await Task.Delay(1000);
                    var status = await _client.GetPersonGroupTrainingStatusAsync(_groupId);
                    switch (status.Status)
                    {
                        case Status.Succeeded:
                            trained = true;
                            break;
                        case Status.Failed:
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> AddImageForPerson(PhotoFace photoFace, IRandomAccessStream stream)
        {
            if (photoFace == null || photoFace.Name == null)
            {
                return false;
            }

            bool trained = false;

            // create group
            try
            {
                if (!await CheckIfGroupExistsAsync())
                {
                    await _client.CreatePersonGroupAsync(_groupId, _groupId);
                }

                if (photoFace.PersonId == null || _personList == null || !_personList.Any(p => p.PersonId == photoFace.PersonId))
                {
                    photoFace.PersonId = (await _client.CreatePersonAsync(_groupId, photoFace.Name)).PersonId;
                }

                await _client.AddPersonFaceAsync(_groupId, photoFace.PersonId, stream.AsStream(), targetFace: photoFace.Rect);

                await _client.TrainPersonGroupAsync(_groupId);

                while (!trained)
                {
                    await Task.Delay(1000);
                    var status = await _client.GetPersonGroupTrainingStatusAsync(_groupId);
                    switch (status.Status)
                    {
                        case Status.Succeeded:
                            trained = true;
                            break;
                        case Status.Failed:
                            return false;
                    }
                }

                _personList = await _client.GetPersonsAsync(_groupId);

                return true;
            }
            catch (FaceAPIException ex)
            {
                return false;
            }
        }

        private async Task<bool> CheckIfGroupExistsAsync()
        {
            PersonGroup group = null;

            // create group if first time
            try
            {
                group = await _client.GetPersonGroupAsync(_groupId);
                _personList = await _client.GetPersonsAsync(_groupId);
                return true;
            }
            catch (FaceAPIException ex)
            {
                return false;
            }
        }
    }
}
