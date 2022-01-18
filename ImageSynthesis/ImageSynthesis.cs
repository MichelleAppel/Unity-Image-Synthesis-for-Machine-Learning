using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MLDataset;
using Newtonsoft.Json;

namespace ArchViz_Interface.Scripts.ImageSynthesis {
	// [ExecuteInEditMode]
	[RequireComponent (typeof(Camera))]
	public class ImageSynthesis : MonoBehaviour {

		// pass configuration
		public static CapturePass[] capturePasses = {
			new CapturePass() { name = "_img" },
			new CapturePass() { name = "_id", supportsAntialiasing = false },
			new CapturePass() { name = "_layer", supportsAntialiasing = false },
			new CapturePass() { name = "_depth" },
			new CapturePass() { name = "_normals" },
			new CapturePass() { name = "_outlines", supportsAntialiasing = false },
			new CapturePass() { name = "_groundtruth", supportsAntialiasing = false }
		};

		public struct CapturePass {
			// configuration
			public string name;
			public bool supportsAntialiasing;
			public bool needsRescale;
			public CapturePass(string name_) { name = name_; supportsAntialiasing = true; needsRescale = false; camera = null; }

			// impl
			public Camera camera;
		};
		
		private Shader uberReplacementShader;
		private Material outlineMaterial;

		public bool saveToDisk = true;
		public bool saveMetaFile = true;
		public string path = "output/MLDataset";
		public string metaFileName = "meta";
		public int fileCounter = 0;
		public int width = 2048;
		public int height = 2048;
		
		void Start()
		{
			// default fallbacks, if shaders are unspecified
			if (!uberReplacementShader)
				uberReplacementShader = Shader.Find("Hidden/UberReplacement");

			// use real camera to capture final image
			capturePasses[0].camera = GetComponent<Camera>();
			for (int q = 1; q < capturePasses.Length; q++)
				capturePasses[q].camera = CreateHiddenCamera (capturePasses[q].name);

			capturePasses[5].camera.gameObject.AddComponent<PostProcessing_Sobel>(); // Outlines

			OnCameraChange();
			OnSceneChange();
		}
		
		void LateUpdate()
		{
			if (DetectPotentialSceneChangeInEditor())
				OnSceneChange();

			// @TODO: detect if camera properties actually changed
			OnCameraChange();

			if ( saveToDisk ) {
				Save("" + fileCounter, width, height, path);
				fileCounter++;
			}
		}
		
		private Camera CreateHiddenCamera(string name)
		{
			var go = new GameObject (name, typeof (Camera));
			go.hideFlags = HideFlags.DontSave;
			go.transform.parent = transform;

			var newCamera = go.GetComponent<Camera>();
			return newCamera;
		}
		
		private static void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacelementModes mode)
		{
			SetupCameraWithReplacementShader(cam, shader, mode, Color.black);
		}

		private static void SetupCameraWithReplacementShader(Camera cam, Shader shader, ReplacelementModes mode, Color clearColor)
		{
			var cb = new CommandBuffer();
			cb.SetGlobalFloat("_OutputMode", (int)mode); // @TODO: CommandBuffer is missing SetGlobalInt() method
			cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
			cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
			cam.SetReplacementShader(shader, "");
			cam.backgroundColor = clearColor;
			cam.clearFlags = CameraClearFlags.SolidColor;
		}

		enum ReplacelementModes {
			ObjectId 			= 0,
			CatergoryId			= 1,
			DepthCompressed		= 2,
			DepthMultichannel	= 3,
			Normals				= 4,
			Outlines 			= 5,
			GroundTruth			= 6
		};

		public void OnCameraChange()
		{
			int targetDisplay = 1;
			var mainCamera = GetComponent<Camera>();
			foreach (var pass in capturePasses)
			{
				if (pass.camera == mainCamera)
					continue;

				// cleanup capturing camera
				pass.camera.RemoveAllCommandBuffers();

				// copy all "main" camera parameters into capturing camera
				pass.camera.CopyFrom(mainCamera);
				pass.camera.renderingPath = RenderingPath.Forward;

				// set targetDisplay here since it gets overriden by CopyFrom()
				pass.camera.targetDisplay = targetDisplay++;
			}

			// setup command buffers and replacement shaders
			SetupCameraWithReplacementShader(capturePasses[1].camera, uberReplacementShader, ReplacelementModes.ObjectId);
			SetupCameraWithReplacementShader(capturePasses[2].camera, uberReplacementShader, ReplacelementModes.CatergoryId);
			SetupCameraWithReplacementShader(capturePasses[3].camera, uberReplacementShader, ReplacelementModes.DepthCompressed, Color.white);
			SetupCameraWithReplacementShader(capturePasses[4].camera, uberReplacementShader, ReplacelementModes.Normals);
			SetupCameraWithReplacementShader(capturePasses[5].camera, uberReplacementShader, ReplacelementModes.Outlines);
			SetupCameraWithReplacementShader(capturePasses[6].camera, uberReplacementShader, ReplacelementModes.GroundTruth);
		}


		private void OnSceneChange()
		{
			var renderers = Object.FindObjectsOfType<Renderer>();
			var mpb = new MaterialPropertyBlock();

			List<Meta> metaObjects = new List<Meta>();

			int id = 0;
			foreach (var r in renderers)
			{
				// var id = r.gameObject.GetInstanceID(); (these range between -10000 to 10000 for some reason)
				var layer = r.gameObject.layer;
				var tag = r.gameObject.tag;
				var objectname = r.gameObject.name;

				Color32 IDColor = ColorEncoding.EncodeIDAsColor(id);
				Color32 NameColor = ColorEncoding.EncodeNameAsColor(objectname);
				
				mpb.SetColor("_ObjectColor", IDColor);
				mpb.SetColor("_CategoryColor", NameColor);
				mpb.SetColor("_Outlines", ColorEncoding.EncodeObjectOutlines(id));
				mpb.SetInt("_GroundTruth", id);
				r.SetPropertyBlock(mpb);

				if (saveMetaFile)
				{
					Meta meta = new Meta();
					meta.objectname = objectname;
					meta.id = id;
					meta.tag = tag;
					meta.layer = layer;
					meta.IDColor = IDColor;
					meta.nameColor = NameColor;

					metaObjects.Add(meta);
				}

				id++;
			}

			if (saveMetaFile)
			{
				if(!Directory.Exists(path))
					Directory.CreateDirectory(path);
				
				var pathWithExtension = Path.Combine(path, metaFileName + ".json");
				string jsonMeta = JsonConvert.SerializeObject(metaObjects);
				File.WriteAllText(
					pathWithExtension, 
					jsonMeta);
			}
			
		}

		private void Save(string filename, int width = -1, int height = -1, string path = "")
		{
			if (width <= 0 || height <= 0)
			{
				width = Screen.width;
				height = Screen.height;
			}

			var filenameExtension = System.IO.Path.GetExtension(filename);
			var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
			
			if(!Directory.Exists(path))
				Directory.CreateDirectory(path);
			
			var pathWithoutExtension = Path.Combine(path, filenameWithoutExtension);

			// execute as coroutine to wait for the EndOfFrame before starting capture
			StartCoroutine(
				WaitForEndOfFrameAndSave(pathWithoutExtension, filenameExtension, width, height));
		}

		private IEnumerator WaitForEndOfFrameAndSave(string filenameWithoutExtension, string filenameExtension, int width, int height)
		{
			yield return new WaitForEndOfFrame();
			Save(filenameWithoutExtension, filenameExtension, width, height);
		}

		private void Save(string filenameWithoutExtension, string filenameExtension, int width, int height)
		{
			foreach (var pass in capturePasses)
				Save(pass.camera, filenameWithoutExtension + pass.name + filenameExtension, width, height, pass.supportsAntialiasing, pass.needsRescale);
		}

		private void Save(Camera cam, string filename, int width, int height, bool supportsAntialiasing, bool needsRescale)
		{
			var mainCamera = GetComponent<Camera>();
			var depth = 24;
			var format = RenderTextureFormat.Default;
			var readWrite = RenderTextureReadWrite.Default;
			var antiAliasing = (supportsAntialiasing) ? Mathf.Max(1, QualitySettings.antiAliasing) : 1;

			var finalRT =
				RenderTexture.GetTemporary(width, height, depth, format, readWrite, antiAliasing);
			var renderRT = (!needsRescale) ? finalRT :
				RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, depth, format, readWrite, antiAliasing);

			var textureformat = cam.name == "_groundtruth" ? TextureFormat.R16 : TextureFormat.RGB24;
			
			var tex = new Texture2D(width, height, textureformat, false);

			var prevActiveRT = RenderTexture.active;
			var prevCameraRT = cam.targetTexture;

			// render to offscreen texture (readonly from CPU side)
			RenderTexture.active = renderRT;
			cam.targetTexture = renderRT;

			cam.Render();

			if (needsRescale)
			{
				// blit to rescale (see issue with Motion Vectors in @KNOWN ISSUES)
				RenderTexture.active = finalRT;
				Graphics.Blit(renderRT, finalRT);
				RenderTexture.ReleaseTemporary(renderRT);
			}

			// read offsreen texture contents into the CPU readable texture
			tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			tex.Apply();
			
			// encode texture

			byte[] bytes = cam.name == "_groundtruth" ? tex.GetRawTextureData() : tex.EncodeToPNG();

			if (cam.name == "_groundtruth")
			{
				filename += ".bin";
			}
			else
			{
				filename += ".png";
			}

			File.WriteAllBytes(filename, bytes);					

			// restore state and cleanup
			cam.targetTexture = prevCameraRT;
			RenderTexture.active = prevActiveRT;

			Object.DestroyImmediate(tex);
			RenderTexture.ReleaseTemporary(finalRT);
		}

		#if UNITY_EDITOR
		private GameObject lastSelectedGO;
		private int lastSelectedGOLayer = -1;
		private string lastSelectedGOTag = "unknown";
		private bool DetectPotentialSceneChangeInEditor()
		{
			bool change = false;
			// there is no callback in Unity Editor to automatically detect changes in scene objects
			// as a workaround lets track selected objects and check, if properties that are 
			// interesting for us (layer or tag) did not change since the last frame
			if (UnityEditor.Selection.transforms.Length > 1)
			{
				// multiple objects are selected, all bets are off!
				// we have to assume these objects are being edited
				change = true;
				lastSelectedGO = null;
			}
			else if (UnityEditor.Selection.activeGameObject)
			{
				var go = UnityEditor.Selection.activeGameObject;
				// check if layer or tag of a selected object have changed since the last frame
				var potentialChangeHappened = lastSelectedGOLayer != go.layer || lastSelectedGOTag != go.tag;
				if (go == lastSelectedGO && potentialChangeHappened)
					change = true;

				lastSelectedGO = go;
				lastSelectedGOLayer = go.layer;
				lastSelectedGOTag = go.tag;
			}

			return change;
		}

#endif // UNITY_EDITOR
	}
}