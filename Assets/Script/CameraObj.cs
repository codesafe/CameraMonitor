using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


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
    [SerializeField] Sprite normal;
    [SerializeField] Sprite normal_gray;

    [SerializeField] Image bg;
    [SerializeField] Image icon;
    [SerializeField] Image progress;
    [SerializeField] Text id;
    [SerializeField] RawImage preview;

    private Vector2 progressSize;
    private Vector2 previewSize;
    public int cameranum;

    void Start()
    {
        SetFocused(false);
        //id.text = "";
        progressSize = progress.rectTransform.sizeDelta;
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

    public void Init(int camnum)
    {
        cameranum = camnum;
        id.text = camnum.ToString();
    }

    public void SetDownloadProgress(int percent)
    {
        if( percent == 0 )
        {
            progress.gameObject.SetActive(false);
        }
        else
        {
            progress.gameObject.SetActive(true);
            Single width = progressSize.x * ((percent * 10.0f) / 100.0f);
            progress.rectTransform.sizeDelta = new Vector2(width, progressSize.y);
        }
    }

    public void SetFocused(bool focused)
    {
        icon.sprite = focused ? normal : normal_gray;
    }

//     private void Update()
//     {
//     }

    void ShowPreview()
    {
        preview.texture = null;
        //string path = string.Format("E:/ftp/name-{0}.jpg", cameranum);
        string path = string.Format("{0}/{1}/name-{2}.jpg", Predef.ftpDirectoryName, Predef.capturedDirectoryName, cameranum);
        StartCoroutine(load_image_preview(path));
    }

    public void OnClick()
    {
//         Vector2 screenpos = RectTransformUtility.WorldToScreenPoint(null, preview.transform.position);
// 
//         float fixx = 0;
//         float fixy = 0;
//         if ( screenpos.y + (previewSize.y/2) >= Screen.height )
//         {
//             fixy = -previewSize.y + (Screen.height - screenpos.y);
//         }
// 
//         if (screenpos.x + (previewSize.x / 2) >= Screen.width)
//         {
//             fixx = -previewSize.x + (Screen.width - screenpos.x);
//         }
//         preview.transform.localPosition = new Vector3(fixx, fixy, 0);
// 
//         preview.enabled = true;
//         string path = string.Format("E:/ftp/name-{0}.jpg", cameranum);
//         StartCoroutine(load_image_preview(path));
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
        preview.enabled = false;
        progress.enabled = false;
        SetFocused(false);
    }
}
