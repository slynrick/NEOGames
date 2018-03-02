using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace Neo.SmartContract
{
    public class NEOPixelBattle : Framework.SmartContract
    {
        public static object Main(string operation, params object[] args)
        {
            if (operation == "Create()")
            {
                return Create();
            }

            if ( operation == "SetColorRGB()")
            {
                if (args.Length != 2) return false;
                return SetColorRGB((int)args[0], (int)args[1], (byte[])args[2]);
            }

            if (operation == "GetData()")
            {
                return GetData();
            }

            return false;
        }

        private static byte[] GetData()
        {
           return Storage.Get(Storage.CurrentContext, "FirstBattleGound".AsByteArray());
        }

        private static byte[] SetColorRGB(int i, int j, byte[] color)
        {
            if (color.Length != 3)
                return null;
            if (i > 1000 || j > 1000)
                return null;

            byte[] data = Storage.Get(Storage.CurrentContext, "FirstBattleGound".AsByteArray());
            data[i * 3000 + j] = color[0];
            data[i * 3000 + j + 1] = color[1];
            data[i * 3000 + j + 2] = color[2];
            return null;
        }

        private static byte[] Create()
        {
            byte[] data = new byte[3000000];
            for (int i = 0; i < data.Length; i += 3)
            {
                data[i] = (byte)255;
                data[i + 1] = (byte)255;
                data[i + 2] = (byte)255;
            }
            // criando o tabuleiro todo branco
            Storage.Put(Storage.CurrentContext, "FirstBattleGound".AsByteArray(), data);
            return null;
        }
    }
}
