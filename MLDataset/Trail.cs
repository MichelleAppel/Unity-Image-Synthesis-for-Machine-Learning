using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using UnityEngine;

namespace MLDataset
{
    public class Trail : MonoBehaviour
    {
        public string path = "output/json";
        public string fileName = "coordinate_list";
        
        // 1. Empty list of coordinates
        [SerializeField]
        private List<Vector3> positionCoordinates = 
            new List<Vector3>();
        
        private Camera _camera;
    
        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            // 2. get game object coordinates
            positionCoordinates.Add(transform.localPosition + _camera.transform.localPosition);
        }

        void OnApplicationQuit()
        {
            var pathWithExtension = Path.Combine(path, fileName+ ".json");

            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            // 3. write to json file
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string jsonCoordinates = JsonConvert.SerializeObject(positionCoordinates, settings);

            File.WriteAllText(
                pathWithExtension, 
                jsonCoordinates);
        }
    }
}
