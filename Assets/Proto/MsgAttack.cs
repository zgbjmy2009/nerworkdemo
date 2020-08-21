using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3
{
    class MsgAttack:MsgBase
    {
        public MsgAttack()
        {
            protoName = "MsgAttack";
        }
        public string desc = "127.0.0.1:6543";
    }
}
