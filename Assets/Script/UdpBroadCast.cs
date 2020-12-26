using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BroadCastInfo
{
    public string myIPaddress;
    public byte[] ipbuffer;
    public IPAddress broadcastip;
}

public class UdpBroadCast : MonoBehaviour
{
    string myIPaddress;
    UdpClient client = new UdpClient();

    List<BroadCastInfo> sendlist = new List<BroadCastInfo>();

    void Start()
    {
        IPAddress[] ips = GetDirectedBroadcastAddresses();
        StartCoroutine(BroadCastMyIP());
    }

    // 1초마다 한번씩
    IEnumerator BroadCastMyIP()
    {
        while (true)
        {
            for(int i=0; i<sendlist.Count; i++)
            {
                IPEndPoint ip = new IPEndPoint(sendlist[i].broadcastip, Predef.udpbroadcastport);
                client.Send(sendlist[i].ipbuffer, sendlist[i].ipbuffer.Length, ip);
                //client.Close();

                //Debug.Log("Send broadcast : " + sendlist[i].broadcastip.ToString());
            }
            yield return new WaitForSecondsRealtime(3.0f);
        }
    }

    private IPAddress[] GetDirectedBroadcastAddresses()
    {
        List<IPAddress> list = new List<IPAddress>();
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            BroadCastInfo binfo = new BroadCastInfo();
            if (item.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            if (item.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            UnicastIPAddressInformationCollection unicasts = item.GetIPProperties().UnicastAddresses;

            foreach (UnicastIPAddressInformation unicast in unicasts)
            {
                IPAddress ipAddress = unicast.Address;

                if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                byte[] addressBytes = ipAddress.GetAddressBytes();
                byte[] subnetBytes = unicast.IPv4Mask.GetAddressBytes();

                if (addressBytes.Length != subnetBytes.Length)
                {
                    continue;
                }

                byte[] broadcastAddress = new byte[addressBytes.Length];
                for (int i = 0; i < broadcastAddress.Length; i++)
                {
                    broadcastAddress[i] = (byte)(addressBytes[i] | (subnetBytes[i] ^ 255));
                }

                binfo.myIPaddress = new IPAddress(addressBytes).ToString();
                binfo.ipbuffer = new byte[5];
                binfo.ipbuffer[0] = (byte)Predef.UDP_BROADCAST_PACKET;
                Buffer.BlockCopy(addressBytes, 0, binfo.ipbuffer, 1, 4);
                binfo.broadcastip = new IPAddress(broadcastAddress);
                sendlist.Add(binfo);

                list.Add(new IPAddress(broadcastAddress));
            }
        }

        return list.ToArray();
    }
}
