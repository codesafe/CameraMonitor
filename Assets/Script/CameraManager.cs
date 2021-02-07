using System;
using System.Text;
using System.IO;
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

    Vector2 pivot = new Vector2(-460, 290);

    private List<string> removeList = new List<string>();
    private RaspObj [] raspMachinelist = new RaspObj[Predef.MAX_RASPI];

    public static CameraManager getInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    int AddRaspMachine(RaspObj machine)
    {
        for(int i=0; i< Predef.MAX_RASPI; i++)
        {
            if (raspMachinelist[i] == null)
            {
                raspMachinelist[i] = machine;
                return i;
            }
        }
        return -1;
    }

    void RemoveRaspMachine(RaspObj machine)
    {
        for (int i = 0; i < Predef.MAX_RASPI; i++)
        {
            if (raspMachinelist[i] == machine)
            {
                Destroy(raspMachinelist[i].gameObject);
                raspMachinelist[i] = null;
            }
        }
    }

    void Update()
    {
/*
        if (Input.GetKeyDown("space"))
        {
            RaspObj raspobj = Instantiate(raspprefab, transform);
            raspobj.Init(null, "test", 1);
            raspMachinelist.Add(raspobj);

            Refresh();
        }*/

        AcceptNewRasp();
        UpdateRasp();
    }

    // Rasp socket 연결이 들어옴
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
        RaspObj raspobj = Instantiate(raspprefab, transform);
        int machinenum = AddRaspMachine(raspobj);
        raspobj.Init(clientsocket, machinenum);

        Refresh();
    }

    void UpdateRasp()
    {
        // Disconnected 된것들 제거
        for (int i = 0; i<raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null && raspMachinelist[i].IsDisconnected())
            {
                RemoveRaspMachine(raspMachinelist[i]);
                Refresh();
            }
        }
    }

    void Refresh()
    {
        int posy = 0;
        for(int i=0; i< raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
            {
                raspMachinelist[i].transform.localPosition = new Vector3(pivot.x, pivot.y - (posy * 70), 0);
                posy++;
            }
        }
    }

    public void SendParameter(int iso_value, int shutterspeed_value, int aperture_value, int captureformat)
    {
        for (int i = 0; i < raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
                raspMachinelist[i].SendParameter(iso_value, shutterspeed_value, aperture_value, captureformat);
        }
    }
    public void SendAutoFocus()
    {
        DateTime dt2 = new DateTime();
        dt2 = DateTime.Now;
        Predef.capturedDirectoryName = dt2.ToString("yyyyMMdd-HH_mm_ss");
        Predef.workingFolder = string.Format("{0}/{1}", Predef.ftpDirectoryName, Predef.capturedDirectoryName);

        for (int i = 0; i < raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
                raspMachinelist[i].SendAutoFocus();
        }
    }

    public void SendAutoFocusToggle(bool focused)
    {
        DateTime dt2 = new DateTime();
        dt2 = DateTime.Now;
        Predef.capturedDirectoryName = dt2.ToString("yyyyMMdd-HH_mm_ss");
        Predef.workingFolder = string.Format("{0}/{1}", Predef.ftpDirectoryName, Predef.capturedDirectoryName);

        for (int i = 0; i < raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
                raspMachinelist[i].SendAutoFocusToggle(focused);
        }
    }

    public void SendUploadPath()
    {
        for (int i = 0; i < raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
                raspMachinelist[i].SendUploadPath();
        }

        MakeDirectory(Predef.workingFolder);
    }

    public void Capture()
    {
        for (int i = 0; i < raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
                raspMachinelist[i].Capture();
        }
    }

    public void Reset()
    {
        for (int i = 0; i < raspMachinelist.Length; i++)
        {
            if (raspMachinelist[i] != null)
                raspMachinelist[i].Reset();
        }
    }

    public void AddRemoveRasp(string rasp)
    {
        removeList.Add(rasp);
    }

    private void MakeDirectory(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        if (di.Exists == false)
        {
            di.Create();
        }
    }
}
