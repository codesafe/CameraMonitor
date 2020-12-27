using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class RaspSocket
{
    private bool disconnected = false;
    private Socket tcpsocket;
//    private Socket udpSocket;

    private Thread Socket_Thread = null;
    private bool loop = true;
    private object lockObject = new object();
    private string ipAddress;
    private IPEndPoint ipep;

    public List<byte[]> packetList = new List<byte[]>();

    public RaspSocket(Socket _socket)
    {
        tcpsocket = _socket;

        if(tcpsocket != null)
        {
            string address = tcpsocket.RemoteEndPoint.ToString();
            string[] array = address.Split(new char[] { ':' });
            ipAddress = array[0];

//             ipep = new IPEndPoint(IPAddress.Parse(ipAddress), Predef.udpport);
//             udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Socket_Thread = new Thread(ReadWorker);
            Socket_Thread.Start();
        }
    }

    private void ReadWorker()
    {
        while (loop)
        {
            try
            {
                byte[] receiveBuffer = new byte[Predef.TCP_BUFFER];

                int totalRecv = 0;
                while(true)
                {
                    int recv = tcpsocket.Receive(receiveBuffer, totalRecv, Predef.TCP_BUFFER - totalRecv, SocketFlags.None);
                    if (recv <= 0)
                    {
                        tcpsocket.Close();
                        loop = false;
                        disconnected = true;
                        Debug.Log("Close Socket");
                        break;
                    }

                    totalRecv += recv;
                    if (totalRecv >= Predef.TCP_BUFFER)
                        break;
                }

                lock (lockObject)
                {
                    packetList.Add(receiveBuffer);
                }
            }
            catch (Exception e)
            {
                loop = false;
                tcpsocket.Close();
                continue;
            }
        }

        //CameraManager.getInstance().AddRemoveRasp(machineName);
        Debug.Log("Exit thread");
    }

    public void Destroy()
    {
        loop = false;
        tcpsocket.Close();
        Socket_Thread.Abort();
        Socket_Thread.Join();
    }

    public byte[] GetRecvPacket()
    {
        byte[] packet = null;

        lock (lockObject)
        {
            if (packetList.Count > 0)
            {
                packet = packetList[0];
                packetList.RemoveAt(0);
            }
        }

        return packet;
    }

    public void SendTcpPacket(byte[] data, int size)
    {
        tcpsocket.Send(data, size, SocketFlags.None);
    }

//     public void SendUdpPacket(byte[] data, int size)
//     {
//         //data[0] = Convert.ToByte(packet);
//         udpSocket.SendTo(data, size, SocketFlags.None, ipep);
//     }

    public bool IsDisconnected()
    {
        return disconnected;
    }

    public Socket GetSocket()
    {
        return tcpsocket;
    }

}
