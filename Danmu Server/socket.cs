using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace serverkhd
{
    struct Net_inf
    {
        public IPAddress server_ip;
        public int port;
    }

    public class StateObject
    {
        // Client   socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    class socket_server
    {
        
        Net_inf server_inf = new Net_inf();
        public int MaxWaitConnectCount = 10;
        private static byte[] result = new byte[1024];
        static Socket server;
        public bool isCreate{get; private set;}

        #region Event
        public delegate void NullparameterEvent();
        public delegate void ClientIn(Socket obj);
        public delegate void clientclose(Socket client);
        public clientclose clientLeave=delegate (Socket client) { };
        public NullparameterEvent StartListen =delegate { };
        public ClientIn ClientInEvent = delegate (Socket client) { };
        public delegate void MsgIn(string str, Socket soc);
        public MsgIn MessageIn = delegate { };
        #endregion

        public string Endsign = "<EOF>";
        public bool isAsyncReceive = true;
        List<Socket> clients = new List<Socket>();
        public socket_server(IPAddress ip,int port)
        {
            server_inf.server_ip = ip;
            server_inf.port = port;
        }

        public bool Create()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(new IPEndPoint(server_inf.server_ip, server_inf.port));
                server.Listen(MaxWaitConnectCount);
                StartListen();
            }
            catch { return false; }
            Task Listening = new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        Socket temp = server.Accept();
                        clients.Add(temp);
                        ClientInEvent(temp);

                        if (!isAsyncReceive)
                        {
                            //同步模式
                            Thread MsgThread = new Thread(ReceiveMessage);
                            MsgThread.Start(temp);
                        }
                        else
                        {
                            //异步接受
                            StateObject state = new StateObject();
                            state.workSocket =temp;
                            temp.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReceiveCallback), state);

                        }
                    }
                    catch
                    {
                        isCreate = false;
                        break;
                    }
                    
                }

            });
            Listening.Start();
            
            isCreate = true;
            return true;
        }

        public void sendMessage(EndPoint e,string str)
        {
            foreach(Socket temp in clients)
            {
               if(temp.RemoteEndPoint.ToString() == e.ToString())
                {
                    byte[] td = Encoding.Unicode.GetBytes(str);
                    temp.BeginSend(td, 0, td.Length, 0, (ar) => {
                        // Retrieve the socket from the state object.     
                        Socket handler = (Socket)ar.AsyncState;
                        // Complete sending the data to the remote device.     
                        int bytesSent = handler.EndSend(ar);


                    }, temp);
                    break;
                }
            }
        }
        private  void ReceiveMessage(object clientSocket)
        {

            byte[] arrb = new byte[1024];
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //Debug.WriteLine("等待信息抵达." + AppDomain.GetCurrentThreadId().ToString());
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(arrb);
                    /*for(int i = 0; i < receiveNumber; i++)
                    {
                        Debug.Write(arrb[i]+",");
                    }
                    Debug.Write("\n");*/
                    if (receiveNumber <= 0) {

                        for(int i = 0; i < clients.Count; i++)
                        {
                            if (clients[i].RemoteEndPoint == myClientSocket.RemoteEndPoint) { clientLeave(clients[i]); clients.RemoveAt(i); }
                        }
                        break; }

                    byte[] factarr = new byte[receiveNumber];
                    Array.Copy(arrb, factarr, receiveNumber);
                    Debug.WriteLine(Encoding.UTF8.GetString(factarr));
                    //Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber));
                    Thread te=new Thread(()=>{ MessageIn(Encoding.UTF8.GetString(factarr), myClientSocket);
                      
                });
                    te.Start();
                    //Debug.WriteLine("信息抵达." + Encoding.ASCII.GetString(result)+" "+ AppDomain.GetCurrentThreadId().ToString());
                    
                }
                catch (Exception ex)
                {
                   
                    for (int i = 0; i < clients.Count; i++)
                    {
                        try
                        {
                            if (clients[i].RemoteEndPoint == myClientSocket.RemoteEndPoint) { clientLeave(clients[i]); clients.RemoveAt(i); }
                        }
                        catch { }
                    }
                    try
                    {
                        try
                        {
                            myClientSocket.Shutdown(SocketShutdown.Both);
                            myClientSocket.Close();
                        }
                        catch
                        {

                        }
                    }
                    catch { }
                    break;
                }
            }
        }
        public EndPoint getEndpoint()
        {
            if (isCreate)
                return server.LocalEndPoint;
            else
                return null;
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            int i = 0;
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                
                // Read data from the remote device.     

                int bytesRead = client.EndReceive(ar);
                Debug.WriteLine(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.     

                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    if (state.sb.ToString().Substring(state.sb.Length - Endsign.Length, Endsign.Length) == Endsign)
                    {
                        MessageIn(state.sb.ToString().Substring(0, state.sb.Length - Endsign.Length),client);
                        state.sb = new StringBuilder("");
                    }
                    // Get the rest of the data.     
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {

                    // All the data has arrived; put it in response.     
                    if (state.sb.Length > 1)
                    {
                        MessageIn(state.sb.ToString(),client);
                    }
                    // Signal that all bytes have been received.     

                }
            }
            catch (Exception e)
            {
                clientLeave(client);
                for (int temp = 0; temp < clients.Count; i++)
                {
                    try { if (clients[temp].RemoteEndPoint == client.RemoteEndPoint) clients.RemoveAt(temp); } catch { }
                }
                Console.WriteLine(e.ToString());
            }
        }
        public void Close()
        {
            Debug.WriteLine(clients.Count);
            if (isCreate) {
                for (int i = 0; i < clients.Count; i++) {
                    clients[i].Send(Encoding.UTF8.GetBytes("<Type>Leave</><IP>"+server.LocalEndPoint+"</>"));
                    clients[i].Shutdown(SocketShutdown.Both);
                    
                    clients[i].Close();
                }

                try { server.Shutdown(SocketShutdown.Both); } catch { }
                server.Close();
                server.Dispose();
                isCreate = false; }
        }
    }
    class socket_client
    {

        Net_inf server_inf= new Net_inf();

        byte[] msg_buff = new byte[1024];
        
        bool isConnected=false;
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //事件托管
        public delegate void Connected();           //成功连接
        public Connected Server_Connected = delegate { };
        public delegate void ReceiveMsg(string str);   //收到信息
        public ReceiveMsg Message_Receive = delegate { };
        public bool isAsyncReceive =false;
        public string Endsign = "<EOF>";
        public socket_client(IPAddress _ip,int _port)
        {
            server_inf.server_ip= _ip;
            server_inf.port = _port;
        }
        public EndPoint getEndpoint()
        {
            if (isConnected)
                return clientSocket.LocalEndPoint;
            else
                return null;
        }
        private void ReceiveMessage()
        {

            byte[] arrb = new byte[1024];
            Socket myClientSocket = clientSocket;
            while (true)
            {
                try
                {
                    
                    //Debug.WriteLine("等待信息抵达." + AppDomain.GetCurrentThreadId().ToString());
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(arrb);
                    /*for(int i = 0; i < receiveNumber; i++)
                    {
                        Debug.Write(arrb[i]+",");
                    }
                    Debug.Write("\n");*/
                    if (receiveNumber <= 0)
                    {

                        
                        break;
                    }

                    byte[] factarr = new byte[receiveNumber];
                    Array.Copy(arrb, factarr, receiveNumber);
                    //Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber));
                    Debug.WriteLine(Encoding.UTF8.GetString(factarr));
                    Thread te = new Thread(() => {
                        Message_Receive(Encoding.UTF8.GetString(factarr));

                    });
                    te.Start();
                    //Debug.WriteLine("信息抵达." + Encoding.ASCII.GetString(result)+" "+ AppDomain.GetCurrentThreadId().ToString());

                }
                catch (Exception ex)
                {

                   
                    break;
                }
            }
        }
        public bool Connect()
        {
            try
            {
                clientSocket.Connect(server_inf.server_ip, server_inf.port);
            }
            catch
            {
                return false;
            }
            Server_Connected();
            isConnected = true;



           

            if (isAsyncReceive) { 
            StateObject state = new StateObject();
            state.workSocket =clientSocket;
            clientSocket.BeginReceive(state.buffer,0,StateObject.BufferSize,0,new AsyncCallback(ReceiveCallback),state);
            }else
            {
                Task t = new Task(() =>
                {
                    ReceiveMessage();
                });
                t.Start();
            }
            return true;
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.     

                int bytesRead = client.EndReceive(ar);
                
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.     

                    state.sb.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));
                    if (state.sb.ToString().Substring(state.sb.Length-Endsign.Length, Endsign.Length) == Endsign)
                    {
                        Message_Receive(state.sb.ToString().Substring(0,state.sb.Length-Endsign.Length));
                        state.sb = new StringBuilder("");
                    }
                    // Get the rest of the data.     
                    
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    
                    // All the data has arrived; put it in response.     
                    if (state.sb.Length > 1)
                    {
                        Message_Receive( state.sb.ToString());
                    }
                    // Signal that all bytes have been received.     
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            if (isConnected) { clientSocket.Close(); isConnected = false; }
        }
        public void sendMessage(string str)
        {
            if (isConnected)
            {
                try
                {
                    byte[] td = Encoding.UTF8.GetBytes(str);
                    clientSocket.BeginSend(td, 0, td.Length, 0, (ar) => {
                        // Retrieve the socket from the state object.     
                        Socket handler = (Socket)ar.AsyncState;
                        // Complete sending the data to the remote device.     
                        int bytesSent = handler.EndSend(ar);
                        

                    }, clientSocket);
                }
                catch
                {

                }
            }
        }



    }
}
