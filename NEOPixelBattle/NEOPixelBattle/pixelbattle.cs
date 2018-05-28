using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class NEOPixelBattle : Framework.SmartContract
    {
        private static readonly byte[] FirstBattleGroundKey = "FirstBattleGround".AsByteArray();
        private static readonly byte[] DefaultColor = "#ffffff".AsByteArray();

        private static readonly byte[] LastAddressKey = "LastAddress".AsByteArray();

        // to create a million of pixels to paint 
        private static readonly int Rows = 1000;
        private static readonly int Columns = 1000;

        public static object Main(string operation, params object[] args)
        {
            if (operation == "StartTheGame()")
            {
                if (args.Length != 0) return false;
                return StartTheGame();
            }

            if (operation == "GetAllBattleGround()")
            {
                if (args.Length != 0) return false;
                return GetAllBattleGround();
            }

            if ( operation == "SetColorRGB()")
            {
                if (args.Length != 4) return false;
                return SetColorRGB((int)args[0], (int)args[1], ((string)args[2]).AsByteArray(), (byte[])args[3]);
            }

            if (operation == "GetColorRGB()")
            {
                if (args.Length != 2) return false;
                return GetColorRGB((int)args[0], (int)args[1]);
            }

            return false;
        }


        /// <summary>This method starts the game.It can only be called one time since the deploy</summary>
        /// <returns>Return true if the game was started in this invocation or false if it was alreay started before</returns>
        private static bool StartTheGame()
        {
            byte[] data = Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);

            // checking if the data was initialized, it will be done only once
            if (data.Length == 0)
            {
                for (int i = 0; i < Rows; ++i)
                    for (int j = 0; j < Columns; ++j)
                        data.Concat(DefaultColor);// initializing with the white color
                return true;
            }

            return false;
        }

        /// <summary>Function resposible for set color to a pixel</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <param name="color">Is the color in hex, like #4286f4. Even the method use byte[] you invoke it as string</param>
        /// <param name="address">The address that invokes the contract method. Use this to check if it's the real caller and to keep the information of the last gamer</param>
        /// <returns>Return true if the pixel select was painted or false otherwise</returns>
        private static bool SetColorRGB(int i, int j, byte[] color, byte[] address)
        {
            //checking if the address is the same of caller's address
            if (!VerifyWitness(address)) 
                return false;

            // checking if the last address that changed a color is the same
            // the idea is make impossible for one address change all the pixels sequentially
            byte[] LastAddress = Storage.Get(Storage.CurrentContext, LastAddressKey);
            if (address == LastAddress)
                return false;

            if (!EnsureHexColor(color))
                return false;
     
            if (!CheckIndex(i, j))
                return false;

            byte[] data = Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);

            // checking if the data was initialized, it will be done only once
            if (data.Length != Rows * Columns * DefaultColor.Length)
                return false;

            // looking for the right index
            int index = GetOneDimIndex(i, j);
            for (int l = 0; l < color.Length; ++l)
                data[index + l] = color[l];

            Storage.Put(Storage.CurrentContext, FirstBattleGroundKey, data);


            // save the current address as the last one that changed the data
            Storage.Put(Storage.CurrentContext, LastAddressKey, address);

            return true;
        }

        /// <summary>Get a color from a specific pixel</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <returns>Return the color of the selected pixel as byte[]</returns>
        private static byte[] GetColorRGB(int i, int j)
        {
            if (!CheckIndex(i,j))
                return null;

            byte[] data = Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);
            return data.Range(GetOneDimIndex(i, j), DefaultColor.Length);
        }

        /// <summary>Get all pixel data</summary>
        /// <returns>Return the color of all pixels as byte[](one dimension)</returns>
        private static byte[] GetAllBattleGround()
        {
            return Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);
        }

        /// <summary>Get the one dimension index ( not accessible for invocation )</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <returns>Return the one dimension index of the matrix</returns>
        private static int GetOneDimIndex(int i, int j)
        {
            return (i * Columns * DefaultColor.Length) + (j * DefaultColor.Length);
        }

        /// <summary>Check if the index is valid ( not accessible for invocation )</summary>
        /// <param name="i">Is the row number of the matrix</param>
        /// <param name="j">Is the column number of the matrix</param>
        /// <returns>Return true if it's valid or false otherwise</returns>
        private static bool CheckIndex(int i, int j)
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

        /*
            
        */
        /// <summary>Ensure hex color ( not accessible for invocation )</summary>
        /// <param name="color">Is the color in byte[], like #4286f4.</param>
        /// <returns>Return true if it's a real hex color or false otherwise</returns>
        private static bool EnsureHexColor(byte[] color)
        {
            if (color.Length != DefaultColor.Length)
                return false;

            if (color[0] == '#')
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
