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

        /*
             This method starts the game
             It can only be called one time since the deploy
        */
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

        /*
            Function resposible for set color to a pixel
            The color must be in hex like #4286f4
        */
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

            if (color.Length != DefaultColor.Length)
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

        /*
            Get a color from a specific pixel
        */
        private static byte[] GetColorRGB(int i, int j)
        {
            if (!CheckIndex(i,j))
                return null;

            byte[] data = Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);
            return data.Range(GetOneDimIndex(i, j), DefaultColor.Length);
        }

        /*
            Get all pixel data
        */
        private static byte[] GetAllBattleGround()
        {
            return Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);
        }

        /*
            Get the one dimension index
        */
        private static int GetOneDimIndex(int i, int j)
        {
            return (i * Columns * DefaultColor.Length) + (j * DefaultColor.Length);
        }

        /*
            Check if the index is valid
        */
        private static bool CheckIndex(int i, int j)
        {
            if (i < 0 || j < 0)
                return false;
            if (i > Rows || j > Columns)
                return false;
            return true;
        }

        /*
            Check if the caller is who claims to be
        */
        private static bool VerifyWitness(byte[] address)
        {
            return Runtime.CheckWitness(address);
        }
    }
}
