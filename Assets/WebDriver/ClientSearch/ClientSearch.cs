using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace tech.ironsheep.WebDriver.ClientSearch
{
    public static class ClientSearch
    {

        public static bool BroadcastAppReady()
        {
            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress broadcastIP = GetBroadcastIP();
            if (broadcastIP == null) return false;
            //localIP.AddressFamily

            byte[] magicPacket = Encoding.ASCII.GetBytes("echo for clients");
            // set up broadcast message
            EndPoint send_ep = new IPEndPoint(broadcastIP, 30718);
            sendSocket.SendTo(magicPacket, magicPacket.Length, SocketFlags.None, send_ep);

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
