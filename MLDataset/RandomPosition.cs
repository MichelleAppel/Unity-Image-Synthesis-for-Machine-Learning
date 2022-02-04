using System.Collections.Generic;
using UnityEngine;
using ArchViz_Interface.Scripts.ImageSynthesis;
using Newtonsoft.Json;
using UnityEngine.UIElements;
using static MLDataset.Trail;

namespace MLDataset
{

    public class RandomPosition : MonoBehaviour
    {
        public bool sampleUniform = false;
        public bool sampleJson = true;
        public bool sampleJsonWithNormal = false;
        public bool inOrder = true;
        public int idx = -1;
        
        public string jsonPath = "output/json/coordinate_list.json";

        private Camera _camera;

        private int positionsLength;

        private string json;
        private Trail trail;
        private List<Vector3> positions;
        private List<UnityEngine.Quaternion> rotations ;
        private int length;
        
        
        // Start is called before the first frame update
        void Start () {
        	Random.InitState(42);

            _camera = GetComponent<Camera>();
            
            if (sampleJson || sampleJsonWithNormal)
            {
                json = System.IO.File.ReadAllText(jsonPath);
                trail = JsonConvert.DeserializeObject<Trail>(json);
                positions = trail.position;
                rotations = trail.rotation;
                length = positions.Count;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (sampleJson)
            {
                SampleFromJson();
            }
            else if (sampleJsonWithNormal)
            {
                SampleJsonWithNormal();
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
            if (!inOrder)
            {
                idx = Random.Range(0, length);
            }
            else
            {
                idx++;
                if (idx >= length)
                {
                    idx = 0;
                }
            }
            
            transform.position = positions[idx];
            transform.rotation = rotations[idx];
        }
        
        private void SampleJsonWithNormal()
        {
            if (!inOrder)
            {
                idx = Random.Range(0, length);
            }
            else
            {
                idx++;
                if (idx >= length)
                {
                    idx = 0;
                }
            }
            transform.position = positions[idx];

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
