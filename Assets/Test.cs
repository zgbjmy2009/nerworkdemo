using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Part3
{
    public class Test : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            NetManager.AddEventListener(NetEvent.ConnectSucc, OnConnectSucc);
            NetManager.AddEventListener(NetEvent.ConnectFail, OnConnectFail);
            NetManager.AddEventListener(NetEvent.Close, OnConnectClose);
        }

        private void OnConnectSucc(string err)
        {
            Debug.Log("OnconnectSucc");
            //进入游戏
        }

        private void OnConnectFail(string err)
        {
            // Debug.Log("OnConnectFail"+err);
            //弹出链接失败的提示
        }

        private void OnConnectClose(string err)
        {
            Debug.Log("OnConnectClose");
            //弹出关闭的提示
            //弹出按钮再次连接进入游戏
        }

        /// <summary>
        /// 点击连接按钮
        /// </summary>
        public void OnConnectClick()
        {
            NetManager.Connet("127.0.0.1", 8888);
            //提示链接中
        }
        /// <summary>
        /// 点击断开按钮
        /// </summary>
        public void OnCloseClicked()
        {
            NetManager.Close();
        }

        public void OnMoveClick()
        {
            MsgMove msg = new MsgMove();
            msg.x = 120;
            msg.y = 123;
            msg.z = -6;
            NetManager.Send(msg);
        }





      

    }
}

