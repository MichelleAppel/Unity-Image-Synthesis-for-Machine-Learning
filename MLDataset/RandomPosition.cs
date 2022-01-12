using System.Collections.Generic;
using UnityEngine;
using ArchViz_Interface.Scripts.ImageSynthesis;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace MLDataset
{

    public class RandomPosition : MonoBehaviour
    {
        public bool sampleUniform = false;
        public bool sampleJson = true;

        public string jsonPath = "output/json/coordinate_list.json";

        private Camera _camera;
        private List<Vector3> positionCoordinates = 
            new List<Vector3>();

        private int positionsLength;
        
        // Start is called before the first frame update
        void Start () {
            _camera = GetComponent<Camera>();
            
            if (sampleJson)
            {
                string positionsJson = System.IO.File.ReadAllText(jsonPath);
                positionCoordinates = JsonConvert.DeserializeObject<List<Vector3>>(positionsJson);
                positionsLength = positionCoordinates.Count;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (sampleJson)
            {
                SampleFromJson();
            }
            else if (sampleUniform)
            {
                RandomCoordinates();
            }
        }

        private void RandomCoordinates()
        {
            Vector3 randomCoordinates = new Vector3(
                UnityEngine.Random.Range(-16.0f, 5.7f), 
                UnityEngine.Random.Range(1.0f, 3.9f), 
                UnityEngine.Random.Range(-7.0f, 1.7f)
            );
            transform.position = randomCoordinates;

            Vector3 randomRotation = new Vector3(
                UnityEngine.Random.Range(-90.0f, 90.0f),
                UnityEngine.Random.Range(0.0f, 445.0f),
                0.0f
            );
            transform.rotation = Quaternion.Euler(randomRotation);
        }
		
        private void SampleFromJson()
        {
            transform.position = positionCoordinates[Random.Range(0, positionsLength)];

            Vector3 randomRotation = new Vector3(
                RandomGaussian(-90.0f, 90.0f),
                UnityEngine.Random.Range(0.0f, 445.0f),
                0.0f
            );
            transform.rotation = Quaternion.Euler(randomRotation);
        }
        
        float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
        {
            float u, v, S;
 
            do
            {
                u = 2.0f * UnityEngine.Random.value - 1.0f;
                v = 2.0f * UnityEngine.Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);
 
            // Standard Normal Distribution
            float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
 
            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            float mean = (minValue + maxValue) / 2.0f;
            float sigma = (maxValue - mean) / 3.0f;
            return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
        
    }
}
