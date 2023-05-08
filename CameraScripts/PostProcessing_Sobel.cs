using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessing_Sobel : MonoBehaviour {
    private Material sobelMat;
    public Shader sobelShader;
    public Color outlineColor;
    public Color backgroundColor;

    void Start () {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        sobelMat = new Material(sobelShader);
        UpdateColors();
    }

    void OnValidate()
    {
        UpdateColors();
    }

    void UpdateColors()
    {
        if (sobelMat != null)
        {
            sobelMat.SetColor("_Outline", outlineColor);
            sobelMat.SetColor("_Background", backgroundColor);
        }
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        sobelMat.SetFloat("_ResX", Screen.width);
        sobelMat.SetFloat("_ResY", Screen.height);
        Graphics.Blit(source, destination, sobelMat);
    }
}