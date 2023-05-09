using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ImageCapture : MonoBehaviour
{
    public Camera targetCamera;
    public int width = 256;
    public int height = 256;

    public string mode;
    
    private Texture2D _captureTexture;

    void Start()
    {
        _captureTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public byte[] CaptureImage()
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;
        targetCamera.Render();
        RenderTexture.active = rt;

        _captureTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        _captureTexture.Apply();

        RenderTexture.active = null;
        targetCamera.targetTexture = null;
        Destroy(rt);

        byte[] data = _captureTexture.EncodeToPNG();
        return data;
    }
    
    void SendModeHeader(NetworkStream stream, string mode)
    {
        byte[] modeBytes = Encoding.ASCII.GetBytes(mode);

        // Send the length of the mode string (4 bytes)
        byte[] modeLengthBytes = BitConverter.GetBytes(modeBytes.Length);
        stream.Write(modeLengthBytes, 0, modeLengthBytes.Length);

        // Send the mode string
        stream.Write(modeBytes, 0, modeBytes.Length);
    }
}