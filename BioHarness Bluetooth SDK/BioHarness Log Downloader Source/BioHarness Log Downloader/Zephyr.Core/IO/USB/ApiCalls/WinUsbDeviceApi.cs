using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;

namespace Zephyr.IO.USB
{
    /// <summary>
    ///  These declarations are translated from the C declarations in various files
    ///  in the Windows DDK. The files are:
    ///  
    ///  winddk\6001\inc\api\usb.h
    ///  winddk\6001\inc\api\usb100.h
    ///  winddk\6001\inc\api\winusbio.h
    ///  
    ///  (your home directory and release number may vary)
    /// <summary>
    public sealed partial class WinUsbDevice
    {
        internal const UInt32 DEVICE_SPEED = ((UInt32)(1));
        internal const Byte USB_ENDPOINT_DIRECTION_MASK = ((Byte)(0X80));

        internal enum POLICY_TYPE
        {
            SHORT_PACKET_TERMINATE = 0x1,
            AUTO_CLEAR_STALL = 0x2,
            PIPE_TRANSFER_TIMEOUT = 0x3,
            IGNORE_SHORT_PACKETS = 0x4,
            ALLOW_PARTIAL_READS = 0x5,
            AUTO_FLUSH = 0x6,
            RAW_IO = 0x7,
            MAXIMUM_TRANSFER_SIZE = 0x8,
            RESET_PIPE_ON_RESUME = 0x9,
        }

        internal enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl,
            UsbdPipeTypeIsochronous,
            UsbdPipeTypeBulk,
            UsbdPipeTypeInterrupt,
        }

        internal enum USB_DEVICE_SPEED
        {
            UsbLowSpeed = 1,
            UsbFullSpeed,
            UsbHighSpeed,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct USB_CONFIGURATION_DESCRIPTOR
        {
            internal Byte bLength;
            internal Byte bDescriptorType;
            internal ushort wTotalLength;
            internal Byte bNumInterfaces;
            internal Byte bConfigurationValue;
            internal Byte iConfiguration;
            internal Byte bmAttributes;
            internal Byte MaxPower;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct USB_INTERFACE_DESCRIPTOR
        {
            internal Byte bLength;
            internal Byte bDescriptorType;
            internal Byte bInterfaceNumber;
            internal Byte bAlternateSetting;
            internal Byte bNumEndpoints;
            internal Byte bInterfaceClass;
            internal Byte bInterfaceSubClass;
            internal Byte bInterfaceProtocol;
            internal Byte iInterface;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINUSB_PIPE_INFORMATION
        {
            internal USBD_PIPE_TYPE PipeType;
            internal Byte PipeId;
            internal ushort MaximumPacketSize;
            internal Byte Interval;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct WINUSB_SETUP_PACKET
        {
            internal Byte RequestType;
            internal Byte Request;
            internal ushort Value;
            internal ushort Index;
            internal ushort Length;
        }


        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr lpOverlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_Initialize(SafeFileHandle DeviceHandle, ref IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_GetAssociatedInterface(IntPtr InterfaceHandle, Byte AssociatedInterfaceIndex, ref IntPtr AssociatedInterfaceHandle);

        //  Use this declaration to retrieve DEVICE_SPEED (the only currently defined InformationType).
        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_QueryDeviceInformation(IntPtr InterfaceHandle, UInt32 InformationType, ref UInt32 BufferLength, ref Byte Buffer);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_QueryPipe(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, Byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        unsafe internal static extern Boolean WinUsb_ReadPipe(IntPtr InterfaceHandle, Byte PipeID, byte* pBuffer /*Byte[] Buffer*/, UInt32 BufferLength, ref ulong LengthTransferred, NativeOverlapped*  Overlapped);

        //  Two declarations for WinUsb_SetPipePolicy. 
        //  Use this one when the returned Value is a Byte (all except PIPE_TRANSFER_TIMEOUT):
        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_SetPipePolicy(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref Byte Value);

        //  Use this alias when the returned Value is a UInt32 (PIPE_TRANSFER_TIMEOUT only):
        [DllImport("winusb.dll", SetLastError = true, EntryPoint = "WinUsb_SetPipePolicy")]
        internal static extern Boolean WinUsb_SetPipePolicy1(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref UInt32 Value);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_GetPipePolicy(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, ref UInt32 ValueLength, ref UInt32 Value);

        [DllImport("winusb.dll", SetLastError = true)]
        unsafe internal static extern Boolean WinUsb_WritePipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, UInt32 BufferLength, ref ulong LengthTransferred, NativeOverlapped* Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_ResetPipe(IntPtr InterfaceHandle, Byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern Boolean WinUsb_GetOverlappedResult(IntPtr InterfaceHandle, IntPtr lpOverlapped, ref Int32 NumberOfBytesTransferred, Boolean Wait);
    }
}
