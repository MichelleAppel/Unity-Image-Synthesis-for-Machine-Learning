using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class NormalsCamera : MonoBehaviour
{
    public Shader normalsShader;

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
        if (_camera != null && normalsShader != null)
        {
            _camera.SetReplacementShader(normalsShader, "RenderType");
        }
    }
}