using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private Thread _listenThread;
    private static ManualResetEvent _allDone = new ManualResetEvent(false);

    private Queue _commandQueue = new Queue();
    private Socket _clientSocket;
    
    public int port = 8090;

    public Camera[] cameras; // Set this in the Unity editor to reference your cameras

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
        while (_commandQueue.Count > 0)
        {
            var command = _commandQueue.Dequeue();

            if ((string) command == "capture")
            {
                print("Capturing images");
                foreach (var camera in cameras)
                {
                    var texture = CaptureCamera(camera);
                    var bytes = texture.EncodeToPNG();
                    SendData(bytes, _clientSocket);
                }
                // Send end of transmission marker after all images are sent
                _clientSocket.Send(Encoding.ASCII.GetBytes("EOT"));
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
        handler.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), buffer);
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        byte[] buffer = (byte[])AR.AsyncState;
        Socket handler = _clientSocket;

        int bytesRead = handler.EndReceive(AR);
        if (bytesRead > 0)
        {
            // Convert the buffer into a command and add it to the command queue
            var command = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log("Received command: " + command);
            _commandQueue.Enqueue(command);
        }
    }
    
    // Capture a camera's view to a Texture2D
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

        camera.targetTexture = null;
        RenderTexture.active = currentRT;

        return image;
    }

    // Send data to the client
    private void SendData(byte[] data, Socket client)
    {
        if (client != null)
        {
            client.Send(data);
            // Send end of image marker
            client.Send(Encoding.ASCII.GetBytes("EOI"));
        }
    }
}
