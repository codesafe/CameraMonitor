using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;



public class CameraManager : MonoBehaviour
{
    [SerializeField] CameraObj prefab;
    [SerializeField] RaspObj raspprefab;

    //Dictionary<string, RaspMachine> raspMachineList = new Dictionary<string, RaspMachine>();
    //List<CameraObj> cameraobjList = new List<CameraObj>();

    private static CameraManager _instance;

    Vector2 pivot = new Vector2(-450, 310);
    Vector2 stride = new Vector2(100, -100);

    private List<string> removeList = new List<string>();
    private List<RaspObj> raspMachinelist = new List<RaspObj>();

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
        if (Input.GetKeyDown("space"))
        {
            RaspObj raspobj = Instantiate(raspprefab, transform);
            raspobj.Init(null, "test", 1);
            raspMachinelist.Add(raspobj);

            Refresh();
        }

        AcceptNewRasp();
        UpdateRasp();
    }

    private void AcceptNewRasp()
    {
        List<Socket> socketlist = new List<Socket>();
        ServerSocket.getInstance().GetAcceptedSocket(ref socketlist);

        for (int i = 0; i < socketlist.Count; i++)
        {
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

            RaspObj raspobj = Instantiate(raspprefab, transform);
            raspobj.Init(clientsocket, machinename, cameranum);
            raspMachinelist.Add(raspobj);

            Debug.Log(machinename);
        }
        else
        {

        }

        Refresh();
    }

    void UpdateRasp()
    {
        // Disconnected 된것들 제거
        for (int i = raspMachinelist.Count - 1; i >= 0; i--)
        {
            if (raspMachinelist[i].IsDisconnected())
            {
                Destroy(raspMachinelist[i].gameObject);
                raspMachinelist.RemoveAt(i);
            }
        }
    }


    void Refresh()
    {
        for(int i=0; i< raspMachinelist.Count; i++)
        {
            raspMachinelist[i].transform.localPosition = new Vector3(pivot.x, pivot.y - (i * 70), 0);
        }
    }

    public void SendPacket(char packet)
    {
/*
        for(int i=0; i<cameraobjList.Count; i++)
        {
            cameraobjList[i].SendPacket(packet);
        }*/
    }

    public void SendPacket(char packet, int param1, int param2, int param3)
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        data[0] = Convert.ToByte(packet);
        data[1] = Convert.ToByte(param1);
        data[2] = Convert.ToByte(param2);
        data[3] = Convert.ToByte(param3);


//         for (int i = 0; i < raspMachinelist.Count; i++)
//         {
//             raspMachinelist[i].SendPacket(data);
//         }

    }

    public void SendAutoFocusWithParam(int iso_value, int shutterspeed_value, int aperture_value, int captureformat)
    {
        for (int i = 0; i < raspMachinelist.Count; i++)
        {
            raspMachinelist[i].SendAutoFocusWithParam(iso_value, shutterspeed_value, aperture_value, captureformat);
        }

    }

    public void Capture()
    {
        for (int i = 0; i < raspMachinelist.Count; i++)
        {
            raspMachinelist[i].Capture();
        }
    }

    public void Reset()
    {
/*
        for (int i = 0; i < cameraobjList.Count; i++)
        {
            cameraobjList[i].Reset();
        }*/
    }

    public void AddRemoveRasp(string rasp)
    {
        removeList.Add(rasp);
    }

}
