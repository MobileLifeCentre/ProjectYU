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
//     file="Registry.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       Registry.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      This static class contains methods to access and modify the registry.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// This static class contains methods to access and modify the registry.
    /// </summary>
    internal static class Registry
    {
        public const int ERROR_SUCCESS = 0;

        // TODO: Transform StringBuilder to a generic byte[] because for the moment that only work with a REG_SZ (that I need currently)
        // Certainly create a stub function to make a interface between the generic buffer and the correct type .Net
        // and use overload ...
        [DllImport("advapi32.dll")]
        public extern static int RegQueryValueEx(
            [In] IntPtr hKey,
            [In] string ValueName,
            [In] UInt32 Reserved,
            [In, Out] ref RegistryType Type,
            [In, Out] StringBuilder Data,
            [In, Out] ref UInt32 cbData);

        [DllImport("advapi32.dll")]
        public extern static int RegCloseKey([In] IntPtr hKey);

        [DllImport("ntdll.dll", EntryPoint = "NtQueryKey")]
        internal static extern int NtQueryKey(
            IntPtr KeyHandle,
            int KeyInformationClass,
            IntPtr KeyInformation,
            UInt32 length,
            ref UInt32 ResultLength);
    }

    internal enum RegistryType : uint
    {
        // Binary data in any form. 
        REG_BINARY = 3,

        // 32-bit number. 
        REG_DWORD = 4,

        // 64-bit number. 
        REG_QWORD = 11,

        // 32-bit number in little-endian format. This is equivalent to REG_DWORD.
        // In little-endian format, a multibyte value is stored in memory from the lowest byte (the "little end") to the highest byte. For example, the value 0x12345678 is stored as (0x78 0x56 0x34 0x12) in little-endian format. 
        // Microsoft Windows NT and Microsoft Windows 95 are designed to run on little-endian computer architectures. A user may connect to computers that have big-endian architectures, such as some UNIX systems. 
        REG_DWORD_LITTLE_ENDIAN = 4,

        // A 64-bit number in little-endian format. This is equivalent to REG_QWORD. 
        REG_QWORD_LITTLE_ENDIAN = 11,

        // 32-bit number in big-endian format.
        // In big-endian format, a multibyte value is stored in memory from the highest byte (the "big end") to the lowest byte. For example, the value 0x12345678 is stored as (0x12 0x34 0x56 0x78) in big-endian format. 
        REG_DWORD_BIG_ENDIAN = 5,

        // Null-terminated string that contains unexpanded references to environment variables (for example, "%PATH%"). It will be a Unicode or ANSI string, depending on whether you use the Unicode or ANSI functions. 
        REG_EXPAND_SZ = 2,

        // Unicode symbolic link. 
        REG_LINK = 6,

        // Array of null-terminated strings that are terminated by two null characters. 
        REG_MULTI_SZ = 7,

        // No defined value type. 
        REG_NONE = 0,

        // Device-driver resource list. 
        REG_RESOURCE_LIST = 8,

        // Null-terminated string. It will be a Unicode or ANSI string, depending on whether you use the Unicode or ANSI functions. 
        REG_SZ = 1,

        // Resource list in the hardware description
        REG_FULL_RESOURCE_DESCRIPTOR = 9,

        REG_RESOURCE_REQUIREMENTS_LIST = 10
    }

    internal enum DeviceRegistryProperty : uint
    {
        SPDRP_DEVICEDESC = 0x00000000,  // DeviceDesc (R/W)
        SPDRP_HARDWAREID = 0x00000001,  // HardwareID (R/W)
        SPDRP_COMPATIBLEIDS = 0x00000002,  // CompatibleIDs (R/W)
        SPDRP_UNUSED0 = 0x00000003,  // unused
        SPDRP_SERVICE = 0x00000004,  // Service (R/W)
        SPDRP_UNUSED1 = 0x00000005,  // unused
        SPDRP_UNUSED2 = 0x00000006,  // unused
        SPDRP_CLASS = 0x00000007,  // Class (R--tied to ClassGUID)
        SPDRP_CLASSGUID = 0x00000008,  // ClassGUID (R/W)
        SPDRP_DRIVER = 0x00000009,  // Driver (R/W)
        SPDRP_CONFIGFLAGS = 0x0000000A,  // ConfigFlags (R/W)
        SPDRP_MFG = 0x0000000B,  // Mfg (R/W)
        SPDRP_FRIENDLYNAME = 0x0000000C,  // FriendlyName (R/W)
        SPDRP_LOCATION_INFORMATION = 0x0000000D,  // LocationInformation (R/W)
        SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,  // PhysicalDeviceObjectName (R)
        SPDRP_CAPABILITIES = 0x0000000F,  // Capabilities (R)
        SPDRP_UI_NUMBER = 0x00000010,  // UiNumber (R)
        SPDRP_UPPERFILTERS = 0x00000011,  // UpperFilters (R/W)
        SPDRP_LOWERFILTERS = 0x00000012,  // LowerFilters (R/W)
        SPDRP_BUSTYPEGUID = 0x00000013,  // BusTypeGUID (R)
        SPDRP_LEGACYBUSTYPE = 0x00000014,  // LegacyBusType (R)
        SPDRP_BUSNUMBER = 0x00000015,  // BusNumber (R)
        SPDRP_ENUMERATOR_NAME = 0x00000016,  // Enumerator Name (R)
        SPDRP_SECURITY = 0x00000017,  // Security (R/W, binary form)
        SPDRP_SECURITY_SDS = 0x00000018,  // Security (W, SDS form)
        SPDRP_DEVTYPE = 0x00000019,  // Device Type (R/W)
        SPDRP_EXCLUSIVE = 0x0000001A,  // Device is exclusive-access (R/W)
        SPDRP_CHARACTERISTICS = 0x0000001B,  // Device Characteristics (R/W)
        SPDRP_ADDRESS = 0x0000001C,  // Device Address (R)
        SPDRP_UI_NUMBER_DESC_FORMAT = 0x0000001D,  // UiNumberDescFormat (R/W)
        SPDRP_DEVICE_POWER_DATA = 0x0000001E,  // Device Power Data (R)
        SPDRP_REMOVAL_POLICY = 0x0000001F,  // Removal Policy (R)
        SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020,  // Hardware Removal Policy (R)
        SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021,  // Removal Policy Override (RW)
        SPDRP_INSTALL_STATE = 0x00000022,  // Device Install State (R)
        SPDRP_LOCATION_PATHS = 0x00000023,  // Device Location Paths (R)  
    }

    // Identifiers for the IADsAccessControlEntry.AccessMask property for registry
    // objects.
    internal enum REGSAM : uint
    {
        KEY_QUERY_VALUE = 0x1,
        KEY_SET_VALUE = 0x2,
        KEY_CREATE_SUB_KEY = 0x4,
        KEY_ENUMERATE_SUB_KEYS = 0x8,
        KEY_NOTIFY = 0x10,
        KEY_CREATE_LINK = 0x20,
        KEY_WOW64_32KEY = 0x200,
        KEY_WOW64_64KEY = 0x100,
        KEY_WOW64_RES = 0x300,

        KEY_READ = ((STANDARD_RIGHTS.STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY) & (STANDARD_RIGHTS.SYNCHRONIZE)),

        KEY_WRITE = (STANDARD_RIGHTS.STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY),

        KEY_EXECUTE = STANDARD_RIGHTS.STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY,

        KEY_ALL_ACCESS = STANDARD_RIGHTS.STANDARD_RIGHTS_REQUIRED | KEY_QUERY_VALUE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY |
            KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY | KEY_CREATE_LINK
    }
}
