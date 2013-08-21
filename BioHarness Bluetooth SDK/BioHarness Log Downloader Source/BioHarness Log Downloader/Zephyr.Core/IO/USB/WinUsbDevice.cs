using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

namespace Zephyr.IO.USB
{
    /// <summary>
    ///  Routines for the WinUsb driver supported by Windows Vista and Windows XP.
    ///  </summary>
    ///  
    public sealed partial class WinUsbDevice : IDisposable
    {
        private const int pipeTimeout = 2000;
        private object sync = new object();

        internal struct DevInfo
        {
            internal SafeFileHandle deviceHandle;
            internal IntPtr winUsbHandle;  // Default interface (Interrupt in our case)
            internal IntPtr winUsbAssociatedHandle;  // Associated interface (Bulk IN / Bulk OUT pipes in our case)
            internal Byte bulkInPipe;
            internal Byte bulkOutPipe;
            internal Int32 bulkInPacketSize;
            internal Int32 bulkOutPacketSize;
            internal Int32 maximumTransferSize;
            internal UInt32 devicespeed;
        }

        internal DevInfo devInfo = new DevInfo();

        /// <summary>
        /// Has the device been opened / initalised
        /// </summary>
        public bool IsOpen
        {
            get { return isOpen; }
        }
        private bool isOpen;

        /// <summary>
        /// Analogous to a COM port name
        /// </summary>
        public string SerialNumber { get; set; }

        public int BytesToRead
        {
            get
            {
                if (!isOpen || rxFIFO == null)
                {
                    return 0;
                }
                else
                {
                    int count = 0;
                    lock (sync)
                    {
                        //rxBufferBusy.WaitOne();
                        count = rxFIFO.Count;
                        //rxBufferBusy.ReleaseMutex();
                    }
                    return count;
                }
            }
        }


        #region Overlapped IO
        private Thread eventThread;

        private IntPtr rxOverlapped = IntPtr.Zero;
        private ManualResetEvent threadStarted = new ManualResetEvent(false);
        private Mutex rxBufferBusy = new Mutex();
        private Queue rxFIFO;
        private const int RX_FIFO_SIZE = 8192;  // Arbitary 8Kb

        private IntPtr txOverlapped = IntPtr.Zero;
        private Mutex txBufferBusy = new Mutex();
        private AutoResetEvent txEvent = new AutoResetEvent(false);


        // Data received event
        public delegate void DataReceivedEvent(object sender, EventArgs e);
        public event DataReceivedEvent DataReceived;

        // Error received event
        public delegate void ErrorReceivedEvent(object sender, ErrorWrapper e);
        public event ErrorReceivedEvent ErrorReceived;


        public void Write(byte[] buffer)
        {
            if (buffer != null)
                Write(buffer, buffer.Length);
        }

        /// <summary>
        /// Write the contents of the buffer to the device
        /// MAX length is 256 bytes
        /// </summary>
        /// <param name="buffer"></param>
        unsafe public void Write(Byte[] buffer, int count)
        {
            ManualResetEvent txDone = new ManualResetEvent(false);

            if (!isOpen)
            {
                throw new System.InvalidOperationException("Port is not open");
            }

            if (buffer == null)
            {
                throw new System.ArgumentNullException("Buffer is null");
            }


            // NOTE: All writes must be a multiple of the MAX_PACKET_SIZE (8, 16, 32, 64, ...) of the end point
            // WinUSB divides things up internally as required, but we get much
            // better performance if we do this ourselves...
            // This could potentially cause garbage collection thrashing downloading logs...
            int NEXT_MULTIPLE = ((count + devInfo.bulkOutPacketSize - 1) / devInfo.bulkOutPacketSize) * devInfo.bulkOutPacketSize;
            Byte[] writeBuffer = new Byte[NEXT_MULTIPLE];
            //System.Diagnostics.Debug.WriteLine("Sending: " + writeBuffer.Length + " bytes");

            /*
              // Pack message at head of request
              for (int i = 0; i < count; i++)
                 writeBuffer[i] = buffer[i];
            */ 

            // Pack message at tail of request
            int startIndex = NEXT_MULTIPLE - count;
            for (int i = startIndex; i < NEXT_MULTIPLE; i++)
            {
                writeBuffer[i] = buffer[i - startIndex];
            }

            IOCompletionCallback ioComplete = delegate(uint errorCode, uint numBytes, NativeOverlapped* _nativeOv)
            {
                unsafe
                {
                    try
                    {
                       
                        txDone.Set();
                    }
                    finally
                    {
                        System.Threading.Overlapped.Unpack(_nativeOv);
                        System.Threading.Overlapped.Free(_nativeOv);
                    }

                }
            };


            Overlapped ov = new Overlapped();
            NativeOverlapped* nativeOv = ov.Pack(ioComplete, buffer);



            ulong bytesWritten = 0;
            Boolean success;

            // Lock and wait for write operation to occur
            //txBufferBusy.WaitOne();
            success = WinUsbDevice.WinUsb_WritePipe
            (
                devInfo.winUsbAssociatedHandle,
                devInfo.bulkOutPipe,
                buffer, //writeBuffer,
                (uint)buffer.Length, //(uint)writeBuffer.Length,
                ref bytesWritten,
                nativeOv //txOverlapped
            );


            // On failure wait for overlapped IO
            if (!success)
            {
                int e = Marshal.GetLastWin32Error();
                if (e == (int)WinUsbFileIO.APIErrors.ERROR_IO_PENDING)
                {
                    /*   success = WinUsbDevice.WinUsb_GetOverlappedResult
                       (
                           devInfo.winUsbAssociatedHandle,
                           txOverlapped,
                           ref numberOfBytesTransferred,
                           true
                       );

                       if (success)
                       {
                           bytesWritten = (ulong)numberOfBytesTransferred;
                       }
                     * */
                    txDone.WaitOne();
                }
                else
                {
                    System.Threading.Overlapped.Unpack(nativeOv);
                    System.Threading.Overlapped.Free(nativeOv);
                    if (ErrorReceived != null)
                    {
                        //txBufferBusy.ReleaseMutex();
                        ErrorReceived(this, new ErrorWrapper(e));
                        return;
                    }
                }
            }
            else
            {
                System.Threading.Overlapped.Unpack(nativeOv);
                System.Threading.Overlapped.Free(nativeOv);
            }

            txDone.Close();
            txDone = null;
            //txBufferBusy.ReleaseMutex();
        }

        public int Read(byte[] buffer)
        {
            if (buffer != null)
                return Read(buffer, 0, buffer.Length);
            else
                return 0;
        }

        /// <summary>
        /// If there is data waiting to be read, then the buffer is populated with the data, else the buffer is set to null
        /// </summary>
        /// <returns>number of bytes read</returns>
        public int Read(byte[] buffer, int count)
        {
            return Read(buffer, 0, count);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (!isOpen)
            {
                throw new System.InvalidOperationException("Port is not open");
            }

            int i = 0;
            lock (sync)
            {
                //rxBufferBusy.WaitOne();
                //if (rxFIFO.Count == 0)
                     // return 0;

                i = 0;
                while (rxFIFO.Count > 0 && i < count)
                {
                    buffer[i + offset] = (byte)rxFIFO.Dequeue();
                    i++;
                }
                //rxBufferBusy.ReleaseMutex();
            }
            // Zero rest of buffer
            int bytesRead = i;
            //while (i < buffer.Length)
            //{
            //    buffer[i] = 0;
            //    i++;
            //}

            return bytesRead;
        }


        /// <summary>
        /// 
        /// </summary>
        unsafe private void CommEventThread()
        {
            AutoResetEvent rxEvent = new AutoResetEvent(false);
            NativeOverlapped overlapped = new NativeOverlapped();
            rxOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(overlapped));
            overlapped.OffsetLow = 0;
            overlapped.OffsetHigh = 0;
            overlapped.EventHandle = rxEvent.SafeWaitHandle.DangerousGetHandle();
            Marshal.StructureToPtr(overlapped, rxOverlapped, true);

            Byte[] buffer = new byte[devInfo.maximumTransferSize];

            ulong bytesRead = 0;
            uint errorValue = 0;

            AutoResetEvent rxDone = new AutoResetEvent(false);
            Boolean success = false;

            IOCompletionCallback ioComplete = delegate(uint errorCode, uint numBytes, NativeOverlapped* _nativeOv)
            {
                unsafe
                {
                    try
                    {
                        errorValue = errorCode;
                        bytesRead = numBytes;
       //                 if (bytesRead > 0)
       //                 { /*success = true;*/ System.Diagnostics.Debug.WriteLine(string.Format("Received : {0} bytes.", bytesRead )); }
                        rxDone.Set();
                    }
                    finally
                    {
                        //System.Threading.Overlapped.Unpack(_nativeOv);
                        //System.Threading.Overlapped.Free(_nativeOv);
                    }

                }
            };

            //List<byte> traceBuffer = new List<byte>();

            fixed (byte* pBuffer = buffer)
            {
                Overlapped ov = new Overlapped();
                NativeOverlapped* nativeOv = ov.Pack(ioComplete, buffer);

                try
                {
                    // Flag as started for anything that is waiting
                    threadStarted.Set();

                    // Try for IO
                    while (devInfo.winUsbAssociatedHandle != IntPtr.Zero && isOpen)
                    {
                        #region Read from USB
                        //ulong bytesRead = 0;
                        //System.Diagnostics.Debug.WriteLine("Read .. .. ");
                        //System.Threading.Thread.Sleep(0);
                        //errorValue = 0;
                        success = WinUsbDevice.WinUsb_ReadPipe
                        (
                            devInfo.winUsbAssociatedHandle,
                            devInfo.bulkInPipe,
                            pBuffer, //buffer,
                            (uint)buffer.Length,
                            ref bytesRead,
                            nativeOv //rxOverlapped
                        );

                        if (!success)
                        {
                            int e = Marshal.GetLastWin32Error();
                            //                System.Diagnostics.Debug.WriteLine(string.Format("Marshal error : {0}.", e));
                            if (e == (int)WinUsbFileIO.APIErrors.ERROR_IO_PENDING)
                            {
                                #region Wait till bytes transmitted from device
                                /*                       // If IO pending, wait for result
                            success = WinUsbDevice.WinUsb_GetOverlappedResult
                            (
                                devInfo.winUsbAssociatedHandle,
                                rxOverlapped,
                                ref bytesTransfered,
                                true
                            );

                            if (success)
                            {
                                bytesRead = (ulong)bytesTransfered;
                            }
                            else
                            {
                                bytesRead = 0;
                            }

      * */
                                //                      System.Diagnostics.Debug.WriteLine("Waiting rxDone");
                                rxDone.WaitOne();
                //                rxDone.Reset();
                                //                      System.Diagnostics.Debug.WriteLine("reseted rxDone.");

                                #endregion Wait for bytes
                            }
                            else if (e == (int)WinUsbFileIO.APIErrors.ERROR_SEM_TIMEOUT)
                            {
                                // System.Threading.Thread.Sleep(0);
                                System.Diagnostics.Debug.WriteLine("SEM Timeout");
                                success = true;
                            }
                            else if (e == (int)WinUsbFileIO.APIErrors.ERROR_DEVICE_NOT_CONNECTED)
                            {
                                CloseDeviceHandle();
                            }
                            else if (ErrorReceived != null)
                            {
                                CloseDeviceHandle();
                                ErrorReceived(this, new ErrorWrapper(e));
                            }
                        }
                        #endregion

                        //                System.Diagnostics.Debug.WriteLine(string.Format("Success = {0}", success));
                        if (errorValue != 0) //if (!success)
                        {
                            int e = Marshal.GetLastWin32Error();
                            e = (int)errorValue;
                            if (e == (int)WinUsbFileIO.APIErrors.ERROR_INVALID_HANDLE)
                            {
                                System.Diagnostics.Debug.WriteLine("***Invalid handle");
                                isOpen = false;
                            }
                            else if (e == (int)WinUsbFileIO.APIErrors.ERROR_NOT_ENOUGH_MEMORY)
                            {
                                System.Diagnostics.Debug.WriteLine("***Not enough memory");
                                isOpen = false;
                            }
                            else if (e == (int)WinUsbFileIO.APIErrors.ERROR_SEM_TIMEOUT)
                            {
                                // Stop thrashing CPU / allow other threads a chance (especially Windows message pump)
                                System.Threading.Thread.Sleep(0);
                            }
                            else if (e == (int)WinUsbFileIO.APIErrors.ERROR_DEVICE_NOT_CONNECTED)
                            {
                                if (isOpen)
                                    CloseDeviceHandle();
                            }
                            else if (ErrorReceived != null)
                            {
                                ErrorReceived(this, new ErrorWrapper(e));
                            }
                        }
                        else
                        {
                            #region safely pull from buffer to queue
                            if (bytesRead > 0)
                            {
                                lock (sync)
                                {
                                    //rxBufferBusy.WaitOne();
                                    //                              System.Diagnostics.Debug.WriteLine(string.Format("Queuing {0} bytes.",bytesRead ));
                                    for (int b = 0; b < (int)bytesRead; b++)
                                    {
                                        rxFIFO.Enqueue(buffer[b]);
                                        //traceBuffer.Add(buffer[b]);
                                    }
                                    //rxBufferBusy.ReleaseMutex();
                                }
                                // Call data received event
                                if (DataReceived != null)
                                {
                                    DataReceived(this, new EventArgs());
                                }
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception e)
                {
                    if (ErrorReceived != null)
                    {
                        ErrorReceived(this, new ErrorWrapper(e));
                    }
                }
                finally
                {
                    System.Threading.Overlapped.Unpack(nativeOv);
                    System.Threading.Overlapped.Free(nativeOv);

                    rxDone.Close();
                    rxDone = null;

                    // When the thread is completed (invalid handle or !IsOpen) release txOverlapped                
                    if (rxOverlapped != IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine("Freeing RX buffer");
                        Marshal.FreeHGlobal(rxOverlapped);
                        rxOverlapped = IntPtr.Zero;
                        rxEvent = null;
                    }
                }
            }
        }
        #endregion



        ///  <summary>
        ///  Closes the device handle obtained with CreateFile and frees resources.
        ///  </summary>
        ///  
        internal void CloseDeviceHandle()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Closing device handle");

                if (devInfo.winUsbHandle != IntPtr.Zero && devInfo.winUsbAssociatedHandle != IntPtr.Zero)
                {
                    if (devInfo.bulkInPipe != 0)
                    {
                        WinUsb_ResetPipe
                        (
                            devInfo.winUsbAssociatedHandle,
                            devInfo.bulkInPipe
                         );
                    }

                    if (devInfo.bulkOutPipe != 0)
                    {
                        WinUsb_ResetPipe
                        (
                            devInfo.winUsbAssociatedHandle,
                            devInfo.bulkOutPipe
                        );
                    }
                }

                if (devInfo.winUsbAssociatedHandle != IntPtr.Zero)
                {
                    WinUsb_Free(devInfo.winUsbAssociatedHandle);
                    devInfo.winUsbAssociatedHandle = IntPtr.Zero;
                }

                if (devInfo.winUsbHandle != IntPtr.Zero)
                {
                    WinUsb_Free(devInfo.winUsbHandle);
                    devInfo.winUsbHandle = IntPtr.Zero;
                }

                if (devInfo.deviceHandle != null)
                {
                    if (!(devInfo.deviceHandle.IsInvalid && !(devInfo.deviceHandle.IsClosed)))
                    {
                        devInfo.deviceHandle.Close();
                    }
                    devInfo.deviceHandle.Dispose();
                    devInfo.deviceHandle = null;
                }


                // CommEventThread() responsible for creating / freeing rxBufferOverlapped
                if (txOverlapped != IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("Freeing TX buffer");
                    Marshal.FreeHGlobal(txOverlapped);
                    txOverlapped = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }

            isOpen = false;
        }


        ///  <summary>
        ///  Requests a handle with CreateFile.
        ///  </summary>
        ///  
        ///  <param name="devicePathName"> Returned by SetupDiGetDeviceInterfaceDetail 
        ///  in an SP_DEVICE_INTERFACE_DETAIL_DATA structure. </param>
        ///  
        ///  <returns>
        ///  The handle.
        ///  </returns>
        internal Boolean GetDeviceHandle(String devicePathName)
        {
            // ***
            // API function

            //  summary
            //  Retrieves a handle to a device.

            //  parameters 
            //  Device path name returned by SetupDiGetDeviceInterfaceDetail
            //  Type of access requested (read/write).
            //  FILE_SHARE attributes to allow other processes to access the device while this handle is open.
            //  Security structure. Using Null for this may cause problems under Windows XP.
            //  Creation disposition value. Use OPEN_EXISTING for devices.
            //  Flags and attributes for files. The winsub driver requires FILE_FLAG_OVERLAPPED.
            //  Handle to a template file. Not used.

            //  Returns
            //  A handle or INVALID_HANDLE_VALUE.
            // ***
            System.Diagnostics.Debug.WriteLine(string.Format("Trying {0}", devicePathName));
            devInfo.deviceHandle = WinUsbFileIO.CreateFile
            (
                devicePathName,
                (WinUsbFileIO.GENERIC_WRITE | WinUsbFileIO.GENERIC_READ),
                (WinUsbFileIO.FILE_SHARE_READ | WinUsbFileIO.FILE_SHARE_WRITE),
                IntPtr.Zero,
                WinUsbFileIO.OPEN_EXISTING,
                (WinUsbFileIO.FILE_ATTRIBUTE_NORMAL | WinUsbFileIO.FILE_FLAG_OVERLAPPED),
                0
            );


            if (devInfo.deviceHandle.IsInvalid)
            {
                System.Diagnostics.Debug.WriteLine("Could not get device handle: " + new Win32Exception((int)Marshal.GetLastWin32Error()).Message + " trying again in one second");

                // Sometimes device enumeration takes a second after the device is attached
                System.Threading.Thread.Sleep(1000);

                devInfo.deviceHandle = WinUsbFileIO.CreateFile
                (
                    devicePathName,
                    (WinUsbFileIO.GENERIC_WRITE | WinUsbFileIO.GENERIC_READ),
                    (WinUsbFileIO.FILE_SHARE_READ | WinUsbFileIO.FILE_SHARE_WRITE),
                    IntPtr.Zero,
                    WinUsbFileIO.OPEN_EXISTING,
                    (WinUsbFileIO.FILE_ATTRIBUTE_NORMAL | WinUsbFileIO.FILE_FLAG_OVERLAPPED),
                    0
                );


                if (devInfo.deviceHandle.IsInvalid)
                {
                    System.Diagnostics.Debug.WriteLine("Could not get device handle: " + new Win32Exception((int)Marshal.GetLastWin32Error()).Message + " not trying again");
                    return false;
                }
                else
                {
                    return  ThreadPool.BindHandle(devInfo.deviceHandle)   ; //true;
                }
            }
            else
            {
                return ThreadPool.BindHandle(devInfo.deviceHandle);  //true;
            }
        }


        internal void ConfigurePipePolicy(IntPtr deviceHandle, Byte pipeId)
        {
/*
#if DEBUG
                System.Diagnostics.Debug.WriteLine("*** Pipe Policy BEFORE changes: ");
                DebugDisplayPipePolicy(pipeId);
#endif
*/
            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.SHORT_PACKET_TERMINATE),
                Convert.ToByte(false)
            );

            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.AUTO_CLEAR_STALL),
                Convert.ToByte(true)
            );

            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.PIPE_TRANSFER_TIMEOUT),
                pipeTimeout
            );

            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.IGNORE_SHORT_PACKETS),
                Convert.ToByte(false)
            );

            SetPipePolicy
            (
               deviceHandle,
               pipeId,
               Convert.ToUInt32(POLICY_TYPE.ALLOW_PARTIAL_READS),
               Convert.ToByte(true)
            );

            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.AUTO_FLUSH),
                Convert.ToByte(false)
            );


            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.RAW_IO),
                Convert.ToByte(false)
            );


            SetPipePolicy
            (
                deviceHandle,
                pipeId,
                Convert.ToUInt32(POLICY_TYPE.RESET_PIPE_ON_RESUME),
                Convert.ToByte(false)
            );

            /*
#if DEBUG
            System.Diagnostics.Debug.WriteLine("*** Pipe Policy AFTER changes: ");
            DebugDisplayPipePolicy(pipeId);
#endif
            */
        }

        ///  <summary>
        ///  Initializes a device interface and obtains information about it.
        ///  Calls these winusb API functions:
        ///    WinUsb_Initialize
        ///    WinUsb_QueryInterfaceSettings
        ///    WinUsb_QueryPipe
        ///  </summary>
        ///  
        ///  <param name="deviceHandle"> A handle obtained in a call to winusb_initialize. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>
        internal Boolean InitializeDevice()
        {
            USB_INTERFACE_DESCRIPTOR ifaceDescriptor = new USB_INTERFACE_DESCRIPTOR();
            WINUSB_PIPE_INFORMATION pipeInfo = new WINUSB_PIPE_INFORMATION();
            Boolean success;
            Boolean associatedSuccess;

            try
            {
                // Zero out structure for subsequent calls
                ifaceDescriptor.bLength = 0;
                ifaceDescriptor.bDescriptorType = 0;
                ifaceDescriptor.bInterfaceNumber = 0;
                ifaceDescriptor.bAlternateSetting = 0;
                ifaceDescriptor.bNumEndpoints = 0;
                ifaceDescriptor.bInterfaceClass = 0;
                ifaceDescriptor.bInterfaceSubClass = 0;
                ifaceDescriptor.bInterfaceProtocol = 0;
                ifaceDescriptor.iInterface = 0;

                // Zero out structure for subsequent calls
                pipeInfo.PipeType = 0;
                pipeInfo.PipeId = 0;
                pipeInfo.MaximumPacketSize = 0;
                pipeInfo.Interval = 0;

                // ***
                //  winusb function 

                //  summary
                //  get a handle for communications with a winusb device

                //  parameters
                //  Handle returned by CreateFile.
                //  Device handle to be returned.

                //  returns
                //  True on success.
                //  ***
                success = WinUsb_Initialize(devInfo.deviceHandle, ref devInfo.winUsbHandle);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("Found USB Interface");

                    // Try for an associated interface
                    associatedSuccess = WinUsb_GetAssociatedInterface(devInfo.winUsbHandle, 0, ref devInfo.winUsbAssociatedHandle);
                    if (!associatedSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine("Could not find associated USB interface");
                        System.Diagnostics.Debug.WriteLine(new System.ComponentModel.Win32Exception((int)Marshal.GetLastWin32Error()).Message);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Could not find USB interface");
                    return false;
                }



                #region default interface
                success = WinUsb_QueryInterfaceSettings
                (
                    devInfo.winUsbHandle,
                    0,
                    ref ifaceDescriptor
                );


                // Check interface matches requirements
                if (success)
                {
                    // InterfaceClass: Communication Device Class
                    // InterfaceSubClass: Abstract Control Model
                    // InterfaceProtocol: AT commands (Hayes)
                    success =
                    (
                        ifaceDescriptor.bInterfaceClass == 0x02 &&
                        ifaceDescriptor.bInterfaceSubClass == 0x02 &&
                        ifaceDescriptor.bInterfaceProtocol == 0x01
                    );
                }

                if (success)
                {
                    for (Int32 i = 0; i <= ifaceDescriptor.bNumEndpoints - 1; i++)
                    {
                        WinUsb_QueryPipe(devInfo.winUsbHandle, 0, System.Convert.ToByte(i), ref pipeInfo);
                        System.Diagnostics.Debug.WriteLine("Default Interface, Found: " + pipeInfo.PipeType + " " + pipeInfo.PipeId + " " + pipeInfo.MaximumPacketSize + " " + pipeInfo.Interval);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error querying USB interface");
                    return false;
                }
                #endregion


                #region Query Associated Interface
                if (devInfo.winUsbAssociatedHandle != IntPtr.Zero)
                {
                    associatedSuccess = WinUsb_QueryInterfaceSettings
                    (
                        devInfo.winUsbAssociatedHandle,
                        0,
                        ref ifaceDescriptor
                    );


                    // Check interface matches requirements
                    if (associatedSuccess)
                    {
                        // InterfaceClass: Data Interface Class
                        // InterfaceSubClass: Unused
                        // InterfaceProtocol: No Specific protocol                   
                        associatedSuccess =
                        (
                            ifaceDescriptor.bInterfaceClass == 0x0A &&
                            ifaceDescriptor.bInterfaceSubClass == 0x00 &&
                            ifaceDescriptor.bInterfaceProtocol == 0x00
                        );
                    }

                    if (associatedSuccess)
                    {
                        //  Get the transfer type, endpoint number, and direction for the interface's
                        //  bulk and interrupt endpoints. Set pipe policies.                        
                        for (Int32 i = 0; i <= ifaceDescriptor.bNumEndpoints - 1; i++)
                        {
                            WinUsb_QueryPipe
                            (
                                devInfo.winUsbAssociatedHandle,
                                0,
                                System.Convert.ToByte(i),
                                ref pipeInfo
                            );

                            if (((pipeInfo.PipeType ==
                                USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                                UsbEndpointDirectionIn(pipeInfo.PipeId)))
                            {

                                System.Diagnostics.Debug.Assert(pipeInfo.MaximumPacketSize > 0, "USB Bulk IN packet size is zero");
                                devInfo.bulkInPipe = pipeInfo.PipeId;
                                devInfo.bulkInPacketSize = pipeInfo.MaximumPacketSize;
                                System.Diagnostics.Debug.WriteLine("Associated Interface, Found BULK IN pipe id: " + pipeInfo.PipeId + " " + pipeInfo.MaximumPacketSize + " " + pipeInfo.Interval);
                                ConfigurePipePolicy(devInfo.winUsbAssociatedHandle, pipeInfo.PipeId);
                            }
                            else if (((pipeInfo.PipeType ==
                                USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                                UsbEndpointDirectionOut(pipeInfo.PipeId)))
                            {
                                System.Diagnostics.Debug.Assert(pipeInfo.MaximumPacketSize > 0, "USB Bulk OUT packet size is zero");
                                devInfo.bulkOutPipe = pipeInfo.PipeId;
                                devInfo.bulkOutPacketSize = pipeInfo.MaximumPacketSize;
                                System.Diagnostics.Debug.WriteLine("Associated Interface, Found BULK OUT pipe id: " + pipeInfo.PipeId + " " + pipeInfo.MaximumPacketSize + " " + pipeInfo.Interval);
                                ConfigurePipePolicy(devInfo.winUsbAssociatedHandle, pipeInfo.PipeId);
                                uint maxTranferSize = 0;
                                GetPipePolicy(pipeInfo.PipeId, (UInt32)POLICY_TYPE.MAXIMUM_TRANSFER_SIZE, ref maxTranferSize);
                                devInfo.maximumTransferSize = (Int32)maxTranferSize;

                                System.Diagnostics.Debug.Assert(devInfo.maximumTransferSize > 0, "USB Pipe Maximum transfer size is zero");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Associated Interface, Found: " + pipeInfo.PipeType + " " + pipeInfo.PipeId + " " + pipeInfo.MaximumPacketSize + " " + pipeInfo.Interval);
                            }
                        }
                    }
                }
                #endregion


                if (success)
                {
                    QueryDeviceSpeed();
                    rxFIFO = new Queue(RX_FIFO_SIZE);
                    if (devInfo.bulkInPipe == 0x0 || devInfo.bulkOutPipe == 0x0)
                    {
                        System.Diagnostics.Debug.WriteLine("Could not find USB bulk IN and bulk OUT pipes");
                        success = false;
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public bool Open()
        {
            return Open(SerialNumber);
        }

        public bool Open(string serialNumber)
        {
            if (!IsWindowsXpOrLater())
            {
                throw new System.InvalidOperationException("System is not Windows XP SP2 or later - no WinUSB support");
            }

            if (isOpen)
            {
                throw new System.InvalidOperationException("Port is already open");
            }


            // Use First device found if none specced
            if (string.IsNullOrEmpty(serialNumber))
            {
                // NOTE: winusb.dll is not natively present on WinXP unless driver installed
                // However, guids will not ever be enumerated if driver not installed, so call is safe
                List<string> serialNumbers = WinUsbDeviceManagement.GetSerialNumbers;
                if (serialNumbers != null && serialNumbers.Count > 0)
                    serialNumber = serialNumbers[0];
            }

            bool initalised = false;
            if (serialNumber != null)
            {
                string deviceString = WinUsbDeviceManagement.GetDeviceStringFromSerialNumber(serialNumber);
                if (!string.IsNullOrEmpty(deviceString))
                {
                    initalised = GetDeviceHandle(deviceString);

                    if (!initalised)
                    {
                        // Device may have been reset and be reenumerating, wait 1.5 seconds and try again
                        System.Threading.Thread.Sleep(1500);
                        deviceString = WinUsbDeviceManagement.GetDeviceStringFromSerialNumber(serialNumber);
                        if (!string.IsNullOrEmpty(deviceString))
                        {
                            initalised = GetDeviceHandle(deviceString);
                        }
                    }
                }
            }

            if (!initalised)
            {
                // throw new System.ArgumentException("USB device with serial number: " + ((serialNumber != null) ? serialNumber : "NULL") + " is not attached");

                System.Diagnostics.Debug.WriteLine("USB device with serial number: " + ((serialNumber != null) ? serialNumber : "NULL") + " is not attached");
                CloseDeviceHandle();

                return false;
            }


            // Initalise device
            initalised = InitializeDevice();
            if (!initalised)
            {
                System.Diagnostics.Debug.WriteLine("Device handle is valid, but device did not initalise");
                CloseDeviceHandle();

                return false;
            }

            isOpen = true;
            SerialNumber = serialNumber;

            // Setup overlapped structure for writes
            NativeOverlapped overlapped = new NativeOverlapped();
            txOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(overlapped));
            overlapped.OffsetLow = 0;
            overlapped.OffsetHigh = 0;
            overlapped.EventHandle = txEvent.SafeWaitHandle.DangerousGetHandle();
            Marshal.StructureToPtr(overlapped, txOverlapped, true);

            // Setup reader thread that waits for reads
            eventThread = new System.Threading.Thread(new System.Threading.ThreadStart(CommEventThread));
            eventThread.Priority = System.Threading.ThreadPriority.Highest;
            eventThread.Start();

            // Wait until the read pump event thread has actually started                
            threadStarted.WaitOne();

            System.Diagnostics.Debug.WriteLine("Opened USB device with serial number: " + ((serialNumber != null) ? serialNumber : "NULL"));

            return true;
        }

        public void Close()
        {
            isOpen = false;
            if (threadStarted != null)
            {
                threadStarted.Set();
            }

            if (eventThread != null)
            {
                System.Diagnostics.Debug.WriteLine("WinUsb.Close() Join");
                eventThread.Join();
                eventThread = null;
            }

            CloseDeviceHandle();



            // System API call timeouts seem to be involved winding down
            System.Threading.Thread.Sleep(100);
            System.Diagnostics.Debug.WriteLine("WinUsb.Close() done");
        }

        ///  <summary>
        ///  Is the current operating system Windows XP or later?
        ///  The WinUSB driver requires Windows XP or later.
        ///  </summary>
        /// 
        ///  <returns>
        ///  True if Windows XP or later, False if not.
        ///  </returns>
        internal Boolean IsWindowsXpOrLater()
        {
            try
            {
                OperatingSystem myEnvironment = Environment.OSVersion;

                //  Windows XP is version 5.1.
                System.Version versionXP = new System.Version(5, 1);

                if (myEnvironment.Version >= versionXP)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void DebugDisplayPipePolicy(Byte pipeId)
        {
            UInt32 value = 0;
            System.Diagnostics.Debug.WriteLine("***PipeID: " + pipeId);
            foreach (POLICY_TYPE policy in Enum.GetValues(typeof(POLICY_TYPE)))
            {
                value = 0;
                if (GetPipePolicy(pipeId, (UInt32)policy, ref value))
                {
                    System.Diagnostics.Debug.WriteLine("" + policy.ToString() + " " + value);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("" + policy.ToString() + " UNKNOWN or N/A");
                }
            }
            System.Diagnostics.Debug.WriteLine("***");
        }


        public string DebugDumpBuffer(Byte[] buffer)
        {
            if (buffer.Length < 1)
            {
                return "Empty buffer returned";
            }

            string retVal = "**Hex";
            var count = 0;
            foreach (var field in buffer)
            {
                string str = (field < 15) ? "0" : "";

                str += string.Format
                (
                    "{0:X0} ", field
                );
                retVal += str;

                count++;
                if (count > 20)
                {
                    retVal += "\n";
                    count = 0;
                }
            }

            retVal += " **ASCII";
            count = 0;
            foreach (var field in buffer)
            {
                string str = (field < 0x0f) ? "0" : "";
                if ((field > 0x20 && field < 0x7f))  // Non printable characters become space
                {
                    str += string.Format
                    (
                        "{0:0}", Convert.ToChar(field)
                    );
                }
                else str += " ";

                retVal += str;

                count++;
                if (count > 20)
                {
                    retVal += "\n";
                    count = 0;
                }
            }

            return retVal;
        }


        ///  <summary>
        ///  Gets a value that corresponds to a USB_DEVICE_SPEED. 
        ///  </summary>
        internal Boolean QueryDeviceSpeed()
        {
            UInt32 length = 1;
            Byte[] speed = new Byte[1];
            Boolean success;

            // ***
            //  winusb function 

            //  summary
            //  Get the device speed. 
            //  (Normally not required but can be nice to know.)

            //  parameters
            //  Handle returned by WinUsb_Initialize
            //  Requested information type.
            //  Number of bytes to read.
            //  Information to be returned.

            //  returns
            //  True on success.
            // ***           

            success = WinUsb_QueryDeviceInformation
            (
                devInfo.winUsbHandle,
                DEVICE_SPEED,
                ref length,
                ref speed[0]
            );

            if (success)
            {
                devInfo.devicespeed = System.Convert.ToUInt32(speed[0]);
            }

            return success;
        }



        ///  <summary>
        ///  Sets pipe policy.
        ///  Used when the value parameter is a Byte (all except PIPE_TRANSFER_TIMEOUT).
        ///  </summary>
        ///  
        ///  <param name="pipeId"> Pipe to set a policy for. </param>
        ///  <param name="policyType"> POLICY_TYPE member. </param>
        ///  <param name="value"> Policy value. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>
        ///  
        private Boolean SetPipePolicy(IntPtr InterfaceHandle, Byte pipeId, UInt32 policyType, Byte value)
        {
            Boolean success;

            try
            {
                // ***
                //  winusb function 

                //  summary
                //  sets a pipe policy 

                //  parameters
                //  handle returned by WinUsb_Initialize
                //  identifies the pipe
                //  POLICY_TYPE member.
                //  length of value in bytes
                //  value to set for the policy.

                //  returns
                //  True on success 
                // ***

                success = WinUsb_SetPipePolicy
                    (InterfaceHandle,
                    pipeId,
                    policyType,
                    1,
                    ref value);

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        ///  <summary>
        ///  Sets pipe policy.
        ///  Used when the value parameter is a UInt32 (PIPE_TRANSFER_TIMEOUT only).
        ///  </summary>
        ///  
        ///  <param name="pipeId"> Pipe to set a policy for. </param>
        ///  <param name="policyType"> POLICY_TYPE member. </param>
        ///  <param name="value"> Policy value. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>
        ///  
        private Boolean SetPipePolicy(IntPtr InterfaceHandle, Byte pipeId, UInt32 policyType, UInt32 value)
        {
            Boolean success;

            try
            {
                // ***
                //  winusb function 

                //  summary
                //  sets a pipe policy 

                //  parameters
                //  handle returned by WinUsb_Initialize
                //  identifies the pipe
                //  POLICY_TYPE member.
                //  length of value in bytes
                //  value to set for the policy.

                //  returns
                //  True on success 
                // ***
                if ((POLICY_TYPE)policyType == POLICY_TYPE.PIPE_TRANSFER_TIMEOUT)
                {
                    success = WinUsb_SetPipePolicy1
                    (
                        InterfaceHandle,
                        pipeId,
                        policyType,
                        4,
                        ref value
                    );
                }
                else
                {
                    byte bVal = 0;
                    success = WinUsb_SetPipePolicy
                    (
                        InterfaceHandle,
                        pipeId,
                        policyType,
                        1,
                        ref bVal
                    );
                    value = bVal;
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }


        private Boolean GetPipePolicy(Byte pipeId, UInt32 policyType, ref UInt32 value)
        {
            Boolean success;

            try
            {
                uint size = 4;
                success = WinUsb_GetPipePolicy
                (
                    devInfo.winUsbAssociatedHandle,
                    pipeId,
                    policyType,
                    ref size,
                    ref value
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new System.ComponentModel.Win32Exception((int)Marshal.GetLastWin32Error()).Message);
                throw;
            }

            return success;
        }


        ///  <summary>
        ///  Is the endpoint's direction IN (device to host)?
        ///  </summary>
        ///  
        ///  <param name="addr"> The endpoint address. </param>
        ///  <returns>
        ///  True if IN (device to host), False if OUT (host to device)
        ///  </returns> 
        private Boolean UsbEndpointDirectionIn(Int32 addr)
        {
            Boolean directionIn;

            try
            {
                if (((addr & 0x80) == 0x80))
                {
                    directionIn = true;
                }
                else
                {
                    directionIn = false;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            return directionIn;
        }


        ///  <summary>
        ///  Is the endpoint's direction OUT (host to device)?
        ///  </summary>
        ///  
        ///  <param name="addr"> The endpoint address. </param>
        ///  
        ///  <returns>
        ///  True if OUT (host to device, False if IN (device to host)
        ///  </returns>
        private Boolean UsbEndpointDirectionOut(Int32 addr)
        {
            Boolean directionOut;

            try
            {
                if (((addr & 0x80) == 0))
                {
                    directionOut = true;
                }
                else
                {
                    directionOut = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            return directionOut;
        }


        public void Dispose()
        {
            if (isOpen)
                Close();
        }
    }
}
