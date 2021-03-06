﻿// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game
{
    /// <summary>
    ///	Binary (De)serializer using BinaryFormatter
    /// </summary>
    public class ByteArray
    {
        private static BinaryFormatter binaryFormatter = new BinaryFormatter();

        /// <summary>
        ///		Serialize the object to a binary byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(Object obj)
        {
            binaryFormatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///		Deserialize a binary byte array to an object
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Object Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                Object obj = binaryFormatter.Deserialize(ms);
                return obj;
            }
        }
    }
}
