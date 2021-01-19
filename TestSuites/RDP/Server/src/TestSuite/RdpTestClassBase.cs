﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Protocols.TestTools;
using Microsoft.Protocols.TestTools.StackSdk.RemoteDesktop.Rdpbcgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Protocols.TestSuites.Rdp
{
    public abstract class RdpTestClassBase : TestClassBase
    {
        #region Adapter Instances

        protected TestConfig testConfig;

        #endregion

        #region Class Initialization and Cleanup

        public static void BaseInitialize(TestContext context)
        {
            TestClassBase.Initialize(context);
            BaseTestSite.DefaultProtocolDocShortName = BaseTestSite.Properties["ProtocolName"];
        }

        public static void BaseCleanup()
        {
            TestClassBase.Cleanup();
        }
        #endregion

        #region Test Initialization and Cleanup
        protected override void TestInitialize()
        {
            testConfig = new TestConfig(Site);

            CheckPlatformCompatibility();
        }

        protected override void TestCleanup()
        {
            base.TestCleanup();
        }

        /// <summary>
        /// Check platform compatibility.
        /// </summary>
        private void CheckPlatformCompatibility()
        {
            // Check CredSSP, which is currently only supported on Windows.
            if (testConfig.transportProtocol == EncryptedProtocol.NegotiationCredSsp || testConfig.transportProtocol == EncryptedProtocol.DirectCredSsp)
            {
                if (!OperatingSystem.IsWindows())
                {
                    BaseTestSite.Assume.Inconclusive("The transport protocols based on CredSSP are only supported on Windows.");
                }
            }
        }
        #endregion
    }
}