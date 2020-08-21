using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using UnityEngine;
using UnityEngine.Animations;

namespace Part3
{
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    public static class NetManager
    {
        static Socket socket;
        static ByteArray readBuff;
        static Queue<ByteArray> writeQueue;
        static bool isConnecting = false; 
        static bool isClosing = false;
        static int msgCount = 0; //消息长度
        static List<MsgBase> msgList = new List<MsgBase>(); //消息列表
        readonly static int MAX_MESSAGE_FIRE = 10; //每次update处理的消息数量
        public delegate void EventListener(string err);
        public delegate void MsgListener(MsgBase msgBase);
        public static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
        public static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
        

        private static void InitSate()
        {
            msgList = new List<MsgBase>();
            msgCount = 0;
        }

        #region 消息监听与分发
        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <param name="msgName">消息名字</param>
        /// <param name="listener">委托方法</param>
        public static void AddMsgListener(string msgName,MsgListener listener)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName] += listener;
            }
            else
            {
                msgListeners[msgName] = listener;
            }
                
        }
        /// <summary>
        /// 移除消息监听
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="listener"></param>
        public static void RemoveMsgListener(string msgName,MsgListener listener)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName] -= listener;
                if(msgListeners[msgName] == null)
                {
                    msgListeners.Remove(msgName);
                }
            }
           
        }
        /// <summary>
        /// 执行消息
        /// </summary>
        /// <param name="msgName">消息名</param>
        /// <param name="msgBase"></param>
        private static void FireMsg(string msgName,MsgBase msgBase)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName](msgBase);
            }
        }


        #endregion

        #region 事件监听与执行
        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="listener"></param>
        public static void AddEventListener(NetEvent netEvent, EventListener listener)
        {
            if (eventListeners.ContainsKey(netEvent))
            {
                eventListeners[netEvent] += listener;
            }
            else
            {
                eventListeners[netEvent] = listener;
            }
        }
        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="listener"></param>
        public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
        {
            if (eventListeners.ContainsKey(netEvent))
            {
                eventListeners[netEvent] -= listener;
                if (eventListeners[netEvent] == null)
                {
                    eventListeners.Remove(netEvent);
                }
            }

        }
        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="err"></param>
        public static void FireEvent(NetEvent netEvent, string err)
        {
            if (eventListeners.ContainsKey(netEvent))
            {
                eventListeners[netEvent](err);
            }
        }
        #endregion


        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void Connet(string ip, int port)
        {
            if (socket != null && socket.Connected)
            {
                Debug.Log("connect failed, already connected ....");
                return;
            }
            if (isConnecting)
            {
                Debug.Log("connect failed, is connecting ....");
                return;
            }
            InitState();
            socket.NoDelay = true;
            isConnecting = true;
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public static void Close()
        {
            if (socket == null || !socket.Connected)
            {
                return;
            }
            if (isConnecting) return;
            if (writeQueue.Count > 0)
            {
                isClosing = true;
            }
            else
            {
                socket.Close();
                FireEvent(NetEvent.Close, "");
            }

        }
        public static void InitState()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            readBuff = new ByteArray();
            writeQueue = new Queue<ByteArray>();
            isConnecting = false;
            isClosing = false;
        }
        private static void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket socket = result.AsyncState as Socket;
                socket.EndConnect(result);
                Debug.Log("Socket connect succ!!!");
                FireEvent(NetEvent.ConnectSucc, "");
                isConnecting = false;
                ///////////////////////////////////////////////////// readBuff.bytes.Length是自己改的
                socket.BeginReceive(readBuff.bytes, readBuff.WriteIndex, readBuff.remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException se)
            {
                Debug.Log(se.ToString());
                FireEvent(NetEvent.ConnectFail, se.ToString());
                isConnecting = false;
            }
        }
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="msg"></param>
        public static void Send(MsgBase msg)
        {
            if(!socket.Connected|| socket == null)
            {
                return;
            }
            if (isClosing) return;
            if (isConnecting) return;
            byte[] namebytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            int len = namebytes.Length + bodyBytes.Length;
            byte[] sendBytes = new byte[len + 2];
            //组装长度
            sendBytes[0] = (byte)(len % 256);
            sendBytes[1] = (byte)(len / 256);
            //组装名字
            //需要试验的
            Array.Copy(bodyBytes, 0, sendBytes, namebytes.Length + 2, bodyBytes.Length);
            ByteArray ba = new ByteArray(sendBytes);
            int count = 0;   //writeQueue的长度
            lock (writeQueue)
            {
                writeQueue.Enqueue(ba);
                count = writeQueue.Count;
            }
            if(count == 1)
            {
                socket.BeginSend(sendBytes, 0, sendBytes.Length,0,SendCallback,socket);
            }

        }
        public static void SendCallback(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;
            if(socket == null || !socket.Connected)
            {
                return;
            }
            int count = socket.EndReceive(ar);
            ByteArray ba;
            lock (writeQueue)
            {
                ba = writeQueue.First();
            }
            ba.readIndex += count;
            if(ba.length == 0)
            {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();
                    ba = writeQueue.First();
                }
            }
            if (ba != null)
            {
                socket.BeginSend(ba.bytes, ba.readIndex, ba.length,0,SendCallback,socket);
                
            }else if (isClosing)
            {
                socket.Close();
            }
        }
        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                int count = socket.EndReceive(ar);
                if(count == 0)
                {
                    Close();
                    return;
                }
                readBuff.WriteIndex += count;
                //OnReceiveData();
                if (readBuff.remain < 8)
                {
                    readBuff.MoveBytes();
                    readBuff.ReSize(readBuff.length * 2);
                }
                socket.BeginReceive(readBuff.bytes, readBuff.WriteIndex, readBuff.remain, 0, ReceiveCallback, socket);

            }
            catch (SocketException se)
            {
                Debug.Log(se.ToString());
            }
        }



    }
}


