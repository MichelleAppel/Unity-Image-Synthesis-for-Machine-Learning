using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ImageCapture : MonoBehaviour
{
    public string mode;

    private Camera _camera;
    private CommandBuffer _commandBuffer;
    private RenderTexture _renderTexture;
    private Texture2D _texture2D;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _commandBuffer = new CommandBuffer();
        _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        _texture2D = new Texture2D(_camera.pixelWidth, _camera.pixelHeight, TextureFormat.RGBA32, false);
    }

    public byte[] CaptureToBuffer()
    {
        _camera.targetTexture = _renderTexture;
        _camera.AddCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
        _camera.Render();

        RenderTexture.active = _renderTexture;
        _texture2D.ReadPixels(new Rect(0, 0, _camera.pixelWidth, _camera.pixelHeight), 0, 0);
        _texture2D.Apply();

        byte[] imageData = ImageConversion.EncodeToPNG(_texture2D);

        _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
        _camera.targetTexture = null;

        return imageData;
    }
}