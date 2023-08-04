using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;
using UnityEngine;

public class ServerManagerMulti : MonoBehaviour
{
    private Thread _listenThread;
    private static ManualResetEvent _allDone = new ManualResetEvent(false);

    private Queue _commandQueue = new Queue();
    private Socket _clientSocket;
    private object _commandQueueLock = new object();

    public int port = 8090;

    public Camera[] cameras; // Set this in the Unity editor to reference your cameras

    public CameraMovement cameraMovement;
    
    void Start()
    {
        _listenThread = new Thread(new ThreadStart(ServerThreadFunc));
        _listenThread.IsBackground = true;
        _listenThread.Start();
    }

    void ServerThreadFunc()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(1);

            while (true)
            {
                _allDone.Reset();

                Debug.Log("Waiting for a connection...");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                _allDone.WaitOne();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    void Update()
    {
        // Process commands on the main thread
        while (true)
        {
            object command = null;
            lock(_commandQueueLock)
            {
                if (_commandQueue.Count > 0)
                {
                    command = _commandQueue.Dequeue();
                }
            }

            if (command != null)
            {
                // Assume command is the index string now
                var indexString = (string) command;
                int index = int.Parse(indexString);
        
                // Use index to set pose deterministically
                SetPose(index);

                // First, send the number of cameras
                int numCameras = cameras.Length;
                byte[] numCamerasBytes = BitConverter.GetBytes(numCameras);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(numCamerasBytes); // Ensure we're sending the bytes in network byte order
                }
                _clientSocket.Send(numCamerasBytes);

                // Continue with image capture and sending process
                foreach (var camera in cameras)
                {
                    var texture = CaptureCamera(camera);
                    var bytes = texture.EncodeToPNG();
                    SendData(bytes, _clientSocket);
                }
                // Send end of transmission marker after all images are sent
                try 
                {
                    _clientSocket.Send(Encoding.ASCII.GetBytes("EOT"));
                }
                catch (Exception e) 
                {
                    Debug.Log("Failed to send EOT: " + e.ToString());
                }
            }
            else 
            {
                break;
            }
        }
    }


    
    
    private void AcceptCallback(IAsyncResult ar)
    {
        _allDone.Set();

        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        _clientSocket = handler;

        // Start receiving data from the client
        byte[] buffer = new byte[1024];
        try
        {
            handler.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), buffer);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to begin receive: " + e.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        byte[] buffer = (byte[])AR.AsyncState;
        Socket handler = _clientSocket;

        int bytesRead = 0;
        try 
        {
            bytesRead = handler.EndReceive(AR);
        }
        catch (Exception e) 
        {
            Debug.Log("Failed to end receive: " + e.ToString());
            return;
        }

        if (bytesRead > 0)
        {
            // Convert the buffer into a command and add it to the command queue
            var command = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            // Debug.Log("Received command: " + command);
            lock(_commandQueueLock)
            {
                _commandQueue.Enqueue(command);
            }
        }

        // Be ready to receive again on this connection after processing the command
        buffer = new byte[1024];
        try 
        {
            handler.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), buffer);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to begin receive: " + e.ToString());
        }
    }

    // Set the pose of the camera based on the index
    private void SetPose(int index)
    {
        if (cameraMovement != null) // Check that the cameraMovement reference has been assigned
        {
            cameraMovement.SetPose(index);
        }
        else
        {
            Debug.LogError("CameraMovement reference not assigned in ServerManager.");
        }
    }


    private Texture2D CaptureCamera(Camera camera)
    {
        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;

        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        image.Apply();

        // Check if the camera is a depth camera
        if (camera.depthTextureMode == DepthTextureMode.Depth)
        {
            // Create a new texture with only one channel
            Texture2D imageOneChannel = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.R8, false);

            // Copy the red channel from the original image to the new image
            for (int y = 0; y < image.height; y++)
            {
                for (int x = 0; x < image.width; x++)
                {
                    float grayscale = image.GetPixel(x, y).r;
                    imageOneChannel.SetPixel(x, y, new Color(grayscale, grayscale, grayscale));
                }
            }
            imageOneChannel.Apply();

            // Replace the original image with the new image
            image = imageOneChannel;
        }

        camera.targetTexture = null;
        RenderTexture.active = currentRT;
        
        return image;
    }


    private void SendData(byte[] data, Socket client)
    {
        if (client != null && client.Connected)
        {

            // Send the length of the data first
            byte[] lengthBytes = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes); // Ensure we're sending the bytes in network byte order
            }
            client.Send(lengthBytes, 4, SocketFlags.None);

            // Then send the actual data
            client.Send(data, data.Length, SocketFlags.None);

            // Send end of image marker
            byte[] markerBytes = Encoding.ASCII.GetBytes("EOI");
            client.Send(markerBytes, markerBytes.Length, SocketFlags.None);
        }
        else
        {
            Debug.Log("Client socket is null or not connected, skipping SendData");
        }
    }

}
