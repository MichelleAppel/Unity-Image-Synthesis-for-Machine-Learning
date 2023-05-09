using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private TcpListener _server;
    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _serverThread;

    public int port = 8080;
    public List<GameObject> cameras;

    void Start()
    {
        StartServer();
    }

    void StartServer()
    {
        try
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            Debug.Log("Server started, listening for connections...");

            _serverThread = new Thread(ListenForClients)
            {
                IsBackground = true
            };
            _serverThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting server: " + e.Message);
        }
    }

    void ListenForClients()
    {
        while (true)
        {
            _client = _server.AcceptTcpClient();
            Debug.Log("Client connected!");

            _stream = _client.GetStream();

            foreach (var camera in cameras)
            {
                ImageCapture imageCapture = camera.GetComponent<ImageCapture>();
                if (imageCapture != null)
                {
                    SendImage(_stream, imageCapture);
                }
            }

            _stream.Close();
            _client.Close();
        }
    }
    
    void SendImage(NetworkStream stream, ImageCapture imageCapture)
    {
        byte[] imageData = imageCapture.CaptureImage();

        // Send mode header
        SendModeHeader(stream, imageCapture.mode);

        // Send the image data length as an integer (4 bytes)
        byte[] imageDataLength = BitConverter.GetBytes(imageData.Length);
        stream.Write(imageDataLength, 0, imageDataLength.Length);

        // Send the image data
        stream.Write(imageData, 0, imageData.Length);
        Debug.Log("Image data sent for mode: " + imageCapture.mode);
    }
    
    void SendModeHeader(NetworkStream stream, string mode)
    {
        byte[] modeBytes = Encoding.ASCII.GetBytes(mode);

        // Send the length of the mode string (4 bytes)
        byte[] modeLengthBytes = BitConverter.GetBytes(modeBytes.Length);
        stream.Write(modeLengthBytes, 0, modeLengthBytes.Length);

        // Send the mode string
        stream.Write(modeBytes, 0, modeBytes.Length);
    }
    
    void OnApplicationQuit()
    {
        // Close the server and all active connections
        _server.Stop();
        if (_stream != null) _stream.Close();
        if (_client != null) _client.Close();
    }
}