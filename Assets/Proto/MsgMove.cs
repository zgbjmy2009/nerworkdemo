using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3
{
    class MsgMove:MsgBase
    {
        public MsgMove() { protoName = "MsgMove"; }
        public int x = 0;
        public int y = 0;
        public int z = 0;
    }
}
