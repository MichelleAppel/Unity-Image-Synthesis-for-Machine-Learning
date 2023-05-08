using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class DepthCamera : MonoBehaviour
{
    public Shader depthShader;

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
        if (_camera != null && depthShader != null)
        {
            _camera.SetReplacementShader(depthShader, "RenderType");
        }
    }
}