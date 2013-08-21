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
//     file="DeviceChange.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       DeviceChange.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Contains class and emums for interop.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Win32
{
    using System;
    using System.Runtime.InteropServices;

    internal enum DeviceChangeEvent : int
    {
        DBT_CONFIGCHANGECANCELED = 0x19,
        DBT_CONFIGCHANGED = 0x18,
        DBT_CUSTOMEVENT = 0x8006,
        DBT_DEVICEARRIVAL = 0x8000,
        DBT_DEVICEQUERYREMOVE = 0x8001,
        DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
        DBT_DEVICEREMOVECOMPLETE = 0x8004,
        DBT_DEVICEREMOVEPENDING = 0x8003,
        DBT_DEVICETYPESPECIFIC = 0x8005,
        DBT_DEVNODES_CHANGED = 0x7,
        DBT_QUERYCHANGECONFIG = 0x17,
        DBT_USERDEFINED = 0xFFFF
    }

    internal enum DeviceChangeType : uint
    {
        DBT_DEVTYP_OEM = 0x00000000,  // oem-defined device type
        DBT_DEVTYP_DEVNODE = 0x00000001,  // devnode number
        DBT_DEVTYP_VOLUME = 0x00000002,  // logical volume
        DBT_DEVTYP_PORT = 0x00000003,  // serial, parallel
        DBT_DEVTYP_NET = 0x00000004,  // network resource
        DBT_DEVTYP_DEVICEINTERFACE = 0x00000005, // device interface class
        DBT_DEVTYP_HANDLE = 0x00000006  // file system handle
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class DEV_BROADCAST_HDR
    {
        public UInt32 dbchSize;
        public DeviceChangeType dbchDeviceType;
        public UInt32 dbchReserved;
    }
}
