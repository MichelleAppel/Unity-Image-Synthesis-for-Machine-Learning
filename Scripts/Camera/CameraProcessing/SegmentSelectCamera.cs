using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class SegmentSelectCamera : MonoBehaviour
{
    private Camera _camera;
    public Shader SegmentSelectShader;
    private Renderer _selectedRenderer;
    
    [Header("Layer Selection")]
    [Tooltip("Select layers you want to be detectable")]
    public LayerMask targetLayers = 1; // Default layer by default

    void Start()
    {
        InitializeCamera();
    }

    void InitializeCamera()
    {
        _camera = GetComponent<Camera>();
        UpdateReplacementShader();
    }

    void UpdateReplacementShader()
    {
        if (_camera != null && SegmentSelectShader != null)
        {
            _camera.SetReplacementShader(SegmentSelectShader, "RenderType");
        }
    }

    void LateUpdate()
    {
        UpdateSelection();
        UpdateAllMaterials();
    }

    void UpdateSelection()
    {
        _selectedRenderer = null;
        float closestDistance = Mathf.Infinity;
        Ray centerRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            // Check if object's layer is included in the target layers mask
            if ((targetLayers.value & (1 << renderer.gameObject.layer)) == 0) continue;

            if (renderer.bounds.IntersectRay(centerRay, out float distance))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    _selectedRenderer = renderer;
                }
            }
        }
    }

    void UpdateAllMaterials()
    {
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            bool isSelected = renderer == _selectedRenderer;
            UpdateMaterialColor(renderer, isSelected);
        }
    }

    void UpdateMaterialColor(Renderer renderer, bool isSelected)
    {
        renderer.sharedMaterial.SetColor("_ObjectColor", 
            isSelected ? Color.white : Color.black);
    }

    private void OnValidate() => InitializeCamera();

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _selectedRenderer == null) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_selectedRenderer.bounds.center, _selectedRenderer.bounds.size);
    }
}