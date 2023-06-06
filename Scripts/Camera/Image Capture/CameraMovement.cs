using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraMovement : MonoBehaviour
{
    [Header("FPS Controller")]
    public GameObject fpsController;

    [Header("Sampling Parameters")]
    public bool enableRandomSampling = true;
    public float padding = 0.5f;

    [Space]
    [Header("Camera Parameters")]
    public float cameraHeightMean = 1.7f;
    public float cameraHeightStd = 0.15f;
    public float fieldOfViewMean = 60f;
    public float fieldOfViewStd = 5f;

    [Space]
    [Header("Rotation Parameters")]
    public float xRotationMean = 0;
    public float xRotationStd = 25f;
    public float zRotationMean = 0f;
    public float zRotationStd = 10f;

    private struct BoundingBox
    {
        public Vector3 minBounds;
        public Vector3 maxBounds;
    }

    private List<BoundingBox> boundingBoxes = new List<BoundingBox>();

    void Start()
    {
        FindFloorBounds();
    }
    
    void OnValidate()
    {
        FindFloorBounds();
    }

    private void FindFloorBounds()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.activeInHierarchy && (obj.name.ToLower().Contains("floor") || obj.name.ToLower().Contains("ground")))
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    BoundingBox boundingBox = new BoundingBox
                    {
                        minBounds = renderer.bounds.min,
                        maxBounds = renderer.bounds.max
                    };
                    boundingBoxes.Add(boundingBox);
                }
            }
        }
    }

    public void SetPose(int index)
    {
        if (!enableRandomSampling || fpsController == null) return;

        System.Random random = new System.Random(index);

        if (boundingBoxes.Count == 0)
        {
            Debug.Log("No ground object found.");
            return;
        }

        BoundingBox boundingBox = boundingBoxes[random.Next(boundingBoxes.Count)];

        float x = (float)(random.NextDouble() * (boundingBox.maxBounds.x - boundingBox.minBounds.x - 2 * padding) + boundingBox.minBounds.x + padding);
        float z = (float)(random.NextDouble() * (boundingBox.maxBounds.z - boundingBox.minBounds.z - 2 * padding) + boundingBox.minBounds.z + padding);
        
        float y = (float)(random.NextDouble() * cameraHeightStd + cameraHeightMean);

        Vector3 position = new Vector3(x, y, z);

        float rotX = (float)(random.NextDouble() * xRotationStd + xRotationMean);
        float rotY = (float)(random.NextDouble() * 360);
        float rotZ = (float)(random.NextDouble() * zRotationStd + zRotationMean);
        Quaternion rotation = Quaternion.Euler(rotX, rotY, rotZ);

        fpsController.transform.position = position;
        fpsController.transform.rotation = rotation;

        float fov = (float)(random.NextDouble() * fieldOfViewStd + fieldOfViewMean);
        Camera[] cameras = fpsController.GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            camera.fieldOfView = fov;
        }
    }

    public void NextPose()
    {
        SetPose(new System.Random().Next());
    }
}
