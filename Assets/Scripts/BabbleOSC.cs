using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Babble
{
    // Copied from OscMessage https://github.com/benaclejames/VRCFaceTracking/blob/master/VRCFaceTracking/OSC/OSCMessage.cs
    internal class BabbleOSC
    {
        public OscMessage message { get; private set; }

        private static readonly Socket ReceiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static Thread _receiveThread;

        public BabbleOSC(string address, int inPort)
        {
            try
            {
                ReceiverClient.Bind(new IPEndPoint(IPAddress.Parse(address), inPort));
                ReceiverClient.ReceiveTimeout = 1000;

                _receiveThread = new Thread(() =>
                {
                    Recv();
                    Thread.Sleep(10);
                });
                _receiveThread.Start();
            }
            catch (Exception)
            {
                Debug.LogError("Failed to bind to OSC receiver.");
            }
        }

        private void Recv()
        {
            byte[] buffer = new byte[2048];
            try
            {
                ReceiverClient.Receive(buffer, buffer.Length, SocketFlags.None);
                message = new OscMessage(buffer);
            }
            catch (SocketException)
            {
                // Ignore as this is most likely a timeout exception
                message = null;
            }
        }

        public void Teardown()
        {
            ReceiverClient.Close();
            ReceiverClient.Dispose();
        }
    }
}
