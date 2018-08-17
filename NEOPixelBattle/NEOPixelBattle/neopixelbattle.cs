using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class NEOPixelBattle : Framework.SmartContract
    {
        private static readonly byte[] BattleGroundKey = "BattleGround".AsByteArray();
        private static readonly byte[] DefaultColor = "#ffffff".AsByteArray();

        private static readonly byte[] LastAddressKey = "LastAddress".AsByteArray();

        // to create a million of pixels to paint 
        private static readonly int Rows = 1000;
        private static readonly int Columns = 1000;

        [DisplayName("Colored")]
        public static event Action<BigInteger, BigInteger> Colored;

        public static object Main(string operation, params object[] args)
        {
            if( Runtime.Trigger == TriggerType.Application || Runtime.Trigger == TriggerType.ApplicationR )
            {
                if (operation == "SetColorRGB()")
                {
                    if (args.Length != 4) return false;
                    return SetColorRGB((BigInteger)args[0], (BigInteger)args[1], ((String)args[2]).AsByteArray(), (byte[])args[3]);
                }

                if (operation == "GetColorRGB()")
                {
                    if (args.Length != 2) return false;
                    return GetColorRGB((BigInteger)args[0], (BigInteger)args[1]);
                }
            }

            return false;
        }

        /// <summary>Function resposible for set color to a pixel</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <param name="color">Is the color in hex, like #4286f4. Even the method use byte[] you invoke it as string</param>
        /// <param name="address">The address that invokes the contract method. Use this to check if it's the real caller and to keep the information of the last gamer</param>
        /// <returns>Return true if the pixel select was painted or false otherwise</returns>
        private static bool SetColorRGB(BigInteger i, BigInteger j, byte[] color, byte[] address)
        {
            if (!VerifyWitness(address)) 
                return false;
            byte[] LastAddress = Storage.Get(Storage.CurrentContext, LastAddressKey);
            if (address == LastAddress)
                return false;

            if (!EnsureHexColor(color))
                return false;
     
            if (!CheckIndex(i, j))
                return false;

            Storage.Put(Storage.CurrentContext, GetPixelStorageKey(i, j), color);
            Storage.Put(Storage.CurrentContext, LastAddressKey, address);

            Colored(i, j);
            return true;
        }

        /// <summary>Get a color from a specific pixel</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <returns>Return the color of the selected pixel as byte[]</returns>
        private static String GetColorRGB(BigInteger i, BigInteger j)
        {
            if (!CheckIndex(i,j))
                return "Not a valid index";
            byte[] data = Storage.Get(Storage.CurrentContext, "oi");
            if (data.Length > 0)
                return data.AsString();
            else
                return DefaultColor.AsString();
        }

        /// <summary>Get key of the pixel selected</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <returns>Return the key as byte[]</returns>
        private static byte[] GetPixelStorageKey(BigInteger i, BigInteger j)
        {
            return BattleGroundKey.Concat(i.AsByteArray()).Concat(j.AsByteArray());
        }

        /// <summary>Check if the index is valid ( not accessible for invocation )</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <returns>Return true if it's valid or false otherwise</returns>
        private static bool CheckIndex(BigInteger i, BigInteger j)
        {
            if (i < 0 || j < 0)
                return false;
            if (i > Rows || j > Columns)
                return false;
            return true;
        }

        /// <summary>Check if the caller is who claims to be ( not accessible for invocation )</summary>
        /// <param name="address">The address that invokes the contract method</param>
        /// <returns>Return true if it's verified or false otherwise</returns>
        private static bool VerifyWitness(byte[] address)
        {
            return Runtime.CheckWitness(address);
        }

        /// <summary>Ensure hex color ( not accessible for invocation )</summary>
        /// <param name="color">Is the color in byte[], like #4286f4.</param>
        /// <returns>Return true if it's a real hex color or false otherwise</returns>
        private static bool EnsureHexColor(byte[] color)
        {
            if (color.Length != DefaultColor.Length)
                return false;

            if (color[0] != '#')
                return false;

            for ( int i = 1; i < color.Length; ++i)
            {
                if (color[i] >= '0' && color[i] <= '9')
                    continue;
                else if (color[i] >= 'a' && color[i] <= 'f')
                    continue;
                else if (color[i] >= 'A' && color[i] <= 'F')
                    continue;
                else
                    return false;
            }

            return true;
        }
    }
}
