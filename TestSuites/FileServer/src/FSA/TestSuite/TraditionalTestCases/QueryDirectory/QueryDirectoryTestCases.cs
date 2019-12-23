﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Protocols.TestSuites.FileSharing.FSA.Adapter;
using Microsoft.Protocols.TestTools;
using Microsoft.Protocols.TestTools.StackSdk;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Fscc;
using Microsoft.SpecExplorer.Runtime.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Protocols.TestSuites.FileSharing.Common.Adapter;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.Smb2;

namespace Microsoft.Protocols.TestSuites.FileSharing.FSA.TestSuite.TraditionalTestCases.QueryDirectory
{

    [TestClassAttribute()]
    public partial class QueryDirectoryTestCases : PtfTestClassBase
    {
        #region Variables
        private FSAAdapter fsaAdapter;
        private const uint BytesToWrite = 1024;
        private const int FileNameLength = 20;
        #endregion

        #region Class Initialization and Cleanup
        [ClassInitializeAttribute()]
        public static void ClassInitialize(TestContext context)
        {
            PtfTestClassBase.Initialize(context);
        }

        [ClassCleanupAttribute()]
        public static void ClassCleanup()
        {
            PtfTestClassBase.Cleanup();
        }
        #endregion

        protected string CurrentTestCaseName
        {
            get
            {
                string fullName = (string)Site.TestProperties["CurrentTestCaseName"];
                return fullName.Split('.').LastOrDefault();
            }
        }

        #region Test Initialization and Cleanup
        protected override void TestInitialize()
        {
            this.InitializeTestManager();
            this.fsaAdapter = new FSAAdapter();
            this.fsaAdapter.Initialize(BaseTestSite);
            this.fsaAdapter.LogTestCaseDescription(BaseTestSite);

            BaseTestSite.Log.Add(LogEntryKind.Comment, "Test environment:");
            BaseTestSite.Log.Add(LogEntryKind.Comment, "\t 1. File System: " + this.fsaAdapter.FileSystem.ToString());
            BaseTestSite.Log.Add(LogEntryKind.Comment, "\t 2. Transport: " + this.fsaAdapter.Transport.ToString());
            BaseTestSite.Log.Add(LogEntryKind.Comment, "\t 3. Share Path: " + this.fsaAdapter.UncSharePath);
            this.fsaAdapter.FsaInitial();
        }

        protected override void TestCleanup()
        {
            this.fsaAdapter.Dispose();
            base.TestCleanup();
            this.CleanupTestManager();
        }
        #endregion

        #region Test cases        
        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Create directory with $INDEX_ALLOCATION as stream type and query directory info.")]
        public void Fs_CreateDiretory_QueryDirectory_Suffix_INDEX_ALLOCATION()
        {
            if (this.fsaAdapter.FileSystem == FileSystem.FAT32)
            {
                this.TestSite.Assume.Inconclusive("File name with stream type or stream data as suffix is not supported by FAT32.");
            }

            // Create a new directory with $INDEX_ALLOCATION as stream type
            string dirName = this.fsaAdapter.ComposeRandomFileName(8);

            dirName = $"{dirName}::$INDEX_ALLOCATION";

            MessageStatus status = CreateDirectory(dirName);

            this.fsaAdapter.AssertAreEqual(this.Manager,
                MessageStatus.SUCCESS,
                status,
                $"Create directory with name {dirName} is expected to succeed.");

            status = QueryDirectory($"{this.fsaAdapter.UncSharePath}\\{dirName}");

            this.fsaAdapter.AssertAreEqual(this.Manager,
                MessageStatus.SUCCESS,
                status,
                $"Query directory with file name { this.fsaAdapter.UncSharePath}\\{ dirName} is expected to succeed.");

        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Create directory with :$I30:$INDEX_ALLOCATION as stream type and stream name, then query the directory info.")]
        public void Fs_CreateDirectory_QueryDirectory_Suffix_I30_INDEX_ALLOCATION()
        {
            if (this.fsaAdapter.FileSystem == FileSystem.FAT32)
            {
                this.TestSite.Assume.Inconclusive("File name with stream type or stream data as suffix is not supported by FAT32.");
            }

            // Create a new directory with name as suffix
            string dirName = this.fsaAdapter.ComposeRandomFileName(8);

            dirName = $"{dirName}:$I30:$INDEX_ALLOCATION";

            MessageStatus status = CreateDirectory(dirName);

            this.fsaAdapter.AssertAreEqual(this.Manager,
                MessageStatus.SUCCESS,
                status,
                $"Create directory with name {dirName} is expected to succeed.");

            status = QueryDirectory($"{this.fsaAdapter.UncSharePath}\\{dirName}");

            this.fsaAdapter.AssertAreEqual(this.Manager,
                MessageStatus.SUCCESS,
                status,
                $"Query directory with file name { this.fsaAdapter.UncSharePath}\\{ dirName} is expected to succeed.");
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Create a lot of files and then query the directoy info one by one with flag SMB2_RETURN_SINGLE_ENTRY.")]
        public void Fs_CreateFiles_QueryDirectory_With_Single_Entry_Flag()
        {
            // Create a new directory
            string dirName = this.fsaAdapter.ComposeRandomFileName(8);
            FILEID dirFileId;
            uint dirTreeId = 0;
            ulong dirSessionId = 0;
            MessageStatus status = this.fsaAdapter.CreateFile(
                       dirName,
                       FileAttribute.DIRECTORY,
                       CreateOptions.DIRECTORY_FILE,
                       (FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE),
                       (ShareAccess.FILE_SHARE_READ | ShareAccess.FILE_SHARE_WRITE),
                       CreateDisposition.OPEN_IF,
                       out dirFileId,
                       out dirTreeId,
                       out dirSessionId);

            this.fsaAdapter.AssertAreEqual(this.Manager,
                MessageStatus.SUCCESS,
                status,
                $"Create directory with name {dirName} is expected to succeed.");

            List<string> files = new List<string>();

            //[MS-FSCC] section 2.4.8 FileBothDirectoryInformation
            //This information class returns a list that contains a FILE_BOTH_DIR_INFORMATION data element
            //for each file or directory within the target directory.
            //This list MUST reflect the presence of a subdirectory named "." (synonymous with the target directory itself) within the target directory 
            //and one named ".." (synonymous with the parent directory of the target directory).
            files.Add(".");
            files.Add("..");

            int filesNumber = 1000;
            FILEID fileId;
            uint treeId = 0;
            ulong sessionId = 0;
            for (int i = 0; i < filesNumber; i++)
            {
                // Create a new file
                string fileName = this.fsaAdapter.ComposeRandomFileName(8, ".txt", CreateOptions.NON_DIRECTORY_FILE, false);
                BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Create a file name: {fileName}");

                status = this.fsaAdapter.CreateFile(
                    $"{dirName}\\{fileName}",
                    (FileAttribute)0,
                    CreateOptions.NON_DIRECTORY_FILE,
                    (FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE),
                    (ShareAccess.FILE_SHARE_READ | ShareAccess.FILE_SHARE_WRITE | ShareAccess.FILE_SHARE_DELETE),
                    CreateDisposition.OPEN_IF,
                    out fileId,
                    out treeId,
                    out sessionId);

                files.Add(fileName);

                this.fsaAdapter.AssertAreEqual(this.Manager,
                    MessageStatus.SUCCESS,
                    status,
                    $"Create file with name {dirName}\\{fileName} is expected to succeed.");
            }

            BaseTestSite.Log.Add(LogEntryKind.TestStep, "Query the dirctory entry one by one.");
            foreach (string file in files)
            {
                status = this.fsaAdapter.QueryDirectoryInfo(
                    dirFileId,
                    dirTreeId,
                    dirSessionId,
                    "*",
                    FileInfoClass.FILE_BOTH_DIR_INFORMATION,
                    true,
                    false,
                    false
                    );
                this.fsaAdapter.AssertAreEqual(this.Manager,
                    MessageStatus.SUCCESS,
                    status,
                    $"Query directory {this.fsaAdapter.UncSharePath }\\{dirName} for {file} is expected to succeed.");

            }
            this.fsaAdapter.CloseOpen();
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Verify the Query Directory response with FileNamesInformation from the server.")]
        public void BVT_QueryDirectory_FileNamesInformation()
        {
            byte[] outputBuffer;
            string fileName;
            FILEID dirFileId;
            PrepareAndQueryDirectory(FileInfoClass.FILE_NAMES_INFORMATION, out outputBuffer, out fileName, out dirFileId);

            Site.Log.Add(LogEntryKind.Debug, "Start to verify the Query Directory response.");
            FileNamesInformation[] namesInformation = FsaUtility.UnmarshalFileInformationArray<FileNamesInformation>(outputBuffer);
            Site.Assert.AreEqual(3, namesInformation.Length, "The returned Buffer should contain two entries of FileNamesInformation.");
            Site.Assert.AreEqual(".", System.Text.Encoding.Unicode.GetString(namesInformation[0].FileName), "FileName of the first entry should be \".\".");
            Site.Assert.AreEqual("..", System.Text.Encoding.Unicode.GetString(namesInformation[1].FileName), "FileName of the second entry should be \"..\".");
            Site.Assert.AreEqual(fileName, System.Text.Encoding.Unicode.GetString(namesInformation[2].FileName), $"FileName of the third entry should be {fileName}.");
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Verify the Query Directory response with FileDirectoryInformation from the server.")]
        public void BVT_QueryDirectory_FileDirectoryInformation()
        {
            byte[] outputBuffer;
            string fileName;
            FILEID dirFileId;
            PrepareAndQueryDirectory(FileInfoClass.FILE_DIRECTORY_INFORMATION, out outputBuffer, out fileName, out dirFileId);

            Site.Log.Add(LogEntryKind.Debug, "Start to verify the Query Directory response.");
            FileDirectoryInformation[] directoryInformation = FsaUtility.UnmarshalFileInformationArray<FileDirectoryInformation>(outputBuffer);
            Site.Assert.AreEqual(3, directoryInformation.Length, "The returned Buffer should contain 3 entries of FileDirectoryInformation.");
            VerifyFileInformation(directoryInformation[0], 1, ".", FileAttribute.DIRECTORY, 0, 0);
            VerifyFileInformation(directoryInformation[1], 2, "..", FileAttribute.DIRECTORY, 0, 0);
            VerifyFileInformation(directoryInformation[2], 3, fileName, FileAttribute.ARCHIVE, BytesToWrite, this.fsaAdapter.ClusterSizeInKB * 1024);
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Verify the Query Directory response with FileFullDirectoryInformation from the server.")]
        public void BVT_QueryDirectory_FileFullDirectoryInformation()
        {
            byte[] outputBuffer;
            string fileName;
            FILEID dirFileId;
            PrepareAndQueryDirectory(FileInfoClass.FILE_FULL_DIR_INFORMATION, out outputBuffer, out fileName, out dirFileId);

            Site.Log.Add(LogEntryKind.Debug, "Start to verify the Query Directory response.");
            FileFullDirectoryInformation[] directoryInformation = FsaUtility.UnmarshalFileInformationArray<FileFullDirectoryInformation>(outputBuffer);
            Site.Assert.AreEqual(3, directoryInformation.Length, "The returned Buffer should contain 3 entries of FileFullDirectoryInformation.");
            VerifyFileInformation(directoryInformation[0], 1, ".", FileAttribute.DIRECTORY, 0, 0, 0);
            VerifyFileInformation(directoryInformation[1], 2, "..", FileAttribute.DIRECTORY, 0, 0, 0);
            VerifyFileInformation(directoryInformation[2], 3, fileName, FileAttribute.ARCHIVE, BytesToWrite, this.fsaAdapter.ClusterSizeInKB * 1024, 0);
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Verify the Query Directory response with FileIdFullDirectoryInformation from the server.")]
        public void BVT_QueryDirectory_FileIdFullDirectoryInformation()
        {
            byte[] outputBuffer;
            string fileName;
            FILEID dirFileId;
            PrepareAndQueryDirectory(FileInfoClass.FILE_ID_FULL_DIR_INFORMATION, out outputBuffer, out fileName, out dirFileId);

            Site.Log.Add(LogEntryKind.Debug, "Start to verify the Query Directory response.");
            FileIdFullDirectoryInformation[] directoryInformation = FsaUtility.UnmarshalFileInformationArray<FileIdFullDirectoryInformation>(outputBuffer);
            Site.Assert.AreEqual(3, directoryInformation.Length, "The returned Buffer should contain 3 entries of FileIdFullDirectoryInformation.");

            VerifyFileInformation(directoryInformation[0], 1, ".", FileAttribute.DIRECTORY, 0, 0, 0);
            if (this.fsaAdapter.Is64bitFileIdSupported)
            {
                Site.Assert.AreNotEqual(0, directoryInformation[0].FileId, "FileId of the entry should not be 0.");
            }
            else
            {
                //For file systems that do not support a 64 - bit file ID, this field MUST be set to 0, and MUST be ignored. 
                Site.Assert.AreEqual(0, directoryInformation[0].FileId, "FileId of the entry should be 0 if the file system does not support a 64-bit file ID.");
            }

            VerifyFileInformation(directoryInformation[1], 2, "..", FileAttribute.DIRECTORY, 0, 0, 0);
            //The NTFS, ReFS, FAT, and exFAT file systems return a FileId value of 0 for the entry named ".." in directory query operations.
            if (this.fsaAdapter.FileSystem == FileSystem.NTFS || this.fsaAdapter.FileSystem == FileSystem.REFS || this.fsaAdapter.FileSystem == FileSystem.FAT
                || this.fsaAdapter.FileSystem == FileSystem.EXFAT)
            {
                Site.Assert.AreEqual(0, directoryInformation[1].FileId, "FileId of the entry should be 0.");
            }

            VerifyFileInformation(directoryInformation[2], 3, fileName, FileAttribute.ARCHIVE, BytesToWrite, this.fsaAdapter.ClusterSizeInKB * 1024, 0);
            if (this.fsaAdapter.Is64bitFileIdSupported)
            {
                Site.Assert.AreNotEqual(0, directoryInformation[2].FileId, "FileId of the entry should not be 0.");
            }
            else
            {
                //For file systems that do not support a 64 - bit file ID, this field MUST be set to 0, and MUST be ignored. 
                Site.Assert.AreEqual(0, directoryInformation[2].FileId, "FileId of the entry should be 0 if the file system does not support a 64-bit file ID.");
            }
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Verify the Query Directory response with FileBothDirectoryInformation from the server.")]
        public void BVT_QueryDirectory_FileBothDirectoryInformation()
        {
            byte[] outputBuffer;
            string fileName;
            FILEID dirFileId;
            PrepareAndQueryDirectory(FileInfoClass.FILE_BOTH_DIR_INFORMATION, out outputBuffer, out fileName, out dirFileId);

            Site.Log.Add(LogEntryKind.Debug, "Start to verify the Query Directory response.");
            FileBothDirectoryInformation[] directoryInformation = FsaUtility.UnmarshalFileInformationArray<FileBothDirectoryInformation>(outputBuffer);
            Site.Assert.AreEqual(3, directoryInformation.Length, "The returned Buffer should contain 3 entries of FileBothDirectoryInformation.");
            VerifyFileInformation(directoryInformation[0], 1, ".", FileAttribute.DIRECTORY, 0, 0, 0, "");
            VerifyFileInformation(directoryInformation[1], 2, "..", FileAttribute.DIRECTORY, 0, 0, 0, "");
            VerifyFileInformation(directoryInformation[2], 3, fileName, FileAttribute.ARCHIVE, BytesToWrite, this.fsaAdapter.ClusterSizeInKB * 1024, 0, GetShortName(fileName));
        }

        [TestMethod()]
        [TestCategory(TestCategories.Bvt)]
        [TestCategory(TestCategories.Fsa)]
        [TestCategory(TestCategories.QueryDirectory)]
        [TestCategory(TestCategories.NonSmb)]
        [Description("Verify the Query Directory response with FileIdBothDirectoryInformation from the server.")]
        public void BVT_QueryDirectory_FileIdBothDirectoryInformation()
        {
            byte[] outputBuffer;
            string fileName;
            FILEID dirFileId;
            PrepareAndQueryDirectory(FileInfoClass.FILE_ID_BOTH_DIR_INFORMATION, out outputBuffer, out fileName, out dirFileId);

            Site.Log.Add(LogEntryKind.Debug, "Start to verify the Query Directory response.");
            FileIdBothDirectoryInformation[] directoryInformation = FsaUtility.UnmarshalFileInformationArray<FileIdBothDirectoryInformation>(outputBuffer);
            Site.Assert.AreEqual(3, directoryInformation.Length, "The returned Buffer should contain 3 entries of FileBothDirectoryInformation.");

            VerifyFileInformation(directoryInformation[0], 1, ".", FileAttribute.DIRECTORY, 0, 0, 0, "");
            if (this.fsaAdapter.Is64bitFileIdSupported)
            {
                Site.Assert.AreNotEqual(0, directoryInformation[0].FileId, "FileId of the entry should not be 0.");
            }
            else
            {
                //For file systems that do not support a 64 - bit file ID, this field MUST be set to 0, and MUST be ignored. 
                Site.Assert.AreEqual(0, directoryInformation[0].FileId, "FileId of the entry should be 0 if the file system does not support a 64-bit file ID.");
            }

            VerifyFileInformation(directoryInformation[1], 2, "..", FileAttribute.DIRECTORY, 0, 0, 0, "");
            //The NTFS, ReFS, FAT, and exFAT file systems return a FileId value of 0 for the entry named ".." in directory query operations.
            if (this.fsaAdapter.FileSystem == FileSystem.NTFS || this.fsaAdapter.FileSystem == FileSystem.REFS || this.fsaAdapter.FileSystem == FileSystem.FAT
                || this.fsaAdapter.FileSystem == FileSystem.EXFAT)
            {
                Site.Assert.AreEqual(0, directoryInformation[1].FileId, "FileId of the entry should be 0.");
            }

            VerifyFileInformation(directoryInformation[2], 3, fileName, FileAttribute.ARCHIVE, BytesToWrite, this.fsaAdapter.ClusterSizeInKB * 1024, 0, GetShortName(fileName));
            if (this.fsaAdapter.Is64bitFileIdSupported)
            {
                Site.Assert.AreNotEqual(0, directoryInformation[2].FileId, "FileId of the entry should not be 0.");
            }
            else
            {
                //For file systems that do not support a 64 - bit file ID, this field MUST be set to 0, and MUST be ignored. 
                Site.Assert.AreEqual(0, directoryInformation[2].FileId, "FileId of the entry should be 0 if the file system does not support a 64-bit file ID.");
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Create directory
        /// </summary>
        /// <param name="dirName">Direcotry name</param>        
        /// <returns>An NTSTATUS code that specifies the result</returns>
        private MessageStatus CreateDirectory(string dirName)
        {
            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Create a directory with name: {dirName}");

            MessageStatus status = MessageStatus.SUCCESS;

            status = this.fsaAdapter.CreateFile(
                        dirName,
                        FileAttribute.DIRECTORY,
                        CreateOptions.DIRECTORY_FILE,
                        (FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE),
                        (ShareAccess.FILE_SHARE_READ | ShareAccess.FILE_SHARE_WRITE),
                        CreateDisposition.OPEN_IF);

            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Create directory and return with status {status}");

            return status;
        }
        /// <summary>
        /// Create directory
        /// </summary>
        /// <param name="dirName">Direcotry name</param>
        /// <param name="fileId">The fileid of the created directory</param>
        /// <param name="treeId">The treeId of the created directory</param>
        /// <param name="sessionId">The sessionId of the created directory</param>
        /// <returns>An NTSTATUS code that specifies the result</returns>
        private MessageStatus CreateDirectory(
            string dirName,
            out FILEID fileId,
            out uint treeId,
            out ulong sessionId)
        {
            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Create a directory with name: {dirName}");

            MessageStatus status = MessageStatus.SUCCESS;

            status = this.fsaAdapter.CreateFile(
                        dirName,
                        FileAttribute.DIRECTORY,
                        CreateOptions.DIRECTORY_FILE,
                        (FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE),
                        (ShareAccess.FILE_SHARE_READ | ShareAccess.FILE_SHARE_WRITE),
                        CreateDisposition.OPEN_IF,
                        out fileId,
                        out treeId,
                        out sessionId
                        );

            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Create directory and return with status {status}");

            return status;
        }

        /// </summary>
        /// <param name="dirName">The directory name for query. </param>
        /// <param name="searchPattern">A Unicode string containing the file name pattern to match. </param>
        /// <param name="fileInfoClass">The FileInfoClass to query. </param>
        /// <param name="returnSingleEntry">A boolean indicating whether the return single entry for query.</param>
        /// <param name="restartScan">A boolean indicating whether the enumeration should be restarted.</param>
        /// <param name="isNoRecordsReturned">True: if No Records Returned.</param>
        /// <param name="isOutBufferSizeLess">True: if OutputBufferSize is less than the size needed to return a single entry</param>
        /// <param name="outBufferSize">The state of OutBufferSize in subsection 
        /// of section 3.1.5.5.4</param>
        /// <returns>An NTSTATUS code that specifies the result</returns>
        private MessageStatus QueryDirectory(
            string dirName,
            string searchPattern = "*",
            FileInfoClass fileinfoClass = FileInfoClass.FILE_ID_BOTH_DIR_INFORMATION,
            bool returnSingleEntry = false,
            bool restartScan = false,
            bool isDirectoryNotRight = false,
            bool isOutPutBufferNotEnough = false
            )
        {
            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Query a directory information: {dirName}");

            MessageStatus status = this.fsaAdapter.QueryDirectoryInfo(
              searchPattern,
              FileInfoClass.FILE_ID_BOTH_DIR_INFORMATION,
              returnSingleEntry,
              restartScan,
              isOutPutBufferNotEnough);

            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Query directory with search pattern {searchPattern} and return with status {status}. ");

            return status;
        }

        /// </summary>
        /// <param name="fileId">The fileid for the directory. </param>
        /// <param name="treeId">The treeId for the directory. </param>
        /// <param name="sessionId">The sessionId for the directory. </param>
        /// <param name="searchPattern">A Unicode string containing the file name pattern to match. </param>
        /// <param name="fileInfoClass">The FileInfoClass to query. </param>
        /// <param name="returnSingleEntry">A boolean indicating whether the return single entry for query.</param>
        /// <param name="restartScan">A boolean indicating whether the enumeration should be restarted.</param>
        /// <param name="isOutBufferSizeLess">True: if OutputBufferSize is less than the size needed to return a single entry</param>
        /// of section 3.1.5.5.4</param>
        /// <returns>An NTSTATUS code that specifies the result</returns>
        private MessageStatus QueryDirectory(
            FILEID fileId,
            uint treeId,
            ulong sessionId,
            string searchPattern = "*",
            FileInfoClass fileinfoClass = FileInfoClass.FILE_ID_BOTH_DIR_INFORMATION,
            bool returnSingleEntry = false,
            bool restartScan = false,
            bool isOutPutBufferNotEnough = false
            )
        {
            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Query a directory information with fileid {fileId}");

            MessageStatus status = this.fsaAdapter.QueryDirectoryInfo(
                fileId,
                treeId,
                sessionId,
                searchPattern,
                FileInfoClass.FILE_ID_BOTH_DIR_INFORMATION,
                returnSingleEntry,
                restartScan,
                isOutPutBufferNotEnough);

            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Query directory with search pattern {searchPattern} and return with status {status}. ");

            return status;
        }

        /// <summary>
        /// Check whether the specified fileTime is close to the current time.
        /// </summary>
        private void VerifyFileTime(DateTime now, FILETIME fileTime, string fileTimeName)
        {
            DateTime dateTime = DateTime.FromFileTimeUtc((((long)fileTime.dwHighDateTime) << 32) | fileTime.dwLowDateTime);
            Site.Log.Add(LogEntryKind.Debug, "The {0} is {1}", fileTimeName, dateTime.ToString("yyyy-MM-dd h:mm:ss.fff"));
            TimeSpan interval = now.Subtract(dateTime);
            Site.Assert.IsTrue(interval.TotalSeconds < 2, $"{fileTimeName} should be close to current time.");
        }

        /// <summary>
        /// Verify the FileCommonDirectoryInformation structure which is shared by 
        /// FileDirectoryInformation, FileFullDirectoryInformation, FileIdFullDirectoryInformation, FileBothDirectoryInformation, FileIdBothDirectoryInformation
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="fileAttribute"></param>
        private void VerifyFileCommonDirectoryInformation(FileCommonDirectoryInformation entry, FileAttribute fileAttribute)
        {
            DateTime now = DateTime.UtcNow;
            Site.Log.Add(LogEntryKind.Debug, "The current time is {0}", now.ToString("yyyy-MM-dd h:mm:ss.fff"));

            VerifyFileTime(now, entry.CreationTime, "CreationTime");
            VerifyFileTime(now, entry.LastAccessTime, "LastAccessTime");
            VerifyFileTime(now, entry.LastWriteTime, "LastWriteTime");
            VerifyFileTime(now, entry.ChangeTime, "ChangeTime");

            Site.Assert.IsTrue(
                ((FileAttribute)entry.FileAttributes).HasFlag(fileAttribute),
                "The FileAttributes of the entry should contain {0}.", fileAttribute);
        }

        /// <summary>
        /// Verify FileDirectoryInformation entry
        /// </summary>
        private void VerifyFileInformation(FileDirectoryInformation entry, int index, string fileName, FileAttribute fileAttribute, long endOfFile, long allocationSize)
        {
            Site.Log.Add(LogEntryKind.Debug, $"Start to verify entry {index}.");
            VerifyFileCommonDirectoryInformation(entry.FileCommonDirectoryInformation, fileAttribute);
            Site.Assert.AreEqual(fileName, System.Text.Encoding.Unicode.GetString(entry.FileName), "FileName of the entry should be \".\".");
            Site.Assert.AreEqual(endOfFile, entry.FileCommonDirectoryInformation.EndOfFile, "The EndOfFile of the entry should be 0.");
            Site.Assert.AreEqual(allocationSize, entry.FileCommonDirectoryInformation.AllocationSize, "The AllocationSize of the entry should be 0.");
        }

        /// <summary>
        /// Verify FileFullDirectoryInformation entry
        /// </summary>
        private void VerifyFileInformation(FileFullDirectoryInformation entry, int index, string fileName, FileAttribute fileAttribute, long endofFile, long allocationSize, uint eaSize)
        {
            Site.Log.Add(LogEntryKind.Debug, $"Start to verify entry {index}.");
            VerifyFileCommonDirectoryInformation(entry.FileCommonDirectoryInformation, fileAttribute);
            Site.Assert.AreEqual(fileName, System.Text.Encoding.Unicode.GetString(entry.FileName), "FileName of the entry should be \"..\".");
            Site.Assert.AreEqual(endofFile, entry.FileCommonDirectoryInformation.EndOfFile, "The EndOfFile of the entry should be 0.");
            Site.Assert.AreEqual(allocationSize, entry.FileCommonDirectoryInformation.AllocationSize, "The AllocationSize of the entry should be 0.");
            Site.Assert.AreEqual(eaSize, entry.EaSize, $"EaSize of the entry should be {eaSize}.");
        }

        /// <summary>
        /// Verify FileIdFullDirectoryInformation entry
        /// </summary>
        private void VerifyFileInformation(FileIdFullDirectoryInformation entry, int index, string fileName, FileAttribute fileAttribute, long endofFile, long allocationSize, uint eaSize)
        {
            Site.Log.Add(LogEntryKind.Debug, $"Start to verify entry {index}.");
            VerifyFileCommonDirectoryInformation(entry.FileCommonDirectoryInformation, fileAttribute);
            Site.Assert.AreEqual(fileName, System.Text.Encoding.Unicode.GetString(entry.FileName), "FileName of the entry should be \"..\".");
            Site.Assert.AreEqual(endofFile, entry.FileCommonDirectoryInformation.EndOfFile, "The EndOfFile of the entry should be 0.");
            Site.Assert.AreEqual(allocationSize, entry.FileCommonDirectoryInformation.AllocationSize, "The AllocationSize of the entry should be 0.");
            Site.Assert.AreEqual(eaSize, entry.EaSize, $"EaSize of the entry should be {eaSize}.");
        }

        /// <summary>
        /// Verify FileBothDirectoryInformation entry
        /// </summary>
        private void VerifyFileInformation(FileBothDirectoryInformation entry, int index, string fileName, FileAttribute fileAttribute, long endofFile, long allocationSize, uint eaSize, string shortName)
        {
            Site.Log.Add(LogEntryKind.Debug, $"Start to verify entry {index}.");
            VerifyFileCommonDirectoryInformation(entry.FileCommonDirectoryInformation, fileAttribute);
            Site.Assert.AreEqual(fileName, System.Text.Encoding.Unicode.GetString(entry.FileName), "FileName of the entry should be \"..\".");
            Site.Assert.AreEqual(endofFile, entry.FileCommonDirectoryInformation.EndOfFile, "The EndOfFile of the entry should be 0.");
            Site.Assert.AreEqual(allocationSize, entry.FileCommonDirectoryInformation.AllocationSize, "The AllocationSize of the entry should be 0.");
            Site.Assert.AreEqual(eaSize, entry.EaSize, $"EaSize of the entry should be {eaSize}.");
            Site.Assert.AreEqual(shortName.Length * 2, entry.ShortNameLength, $"The ShortNameLength of the entry should be {shortName.Length * 2}."); // ShortName is unicode in protocol
            Site.Assert.AreEqual(shortName, System.Text.Encoding.Unicode.GetString(entry.ShortName).Replace("\0", String.Empty), $"The ShortName of the entry should be \"{shortName}\".");
        }

        /// <summary>
        /// Verify FileIdBothDirectoryInformation entry
        /// </summary>
        private void VerifyFileInformation(FileIdBothDirectoryInformation entry, int index, string fileName, FileAttribute fileAttribute, long endofFile, long allocationSize, uint eaSize, string shortName)
        {
            Site.Log.Add(LogEntryKind.Debug, $"Start to verify entry {index}.");
            VerifyFileCommonDirectoryInformation(entry.FileCommonDirectoryInformation, fileAttribute);
            Site.Assert.AreEqual(fileName, System.Text.Encoding.Unicode.GetString(entry.FileName), "FileName of the entry should be \"..\".");
            Site.Assert.AreEqual(endofFile, entry.FileCommonDirectoryInformation.EndOfFile, "The EndOfFile of the entry should be 0.");
            Site.Assert.AreEqual(allocationSize, entry.FileCommonDirectoryInformation.AllocationSize, "The AllocationSize of the entry should be 0.");
            Site.Assert.AreEqual(eaSize, entry.EaSize, $"EaSize of the entry should be {eaSize}.");
            Site.Assert.AreEqual(shortName.Length * 2, entry.ShortNameLength, $"The ShortNameLength of the entry should be {shortName.Length * 2}."); // ShortName is unicode in protocol
            Site.Assert.AreEqual(shortName, System.Text.Encoding.Unicode.GetString(entry.ShortName).Replace("\0", String.Empty), $"The ShortName of the entry should be \"{shortName}\".");
        }

        /// <summary>
        /// Prepare before testing, including:
        /// 1. creating a new directory
        /// 2. creating a new file under the directory
        /// 3. writing some content to the file
        /// 4. closing the file to flush the data to the disk
        /// Then send QueryDirectory with specified FileInfoClass to the server and return the outputBuffer.
        /// </summary>
        private void PrepareAndQueryDirectory(FileInfoClass fileInfoClass, out byte[] outputBuffer, out string fileName, out FILEID dirFileId)
        {
            outputBuffer = null;
            string dirName = this.fsaAdapter.ComposeRandomFileName(8);
            uint treeId = 0;
            ulong sessionId = 0;

            MessageStatus status = CreateDirectory(dirName, out dirFileId, out treeId, out sessionId);

            Site.Assert.AreEqual(
                MessageStatus.SUCCESS,
                status,
                $"Create should succeed.");

            fileName = this.fsaAdapter.ComposeRandomFileName(FileNameLength, opt: CreateOptions.NON_DIRECTORY_FILE);
            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Create a file with name: {fileName} under the directory {dirName}");
            status = this.fsaAdapter.CreateFile(
                $"{dirName}\\{fileName}",
                (FileAttribute)0,
                CreateOptions.NON_DIRECTORY_FILE,
                (FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE),
                (ShareAccess.FILE_SHARE_READ | ShareAccess.FILE_SHARE_WRITE | ShareAccess.FILE_SHARE_DELETE),
                CreateDisposition.OPEN_IF);
            Site.Assert.AreEqual(
                MessageStatus.SUCCESS,
                status,
                $"Create should succeed.");
            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Write {BytesToWrite} bytes to the file {fileName}");
            long bytesWritten;
            status = this.fsaAdapter.WriteFile(0, BytesToWrite, out bytesWritten);
            Site.Assert.AreEqual(
                MessageStatus.SUCCESS,
                status,
                $"Write should succeed.");

            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Close the open to the file {fileName}");
            status = this.fsaAdapter.CloseOpen();
            Site.Assert.AreEqual(
                MessageStatus.SUCCESS,
                status,
                $"Close should succeed.");

            BaseTestSite.Log.Add(LogEntryKind.TestStep, $"Query directory with {fileInfoClass}");
            status = this.fsaAdapter.QueryDirectory(dirFileId, treeId, sessionId, "*", fileInfoClass, false, true, out outputBuffer);
            Site.Assert.AreEqual(
                MessageStatus.SUCCESS,
                status,
                $"Query directory should succeed.");
        }

        /// <summary>
        /// Get 8.3 short name from the original file name.
        /// </summary>
        private string GetShortName(string fileName)
        {
            if (fileName == null || fileName.Length <= 8)
                throw new Exception("The fileName length is smaller than 8. It does not have a short name.");
            string shortName = fileName.Substring(0, 6).ToUpper();
            shortName += "~1";
            return shortName;
        }
        #endregion
    }
}

