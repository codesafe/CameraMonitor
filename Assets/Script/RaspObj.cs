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
    private int camnum = 0;         // 카메라가 몇대인가
    private int machinenum = 0;     // 머신의 번호
    private string machineName;
    private bool ismasterRasp = false;

    void Start()
    {
    }

    private void OnDestroy()
    {
        if (raspsocket != null)
            raspsocket.Destroy();

        for (int i=0; i<cameraobjList.Count; i++)
        {
            CameraProperty.getInstance().RemoveCamera(cameraobjList[i]);
            Destroy(cameraobjList[i]);
        }


        cameraobjList.Clear();
    }

    public void Init(Socket _socket, int _machinenum)
    {
        machinenum = _machinenum;
        raspsocket = new RaspSocket(_socket);

        // RASP 머신 번호
        SendMachineNumber(machinenum);

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

                // 패킷 받았다. 처리해야한다
                char packet = Convert.ToChar(buf[0]);

                if (packet == Predef.PACKET_MACHINE_INFO)
                {
                    // 마스터 라즈베리인가?
                    ismasterRasp = Convert.ToChar(buf[1]) == Predef.RASP_MASTER ? true : false;
                    
                    // 몇대의 카메라?
                    camnum = (int)Convert.ToChar(buf[2]);

                    string _machinename = Encoding.UTF8.GetString(buf, 3, Predef.TCP_BUFFER - 3);
                    _machinename = _machinename.Replace("\0", string.Empty);

                    machineName = _machinename;
                    nameText.text = _machinename;

                    string address = raspsocket.GetSocket().RemoteEndPoint.ToString();
                    string[] array = address.Split(new char[] { ':' });
                    string ipAddress = array[0];

                    for (int i = 0; i < camnum; i++)
                    {
                        // 포트 계산 : 11000 + (머신번호*100) + 카메라 번호
                        int udpport = Predef.udpport + (machinenum * 100) + i;
                        CameraObj obj = Instantiate(prefab, cameras.transform);
                        obj.Init(i, ipAddress, udpport);
                        cameraobjList.Add(obj);

                        CameraProperty.getInstance().AddCamera(obj);
                    }

                    Refresh();
                }
                else if (packet == Predef.PACKET_CAMERA_NAME)
                {
                    // 카메라 이름 수신
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    string cameraname = Encoding.UTF8.GetString(buf, 2, Predef.TCP_BUFFER-2);
                    cameraname = cameraname.Replace("\0", string.Empty);
                    SetCameraName(cameraNum, cameraname);

                }
                else if (packet == Predef.PACKET_SETPARAMETER_RESULT)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int result = (int)Convert.ToChar(buf[2]);

                    if( result == Predef.RESPONSE_OK )
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_MID);
                    else
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_ERR);
                }
                else if ( packet == Predef.PACKET_AUTOFOCUS_RESULT)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int result = (int)Convert.ToChar(buf[2]);
                    if (result == Predef.RESPONSE_OK)
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_NORMAL);
                    else
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_ERR);
                }
                else if (packet == Predef.PACKET_SHOT_RESULT)
                {
                    int cameraNum = (int)Convert.ToChar(buf[1]);
                    int result = (int)Convert.ToChar(buf[2]);
                    if (result == Predef.RESPONSE_OK)
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_NORMAL);
                    else
                        cameraobjList[cameraNum].SetIcon(CameraObj.CAMICON.ICON_ERR);
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

    // 이건 머신번호 (포트계산을 위하여)
    public void SendMachineNumber(int num)
    {
        byte[] data = new byte[Predef.TCP_BUFFER];
        data[0] = Convert.ToByte(Predef.PACKET_MACHINE_NUMBER);
        data[1] = Convert.ToByte(num);
        raspsocket.SendTcpPacket(data, Predef.TCP_BUFFER);
    }

    // 설정
    public void SendParameter(int iso_value, int shutterspeed_value, int aperture_value, int captureformat)
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        char packet = Predef.PACKET_SET_PARAMETER;
        data[0] = Convert.ToByte(packet);
        data[1] = Convert.ToByte(iso_value);
        data[2] = Convert.ToByte(shutterspeed_value);
        data[3] = Convert.ToByte(aperture_value);
        data[4] = Convert.ToByte(captureformat);

        for(int i=0; i< cameraobjList.Count; i++)
        {
            // applydelaytime이 실제로 적용될 값이다
            byte[] FloatToByte = BitConverter.GetBytes(cameraobjList[i].applydelaytime);
            data[5] = FloatToByte[0];
            data[6] = FloatToByte[1];
            data[7] = FloatToByte[2];
            data[8] = FloatToByte[3];
//            Buffer.BlockCopy(FloatToByte, 0, data, 5, 4);
//            cameraobjList[i].SendUdpPacket(data, Predef.UDP_BUFFER);
        }

        raspsocket.SendTcpPacket(data, Predef.TCP_BUFFER);
    }

    // 포커스 잡아라
    public void SendAutoFocus()
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        char packet = Predef.PACKET_AUTOFOCUS;
        data[0] = Convert.ToByte(packet);

//         for (int i = 0; i < cameraobjList.Count; i++)
//             cameraobjList[i].SendUdpPacket(data, Predef.UDP_BUFFER);
        raspsocket.SendTcpPacket(data, Predef.TCP_BUFFER);
    }

    // 찍어
    public void Capture()
    {
        byte[] data = new byte[Predef.UDP_BUFFER];
        char packet = Predef.PACKET_SHOT;
        data[0] = Convert.ToByte(packet);

        // ftp의 폴더를 전달
        byte[] namebytes = Encoding.UTF8.GetBytes(Predef.capturedDirectoryName);
        for(int i=0; i< namebytes.Length; i++)
            data[i + 1] = namebytes[i];

//         for (int i = 0; i < cameraobjList.Count; i++)
//             cameraobjList[i].SendUdpPacket(data, Predef.UDP_BUFFER);

        raspsocket.SendTcpPacket(data, Predef.TCP_BUFFER);

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

    public void SetCameraName(int camnum, string cameraName)
    {
        cameraobjList[camnum].SetcameraName(cameraName);
    }
}
