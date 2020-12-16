using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;





public class CameraManager : MonoBehaviour
{
    [SerializeField] CameraObj prefab;

    Dictionary<string, RaspMachine> raspMachineList = new Dictionary<string, RaspMachine>();
    List<CameraObj> cameraobjList = new List<CameraObj>();
    private static CameraManager _instance;

    Vector2 pivot = new Vector2(-430, 300);
    Vector2 stride = new Vector2(100, -100);

    private List<string> removeList = new List<string>();

    public static CameraManager getInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        //         if (Input.GetKeyDown("space"))
        //         {
        //             AddCamera();
        //         }

        RemoveRasp();
        AcceptNewRasp();
        UpdateRasp();
    }

    void RemoveRasp()
    {
        if(removeList.Count > 0)
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                List<CameraObj> camlist = raspMachineList[removeList[i]].GetCameraList();
                for (int j = 0; j < camlist.Count; j++)
                {
                    cameraobjList.Remove(camlist[j]);
                    //DestroyImmediate(camlist[j]);
                }

                raspMachineList[removeList[i]].Destroy();
                raspMachineList.Remove(removeList[i]);
            }
            removeList.Clear();
            Refresh();
        }
    }

    private void AcceptNewRasp()
    {
        List<Socket> socketlist = new List<Socket>();
        ServerSocket.getInstance().GetAcceptedSocket(ref socketlist);

        for (int i = 0; i < socketlist.Count; i++)
        {
            //AddCamera(socketlist[i]);
            AddRaspMachine(socketlist[i]);
        }
    }

    private void AddRaspMachine(Socket clientsocket)
    {
        byte[] receiveBuffer = new byte[Predef.TCP_BUFFER];
        int recv = clientsocket.Receive(receiveBuffer, 0, Predef.TCP_BUFFER, SocketFlags.None);

        if ( recv > 0 )
        {
            int cameranum = Convert.ToInt32(receiveBuffer[0]);
            string machinename = Encoding.UTF8.GetString(receiveBuffer, 1, Predef.TCP_BUFFER - 1);
            machinename = machinename.Replace("\0", string.Empty);

            RaspMachine rasp = new RaspMachine(clientsocket, machinename, cameranum);
            raspMachineList.Add(machinename, rasp);

            for(int i=0; i< cameranum; i++)
            {
                CameraObj obj = Instantiate(prefab, transform);
                //obj.Init(cameraNum++, clientsocket);
                cameraobjList.Add(obj);
                rasp.AddCameraObj(obj);
            }

            Debug.Log(machinename);
        }
        else
        {

        }

        Refresh();
    }

    // 77개가 MAX
    public void AddCamera(Socket clientsocket)
    {
//         CameraObj obj = Instantiate(prefab, transform);
//         obj.Init(cameraNum++, clientsocket);
//         cameraobjList.Add(obj);

        Refresh();
    }

    void UpdateRasp()
    {
        foreach(var r in raspMachineList)
        {
            r.Value.Update();
        }
    }


    void Refresh()
    {
        int x = 0;
        int y = 0;

        for(int i=0; i<cameraobjList.Count; i++)
        {
            if( x > 10)
            {
                x = 0;
                y++;
            }
            cameraobjList[i].transform.localPosition = new Vector3(pivot.x + (stride.x * x), pivot.y + (stride.y * y), 0);
            x++;
        }
    }

    public void SendPacket(char packet)
    {
        for(int i=0; i<cameraobjList.Count; i++)
        {
            cameraobjList[i].SendPacket(packet);
        }
    }

    public void SendPacket(char packet, int param1, int param2, int param3)
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        data[0] = Convert.ToByte(packet);
        data[1] = Convert.ToByte(param1);
        data[2] = Convert.ToByte(param2);
        data[3] = Convert.ToByte(param3);

        for (int i = 0; i < cameraobjList.Count; i++)
        {
            cameraobjList[i].SendPacketWithBuf(data);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < cameraobjList.Count; i++)
        {
            cameraobjList[i].Reset();
        }
    }

    public void AddRemoveRasp(string rasp)
    {
        removeList.Add(rasp);
    }

}
