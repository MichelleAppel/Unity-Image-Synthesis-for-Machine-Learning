using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessing_Sobel : MonoBehaviour {

    private Material sobelMat;
    public Color outlineColor;
    
    void Start () {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        sobelMat = new Material(Shader.Find("Hidden/SobelOutline"));
    }
	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        sobelMat.SetFloat("_ResX", Screen.width);
        sobelMat.SetFloat("_ResY", Screen.height);
        sobelMat.SetColor("_Outline", outlineColor);
        Graphics.Blit(source, destination, sobelMat);
    }
}
