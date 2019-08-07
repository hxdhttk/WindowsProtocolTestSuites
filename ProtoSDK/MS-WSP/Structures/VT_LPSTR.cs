﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.WSP
{
    public struct VT_LPSTR : IWSPObject
    {
        /// <summary>
        /// A 32-bit unsigned integer, indicating the size of the string field including the terminating null. 
        /// </summary>
        public UInt32 cLen;

        /// <summary>
        /// Null-terminated string.
        /// </summary>
        public string _string;

        public void ToBytes(WSPBuffer buffer)
        {
            buffer.Add(cLen);

            if (cLen != 0)
            {
                buffer.Add(_string);
            }
        }
    }
}
