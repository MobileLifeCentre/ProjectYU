using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Zephyr.IO.USB
{
    ///  <summary>
    ///  API declarations relating to file I/O (and used by WinUsb).
    ///  </summary>
    sealed internal class WinUsbFileIO
    {
        internal const Int32 FILE_ATTRIBUTE_NORMAL = 0x80;
        internal const Int32 FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const Int32 FILE_SHARE_READ = 1;
        internal const Int32 FILE_SHARE_WRITE = 2;
        internal const UInt32 GENERIC_READ = 0x80000000;
        internal const UInt32 GENERIC_WRITE = 0x40000000;
        internal const Int32 INVALID_HANDLE_VALUE = -1;
        internal const Int32 OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);


        /// <summary>
        /// Error values from serial API calls
        /// </summary>
        internal enum APIErrors : int
        {
            /// <summary>
            /// Port not found
            /// </summary>
            ERROR_FILE_NOT_FOUND = 2,

            /// <summary>
            /// Invalid port name
            /// </summary>
            ERROR_INVALID_NAME = 123,

            /// <summary>
            /// Access denied
            /// </summary>
            ERROR_ACCESS_DENIED = 5,

            /// <summary>
            /// invalid handle
            /// </summary>
            ERROR_INVALID_HANDLE = 6,

            /// <summary>
            /// Not enough memory
            /// </summary>
            ERROR_NOT_ENOUGH_MEMORY = 8,

            /// <summary>
            /// The device does not recognize the command.
            /// </summary>
            ERROR_BAD_COMMAND = 22,

            /// <summary>
            /// A device attached to the system is not functioning.
            /// </summary>
            ERROR_GEN_FAILURE = 31,

            /// <summary>
            /// Timeout waiting for response from pipe
            /// </summary>
            ERROR_SEM_TIMEOUT = 121,

            /// <summary>
            /// Operation aborted (COM port unplugged)
            /// </summary>
            ERROR_OPERATION_ABORTED = 995,

            /// <summary>
            /// Overlapped I/O operation is in progress
            /// </summary>
            ERROR_IO_PENDING = 997,

            /// <summary>
            /// The device is not connected.
            /// </summary>
            ERROR_DEVICE_NOT_CONNECTED = 1167
        }
    }
}
