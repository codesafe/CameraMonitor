using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//using FreeImageAPI;
//using System.Runtime.InteropServices;
using SFB;

public class ServerObj : MonoBehaviour
{
    private static ServerObj _instance;
    [SerializeField] Button btnAutoFocus;
    [SerializeField] Button btnCapture;

    [SerializeField] Dropdown iso;
    [SerializeField] Dropdown shutterspeed;
    [SerializeField] Dropdown aperture;
    [SerializeField] Dropdown format;

    [SerializeField] RawImage previewImage;
    [SerializeField] Text ftpPath;
    [SerializeField] Text workingpath;


    private int iso_value;
    private int shutterspeed_value;
    private int aperture_value;
    private int captureformat_value;

    private readonly string[] isoString = { "Auto", "100", "200", "400", "800", "1600", "3200", "6400" };
    private readonly string[] shutterspeedString =
        { "bulb", "30", "25", "20", "15", "13", "10", "8", "6", "5", "4", "3.2", "2.5", "2", "1.6",
                "1.3", "1", "0.8", "0.6", "0.5", "0.4", "0.3", "1/4", "1/5", "1/6", "1/8", "1/10", "1/13",
                "1/15", "1/20", "1/25", "1/30", "1/40", "1/50", "1/60", "1/80", "1/100", "1/125", "1/160",
                "1/200", "1/250", "1/320", "1/400", "1/500", "1/640", "1/800", "1/1000", "1/1250", "1/1600",
                "1/2000", "1/2500", "1/3200", "1/4000"};
    private readonly string[] apertureString = { "5", "5.6", "6.3", "7.1", "8", "9", "10", "11", "13", "14", "16", "18", "20", "22", "25", "29", "32" };

    private readonly string [] captureformatString = { 
        "Large Fine JPEG", "Large Normal JPEG", "Medium Fine JPEG", "Medium Normal JPEG", "Small Fine JPEG",
        "Small Normal JPEG", "Smaller JPEG", "Tiny JPEG", "RAW + Large Fine JPEG", "RAW" };


    public static ServerObj getInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        ServerSocket.getInstance().Start();
        ReadPath();
        InitOption();
    }

    private void ReadPath()
    {
        if( PlayerPrefs.HasKey("FTP_PATH") == true )
        {
            Predef.ftpDirectoryName = PlayerPrefs.GetString("FTP_PATH");
            Predef.workingFolder = Predef.ftpDirectoryName;

            ftpPath.text = string.Format("Root path : {0}", Predef.ftpDirectoryName);
            workingpath.text = Predef.workingFolder;
        }
        else
        {
            ftpPath.text = "Please Select FTP Path.";
            workingpath.text = Predef.workingFolder;
        }

/*
        try
        {
            FileInfo theSourceFile = null;
            StreamReader reader = null;
            theSourceFile = new FileInfo("./config.txt");
            reader = theSourceFile.OpenText();
            Predef.ftpDirectoryName = reader.ReadLine();
            Predef.workingFolder = Predef.ftpDirectoryName;

            ftpPath.text = string.Format("Root path : {0}", Predef.ftpDirectoryName);
            workingpath.text = Predef.workingFolder;
        }
        catch (Exception ex)
        {
            ftpPath.text = "FTP Path ERR.";
        }
*/
    }

    private void OnDestroy()
    {
        ServerSocket.getInstance().Destroy();
    }


    void InitOption()
    {
        iso_value = 1;

        iso.options.Clear();
        for (int i=0; i< isoString.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = isoString[i];
            iso.options.Add(option);
        }
        iso.value = iso_value;
        iso.onValueChanged.AddListener(OnISOValueChanged);


        shutterspeed_value = 36;
        shutterspeed.options.Clear();
        for (int i = 0; i < shutterspeedString.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = shutterspeedString[i];
            shutterspeed.options.Add(option);
        }
        shutterspeed.value = shutterspeed_value;
        shutterspeed.onValueChanged.AddListener(OnShuutterSpeedValueChanged);

        aperture_value = 6;
        aperture.options.Clear();
        for (int i = 0; i < apertureString.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = apertureString[i];
            aperture.options.Add(option);
        }
        aperture.value = aperture_value;
        aperture.onValueChanged.AddListener(OnApertureValueChanged);

        captureformat_value = 0;
        format.options.Clear();
        for (int i = 0; i < captureformatString.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = captureformatString[i];
            format.options.Add(option);
        }
        format.value = captureformat_value;
        format.onValueChanged.AddListener(OnCaptureFormatValueChanged);
    }

    public void onClickReset()
    {
        CameraManager.getInstance().Reset();
        btnCapture.GetComponent<Button>().interactable = true;
    }

    public void onClickSetParameter()
    {
        //CameraManager.getInstance().Reset();
        CameraManager.getInstance().SendParameter(iso_value, shutterspeed_value, aperture_value, captureformat_value);

        if (captureformatString[captureformat_value] == "RAW + Large Fine JPEG" || captureformatString[captureformat_value] == "RAW")
            Predef.capturedFileExt = "raw";
        else
            Predef.capturedFileExt = "jpg";

        //UnityEngine.Debug.Log("Auto Focus!");
        //btnCapture.GetComponent<Button>().interactable = true;
    }

    public void onClickAutoFocus()
    {
        CameraManager.getInstance().Reset();

        CameraManager.getInstance().SendAutoFocus();
//         if (captureformatString[captureformat_value] == "RAW + Large Fine JPEG" || captureformatString[captureformat_value] == "RAW")
//             Predef.capturedFileExt = "raw";
//         else
//             Predef.capturedFileExt = "jpg";

        //UnityEngine.Debug.Log("Auto Focus!");
        btnCapture.GetComponent<Button>().interactable = true;
    }

    public void onClickCapture()
    {
        //CameraManager.getInstance().SendPacket(Predef.PACKET_SHOT);
        CameraManager.getInstance().Capture();
        workingpath.text = Predef.workingFolder;
        UnityEngine.Debug.Log("Shot!");
        btnCapture.GetComponent<Button>().interactable = false;
    }

    public void OnISOValueChanged(int value)
    {
        iso_value = value;
        UnityEngine.Debug.Log(isoString[iso_value]);
    }

    public void OnShuutterSpeedValueChanged(int value)
    {
        shutterspeed_value = value;
        UnityEngine.Debug.Log(shutterspeedString[shutterspeed_value]);
    }

    public void OnApertureValueChanged(int value)
    {
        aperture_value = value;
        UnityEngine.Debug.Log(apertureString[aperture_value]);
    }

    public void OnCaptureFormatValueChanged(int value)
    {
        captureformat_value = value;
        UnityEngine.Debug.Log(captureformatString[captureformat_value]);
    }

    public void OnHover(CameraObj camobj)
    {
//         if (camobj != null)
//             Debug.Log(string.Format("Hover Cam {0}", camobj.cameranum));
// 
//         string path = string.Format("E:/ftp/name-{0}.jpg", camobj.cameranum);
//         StartCoroutine(load_image_preview(path));
    }

    private IEnumerator load_image_preview(string _path)
    {
        WWW www = new WWW(_path);
        yield return www;
        Texture2D texTmp = new Texture2D(128, 128, TextureFormat.RGB24, false);

        www.LoadImageIntoTexture(texTmp);
        //cur_image_loaded = new Texture2D(256, 256, TextureFormat.RGB24, false);
        //cur_image_loaded = texTmp;
        previewImage.texture = texTmp;
    }

    public void OnOpenSelectFolder()
    {
        string [] paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Predef.ftpDirectoryName, false);
        if(paths.Length >0)
        {
            PlayerPrefs.SetString("FTP_PATH", paths[0]);
            Predef.ftpDirectoryName = paths[0];
            ftpPath.text = string.Format("Root path : {0}", Predef.ftpDirectoryName);
            UnityEngine.Debug.Log(paths[0]);
        }
    }

    public void OnOpenWorkingFolder()
    {
        string path = string.Format("file://{0}", Predef.workingFolder);
        Application.OpenURL(path);
        //Application.OpenURL("file://[dir]");
        //EditorUtility.RevealInFinder(path)
    }
}
