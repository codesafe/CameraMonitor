using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class RaspMachine
{
    private Socket socket;
    private Thread Socket_Thread = null;
    private bool loop = true;
    private object lockObject = new object();
    private int camnum = 0;
    private string machineName;
    private List<CameraObj> cameraList = new List<CameraObj>();

    public List<byte[]> packetList = new List<byte[]>();

    public RaspMachine(Socket _socket, string name, int _camnum)
    {
        camnum = _camnum;
        machineName = name;
        socket = _socket;
        Socket_Thread = new Thread(ReadWorker);
        Socket_Thread.Start();
    }

    public void AddCameraObj(CameraObj obj)
    {
        cameraList.Add(obj);
    }

    public List<CameraObj> GetCameraList()
    {
        return cameraList;
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
                int recvn = socket.Receive(receiveBuffer, 0, Predef.TCP_BUFFER, SocketFlags.None);

                if (recvn == 0)
                {
                    Debug.Log("Close Socket");
                    socket.Close();
                    loop = false;
                    continue;
                }

                lock (lockObject)
                {
                    Debug.Log("Recv Packet");
                    packetList.Add(receiveBuffer);
                }
                //string Test = Encoding.Default.GetString(receiveBuffer);
                //Debug.Log(Test);
            }

            catch (Exception e)
            {
                loop = false;
                socket.Close();
                continue;
            }
        }

        CameraManager.getInstance().AddRemoveRasp(machineName);
        Debug.Log("Exit thread");
    }

    public void Destroy()
    {
        loop = false;
        socket.Close();
        Socket_Thread.Abort();
        Socket_Thread.Join();

        for(int i=0; i< cameraList.Count; i++)
            UnityEngine.Object.DestroyImmediate(cameraList[i].gameObject);

        cameraList.Clear();
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

    public void Update()
    {
        byte[] buf = GetRecvPacket();
        if (buf != null)
        {

        }
    }
}
