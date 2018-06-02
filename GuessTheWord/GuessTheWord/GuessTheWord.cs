using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class GuessTheWord : Framework.SmartContract
    {
        private static readonly byte[] PointsKey = "PointsKey".AsByteArray();
        private static readonly byte[] WordsKey = "WordsKey".AsByteArray();
        private static readonly byte[] StatisticsKey = "StatisticsKey".AsByteArray();
        private static readonly byte[] CurrentPackKey = "CurrentPackKey".AsByteArray();
        private static readonly byte[] WordCountKey = "WordCountKey".AsByteArray();
        private static readonly BigInteger WordsPerPack = 500;

        public static object Main(string operation, params object[] args)
        {
            if (Runtime.Trigger == TriggerType.Application || Runtime.Trigger == TriggerType.ApplicationR)
            {
                if (operation == "SendTheWord()")
                {
                    if (args.Length != 3) return false;
                    return SendTheWord((String)args[0], (String)args[1], (byte[])args[2]);
                }

                if (operation == "GetWords()")
                {
                    if (args.Length != 1) return false;
                    return GetWord((byte[])args[0]);
                }

                if (operation == "GuessTheWord()")
                {
                    if (args.Length != 2) return false;
                    return GuessWord((String)args[0], (byte[])args[1]);
                }

                if (operation == "GetAddressPoints()")
                {
                    if (args.Length != 1) return false;
                    return GetAddressPoints((byte[])args[1]);
                }
                
            }

            return false;
        }

        /// <summary>Add a word to the contract</summary>
        /// <param name="word">The word you want to add</param>
        /// <param name="description">A tip for the word</param>
        /// <param name="address">The addres that send the word</param>
        /// <returns>Return true if the word was added</returns>
        private static bool SendTheWord(String word, String description, byte[] address)
        {
            if (!VerifyWitness(address))
                return false;

            BigInteger wordCount;
            Map<int, String[]> words;
            byte[] key = GetCurrentKey();
            byte[] w = Storage.Get(Storage.CurrentContext, key);

            if (w.Length == 0)
            {
                wordCount = 0;
                words = new Map<int, String[]>();
            }
            else
            {
                wordCount = GetWordCount();
                words = (Map<int, String[]>)Framework.Helper.Deserialize(w);
            }

            int index = (int)(wordCount % GetCurrentPack());
            words[index] = new String[3];
            words[index][0] = word;
            words[index][1] = description;
            words[index][2] = address.AsString();
            UpdateWordCount(wordCount + 1);

            Storage.Put(Storage.CurrentContext, key, words.Serialize());

            if ( wordCount % WordsPerPack == 0 )
            {
                UpdateCurrentPack();
                key = GetCurrentKey();
            }

            return true;
        }

        /// <summary>Get a random word</summary>
        /// <param name="address">The addres that calls for a random word</param>
        /// <returns>Return the tip of a random word or an ERROR as string</returns>
        private static String GetWord( byte[] address )
        {
            if (!VerifyWitness(address))
                return "ERROR: Not the Address";

            Random rand = new Random((int)Blockchain.GetHeight());
            BigInteger value = new BigInteger(rand.Serialize());
            BigInteger CurrentPack = GetCurrentPack();

            BigInteger pack = value % CurrentPack;
            BigInteger maxIndex = WordsPerPack;
            if (pack == 0)
            {
                pack = CurrentPack;
                maxIndex = GetWordCount() % pack;
            }

            byte[] packKey = GetPackKey(pack);

            byte[] w = Storage.Get(Storage.CurrentContext, packKey);

            if (w.Length == 0)
                return "ERROR: No word added yet";

            Map<int, String[]> words = (Map<int, String[]>)Framework.Helper.Deserialize(w);

            int word_idx = (int)(value % maxIndex);

            AddWordToAddress(words[word_idx][0], address);

            return words[word_idx][1];
        }

        /// <summary>Guess the word you choose randomly</summary>
        /// <param name="word">The word you guessed</param>
        /// <param name="address">The addres that is trying to guess the word</param>
        /// <returns>Return true if the word is correct or false otherwise</returns>
        private static bool GuessWord(String word, byte[] address)
        {
            if (!VerifyWitness(address))
                return false;

            byte[] data = Storage.Get(Storage.CurrentContext, address);

            Map<byte[], byte[]> addressInfo = (Map<byte[], byte[]>)Framework.Helper.Deserialize(data);

            if (data.Length == 0)
                return false;

            if (data.AsString() != word)
                return false;

            AddPointToTheAddress(address);
            CleanAddressWord(address);
            return true;
        }

        /// <summary>Get the points of an address</summary>
        /// <param name="address">The target address</param>
        /// <returns>Return the points of this address as BigInteger</returns>
        private static BigInteger GetAddressPoints(byte[] address)
        {
            Map<byte[], byte[]> addressInfo = GetAddressInfo(address);
            return addressInfo[PointsKey].AsBigInteger();
        }

        /// <summary>Check if the caller is who claims to be ( not accessible for invocation )</summary>
        /// <param name="address">The address that invokes the contract method</param>
        /// <returns>Return true if it's verified or false otherwise</returns>
        private static bool VerifyWitness(byte[] address)
        {
            return Runtime.CheckWitness(address);
        }

        /// <summary>Get the word count and the pack count</summary>
        /// <returns>Return the Map with the statistics</returns>
        private static Map<byte[], BigInteger> GetContractStatistics()
        {
            byte[] data = Storage.Get(Storage.CurrentContext, StatisticsKey);

            Map<byte[], BigInteger> contractStatistics;
            if (data.Length == 0)
            {
                contractStatistics = new Map<byte[], BigInteger>();
                contractStatistics[CurrentPackKey] = 0;
                contractStatistics[WordCountKey] = 0;
            }
            else
            {
                contractStatistics = (Map<byte[], BigInteger>)Framework.Helper.Deserialize(data);
            }

            return contractStatistics;
        }

        /// <summary>Get the current pack of words</summary>
        /// <returns>Return the number as  BigInteger</returns>
        private static BigInteger GetCurrentPack()
        {
            return GetContractStatistics()[CurrentPackKey];
        }

        /// <summary>Increment the pack by one each 500 words added</summary>
        /// <returns>Nothing</returns>
        private static void UpdateCurrentPack()
        {
            Map<byte[], BigInteger> contractStatistics = GetContractStatistics();
            contractStatistics[CurrentPackKey] += 1;
            Storage.Put(Storage.CurrentContext, StatisticsKey, contractStatistics.Serialize());
        }

        /// <summary>Get the number of words added until the beginning</summary>
        /// <returns>Return the number as BigInteger</returns>
        private static BigInteger GetWordCount()
        {
            return GetContractStatistics()[WordCountKey];
        }

        /// <summary>Update the number of word added into the contract</summary>
        /// <param name="count">The new value of the word count</param>
        /// <returns>Nothing</returns>
        private static void UpdateWordCount(BigInteger count)
        {
            Map<byte[], BigInteger> contractStatistics = GetContractStatistics();
            contractStatistics[WordCountKey] += 1;
            Storage.Put(Storage.CurrentContext, StatisticsKey, contractStatistics.Serialize());
        }

        /// <summary>Get the storage of a pack</summary>
        /// <returns>Return true if the word was added</returns>
        private static byte[] GetCurrentKey()
        {
            byte[] CurrentPack = GetCurrentPack().AsByteArray();
            return Hash256(WordsKey.Concat(CurrentPack));
        }

        /// <summary>Get the key of one pack of words</summary>
        /// <param name="pack">the number of the pack</param>
        /// <returns>Return a Hash256 as a storage key</returns>
        private static byte[] GetPackKey(BigInteger pack)
        {
            return Hash256(WordsKey.Concat(pack.AsByteArray()));
        }

        /// <summary>Get the map with the address information</summary>
        /// <param name="address">the target address</param>
        /// <returns>Return a Map</returns>
        private static Map<byte[], byte[]> GetAddressInfo(byte[] address)
        {
            byte[] data = Storage.Get(Storage.CurrentContext, address);

            Map<byte[], byte[]> addressInfo;
            if(data.Length == 0)
            {
                addressInfo = new Map<byte[], byte[]>();
                BigInteger points = 0;
                addressInfo[PointsKey] = points.AsByteArray();
                addressInfo[WordsKey] = null;
            }
            else
            {
                addressInfo = (Map<byte[], byte[]>)Framework.Helper.Deserialize(data);
            }

            return addressInfo;
        }

        /// <summary>Add a word to the address</summary>
        /// <param name="word">The word randomly choosed</param>
        /// <param name="address">The target address</param>
        /// <returns>Nothing</returns>
        private static void AddWordToAddress(String word, byte[] address)
        {
            Map<byte[], byte[]> addressInfo = GetAddressInfo(address);
            addressInfo[WordsKey] = word.AsByteArray();
            Storage.Put(Storage.CurrentContext, address, addressInfo.Serialize());
        }

        /// <summary>Remove a word from the address after guess it right</summary>
        /// <param name="address">The target address</param>
        /// <returns>Nothing</returns>
        private static void CleanAddressWord(byte[] address)
        {
            Map<byte[], byte[]> addressInfo = GetAddressInfo(address);
            addressInfo[WordsKey] = null;
            Storage.Put(Storage.CurrentContext, address, addressInfo.Serialize());
        }

        /// <summary>Increment one point to the address after guess the word</summary>
        /// <param name="address">The target address</param>
        /// <returns>Nothing</returns>
        private static void AddPointToTheAddress(byte[] address)
        {
            Map<byte[], byte[]> addressInfo = GetAddressInfo(address);
            addressInfo[PointsKey] = (addressInfo[PointsKey].AsBigInteger() + 1).AsByteArray();
            Storage.Put(Storage.CurrentContext, address, addressInfo.Serialize());
        }
    }
}
