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
//     file="SerialPortDevices.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       SerialPortDevices.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Gets information about serial port devices
*/
////////////////////////////////////////////////////////////

namespace Zephyr.IO.Ports
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Zephyr.Win32;


    /// <summary>
    /// Enum used to indicate what sort of change has occured
    /// </summary>
    [CLSCompliant(true)]
    public enum SerialPortListAction
    {
        /// <summary>
        /// Device has been added to the system
        /// </summary>
        Added,

        /// <summary>
        /// Device has been removed from the system
        /// </summary>
        Removed
    }

    [CLSCompliant(true)]
    public static class SerialPortDevice
    {
        private static Guid GUID_DEVINTERFACE_COMPORT = new Guid(0x86E0D1E0, 0x8089, 0x11D0, 0x9C, 0xE4, 0x08, 0x00, 0x3E, 0x30, 0x1F, 0x73);
        private static IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

        private static int WM_DEVICECHANGE = 0x0219;

        /// <summary>
        /// Return a list of available COM port from a specific manufacturer
        /// </summary>
        /// <param name="manufacturer">string contains into the manufacturer name (don't take care about the lower or upper case)</param>
        /// <returns></returns>
        public static ReadOnlyCollection<SerialPortInfo> GetPorts(string manufacturer)
        {
            ReadOnlyCollection<SerialPortInfo> ports = GetAvailablePorts();

            List<SerialPortInfo> selectedPorts = new List<SerialPortInfo>();

            if (string.IsNullOrEmpty(manufacturer) == true)
            {
                return ports;
            }

            foreach (SerialPortInfo serialInfo in ports)
            {
                if (serialInfo.Manufacturer.ToUpper(CultureInfo.InvariantCulture).Contains(manufacturer.ToUpper(CultureInfo.InvariantCulture)) == true)
                {
                    selectedPorts.Add(serialInfo);
                }
            }

            return new ReadOnlyCollection<SerialPortInfo>(selectedPorts);
        }
        
        /// <summary>
        /// Return the list of all available COM port into the system
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyCollection<SerialPortInfo> GetAvailablePorts()
        {
            IntPtr hdevInfo = SetupDi.SetupDiGetClassDevs(
                ref GUID_DEVINTERFACE_COMPORT, 
                null, 
                IntPtr.Zero,
                SetupDIGetClassDevsFlags.DIGCF_PRESENT | SetupDIGetClassDevsFlags.DIGCF_DEVICEINTERFACE);

            if (hdevInfo == INVALID_HANDLE_VALUE)
            {
                throw new Exception("can't access to serial port communication information.");
            }

            List<SerialPortInfo> ports = new List<SerialPortInfo>();

            uint index = 0;
            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            while (SetupDi.SetupDiEnumDeviceInfo(hdevInfo, index++, deviceInfoData) == true)
            {
                IntPtr hKey = SetupDi.SetupDiOpenDevRegKey(
                    hdevInfo,
                    deviceInfoData,
                    SetupDiOpenDevRegKeyScopeFlags.DICS_FLAG_GLOBAL,
                    0,
                    SetupDiOpenDevRegKeyKeyTypeFlags.DIREG_DEV,
                    REGSAM.KEY_QUERY_VALUE);

                if (hKey != IntPtr.Zero)
                {
                    SerialPortInfo portInfo = new SerialPortInfo();
                    
                    UInt32 resultLength = 1024;
                    IntPtr registryName = Marshal.AllocHGlobal(1024);
                    
                    var retValue = Registry.NtQueryKey(hKey, 3, registryName, 1024, ref resultLength);
                    if (retValue == 0)
                    {
                        string registryNameStr = Marshal.PtrToStringAuto((IntPtr)(registryName.ToInt32() + 4), Marshal.ReadInt32(registryName) / 2);
                        Marshal.FreeHGlobal(registryName);                        

                        var registryPath = registryNameStr.Split('\\');

                        if (registryPath != null && registryPath.Length > 3)
                        {
                            var sNumber = registryPath[registryPath.Length - 2];
                            if (sNumber != null && sNumber.Contains('_'))
                            {
                                portInfo.SerialName = sNumber.Remove(sNumber.IndexOf('_'));
                            }
                            else
                            {
                                portInfo.SerialName = sNumber;
                            }

                            var deviceVidPid = registryPath[registryPath.Length - 3].ToUpperInvariant();
                            if (deviceVidPid != null && deviceVidPid.Length > 0 && deviceVidPid.Contains("pid_") && deviceVidPid.Contains("vid_"))
                            {
                                try
                                {
                                    char separator = '&';
                                    if (deviceVidPid.Contains('&'))
                                    {
                                        separator = '&';
                                    }
                                    else if (deviceVidPid.Contains('+'))
                                    {
                                        separator = '+';
                                    }

                                    var position = deviceVidPid.IndexOf("vid_") + 4;
                                    var vid = deviceVidPid.Substring(position, deviceVidPid.IndexOf(separator) - position);

                                    position = deviceVidPid.IndexOf("pid_") + 4;
                                    var pid = deviceVidPid.Substring(position, deviceVidPid.Length - position);
                                    if (pid.Contains(separator))
                                    {
                                        pid = pid.Remove(pid.IndexOf(separator));
                                    }

                                    portInfo.DevicePid = int.Parse(pid, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                    portInfo.DeviceVid = int.Parse(vid, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                }
                                catch 
                                {
                                }
                            }
                        }
                    }

                    RegistryType type = RegistryType.REG_NONE; 
                    UInt32 capacity;
                    StringBuilder data = new StringBuilder(256); 
                    capacity = (UInt32)data.Capacity;
                    int result = Registry.RegQueryValueEx(hKey, "PortName", 0, ref type, data, ref capacity);

                    if ((result == Registry.ERROR_SUCCESS) & (type == RegistryType.REG_SZ))
                    {
                        portInfo.Name = data.ToString();

                        string property = GetDeviceRegistryProperty(hdevInfo, deviceInfoData, DeviceRegistryProperty.SPDRP_DEVICEDESC);
                        if (property != null)
                        {
                            portInfo.Description = property;
                        }

                        property = GetDeviceRegistryProperty(hdevInfo, deviceInfoData, DeviceRegistryProperty.SPDRP_MFG);
                        if (property != null)
                        {
                            portInfo.Manufacturer = property;
                        }

                        property = GetDeviceRegistryProperty(hdevInfo, deviceInfoData, DeviceRegistryProperty.SPDRP_FRIENDLYNAME);
                        if (property != null)
                        {
                            portInfo.FriendlyName = property;
                        }

                        property = GetDeviceRegistryProperty(hdevInfo, deviceInfoData, DeviceRegistryProperty.SPDRP_LOCATION_INFORMATION);
                        if (property != null)
                        {
                            portInfo.LocalInformation = property;
                        }

                        property = GetDeviceRegistryProperty(hdevInfo, deviceInfoData, DeviceRegistryProperty.SPDRP_SERVICE);
                        if (property != null)
                        {
                            portInfo.Service = property;
                        }
                    }
                    
                    ports.Add(portInfo);

                    Registry.RegCloseKey(hKey);
                }
            }

            SetupDi.SetupDiDestroyDeviceInfoList(hdevInfo);
            //PatchInZModemSerials(ports);
            return new ReadOnlyCollection<SerialPortInfo>(ports);
        }



        /// <summary>
        /// HACK: Microsoft CDC driver (usbser.sys) does not register properly as a serial port
        /// It needs serenum.sys upper port filter which causes problems of its own...
        /// 
        /// </summary>
        /// <param name="ports"></param>
        private static void PatchInZModemSerials(List<SerialPortInfo> ports)
        {
            /* SiLabs licensed USB to serial driver PID/VID */
            const string SILABS_VID_PID = @"USB\VID_10C4&PID_81E8"; 
            /* Zephyr VID (not specific on PID) */       
            const string ZEPHYR_VID = @"USB\VID_22F3";
            string pnpDevId;
            int startIndex;
            int length;
            System.Management.ManagementScope scope;
            System.Management.ObjectQuery query;
            System.Management.ManagementObjectSearcher searcher;
            System.Management.ManagementObjectCollection returnCollection;
            System.Management.EnumerationOptions opt = new System.Management.EnumerationOptions();


            try
            {
                /* get the data NOW, don't put off the query until it is required (30sec timeout) */
                opt.ReturnImmediately = false;
                opt.Timeout = new TimeSpan(0, 0, 30);

                /* define scope, in this case, this machine */
                scope = new System.Management.ManagementScope(@"root\CIMV2");
                /* set the query, get all information on the serial ports on the system */
                query = new System.Management.ObjectQuery("SELECT * from Win32_SerialPort");

                /* stick two together into a search */
                searcher = new System.Management.ManagementObjectSearcher(scope, query, opt);
                
                /* Run the query */
                returnCollection = searcher.Get();

                foreach (var oReturn in returnCollection)
                {
                    pnpDevId = oReturn["PNPDeviceID"] as string;
                    pnpDevId = pnpDevId.ToUpper();  // Vista / Win7 seem to use upper case PID/VID/GUID


                    /* Checks to see if the candidate mySerialPort is a zephyr Serial Port */
                    if ((pnpDevId.LastIndexOf(SILABS_VID_PID) > -1) || (pnpDevId.LastIndexOf(ZEPHYR_VID) > -1))
                    {
                        /* Extract the serial number from the PnP mySerialPort ID */
                        startIndex = pnpDevId.LastIndexOf(@"\") + 1;
                        length = pnpDevId.LastIndexOf(@"_") - startIndex;
                        if (length < 0)
                        {
                            length = pnpDevId.Length - startIndex;
                        }

                        /* If port not already known, add to list */
                        string serial = pnpDevId.Substring(startIndex, length);
                        string portName = oReturn["DeviceID"] as string;
                        string caption = oReturn["Caption"] as string;
                        var search = ports.FirstOrDefault(item => item.Name == portName);
                        if (search == null)
                        {                            
                            System.Diagnostics.Debug.WriteLine("Found: " + oReturn["DeviceID"] + " " + oReturn["Caption"]);
                            SerialPortInfo spi = new SerialPortInfo();
                            spi.SerialName = serial;
                            spi.Name = portName;
                            spi.FriendlyName = caption;
                            spi.Description = caption;
                            spi.Manufacturer = "Zephyr";
                            spi.Driver = "usbser.sys";
                            ports.Add(spi);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static string GetDeviceRegistryProperty(IntPtr hDeviceInfo, SP_DEVINFO_DATA deviceInfoData, DeviceRegistryProperty propertyType)
        {
            RegistryType propertyRegistryType = RegistryType.REG_NONE;

            StringBuilder propertyBuffer = new StringBuilder(512);
            UInt32 sizeRequired = 0;
            bool result = SetupDi.SetupDiGetDeviceRegistryProperty(
                hDeviceInfo, 
                deviceInfoData, 
                propertyType,
                ref propertyRegistryType,
                propertyBuffer,
                (UInt32)propertyBuffer.Capacity,
                ref sizeRequired);

            if ((result == true) & (propertyRegistryType == RegistryType.REG_SZ))
            {
                return propertyBuffer.ToString();
            }
            else
            {
                return null;
            }
        }

        public static event EventHandler<SerialPortListChangedEventArgs> ListChanged;

        public static void OnDeviceChange(System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                var lpdb = (DEV_BROADCAST_HDR)m.GetLParam(typeof(DEV_BROADCAST_HDR));

                if ((DeviceChangeEvent)m.WParam.ToInt32() == DeviceChangeEvent.DBT_DEVICEREMOVECOMPLETE)
                {
                    if (lpdb.dbchDeviceType == DeviceChangeType.DBT_DEVTYP_PORT)
                    {
                        string portName = Marshal.PtrToStringUni((IntPtr)(m.LParam.ToInt32() + Marshal.SizeOf(typeof(DEV_BROADCAST_HDR))));

                        var handler = ListChanged;
                        if (handler != null)
                        {
                            handler(null, new SerialPortListChangedEventArgs(portName, SerialPortListAction.Removed));
                        }
                    }
                }
                else if ((DeviceChangeEvent)m.WParam.ToInt32() == DeviceChangeEvent.DBT_DEVICEARRIVAL)
                {
                    if (lpdb.dbchDeviceType == DeviceChangeType.DBT_DEVTYP_PORT)
                    {
                        string portName = Marshal.PtrToStringUni((IntPtr)(m.LParam.ToInt32() + Marshal.SizeOf(typeof(DEV_BROADCAST_HDR))));

                        var handler = ListChanged;
                        if (handler != null)
                        {
                            handler(null, new SerialPortListChangedEventArgs(portName, SerialPortListAction.Added));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Event class used to signal a change in the devices attached to the system
    /// </summary>
    [CLSCompliant(true)]
    public class SerialPortListChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the SerialPortListChangedEventArgs class.
        /// </summary>
        /// <param name="portName">The port string, e.g. COM1</param>
        /// <param name="evt">Event type, e.q. added or removed</param>
        public SerialPortListChangedEventArgs(string portName, SerialPortListAction evt)
        {
            this.PortName = portName;
            this.Event = evt;
        }

        /// <summary>
        /// Gets a value indicating the port string.
        /// </summary>
        public string PortName { get; private set; }

        /// <summary>
        /// Gets a value indicating the type of change, addition or removal of the device.
        /// </summary>
        public SerialPortListAction Event { get; private set; }
    }
}
