/*

Class: DiskUtils.cs
==============================================
Last update: 2016-05-12  (by Dikra)
==============================================

Copyright (c) 2016  M Dikra Prasetya

 * MIT LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_IOS
using System.Runtime.InteropServices;
#endif
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace SimpleDiskUtils
{

	public class DiskUtils
	{
		#region DISK_TOOLS

#if UNITY_STANDALONE || UNITY_EDITOR

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
		[DllImport ("diskutils")]
		private static extern ulong getAvailableDiskSpace ();

		[DllImport ("diskutils")]
		private static extern ulong getTotalDiskSpace ();

		[DllImport ("diskutils")]
		private static extern ulong getBusyDiskSpace ();

		/// <summary>
		/// Checks the available space.
		/// </summary>
		/// <returns>The available space in MB.</returns>
		public static ulong CheckAvailableSpace ()
		{
			return DiskUtils.getAvailableDiskSpace ();
		}

		/// <summary>
		/// Checks the total space.
		/// </summary>
		/// <returns>The total space in MB.</returns>
		public static ulong CheckTotalSpace ()
		{
			return DiskUtils.getTotalDiskSpace ();
		}

		/// <summary>
		/// Checks the busy space.
		/// </summary>
		/// <returns>The busy space in MB.</returns>
		public static ulong CheckBusySpace ()
		{
			return DiskUtils.getBusyDiskSpace ();
		}


#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("DiskUtilsWinAPI")]
        private static extern ulong getAvailableDiskSpace(StringBuilder drive);

        [DllImport("DiskUtilsWinAPI")]
        private static extern ulong getTotalDiskSpace(StringBuilder drive);

        [DllImport("DiskUtilsWinAPI")]
        private static extern ulong getBusyDiskSpace(StringBuilder drive);

        internal static string DISK_DRIVE = "C:/";

        /// <summary>
	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available spaces in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
		public static ulong CheckAvailableSpace(bool isexternal = false)
        {
            return DiskUtils.getAvailableDiskSpace(new StringBuilder(DISK_DRIVE));
        }

        /// <summary>
	    /// Checks the total space.
	    /// </summary>
	    /// <returns>The total space in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static ulong CheckTotalSpace(bool isexternal = false)
        {
            return DiskUtils.getTotalDiskSpace(new StringBuilder(DISK_DRIVE));
        }

        /// <summary>
	    /// Checks the busy space.
	    /// </summary>
	    /// <returns>The busy space in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static ulong CheckBusySpace(bool isexternal = false)
        {
            return DiskUtils.getBusyDiskSpace(new StringBuilder(DISK_DRIVE));
        }

        public static string[] GetDriveNames()
        {
            return Directory.GetLogicalDrives();
        }

#else

	    internal static string DISK_DRIVE = "/";
        
	    /// <summary>
	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available space in MB.</returns>
	    public static ulong CheckAvailableSpace(){
		    DriveInfo drive = GetDrive (DISK_DRIVE);
		    if (drive == null)
			    return -1;
		    return drive.AvailableFreeSpace;
	    }

	    /// <summary>
	    /// Checks the total space.
	    /// </summary>
	    /// <returns>The total space in MB.</returns>
	    public static ulong CheckTotalSpace(){
		    DriveInfo drive = GetDrive (DISK_DRIVE);
		    if (drive == null)
			    return -1;
		    return drive.TotalSize;
	    }

	    /// <summary>
	    /// Checks the busy space.
	    /// </summary>
	    /// <returns>The busy space in MB.</returns>
	    public static ulong CheckBusySpace(){
		    DriveInfo drive = GetDrive (DISK_DRIVE);
		    if (drive == null)
			    return -1;

		    return (drive.TotalSize - drive.AvailableFreeSpace);
	    }

#endif

#elif UNITY_ANDROID

		/// <summary>
		/// Checks the available space.
		/// </summary>
		/// <returns>The available space in bytes.</returns>
		/// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
		public static ulong CheckAvailableSpace()
		{
			AndroidJavaObject statFs = new AndroidJavaObject("android.os.StatFs", Application.persistentDataPath);
			long free = statFs.Call<long>("getBlockSizeLong") * statFs.Call<long>("getAvailableBlocksLong");
			return (ulong)free;
		}

		/// <summary>
		/// Checks the total space.
		/// </summary>
		/// <returns>The total space in bytes.</returns>
		/// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
		public static ulong CheckTotalSpace()
		{
			AndroidJavaObject statFs = new AndroidJavaObject("android.os.StatFs", Application.persistentDataPath);
			long total = statFs.Call<long>("getBlockCountLong") * statFs.Call<long>("getBlockSizeLong");
			return (ulong)total;
		}

		/// <summary>
		/// Checks the busy space.
		/// </summary>
		/// <returns>The busy space in bytes.</returns>
		/// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
		public static ulong CheckBusySpace()
		{
			return (CheckTotalSpace() - CheckAvailableSpace());
		}

	
#elif UNITY_IOS
	
		[DllImport ("__Internal")]
		private static extern ulong getAvailableDiskSpace();
		[DllImport ("__Internal")]
		private static extern ulong getTotalDiskSpace();
		[DllImport ("__Internal")]
		private static extern ulong getBusyDiskSpace();

		/// <summary>
		/// Checks the available space.
		/// </summary>
		/// <returns>The available space in MB.</returns>
		public static ulong CheckAvailableSpace()
		{
			return getAvailableDiskSpace();
		}

		/// <summary>
		/// Checks the total space.
		/// </summary>
		/// <returns>The total space in MB.</returns>
		public static ulong CheckTotalSpace()
		{
			return getTotalDiskSpace();
		}

		/// <summary>
		/// Checks the busy space.
		/// </summary>
		/// <returns>The busy space in MB.</returns>
		public static ulong CheckBusySpace()
		{
			return getBusyDiskSpace();
		}
#else
		public static ulong CheckAvailableSpace()
		{
			return 0;
		}

		public static ulong CheckTotalSpace()
		{
			return 0;
		}

		public static ulong CheckBusySpace()
		{
			return 0;
		}

		public static string[] GetDriveNames()
		{
			return Directory.GetLogicalDrives();
		}
#endif

		#endregion
	}

}