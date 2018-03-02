using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class NEOPixelBattle : Framework.SmartContract
    {
        [DisplayName("colored")]
        public static event Action<BigInteger,BigInteger,byte[]> Colored;

        public static object Main(string operation, params object[] args)
        {
            if ( operation == "SetColorRGB()")
            {
                if (args.Length != 3) return false;
                return SetColorRGB((byte[])args[0], (byte[])args[1], (byte[])args[2]);
            }

            if (operation == "GetColorRGB()")
            {
                if (args.Length != 2) return false;
                return GetColorRGB((byte[])args[0], (byte[])args[1]);
            }

            return false;
        }

        private static bool SetColorRGB(byte[] i, byte[] j, byte[] color)
        {
            if (color.Length != 3)
                return false;
            if (i.AsBigInteger() > 3000 || j.AsBigInteger() > 3000)
                return false;

            byte[] key = new byte[] { };
            key.Concat("FirstBattleGound".AsByteArray());
            key.Concat(i);
            key.Concat(j);
            Storage.Put(Storage.CurrentContext, key, color);
            Colored(i.AsBigInteger(), j.AsBigInteger(), color);
            return true;
        }

        private static byte[] GetColorRGB(byte[] i, byte[] j)
        {
            if (i.AsBigInteger() > 3000 || j.AsBigInteger() > 3000)
                return null;

            byte[] key = new byte[] { };
            key.Concat("FirstBattleGound".AsByteArray());
            key.Concat(i);
            key.Concat(j);
            return Storage.Get(Storage.CurrentContext, key);
        }
    }
}
