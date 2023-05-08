using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class ServerManager : MonoBehaviour
{
    private TcpListener _server;
    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _serverThread;

    public int port = 8080;

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

            // TODO: Replace with the actual image data from your cameras
            byte[] imageData = Encoding.ASCII.GetBytes("Dummy image data");

            try
            {
                // Send the image data length as an integer (4 bytes)
                byte[] imageDataLength = BitConverter.GetBytes(imageData.Length);
                _stream.Write(imageDataLength, 0, imageDataLength.Length);

                // Send the image data
                _stream.Write(imageData, 0, imageData.Length);
                Debug.Log("Image data sent.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending image data: " + e.Message);
            }

            _stream.Close();
            _client.Close();
        }
    }

    void OnApplicationQuit()
    {
        // Close the server and all active connections
        _server.Stop();
        if (_stream != null) _stream.Close();
        if (_client != null) _client.Close();
    }
}
