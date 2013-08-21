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
//     file="BioHarnessCommunications.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       BioHarnessCommunications.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      This class provides static methods for getting log data
                from bioharness devices implimenting the RIFF file system.
*/
////////////////////////////////////////////////////////////

namespace BioHarnessLogDownloader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Text;
    using Zephyr.IO;
    using Zephyr.Logging;

    /// <summary>
    /// This class provides static methods for getting log data 
    /// from bioharness devices implimenting the RIFF file system.
    /// </summary>
    [CLSCompliant(true)]
    public static class BioHarnessCommunications
    {
        /* Magic numbers for request/response message */
        private const int STX = 0x02;
        private const int MSG_ID_GET_LOG = 0x01;
        private const int DLC = 0x06;
        private const int ETX = 0x03;
        private const int NAK = 0x15;
        private const int ACK = 0x06;

        /* Error checking counts */
        private const int BAD_DATA_RETRIES = 10;
        private static int badSTXCount;
        private static int badMsgCount;
        private static int badBytesReadCount;
        private static int badCRCCount;
        private static int nakRetryCount;
        private static int numRequests;

        private static CRC8 crc = new CRC8(0x8c);
        private static byte[] request = new byte[] { STX, MSG_ID_GET_LOG, DLC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, ETX };
        private static SerialPort vcp = new SerialPort();
        private static Zephyr.IO.USB.WinUsbDevice winUsbDevice = new Zephyr.IO.USB.WinUsbDevice();
        private static bool isSerial = false;

        /// <summary>
        /// Syncronously obtains a directory of the log sessions on a device,
        /// including file format information. Will throw exceptions if there is
        /// an error in communication or gaining access to the com port.
        /// </summary>
        /// <param name="portName">The comport string to use to communicate withe the device.</param>
        /// <returns>A session directory structure representing the device.</returns>
        public static SessionDirectory SyncGetSessionDirectory(string portName)
        {
            string fileVersion = string.Empty;
            List<Session> list = new List<Session>();            
            Session session = null;

            try
            {
                Open(portName);                
                bool ok = true;

                if (isSerial == false)
                {
                    /* 
                     * Force to flush the read buffer before the process - in case of 
                     * For example the first time you plug the BH3 you retreive BioHarness3<cr><lf> into the BUlK IN 
                     */
                    System.Threading.Thread.Sleep(400);
                    var empty = new byte[128];
                    winUsbDevice.Read(empty);
                }

                ValidChunk lastValidChunk = null;
                bool error = false; 
                bool reportError = false;
                int offset = 0;
                var riff = Read(offset, 12, out ok);                                                    

                if (ok == false)
                {
                    // Retry
                    nakRetryCount++;
                    riff = Read(offset, 12, out ok);
                    if (ok == false)
                    {                        
                        throw new Exception("Cant read bioharness");
                    }
                }

                var strRiff = Encoding.ASCII.GetString(riff, 0, 4);
                if (strRiff.ToUpper().Equals("RIFF") == false)
                {                    
                    throw new Exception("Not a RIFF Chunk");                            
                }

                var tLength = BitConverter.ToInt32(riff, 4);
                /* DateTimeFormatInfo to parse DateTime */
                IFormatProvider culture = new CultureInfo("en-NZ", false);

                bool cancel = false;
                offset = 12;
                while (cancel == false)
                {
                    if (offset >= tLength)
                    {
                        break;
                    }

                    riff = Read(offset, 8, out ok);
                    if (ok == false)
                    {
                        nakRetryCount++;
                        riff = Read(offset, 8, out ok);
                        if (ok == false)
                        {                            
                            throw new Exception("Cant read Bioharness");
                        }
                    }         
           
                    offset += 8;
                    int ckLength = BitConverter.ToInt32(riff, 4);

                    string chunk;
                    try
                    {
                        error = false;
                        chunk = Encoding.ASCII.GetString(riff, 0, 4);

                        switch (chunk)
                        {
                            case "JUNK": /* JUNK */
                                lastValidChunk = new ValidChunk(offset, "JUNK");
                                offset += ckLength;
                                /* We can have JUNK between logh and logr, so don't reset session to null here */
                                break;
                            case "zphr": /* zphr */
                                lastValidChunk = new ValidChunk(offset, "zphr");
                                var zphr = Read(offset, ckLength, out ok);
                                if (ok == false)
                                {
                                    nakRetryCount++;
                                    zphr = Read(offset, ckLength, out ok);
                                    if (ok == false)
                                    {
                                        throw new Exception("Cant read bioharness");
                                    }
                                }

                                fileVersion = Encoding.ASCII.GetString(zphr, 4, 4);
                                offset += ckLength;
                                session = null;
                                break;
                            case "fms ": /* fms */
                                lastValidChunk = new ValidChunk(offset, "fms");
                                offset += ckLength;
                                session = null;
                                break;
                            case "logr": /* logr */
                                {
                                    lastValidChunk = new ValidChunk(offset, "logr");
                                    if (session == null)
                                    {
                                        error = true;
                                    }
                                    else
                                    {
                                        /* Calculate and round milliseconds */
                                        session.Duration = TimeSpan.FromMilliseconds((((ckLength - session.PadBytes) / (session.Channels * 2)) - 1) * session.Period);

                                        /* Because we have nothing to compare too, a single element comes up as zero */
                                        if (session.Duration == TimeSpan.FromSeconds(0))
                                        {
                                            session.Duration = TimeSpan.FromSeconds(1);
                                        }

                                        session.Offset = offset + session.PadBytes;
                                        session.Length = ckLength - session.PadBytes;
                                        list.Add(session);
                                    }

                                    offset += ckLength;
                                }

                                session = null;
                                break;
                            case "logh": 
                            case "log1": 
                            case "log2": /* logh */
                                {
                                    lastValidChunk = new ValidChunk(offset, "logh");
                                    var logh = Read(offset, ckLength, out ok);
                                    if (ok == false)
                                    {
                                        nakRetryCount++;
                                        logh = Read(offset, ckLength, out ok);
                                        if (ok == false)
                                        {                                            
                                            throw new Exception("Cant read bioharness");
                                        }
                                    }

                                    int period = BitConverter.ToInt16(logh, 8);
                                    period = (period == 0) ? 1008 : period;
                                    int channels = BitConverter.ToInt16(logh, 12);
                                    int padRaw = BitConverter.ToInt16(logh, 14);
                                    /* DateTime format should be invariant from the culture in USA Month and Day are reverse. */
                                    DateTime timestamp = DateTime.Parse(
                                        string.Format(
                                        "{0}/{1}/{2} {3}",
                                        logh[3],
                                        logh[2],
                                        logh[1] * 256 + logh[0],
                                        TimeSpan.FromMilliseconds(BitConverter.ToInt32(logh, 4)).ToString()),
                                        culture);
                                    session = new Session(timestamp, channels, padRaw);
                                    session.Period = period;
                                    offset += ckLength;
                                }

                                break;                            
                            default:
                                error = true;
                                session = null;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        /* This usually means device unplugged */
                        error = true;                        
                        throw new IOException("Sorry problem reading data.", ex);                                            
                    }

                    if (error == true)
                    {
                        /* Only report each block of errors once */
                        if (reportError == false)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("Error at: 0x{0:x8}, continuing ...", offset));
                            reportError = true;
                            offset = lastValidChunk.Offset;
                        }

                        offset -= 8;
                        int length = 2048 - (offset % 2048);

                        offset += length;
                    }
                    else
                    {
                        reportError = false;
                    }
                }  
                             
                Close();
            }            
            catch (Exception ex)
            {
                try
                {
                    Close();
                }
                catch 
                {
                }

                throw new IOException("Sorry problem reading data.", ex);                                                            
            }

            return new SessionDirectory(list.ToArray(), fileVersion);
        }        

        /// <summary>
        /// Syncronously downloads the log session data specified.
        /// Does no interprestation of the data, returns a byte array representing the log data.
        /// </summary>
        /// <param name="comPort">The compurt where the device can be found.</param>
        /// <param name="log">The log session to be retrieved</param>
        /// <returns>A byte array containing all the log data</returns>
        public static byte[] SyncLoadData(string comPort, Session log)
        {            
            try
            {
                Open(comPort);
            }
            catch (Exception ex)
            {
                throw new IOException("Error opening Com Port", ex);
            }

            bool ok = true;
            var data = Read(log.Offset, log.Length, out ok);
            Close();
            if (ok == false)
            {
                throw new Exception("Could not download the session.");
            }

            return data;
        }

        /// <summary>
        /// Syncronously reads data from the BioHarness.
        /// Handles the maximum message size constraints, splitting the
        /// request into manageable chunks, and reassembling the response.
        /// </summary>
        /// <param name="offset">The byte to start read at</param>
        /// <param name="length">The number of bytes to read</param>        
        /// <returns>A byte array conaining the requested data</returns>
        private static byte[] SyncRead(int offset, int length)
        {
            CRC8 crc = new CRC8(0x8c);
            var response = new byte[length];
            var answer = new byte[133];
            /* request = STX, MSG_ID_LOG,  DLC, ... ETX */
            byte[] request = new byte[] { STX, MSG_ID_GET_LOG, DLC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, ETX };

            var loop = length / 128;
            if ((length % 128) > 0)
            {
                loop++;
            }

            int read = 0;
            byte nb = 128;
            do
            {
                if ((length - read) < 128)
                {
                    nb = (byte)(length - read);
                }

                var offsetBytes = BitConverter.GetBytes(offset);
                request[4] = offsetBytes[0];
                request[5] = offsetBytes[1];
                request[6] = offsetBytes[2];
                request[7] = offsetBytes[3];
                request[8] = nb;
                request[9] = crc.Calculate(request.Skip(3).Take(6).ToArray());
                if (isSerial)
                {
                    vcp.Write(request, 0, 11);
                }
                else
                {
                    winUsbDevice.Write(request, 11);
                }

                int r = 0;
                if (isSerial)
                {
                    while ((r += vcp.Read(answer, r, nb + 5 - r)) < nb + 5)
                    {
                    }
                }
                else
                {
                    while ((r += winUsbDevice.Read(answer, r, nb + 5 - r)) < nb + 5)
                    {
                    }
                }

                Array.Copy(answer, 3, response, read, nb);
                offset += nb;
                read += nb;
            }
            while (--loop > 0);

            return response;
        }

        #region Serial
        /// <summary>
        /// Opens a connection to the device
        /// </summary>
        /// <param name="portName">The port name to open</param>
        private static void Open(string portName)
        {
            // NOTE: detect if this is a COM port, or the serial number of a USB device
            // 'COM' = COM port, 'CNC' = COM0COM
            isSerial = portName != null && (portName.StartsWith("COM") || portName.StartsWith("CNC"));
            if (isSerial)
            {
                vcp.BaudRate = 115200;
                vcp.DataBits = 8;
                vcp.Parity = Parity.None;
                vcp.PortName = portName;
                vcp.StopBits = StopBits.One;
                vcp.Handshake = Handshake.None;
                vcp.ReadTimeout = 2000;
                vcp.WriteTimeout = 2000;
                vcp.Open();
            }
            else
            {
                OpenUsb(portName);
            }
        }

        /// <summary>
        /// Closes connection to the device (whatever the connection type)
        /// </summary>
        private static void Close()
        {
            if (isSerial)
            {
                if (vcp.IsOpen)
                {
                    vcp.Close();
                }
            }
            else
            {
                if (winUsbDevice.IsOpen)
                {
                    winUsbDevice.Close();
                }
            }   
        }

        /// <summary>
        /// Read bytes from the device
        /// </summary>
        /// <param name="offset">Location or address in the device</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="ok">A ref to a bool that indicates success</param>
        /// <returns>An array of bytes read from the device</returns>
        private static byte[] Read(int offset, int length, out bool ok)
        {
            if (!isSerial)
            {
                return ReadUsb(offset, length, out ok);
            }

            var response = new byte[length];
            var answer = new byte[133];

            bool nak = false;

            var loop = length / 128;
            if ((length % 128) > 0)
            {
                loop++;
            }

            int read = 0;
            byte nb = 128;
            int badDataRetryCount = 0;
            bool badData = false;            

            do /* Each 128 byte (max) frame */
            {
                do /* Retry up to BAD_DATA_RETRIES times when bad data is read */
                {                    
                    badData = false;

                    if ((length - read) < 128)
                    {
                        nb = (byte)(length - read);
                    }

                    var offsetBytes = BitConverter.GetBytes(offset);
                    request[4] = offsetBytes[0];
                    request[5] = offsetBytes[1];
                    request[6] = offsetBytes[2];
                    request[7] = offsetBytes[3];
                    request[8] = nb;
                    request[9] = crc.Calculate(request.Skip(3).Take(6).ToArray());
                    vcp.Write(request, 0, 11);

                    int r = 0;
                    try
                    {
                        while ((r += vcp.Read(answer, r, nb + 5 - r)) < nb + 5)
                        {
                            /* Read into answer until full chunk is recieved */
                        }
                    }
                    catch
                    {
                        /* Typically timeout*/
                        nak = true;
                    }

                    if (r > 0)
                    {
                        /* Error checking */

                        /* Device said NAK */
                        if (answer[r - 1] == NAK)
                        {
                            nak = true;
                            break;
                        }

                        /* Check for device ACK */
                        if (answer[nb + 4] != ACK)
                        {
                            badData = true;
                        }

                        /* STX header */
                        if (answer[0] != STX)
                        {
                            badData = true;
                        }

                        /* MsgID header */
                        if (answer[1] != MSG_ID_GET_LOG)
                        {
                            badData = true;
                        }

                        /* Number of bytes read matches request */
                        if (answer[2] != nb)
                        {
                            badData = true;
                        }

                        /* CRC */
                        CRC8 frameCrc = new CRC8(0x8c);
                        byte crcVal = frameCrc.Calculate(answer.Skip(3).Take(nb).ToArray());
                        if (answer[nb + 3] != crcVal)
                        {
                            badData = true;
                        }
                    }

                    if (nak == true)
                    {
                        break;
                    }

                    if (badData)
                    {
                        badDataRetryCount++;
                    }
                    else
                    {
                        Array.Copy(answer, 3, response, read, nb);
                        offset += nb;
                        read += nb;
                    }
                } 
                while (badData && badDataRetryCount < BAD_DATA_RETRIES);
                
                if (badData)
                {
                    /* If we still had bad data after BAD_DATA_RETRIES then give up with NAK
                     * Read() function will be called once more externally on NAK (ok = false)
                     */
                    nak = true;
                    break;
                }
            } 
            while (--loop > 0);  /* Next 128 byte frame */

            ok = !nak;
            return response;
        }
        #endregion

        #region USB
        /// <summary>
        /// Open a USB connection to the specified device
        /// </summary>
        /// <param name="serialNumber">Serial number of the device to connect to</param>
        private static void OpenUsb(string serialNumber)
        {
            winUsbDevice.Open(serialNumber);
        }

        /// <summary>
        /// Close the USB connection
        /// </summary>
        private static void CloseUsb()
        {
            winUsbDevice.Close();
        }

        /// <summary>
        /// Read bytes from the USB device
        /// </summary>
        /// <param name="offset">Location into the file (address in device)</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="ok">Boolean indicating whether the read was successfull</param>
        /// <returns>An array of bytes read from the device</returns>
        private static byte[] ReadUsb(int offset, int length, out bool ok)
        {
            var response = new byte[length];
            var answer = new byte[133];

            bool nak = false;

            var loop = length / 128;
            if ((length % 128) > 0)
            {
                loop++;
            }

            int read = 0;
            byte nb = 128;
            int badDataRetryCount = 0;
            bool badData = false;

            do /* Each 128 byte (max) frame */
            {
                badDataRetryCount = 0;
                do /* Retry up to BAD_DATA_RETRIES times when bad data is read */
                {
                    numRequests++;
                    badData = false;

                    if ((length - read) < 128)
                    {
                        nb = (byte)(length - read);
                    }

                    var offsetBytes = BitConverter.GetBytes(offset);
                    request[4] = offsetBytes[0];
                    request[5] = offsetBytes[1];
                    request[6] = offsetBytes[2];
                    request[7] = offsetBytes[3];
                    request[8] = nb;
                    request[9] = crc.Calculate(request.Skip(3).Take(6).ToArray());
                    winUsbDevice.Write(request, 11);

                    DateTime timeout = DateTime.Now;

                    int r = 0;
                    try
                    {
                        while ((r += winUsbDevice.Read(answer, r, nb + 5 - r)) < nb + 5)
                        {
                            /* Read into answer until full chunk is recieved */
                            System.Threading.Thread.Sleep(0);

                            /* 
                             * XXX: TODO:
                             * On ZModem that does not support 0x02 log message 
                             * When operating via USB this returns a NAK
                             * then ends up in an infinate loop.
                             * WONT affect commercial products (ZModem is serial not USB)
                             */
                            if ((DateTime.Now - timeout) > TimeSpan.FromMilliseconds(2000))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("Error : Timeout - {0}.", offset));
                                break;
                            }
                        }
                    }
                    catch
                    {
                        /* Typically timeout */
                        nak = true;
                        badData = true;
                    }

                    if (r > 0)
                    {
                        /* Error checking */ 

                        /* Device said NAK */
                        if (answer[r - 1] == NAK)
                        {
                            badData = true; /* nak = true; */
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : NAK - {0}.", offset));
                        }

                        /* Check for device ACK */
                        if (answer[nb + 4] != ACK)
                        {
                            badData = true;
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : Not ACK - {0}.", offset));
                        }

                        /* STX header */
                        if (answer[0] != STX)
                        {
                            badSTXCount++;
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : Not STX - {0}.", offset));
                            badData = true;
                        }

                        /* MsgID header */
                        if (answer[1] != MSG_ID_GET_LOG)
                        {
                            badMsgCount++;
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : Wrong Msg ID - {0}.", offset));
                            badData = true;
                        }

                        /* Number of bytes read matches request */
                        if (answer[2] != nb)
                        {
                            badBytesReadCount++;
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : Wrong number of bytes - {0}.", offset));
                            badData = true;
                        }

                        /* CRC */
                        CRC8 frameCrc = new CRC8(0x8c);
                        byte crcVal = frameCrc.Calculate(answer.Skip(3).Take(nb).ToArray());
                        if (answer[nb + 3] != crcVal)
                        {
                            badCRCCount++;
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : Wrong CRC - {0}.", offset));
                            badData = true;
                        }
                    }

                    if (nak == true)
                    {
                        break;
                    }

                    if (badData)
                    {
                        badDataRetryCount++;
                        var byteToRead = winUsbDevice.BytesToRead;
                        if (byteToRead > 0)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("Error : Flush the buffer of {1} bytes - {0}.", offset, byteToRead));
                            winUsbDevice.Read(new byte[byteToRead]);
                        }
                    }
                    else
                    {
                        if (r > 0)
                        {
                            Array.Copy(answer, 3, response, read, nb);
                            offset += nb;
                            read += nb;
                        }
                        else
                        {
                            badData = true;
                        }
                    }
                } 
                while (badData && (badDataRetryCount < BAD_DATA_RETRIES));

                if (badData)
                {
                    /* 
                     * If we still had bad data after BAD_DATA_RETRIES then give up with NAK
                     * Read() function will be called once more externally on NAK (ok = false)
                     */
                    nak = true;
                    break;
                }
            } 
            while (--loop > 0);  /* Next 128 byte frame */

            ok = !nak;
            return response;
        }
        #endregion

        /// <summary>
        /// Class that represents a valid riff file chunk
        /// </summary>
        private class ValidChunk
        {
            /// <summary>
            /// Gets the Location in the file
            /// </summary>
            public int Offset { get; private set; }

            /// <summary>
            /// Gets the ID string
            /// </summary>
            public string Type { get; private set; }

            /// <summary>
            /// Initializes a new instance of the ValidChunk class.
            /// </summary>
            /// <param name="offset">The location of the chunk</param>
            /// <param name="type">The id of the chunk</param>
            public ValidChunk(int offset, string type)
            {
                this.Offset = offset;
                this.Type = type;
            }
        }
    }    
}
