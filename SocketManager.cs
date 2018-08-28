using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketDemo1
{
    public class StateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public string Json ="";
    }

    public class SocketManager
    {
        private static string _ip = "";
        private static int _port = 0;

        static SocketManager()
        {
            _ip = "127.0.0.1";
            _port = 1001;
        }

        #region 连接

        public static void Connection()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(_ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);
                Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                Send(client, "MarkSession {\"Type\":10,\"No\":\"C003\"}\r\n");
                Receive(client);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region 接收

        private static void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.WorkSocket = client;
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,

                                   new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private static void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                StateObject state = (StateObject)asyncResult.AsyncState;
                Socket client = state.WorkSocket;
                int bytesRead = client.EndReceive(asyncResult);
                if (bytesRead > 0)
                {
                    Console.WriteLine("长度："+bytesRead);
                    state.Json=Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
                    client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    Console.WriteLine("-----------"+state.Json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region 发送

        private static void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket client = (Socket)asyncResult.AsyncState;
                client.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }
}
