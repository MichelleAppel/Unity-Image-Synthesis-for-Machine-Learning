using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageCapture : MonoBehaviour
{
    public int captureWidth = 256;
    public int captureHeight = 256;
    public string outputPath = "CapturedImages";
    public string imageNamePrefix = "capture_";
    public int imageCounter = 0;
    private Camera captureCamera;
    private RenderTexture renderTexture;

    void Start()
    {
        captureCamera = GetComponent<Camera>();

        // Create a new RenderTexture and set the camera's target texture to it
        renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        captureCamera.targetTexture = renderTexture;
    }

    public void CaptureImage()
    {
        // Set the RenderTexture as active and read pixels from it
        RenderTexture.active = renderTexture;
        Texture2D image = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        image.Apply();

        // Save the image to disk
        byte[] bytes = image.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/" + outputPath + "/" + imageNamePrefix + imageCounter.ToString() + ".png", bytes);

        // Increment the image counter
        imageCounter++;

        // Clean up
        RenderTexture.active = null;
        Destroy(image);
    }
}