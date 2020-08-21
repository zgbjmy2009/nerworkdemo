using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Part3
{
    public class MsgBase
    {
        public string protoName = "";
        /// <summary>
        /// 消息体编码
        /// </summary>
        /// <param name="msgBase">消息体对象</param>
        /// <returns></returns>
        public static Byte[] Encode(MsgBase msgBase)
        {
            if (msgBase == null) return null;
            string s = JsonUtility.ToJson(msgBase);
            return Encoding.Default.GetBytes(s);
        }
        /// <summary>
        /// 消息体解码
        /// </summary>
        /// <param name="protoName">消息体名字</param>
        /// <param name="bytes">消息体字符数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public static MsgBase Decode(string protoName,Byte[] bytes,int offset,int count)
        {
            string s = Encoding.Default.GetString(bytes, offset, count);
            MsgBase msgBase = JsonUtility.FromJson(s, Type.GetType(protoName)) as MsgBase;
            return msgBase;
        }
        public static Byte[] EncodeName(MsgBase msgBase)
        {
            byte[] nameBytes = Encoding.Default.GetBytes(msgBase.protoName);
            Int16 len =(Int16) nameBytes.Length ;
            byte[] bytes = new byte[2 + len];
            bytes[0] = (byte)(len % 256);
            bytes[1] = (byte)(len / 256);
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }
        public static string DecodeName(byte[]byts,int offset,out int count)
        {
            count = 0;
            if (offset + 2 > byts.Length) return"";
            //需要再查看的
            int len = (byts[offset] | (byts[offset + 1] << 8));
            //
            if((offset +2 +len > byts.Length) || len<=0)
            {
                return "";
            }
            count = 2 + len;
            string name = Encoding.UTF8.GetString(byts, offset + 2, len);
            return name;

        }

        
    }
}
