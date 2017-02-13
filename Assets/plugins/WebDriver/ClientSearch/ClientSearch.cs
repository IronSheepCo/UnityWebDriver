using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


namespace tech.ironsheep.WebDriver.ClientSearch
{
    public static class ClientSearch
    {
        const int BROADCAST_IP = 23923;
        private static Socket _broadcastSocket;
        private static Socket broadcastSocket
        {
            get
            {
                if (_broadcastSocket == null)
                {
                    _broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _broadcastSocket.EnableBroadcast = true;
                    return _broadcastSocket;
                }
                else
                {
                    return _broadcastSocket;
                }
            }
        }

        private static IPAddress _broadcastIP;
        private static IPAddress broadcastIP
        {
            get
            {
                if (_broadcastIP == null)
                {
                    _broadcastIP = GetBroadcastIP();
                    return _broadcastIP;
                }
                else
                {
                    return _broadcastIP;
                }
            }
        }

        private static IPEndPoint _sendingIP;
        private static IPEndPoint sendingIP
        {
            get
            {
                if (_sendingIP == null)
                {
                    _sendingIP = new IPEndPoint(broadcastIP, BROADCAST_IP);
                    return _sendingIP;
                }
                else
                {
                    return _sendingIP;
                }
            }
        }

        public static bool BroadcastAppReady()
        {
            if (broadcastIP == null) return false;
            if (sendingIP == null) return false;

            string deviceIdentifier = "";

            if (!SystemInfo.deviceUniqueIdentifier.Equals(SystemInfo.unsupportedIdentifier))
            {
                deviceIdentifier += "+++" + SystemInfo.deviceUniqueIdentifier;
            }
            else
            {
                deviceIdentifier += "+++" + "default device ID";
            }

            if (!SystemInfo.deviceModel.Equals(SystemInfo.unsupportedIdentifier))
            {
                deviceIdentifier += "+++" + SystemInfo.deviceModel;
            }
            else
            {
                deviceIdentifier += "+++" + "default Model";
            }

            byte[] magicPacket = Encoding.ASCII.GetBytes("echo for clients" + deviceIdentifier);
            broadcastSocket.SendTo(magicPacket, magicPacket.Length, SocketFlags.None, sendingIP);
            
            return true;
        }

        private static IPAddress GetBroadcastIP()
        {
            IPAddress _ip = GetLocalIP();
            if (_ip == null) return null;

            byte[] ipToBytes = _ip.GetAddressBytes();
            ipToBytes[3] = 255; // set broadcast value

            IPAddress broadCastIp = new IPAddress(ipToBytes);
            return broadCastIp;
        }

        private static IPAddress GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }
    }
}
