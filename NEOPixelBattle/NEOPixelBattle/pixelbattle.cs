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

        private static readonly int Rows = 3000;
        private static readonly int Columns = 3000;

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
                if (args.Length != 3) return false;
                return SetColorRGB((int)args[0], (int)args[1], ((string)args[2]).AsByteArray());
            }

            if (operation == "GetColorRGB()")
            {
                if (args.Length != 2) return false;
                return GetColorRGB((int)args[0], (int)args[1]);
            }

            return false;
        }

        // it can only be done once, it was made to start the game after deploy it
        private static bool StartTheGame()
        {
            byte[] data = Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);

            // checking if the data was initialized, it will be done only once
            if (data.Length != Rows * Columns * DefaultColor.Length)
            {
                data = new byte[] { };
                for (int i = 0; i < Rows * Columns; ++i)
                    data.Concat(DefaultColor); // initializing with the white color
                return true;
            }

            return false;
        }

        // function resposible for set color to a pixel
        // the color must be in hex like #4286f4
        private static bool SetColorRGB(int i, int j, byte[] color)
        {
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

            return true;
        }

        // get a color from a specific pixel
        private static byte[] GetColorRGB(int i, int j)
        {
            if (!CheckIndex(i,j))
                return null;

            byte[] data = Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);
            return data.Range(GetOneDimIndex(i, j), DefaultColor.Length);
        }

        // get all pixel data
        private static byte[] GetAllBattleGround()
        {
            return Storage.Get(Storage.CurrentContext, FirstBattleGroundKey);
        }

        // get the one dimension index
        private static int GetOneDimIndex(int i, int j)
        {
            return (i * Columns * DefaultColor.Length) + (j * DefaultColor.Length);
        }

        // get the one dimension index
        private static bool CheckIndex(int i, int j)
        {
            if (i < 0 || j < 0)
                return false;
            if (i > Rows || j > Columns)
                return false;
            return true;
        }
    }
}
