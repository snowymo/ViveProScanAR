using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class UdpClient : MonoBehaviour {

    Socket socket;
    EndPoint serverEnd;
    IPEndPoint ipEnd;

    byte[] recvData = new byte[1024];
    byte[] sendData = new byte[1024];
    int recvLen;
    Thread connectThread;
    string recvStr;

    public string defaultComputerIP;
    public int sendingPort;

    bool isConnect;

    public void InitSocket()
    {
        isConnect = false;
        //
        ipEnd = new IPEndPoint(IPAddress.Parse(defaultComputerIP), sendingPort);
        //
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        //print("waiting for sending UDP dgram");

        // 
        SocketSend("Is anybody there?");


        //开启一个线程连接，必须的，否则主线程卡死  
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketSend(string sendStr)
    {
        //  
        sendData = new byte[1024];
        // 
        sendData = Encoding.ASCII.GetBytes(sendStr);
        // 
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }

    void SocketSend(byte[] sendStr)
    {
        // 
        socket.SendTo(sendStr, sendStr.Length, SocketFlags.None, ipEnd);
    }

    void SocketReceive()
    {
        //
        while (true)
        {
            //  
            recvData = new byte[1024];
            //  
            recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
            print("message from: " + serverEnd.ToString()); //  
            //  
            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            print(recvStr);

            isConnect = true;
        }
    }

    void SocketQuit()
    {
        //  
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //  
        if (socket != null)
            socket.Close();
    }

    // Use this for initialization
    void Start () {
        InitSocket();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnApplicationQuit()
    {
        SocketQuit();
    }
}
