using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using keccak;
using System.Threading;

namespace VinPlaning
{
    public class Node
    {
        public string NodeText
        {
            set
            {
                text = value;
            }
            get
            {
                return text;
            }
        }

        protected string text;


        public readonly byte[] NodeID;

        static protected byte[] staticBytes  = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32};
        static protected uint   cyclicBase   = (uint) new Random().Next(int.MinValue + 1, int.MaxValue - 1);
        static protected uint   cyclicNumber = 1;
        static readonly  object syncObject = new object();
        static protected byte[] sessionGuid;
        static Node()
        {
            newSessionGuid();
        }

        public Node()
        {
            NodeID = getNewNodeID();
        }

        public Node(string text): this()
        {
            this.text = text;
        }

        protected static void newSessionGuid()
        {
            lock (syncObject)
            {
                var bb = new BytesBuilder();
                byte[] target = null;

                BytesBuilder.ULongToBytes((ulong)DateTime.Now.Ticks, ref target);
                bb.add(target);
                bb.add(staticBytes);

                sessionGuid = SHA3.generateRandomPwd(bb.getBytes(), 20);
            }
        }

        static public byte[] getNewNodeID()
        {
            var  result = new byte[20 + 12];
            uint cn;

            lock (syncObject)
            {
                cyclicNumber++;
                if (cyclicNumber == 0 || cyclicNumber == unchecked((uint) -1) ) // для повышения надёжности двойная проверка на смену сессии
                {
                    newSessionGuid();
                    cyclicNumber++;
                }

                cn = unchecked( cyclicNumber + cyclicBase );

                BytesBuilder.CopyTo(sessionGuid, result);
            }

            byte[] target = null;
            BytesBuilder.ULongToBytes((ulong) DateTime.Now.Ticks, ref target);
            BytesBuilder.CopyTo(target, result, 20);

            target = null;
            BytesBuilder.UIntToBytes(cn, ref target);
            BytesBuilder.CopyTo(target, result, 28);


            return result;
        }
    }
}
