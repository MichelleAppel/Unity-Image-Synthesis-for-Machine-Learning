using System.Collections.Generic;
using UnityEngine;
// using Newtonsoft.Json;

namespace MLDataset
{

    public class RandomPosition : MonoBehaviour
    {
        [Header("Sample method")]
        public int seed = 42;
        public bool sampleUniform = false;
        public float rollRange = 0.2f;
        
        [Space(10)]
        public bool sampleJson = true;
        public bool sampleJsonWithNormal = false;
        [Space(10)]
        public bool inOrder = true;
        public int idx = -1;
        [Space(10)]
        public string jsonPath = "output/json/coordinate_list.json";

        private Camera _camera;

        private int positionsLength;

        private string json;
        private Trail trail = new Trail();
        private List<Vector3> positions;
        private List<UnityEngine.Quaternion> rotations ;
        private int length;
        
        
        
        // Start is called before the first frame update
        void Start () {
        	Random.InitState(seed);

            _camera = GetComponent<Camera>();
            
            if (sampleJson || sampleJsonWithNormal)
            {
                json = System.IO.File.ReadAllText(jsonPath);
                // Debug.Log(json);
                // trail = JsonConvert.DeserializeObject<Trail>();
                JsonUtility.FromJsonOverwrite(json, trail);
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
                SampleFromJson(); // Samples position and orientation from JSON
            }
            else if (sampleJsonWithNormal)
            {
                SampleJsonWithNormal(); // Samples position from JSON, samples rotation from uniform (y) and normal distribution (x)
            }
            else if (sampleUniform)
            {
                RandomCoordinates(); // Samples uniformly between given boundary
            }
        }

        private void RandomCoordinates()
        {
            Vector3 randomCoordinates = new Vector3( //TODO: automate range
                UnityEngine.Random.Range(-16.0f, 5.7f), 
                UnityEngine.Random.Range(1.0f, 3.9f), 
                UnityEngine.Random.Range(-7.0f, 1.7f)
            );
            transform.position = randomCoordinates;

            Vector3 randomRotation = new Vector3(
                UnityEngine.Random.Range(-90.0f, 90.0f),
                UnityEngine.Random.Range(0.0f, 445.0f),
                UnityEngine.Random.Range(-rollRange, rollRange)
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
                UnityEngine.Random.Range(-rollRange, rollRange)
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
