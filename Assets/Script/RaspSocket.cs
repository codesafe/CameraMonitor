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
        //IPEndPoint clientep = (IPEndPoint)socket.RemoteEndPoint;
        //NetworkStream recvStm = new NetworkStream(socket);

        // 카메라 번호를 알려줌
        //         byte[] sendBuf = new byte[Predef.TCP_BUFFER];
        //         sendBuf[0] = Convert.ToByte(camnum);
        //         socket.Send(sendBuf, Predef.TCP_BUFFER, SocketFlags.None);

        while (loop)
        {
            try
            {
                byte[] receiveBuffer = new byte[Predef.TCP_BUFFER];

                //int recvn = recvStm.Read(receiveBuffer, 0, Predef.TCP_BUFFER);
                int recvn = tcpsocket.Receive(receiveBuffer, 0, Predef.TCP_BUFFER, SocketFlags.None);

                if (recvn == 0)
                {
                    Debug.Log("Close Socket");
                    tcpsocket.Close();
                    loop = false;
                    disconnected = true;
                    continue;
                }

                lock (lockObject)
                {
                    //Debug.Log("Recv Packet");
                    packetList.Add(receiveBuffer);
                }
                //string Test = Encoding.Default.GetString(receiveBuffer);
                //Debug.Log(Test);
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

}
