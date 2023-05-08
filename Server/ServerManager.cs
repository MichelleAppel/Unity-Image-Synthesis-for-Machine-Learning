using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// [ExecuteInEditMode]
public class ServerManager : MonoBehaviour
{
    public int port = 8080;
    private TcpListener server;
    private Thread serverThread;
    private List<TcpClient> clients;
    private bool isRunning = false;

    private void Start()
    {
        clients = new List<TcpClient>();
        serverThread = new Thread(new ThreadStart(ListenForIncomingConnections));
        isRunning = true;
        serverThread.Start();
    }

    private void Update()
    {
        // You can call SendImageData() whenever you want to send the captured images to the connected clients.
        // For example, call this function in Update() if you want to send images on every frame.
    }

    private void ListenForIncomingConnections()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Debug.Log("Server started on port " + port);

            while (isRunning)
            {
                TcpClient client = server.AcceptTcpClient();
                lock (clients)
                {
                    clients.Add(client);
                }
                Debug.Log("Client connected: " + client.Client.RemoteEndPoint);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket error: " + ex.Message);
        }
    }

    private void SendImageData(byte[] imageData)
    {
        lock (clients)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                TcpClient client = clients[i];
                NetworkStream stream = client.GetStream();

                if (!client.Connected)
                {
                    clients.RemoveAt(i);
                    Debug.Log("Client disconnected: " + client.Client.RemoteEndPoint);
                }
                else
                {
                    try
                    {
                        stream.Write(imageData, 0, imageData.Length);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error sending data: " + ex.Message);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        if (server != null) server.Stop();
        if (serverThread != null && serverThread.IsAlive) serverThread.Abort();
    }
}
