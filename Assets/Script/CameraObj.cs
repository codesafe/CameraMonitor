using System;
using System.IO;
using System.Collections;
//using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FreeImageAPI;

/*

public class Client
{
    private int camnum;
    private Socket socket;
    private Thread Socket_Thread = null;
    private bool loop = true;

    private object lockObject = new object();
    public List<byte[]> packetList = new List<byte[]>();

    public Client(int _camnum, Socket sock)
    {
        camnum = _camnum;
        socket = sock;

        Socket_Thread = new Thread(ReadWorker);
        Socket_Thread.Start();
    }

    private void ReadWorker()
    {
        //IPEndPoint clientep = (IPEndPoint)socket.RemoteEndPoint;
        //NetworkStream recvStm = new NetworkStream(socket);

        // 카메라 번호를 알려줌
        byte[] sendBuf = new byte[Predef.TCP_BUFFER];
        sendBuf[0] = Convert.ToByte(camnum);
        socket.Send(sendBuf, Predef.TCP_BUFFER, SocketFlags.None);

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
    }

    public void Destroy()
    {
        loop = false;
        socket.Close();
        Socket_Thread.Abort();
        Socket_Thread.Join();
    }

    public byte [] GetRecvPacket()
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
}


*/


// 일단 Client Disconnect은 생각하지 않는다
public class CameraObj : MonoBehaviour
{
    public enum CAMICON
    {
        ICON_GRAY,
        ICON_MID,
        ICON_NORMAL,
        ICON_ERR
    };

    [SerializeField] Sprite normal;
    [SerializeField] Sprite normal_gray;
    [SerializeField] Sprite normal_mid;
    [SerializeField] Sprite normal_err;

    [SerializeField] Image bg;
    [SerializeField] Image icon;
    [SerializeField] Image progress;
    [SerializeField] Text id;
    [SerializeField] RawImage preview;
    [SerializeField] Text camName;

    private Vector2 progressSize;
    private Vector2 previewSize;
    private Socket udpSocket;
    private IPEndPoint ipep;

    public int cameranum;
    public string cameraName;
    public float delaytime = 0;
    public float applydelaytime = 0;    // 실제로 모든 카메라를 통합한 적용값

    void Start()
    {
        SetIcon(CAMICON.ICON_GRAY);
        //id.text = "";
        progressSize = progress.rectTransform.sizeDelta;
        //progressSize.x = 71;

        previewSize = preview.rectTransform.sizeDelta;
        preview.enabled = false;

        EventTrigger trigger = GetComponent<EventTrigger>();
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerEnter;
        pointerDown.callback.AddListener((e) => ServerObj.getInstance().OnHover(this));
        trigger.triggers.Add(pointerDown);
    }

    private void OnDestroy()
    {

    }

    public void Init(int camnum, string ipAddress, int port)
    {
        cameranum = camnum;
        id.text = camnum.ToString();

//         ipep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
//         udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public void SetDownloadProgress(int percent)
    {
        if( percent == 0 )
        {
            progress.enabled = false;
        }
        else
        {
            progress.gameObject.SetActive(true);
            progress.enabled = true;

            Single per = ((percent * 10.0f) / 100.0f);
            Single width = progressSize.x * per;

            progress.rectTransform.sizeDelta = new Vector2(width, progressSize.y);
        }
    }

    public void SetIcon(CAMICON _icon)
    {
        //icon.sprite = focused ? normal : normal_gray;

        switch(_icon)
        {
            case CAMICON.ICON_GRAY:
                icon.sprite = normal_gray;
                break;
            case CAMICON.ICON_MID:
                icon.sprite = normal_mid;
                break;
            case CAMICON.ICON_NORMAL:
                icon.sprite = normal;
                break;
            case CAMICON.ICON_ERR:
                icon.sprite = normal_err;
                break;
        }

    }

//     private void Update()
//     {
//     }

    public void ShowPreview(string machineName)
    {
        preview.texture = null;
        //string path = string.Format("E:/ftp/name-{0}.jpg", cameranum);

        string path = string.Format("{0}/{1}/{2}-{3}.{4}", 
            Predef.ftpDirectoryName, Predef.capturedDirectoryName, 
            machineName , cameranum, Predef.capturedFileExt);

        if ( Predef.capturedFileExt == "raw" )
        {
            //string dcrawpath = Predef.ftpDirectoryName + "dcraw64.exe";

            // 그냥 읽을수 없어서 변환해야한다
            string outpath = string.Format("{0}/{1}/{2}-{3}.{4}",
            Predef.ftpDirectoryName, Predef.capturedDirectoryName,
            machineName, cameranum, "jpg");

            //string args = "-c -e " + path + " > " + outpath;
            //ProcessStartInfo p = new ProcessStartInfo(dcrawpath, args);
            //Process process = Process.Start(p);

            StartCoroutine(ConvertRawToJpg(path, outpath));
        }
        else
            StartCoroutine(load_image_preview(path));
    }


    public bool IsFileReady(string filename)
    {
        Debug.Log("Check file :  " + filename);
        try
        {
            using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                return inputStream.Length > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    const int ERROR_SHARING_VIOLATION = 32;
    const int ERROR_LOCK_VIOLATION = 33;
    private bool IsFileLocked(string file)
    {
        Debug.Log("Check file :  " + file);

        //check that problem is not in destination file
        if (File.Exists(file) == true)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex2)
            {
                //_log.WriteLog(ex2, "Error in checking whether file is locked " + file);
                int errorCode = Marshal.GetHRForException(ex2) & ((1 << 16) - 1);
                if ((ex2 is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
                {
                    return false;
                }
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
        return true;
    }

    private IEnumerator ConvertRawToJpg(string originpath, string outpath)
    {
        int retrycount = 0;
        while (true)
        {
            if (IsFileLocked(originpath) || retrycount > 10)
                break;

            yield return new WaitForSeconds(1.0f);
            retrycount++;
        }
        //yield return new WaitUntil(() => IsFileReady(originpath) == true);
        yield return new WaitForSeconds(1.0f);
        //yield return new WaitForSeconds(0.1f);

        FreeImageAPI.FREE_IMAGE_FORMAT format = FreeImageAPI.FreeImage.GetFileType(originpath, 0);
        Debug.Log("FreeImage Format : " + format.ToString());

        FIBITMAP handle = FreeImageAPI.FreeImage.Load(format, originpath, FREE_IMAGE_LOAD_FLAGS.RAW_PREVIEW);
        if( !handle.IsNull )
        {
            bool ret = FreeImageAPI.FreeImage.Save(FREE_IMAGE_FORMAT.FIF_JPEG, handle, outpath, FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYNORMAL);
            Debug.Log("FreeImage Save : " + (ret == true ? "OK" : "Fail"));
            FreeImageAPI.FreeImage.Unload(handle);
            handle.SetNull();
        }
        else
            Debug.Log("FreeImage handle is null ");

        StartCoroutine(load_image_preview(outpath));
    }

    public void OnClick()
    {
        CameraProperty.getInstance().SetSelectCamera(this);
        //Debug.Log("Onclick");
    }

    private IEnumerator load_image_preview(string _path)
    {
        yield return new WaitForSeconds(1.0f);
        WWW www = new WWW(_path);
        yield return www;
        Texture2D texTmp = new Texture2D(100, 100, TextureFormat.RGB24, false);

        www.LoadImageIntoTexture(texTmp);
        preview.texture = texTmp;
        preview.enabled = true;
    }


    public void OnHoverout()
    {
//        preview.enabled = false;
    }

    public void Reset()
    {
        preview.texture = null;
        preview.enabled = false;
        progress.enabled = false;
        SetIcon(CAMICON.ICON_GRAY);
    }

    public void SendUdpPacket(byte[] data, int size)
    {
        //data[0] = Convert.ToByte(packet);
        udpSocket.SendTo(data, size, SocketFlags.None, ipep);
    }

    public void SetcameraName(string _name)
    {
        cameraName = _name;
        camName.text = cameraName;
    }

    public void SetSelected(bool enable)
    {
        Color selcolor, normalcolor;
        
        ColorUtility.TryParseHtmlString("#FFFFFFFF", out normalcolor);
        ColorUtility.TryParseHtmlString("#00FFB6FF", out selcolor);
        bg.color = enable ? selcolor : normalcolor;
    }

}
