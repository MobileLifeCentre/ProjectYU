////////////////////////////////////////////////////////////
//                                                        //
//  ZZZZZZZ  EEEEEEE  PPPP     HH   HH  YY   YY  RRRRR    //
//      ZZ   EE       PP PPP   HH   HH  YY   YY  RR  RRR  //
//     ZZ    EE       PP   PP  HH   HH  YY   YY  RR   RR  //
//    ZZ     EEEEEEE  PPPPPP   HHHHHHH   YYYYY   RRRRRR   //
//   ZZ      EE       PPP      HH   HH    YY     RRRR     //
//  ZZ       EE       PP       HH   HH    YY     RR RRR   //
//  ZZZZZZZ  EEEEEEE  PP       HH   HH    YY     RR   RR  //
//                                                        //
////////////////////////////////////////////////////////////
//                                                        //
//  Reproduction in whole or in part is prohibited        //
//  without the written consent of the copyright owner.   //
//                                                        //
////////////////////////////////////////////////////////////
// <copyright 
//     file="SetupDi.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       SetupDi.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Static class for getting information about devices.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class SetupDi
    {
        [DllImport("setupapi.dll")]
        public extern static IntPtr SetupDiGetClassDevs(
            [In] ref Guid ClassGuid,
            [In] string Enumerator,
            [In] IntPtr hwndParent,
            [In] SetupDIGetClassDevsFlags Flags);

        [DllImport("setupapi.dll")]
        public extern static bool SetupDiEnumDeviceInfo(
            [In] IntPtr DeviceInfo,
            [In] UInt32 MemberIndex,
            [In, Out] SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll")]
        public extern static IntPtr SetupDiOpenDevRegKey(
            [In] IntPtr DeviceInfo,
            [In] SP_DEVINFO_DATA DeviceInfoData,
            [In] SetupDiOpenDevRegKeyScopeFlags Scope,
            [In] UInt32 HwProfile,
            [In] SetupDiOpenDevRegKeyKeyTypeFlags KeyType,
            [In] REGSAM samDesired);

        [DllImport("setupapi.dll")]
        public extern static bool SetupDiDestroyDeviceInfoList(
            [In] IntPtr DeviceInfo);

        // TODO: Transform StringBuilder to a generic byte[] because for the moment that only work with a REG_SZ (that I need currently)
        // Certainly create a stub function to make a interface between the generic buffer and the correct type .Net
        // and use overload ...
        [DllImport("setupapi.dll")]
        public extern static bool SetupDiGetDeviceRegistryProperty(
            [In] IntPtr DeviceInfo,
            [In] SP_DEVINFO_DATA DeviceInfoData,
            [In] DeviceRegistryProperty Property,
            [In, Out] ref RegistryType PropertyRegDataType,
            [In, Out] StringBuilder PropertyBuffer,
            [In] UInt32 PropertyBufferSize,
            [In, Out] ref UInt32 RequiredSize);
    }

    internal enum SetupDIGetClassDevsFlags : uint
    {
        // Return a list of installed devices for all device setup classes or all device interface classes.
        DIGCF_ALLCLASSES = 0x00000004,

        // Return devices that support device interfaces for the specified device interface classes.  
        DIGCF_DEVICEINTERFACE = 0x00000010,

        // Return only the device that is associated with the system default device interface, if one is set, for the specified device interface classes. 
        DIGCF_DEFAULT = 0x00000001,

        // Return only devices that are currently present in a system. 
        DIGCF_PRESENT = 0x00000002,

        // Return only devices that are a part of the current hardware profile. 
        DIGCF_PROFILE = 0x00000008
    }

    internal enum SetupDiOpenDevRegKeyScopeFlags : uint
    {
        DICS_FLAG_GLOBAL = 0x00000001,
        DICS_FLAG_CONFIGSPECIFIC = 0x00000002
    }

    internal enum SetupDiOpenDevRegKeyKeyTypeFlags : uint
    {
        DIREG_DEV = 0x00000001,
        DIREG_DRV = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class SP_DEVINFO_DATA
    {
        public UInt32 cbSize = (UInt32)Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
        public Guid ClassGuid = new Guid();
        public UInt32 DevInst;
        private IntPtr reserved;
    }
}
