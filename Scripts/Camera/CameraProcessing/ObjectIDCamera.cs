using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class ObjectIDCamera : MonoBehaviour
{
    public Shader objectIDShader;

    private Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
        UpdateReplacementShader();
    }

    private void OnValidate()
    {
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }
        UpdateReplacementShader();
    }

    private void UpdateReplacementShader()
    {
        if (_camera != null && objectIDShader != null)
        {
            _camera.SetReplacementShader(objectIDShader, "RenderType");
        }
    }

    void OnPreRender()
    {
        var renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Color objectColor = ColorEncoding.EncodeIDAsColor(renderer.GetInstanceID());
            renderer.sharedMaterial.SetColor("_ObjectColor", objectColor);
        }
    }
}