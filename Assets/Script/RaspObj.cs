using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class RaspObj : MonoBehaviour
{
    [SerializeField] CameraObj prefab;
    [SerializeField] GameObject cameras;
    [SerializeField] Text nameText;
    private RaspSocket raspsocket = null;
    private List<CameraObj> cameraobjList = new List<CameraObj>();
    private int camnum = 0;
    private string machineName;

    void Start()
    {
    }

    private void OnDestroy()
    {
        if (raspsocket != null)
            raspsocket.Destroy();

        for (int i=0; i<cameraobjList.Count; i++)
            Destroy(cameraobjList[i]);

        cameraobjList.Clear();
    }

    public void Init(Socket _socket, string name, int _camnum)
    {
        machineName = name;
        nameText.text = name;
        camnum = _camnum;

        raspsocket = new RaspSocket(_socket);

        for (int i = 0; i < _camnum; i++)
        {
            CameraObj obj = Instantiate(prefab, cameras.transform);
            obj.Init(i);
            cameraobjList.Add(obj);
        }

        Refresh();
    }

    void Refresh()
    {
        for(int i=0; i<cameraobjList.Count; i++)
        {
            cameraobjList[i].transform.localPosition = new Vector3(i * 110, 0, 0);
        }
    }

    private void Update()
    {
        if( raspsocket != null )
        {
            // Update packet
            while(true)
            {
                byte[] buf = raspsocket.GetRecvPacket();
                if (buf == null)
                    break;

                // TODO. 패킷 받았다. 처리해야한다
                char packet = Convert.ToChar(buf[0]);
                if (packet == Predef.PACKET_SETPARAMETER_RESULT)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int result = (int)Convert.ToChar(buf[2]);

                    if( result == Predef.RESPONSE_OK )
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_MID);
                    else
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_GRAY);
                }
                else if ( packet == Predef.PACKET_AUTOFOCUS_RESULT)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int result = (int)Convert.ToChar(buf[2]);
                    if (result == Predef.RESPONSE_OK)
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_NORMAL);
                    else
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_GRAY);
                }
                else if (packet == Predef.PACKET_SHOT_RESULT)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int result = (int)Convert.ToChar(buf[2]);
                    if (result == Predef.RESPONSE_OK)
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_NORMAL);
                    else
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_GRAY);
                }
                else if ( packet == Predef.PACKET_UPLOAD_PROGRESS )
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int percent = (int)Convert.ToChar(buf[2]);
                    cameraobjList[cameraNum].SetDownloadProgress(percent);

                    Debug.Log(string.Format("Upload Progress : {0} : {1}", cameraNum, percent));
                }
                else if (packet == Predef.PACKET_UPLOAD_DONE)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    cameraobjList[cameraNum].SetDownloadProgress(10);
                    cameraobjList[cameraNum].ShowPreview(machineName);
                    Debug.Log(string.Format("Upload Done : {0} ", cameraNum));

                }
            }
        }
    }

    // 끊겼나?? 그럼 제거해야한다
    public bool IsDisconnected()
    {
        if (raspsocket != null)
            return raspsocket.IsDisconnected();
        return false;
    }

    public void SendParameter(int iso_value, int shutterspeed_value, int aperture_value, int captureformat)
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        char packet = Predef.PACKET_SET_PARAMETER;
        data[0] = Convert.ToByte(packet);
        data[1] = Convert.ToByte(iso_value);
        data[2] = Convert.ToByte(shutterspeed_value);
        data[3] = Convert.ToByte(aperture_value);
        data[4] = Convert.ToByte(captureformat);

        raspsocket.SendUdpPacket(data, Predef.UDP_BUFFER);
    }

    public void SendAutoFocus()
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        char packet = Predef.PACKET_HALFPRESS;
        data[0] = Convert.ToByte(packet);
        raspsocket.SendUdpPacket(data, Predef.UDP_BUFFER);
    }

    public void Capture(string path)
    {
//         DateTime dt2 = new DateTime();
//         dt2 = DateTime.Now;
//         Predef.capturedDirectoryName = dt2.ToString("yyyyMMdd-HH_mm_ss");
//         string path = string.Format("{0}/{1}", Predef.ftpDirectoryName, Predef.capturedDirectoryName);
//         MakeDirectory(path);

        byte[] data = new byte[Predef.UDP_BUFFER];
        char packet = Predef.PACKET_SHOT;
        data[0] = Convert.ToByte(packet);

        // ftp의 폴더를 전달
        byte[] namebytes = Encoding.UTF8.GetBytes(Predef.capturedDirectoryName);
        for(int i=0; i< namebytes.Length; i++)
            data[i + 1] = namebytes[i];

        raspsocket.SendUdpPacket(data, Predef.UDP_BUFFER);

        for (int i = 0; i < cameraobjList.Count; i++)
        {
            cameraobjList[i].Reset();
            cameraobjList[i].SetIcon(CameraObj.CAMICON.ICON_NORMAL);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < cameraobjList.Count; i++)
        {
            cameraobjList[i].Reset();
        }
    }
}
