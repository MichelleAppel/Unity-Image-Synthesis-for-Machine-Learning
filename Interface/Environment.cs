using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ImgSynthesis = ArchViz_Interface.Scripts.ImageSynthesis.ImageSynthesis;

[ExecuteInEditMode]
public class Environment : MonoBehaviour {

    // Image processing variables
    private Camera _camera;
    private List<Color32[]> _state;
    private RenderTexture _targetTexture;
    public ImgSynthesis imgSynthesis;

    public int width = 1024;
    public int height = 1024;
    public byte[] _data;

    public byte[] Output {
        get {
            // Render the state (for the different render types: colors, semantic segmentation, depth, etc.)
            var tex = new Texture2D(width, height);
            _state = new List<Color32[]>();
            for(var idx = 0; idx<=6; idx++)
            {
                // Get hidden camera 
                var cam = ImgSynthesis.capturePasses[idx].camera;

                // Render
                RenderTexture.active = _targetTexture; //renderRT;
                cam.targetTexture = _targetTexture; // renderRT;
                tex.ReadPixels(new Rect(0, 0, _targetTexture.width, _targetTexture.height), 0, 0);
                tex.Apply();
                _state.Add(tex.GetPixels32());
            }
            Object.Destroy(tex);

            // Color32 arrays for each of the render types:
            var colors  = _state.ElementAt(0);
            var objseg  = _state.ElementAt(1);
            var semseg  = _state.ElementAt(2);
            var depth   = _state.ElementAt(3);
            var normals = _state.ElementAt(4);
            var sobel   = _state.ElementAt(5);
            var flow    = _state.ElementAt(6);
            
            // Write state to _data
            for (var y = 0;
                y < height;
                y++)
                for (var x = 0;
                    x < width;
                    x++) {
                    var i = 16 * (x - y * width + (height - 1) * width);
                    var j = 1 * (x + y * width);
                    _data[i + 2]  = colors[j].r;
                    _data[i + 3]  = colors[j].g;
                    _data[i + 4]  = colors[j].b;
                    _data[i + 5]  = objseg[j].r;
                    _data[i + 6]  = objseg[j].g;
                    _data[i + 7]  = objseg[j].b;
                    _data[i + 8]  = semseg[j].r;
                    _data[i + 9]  = semseg[j].g;
                    _data[i + 10] = semseg[j].b;
                    _data[i + 11] = normals[j].r;
                    _data[i + 12] = normals[j].g;
                    _data[i + 13] = normals[j].b;
                    _data[i + 14] = flow[j].r;
                    _data[i + 15] = flow[j].g;
                    _data[i + 16] = flow[j].b;
                    _data[i + 17] = depth[j].r;
                    _data[i + 18] = sobel[j].r;
                }

            return _data;
        }
    }

    public void Reset() 
    { 

    }

    private void Start() {
        
        // Initialize camera
        _camera = Camera.main;
        (_camera.targetTexture = new RenderTexture(width, height, 0)).Create();
        _targetTexture = Camera.main.targetTexture;
        imgSynthesis = _camera.GetComponent<ImgSynthesis>();
        
        // Output data
        _data = new byte[2 + 16 * width * height];
    }
}