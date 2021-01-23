using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Predef
{

    static public int RASP_SLAVE = 0;       // 슬레이브 라즈베리
    static public int RASP_MASTER = 1;      // 마스터 라즈베리

    static public int MAX_RASPI = 20;       // 라즈베리는 20대까지만

    static public string defaultIso = "100";
    static public string defaultAperture = "100";
    static public string defaultShutterSpeed = "100";

    static public int tcpport = 8888;
    static public int udpport = 11000;
    static public int udpbroadcastport = 9999;

    static public int TCP_BUFFER = 32;
    static public int UDP_BUFFER = 32;

    static public string ftpDirectoryName;
    static public string capturedDirectoryName;
    static public string capturedFileExt = "jpg";

    static public string workingFolder = "";

    // Packet
    static public char PACKET_MACHINE_NUMBER = (char)0x07;   // 머신 번호 ( Server -> Rasp )
    static public char PACKET_MACHINE_INFO = (char)0x08;     // 머신 정보
    static public char PACKET_CAMERA_NAME = (char)0x09;     // 카메라 이름

    static public char PACKET_SHOT = (char)0x10;        	// shot picture
    static public char PACKET_AUTOFOCUS = (char)0x20;   	// auto focus


    static public char PACKET_SET_PARAMETER = (char)0x30;   // 파라메터 조정

    static public char PACKET_FORCE_UPLOAD = (char)0x40;
    static public char PACKET_UPLOAD_PROGRESS = (char)0x41;
    static public char PACKET_UPLOAD_DONE = (char)0x42;

    static public char PACKET_SETPARAMETER_RESULT = (char)0x50;
    static public char PACKET_AUTOFOCUS_RESULT = (char)0x51;
    static public char PACKET_SHOT_RESULT = (char)0x52;

    static public char RESPONSE_OK = (char)0x07;
    static public char RESPONSE_FAIL = (char)0x08;

    // Broadcast info
    static public int UDP_BROADCAST_BUFFER = 32;
    static public char UDP_BROADCAST_PACKET = (char)0xA0;
}

