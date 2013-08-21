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
//     file="SerialPortInfo.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       SerialPortInfo.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Class to contain all serial port information.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.IO.Ports
{
    using System;
    using System.Collections.Generic;

    // TODO: Add the DEVICE_TYPE as an enum, but be carefull it's not always respected, depend of the manufacturer
    [Serializable]
    [CLSCompliant(true)]
    public class SerialPortInfo
    {
        private string _name = string.Empty;

        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        private string _manufacturer = string.Empty;

        public string Manufacturer
        {
            get { return this._manufacturer; }
            set { this._manufacturer = value; }
        }

        private string _description = string.Empty;

        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        private string _friendlyName = string.Empty;

        public string FriendlyName
        {
            get { return this._friendlyName; }
            set { this._friendlyName = value; }
        }

        private string _service = string.Empty;

        public string Service
        {
            get { return this._service; }
            set { this._service = value; }
        }

        private string _driver = string.Empty;

        public string Driver
        {
            get { return this._driver; }
            set { this._driver = value; }
        }

        public string LocalInformation { get; set; }

        /// <summary>
        /// This is not necesserily the serial number, with Zephyr devices it is, but not with all USB devices!
        /// That's why it is called SerialName
        /// </summary>
        public string SerialName { get; set; }

        public int DeviceVid { get; set; }
        
        public int DevicePid { get; set; }

        public override string ToString()
        {
            return Serialization.XmlSerialize(this);
        }
    }

    [CLSCompliant(true)]
    public class SerialPortInfoComparer : IEqualityComparer<SerialPortInfo>
    {
        #region IEqualityComparer<SerialPortInfo> Members

        public bool Equals(SerialPortInfo x, SerialPortInfo y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(SerialPortInfo obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}
