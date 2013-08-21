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
//     file="DownloadForm.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       DownloadForm.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Main form of the log downloader.
*/
////////////////////////////////////////////////////////////

namespace BioHarnessLogDownloader
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Zephyr.IO.Ports;
    using Zephyr.Logging;
    using Zephyr.Threading;

    /// <summary>
    /// Main form of the log downloader.
    /// </summary>
    [CLSCompliant(true)]
    public partial class DownloadForm : Form
    {
        /// <summary>
        /// Zephyr Part Number - for inclusion in the title bar. Update with each new rev.
        /// </summary>
        private const string PartNumber = "9500.0078.V1b";

        /// <summary>
        /// Constant used in interpretation of accelerometer data
        /// </summary>
        private const double ZeroGoffset = 512.0;

        /// <summary>
        /// Constant used in interpretation of accelerometer data
        /// </summary>
        private const double Conversion = 19.456;

        /// <summary>
        /// culture settings used for all interpretation and file writing, so international machines bechave like NZ machines.
        /// </summary>
        private static IFormatProvider culture = new CultureInfo("en-NZ", false);

        /// <summary>
        /// Contains a list of serial number strings (keys) and related com port strings (values)
        /// </summary>
        private Hashtable deviceInformation;

        /// <summary>
        /// Contains the most recent directory of sessions read from a device.
        /// </summary>
        private SessionDirectory directory;

        /// <summary>
        /// Instance of a processing dialog class - which is a form with a fake progress bar
        /// displaied when we are downloading so it looks like we are doing something.
        /// </summary>
        private ProcessingDialog fakeProgress;

        /// <summary>
        /// Initializes a new instance of the DownloadForm class.
        /// </summary>
        public DownloadForm()
        {
            this.InitializeComponent();
            this.deviceInformation = new Hashtable();
            this.InitialiseLogListView();
            this.InitialiseTypeSelectDropdown();
            this.Text += PartNumber + " (" + Application.ProductVersion + ")";
            /* Register to receive add / remove USB hardware notifications */
            Zephyr.IO.USB.WinUsbDeviceManagement.RegisterForDeviceNotifications(this.Handle);
        }

        /// <summary>
        /// Monitor windows messaging que for hardware monitoring
        /// </summary>
        /// <param name="msg">Windows message</param>
        protected override void WndProc(ref Message msg)
        {
            // Catch WM_DEVICECHANGE for add / remove hardware events
            Zephyr.IO.USB.WinUsbDeviceManagement.OnDeviceChange(msg);

            // Call base class function
            base.WndProc(ref msg);
        }

        /// <summary>
        /// Initialises the log list view box
        /// </summary>
        private void InitialiseLogListView()
        {
            int usableWidth = this.logListView.Width - 30;
            ColumnHeader sessionHeader = new ColumnHeader();
            sessionHeader.Text = "Name";
            sessionHeader.Width = (int)((2.0D / 6.0D) * usableWidth);
            this.logListView.Columns.Add(sessionHeader);

            ColumnHeader lengthHeader = new ColumnHeader();
            lengthHeader.Text = "Length";
            lengthHeader.Width = (int)((1.0D / 6.0D) * usableWidth);
            this.logListView.Columns.Add(lengthHeader);

            ColumnHeader typeHeader = new ColumnHeader();
            typeHeader.Text = "Type";
            typeHeader.Width = (int)((1.0D / 6.0D) * usableWidth);
            this.logListView.Columns.Add(typeHeader);

            ColumnHeader dateHeader = new ColumnHeader();
            dateHeader.Text = "Date Created";
            dateHeader.Width = (int)((2.0D / 6.0D) * usableWidth);
            this.logListView.Columns.Add(dateHeader);

            this.logListView.ShowItemToolTips = true;
        }

        /// <summary>
        /// Initialise the save-as type select combobox
        /// </summary>
        private void InitialiseTypeSelectDropdown()
        {
            this.saveAsTypeComboBox.Items.Add("All Formats");
            this.saveAsTypeComboBox.Items.Add("CSV Format");
            this.saveAsTypeComboBox.Items.Add("DaDISP Format");
            this.saveAsTypeComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// This method refreshes the list of detected devices displayed in the comboBoxDeviceList.
        /// Any previously selected index information is lost.
        /// </summary>
        private void CreateDeviceList()
        {
            var list = SerialPortDevice.GetPorts("Zephyr");

            // Must clear the arrays, and the combobox list, or rescans would result in 
            // multiple entries.
            this.deviceSelectionComboBox.Items.Clear();

            this.deviceInformation.Clear();

            // populate combobox drop down list
            foreach (SerialPortInfo serial in list)
            {
                // if, and only if, serial indicates a BioHarness, add to the dropdown.
                if (serial.SerialName.ToUpper(CultureInfo.InvariantCulture).Contains("ZBH") || serial.SerialName.ToUpper(CultureInfo.InvariantCulture).Contains("BHT"))
                {
                    this.deviceSelectionComboBox.Items.Add(serial.SerialName.ToUpper(CultureInfo.InvariantCulture));
                    this.deviceInformation.Add(serial.SerialName.ToUpper(CultureInfo.InvariantCulture), serial.Name.ToUpper(CultureInfo.InvariantCulture));
                }
            }

            foreach (string serialNumber in Zephyr.IO.USB.WinUsbDeviceManagement.GetSerialNumbers)
            {                
                // NOTE: Could filter by serial number prefix here 
                this.deviceSelectionComboBox.Items.Add(serialNumber);
                this.deviceInformation.Add(serialNumber, serialNumber);
            }
        }

        /// <summary>
        /// Auto populate the drop down list
        /// </summary>
        /// <param name="sender">Sender class - in this case the dropdown box</param>
        /// <param name="e">Event Args</param>
        private void DeviceSelectionComboBoxDropDown(object sender, EventArgs e)
        {
            this.CreateDeviceList();
        }

        /// <summary>
        /// Get log directory for selected device.
        /// </summary>
        /// <param name="sender">Sender class - in this case the dropdown box</param>
        /// <param name="e">Event Args</param>
        private void DeviceSelectionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.deviceSelectionComboBox.SelectedItem == null)
            {
                return;
            }

            ListViewItem item;
            string type;
            int imageIndex;
            string sessionDuration;

            this.logListView.Items.Clear();
            this.selectedLogRecordTextBox.AutoCompleteCustomSource.Clear();

            if (this.deviceInformation.ContainsKey(this.deviceSelectionComboBox.SelectedItem as string))
            {
                this.directory = BioHarnessCommunications.SyncGetSessionDirectory(this.deviceInformation[this.deviceSelectionComboBox.SelectedItem as string] as string);

                switch (this.directory.FormatVersion)
                {
                    default:
                        type = string.Format(culture, "Unsupported - {0}", this.directory.FormatVersion);
                        imageIndex = 0;
                        break;
                    case "0003":
                        type = "Standard Log";
                        imageIndex = 1;
                        break;
                    case "0004":
                        type = "ECG Log";
                        imageIndex = 2;
                        break;
                    case "0005":
                        type = "Accelerometer Log";
                        imageIndex = 3;
                        break;
                    case "0006":
                        type = "BioRICS custom Log";
                        imageIndex = 4;
                        break;
                    case "0007":
                        type = "Accelerometer Magnitude Log";
                        imageIndex = 3;
                        break;
                    case "0008":
                        type = "Accelerometer Magnitude Log (100Hz)";
                        imageIndex = 3;
                        break;
                }

                for (int recordNumber = 0; recordNumber < this.directory.SessionData.Count; recordNumber++)
                {
                    if (this.directory.SessionData[recordNumber].Duration.TotalMinutes < 60)
                    {
                        sessionDuration = ((int)this.directory.SessionData[recordNumber].Duration.Minutes).ToString(culture).PadLeft(2, '0') + "m" +
                        ((int)this.directory.SessionData[recordNumber].Duration.Seconds).ToString(culture).PadLeft(2, '0') + "s";
                    }
                    else if (this.directory.SessionData[recordNumber].Duration.TotalHours < 24)
                    {
                        sessionDuration = ((int)this.directory.SessionData[recordNumber].Duration.Hours).ToString(culture).PadLeft(2, '0') + "h" +
                        ((int)this.directory.SessionData[recordNumber].Duration.Minutes).ToString(culture).PadLeft(2, '0') + "m";
                    }
                    else
                    {
                        sessionDuration = ((int)this.directory.SessionData[recordNumber].Duration.TotalDays).ToString(culture).PadLeft(1, '0') + "d" +
                            ((int)this.directory.SessionData[recordNumber].Duration.Hours).ToString(culture).PadLeft(2, '0') + "h";
                    }

                    item = new ListViewItem(new string[] 
                                                { 
                                                    string.Format(culture, "Record {0}", this.logListView.Items.Count + 1),
                                                    sessionDuration, 
                                                    type, 
                                                    this.directory.SessionData[recordNumber].Timestamp.ToString(CultureInfo.CurrentCulture),
                                                    recordNumber.ToString(CultureInfo.InvariantCulture)
                                                });
                    item.ToolTipText = string.Format(culture, "Type:{0} Duration:{1} Epoch:{2}", type, sessionDuration, this.directory.SessionData[recordNumber].Period.ToString());
                    item.ImageIndex = imageIndex;
                    this.logListView.Items.Add(item);

                    /* Now do the autocomplete for the text box */
                    this.selectedLogRecordTextBox.AutoCompleteCustomSource.Add(string.Format(culture, "Record {0}", this.logListView.Items.Count));
                }
            }
        }

        /// <summary>
        /// Updates the selected record when an item in the listview is changed
        /// </summary>
        /// <param name="sender">The listview box</param>
        /// <param name="e">Dummy event args</param>
        private void LogListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.logListView.SelectedItems.Count > 0)
            {
                this.selectedLogRecordTextBox.Text = this.logListView.SelectedItems[0].Text;
            }
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        /// <param name="sender">The save button</param>
        /// <param name="e">Dummy event args</param>
        private void SaveButtonClick(object sender, EventArgs e)
        {
            this.SaveLog();
        }

        /// <summary>
        /// Starts (or tries to) the download process if the user presses return while the text box has focus
        /// </summary>
        /// <param name="sender">The text box object</param>
        /// <param name="e">PreviewKeyDownEventArgs indicating which key is pressed</param>
        private void SelectedLogRecordTextBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                this.SaveLog();
            }
        }

        /// <summary>
        /// Starts the download process for the selected item (if there is one) when the control is double clicked.
        /// Not ideal, would have prefered to have had one that worked on double clicking the item itself, but we take what we are given.
        /// </summary>
        /// <param name="sender">The sending object (the listview)</param>
        /// <param name="e">Dummy eventargs</param>
        private void LogListViewDoubleClick(object sender, EventArgs e)
        {
            if (this.logListView.SelectedItems.Count > 0)
            {
                this.SaveLog();
            }
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        private void SaveLog()
        {
            Session logToGet = null;

            foreach (ListViewItem item in this.logListView.Items)
            {
                if (this.selectedLogRecordTextBox.Text.ToUpperInvariant() == item.Text.ToUpperInvariant())
                {
                    logToGet = this.directory.SessionData[int.Parse(item.SubItems[4].Text, CultureInfo.InvariantCulture)];
                }
            }

            if (logToGet == null)
            {
                MessageBox.Show("Specified record not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.saveFolderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.DownloadLogData(this.deviceInformation[this.deviceSelectionComboBox.SelectedItem as string] as string, logToGet);
            this.fakeProgress = new ProcessingDialog(this);
            this.fakeProgress.ShowDialog();
        }

        /// <summary>
        /// The other thread method used to download the data
        /// </summary>
        /// <param name="comport">The comport to communicate on</param>
        /// <param name="log">The particular log session to download</param>
        /// <returns>A reference to the future</returns>
        private Future DownloadLogData(string comport, Session log)
        {
            byte[] logData = null;

            var future = Future.Create(() =>
                {
                    try
                    {
                        logData = BioHarnessCommunications.SyncLoadData(comport, log);
                    }
                    catch (Exception ex)
                    {
                        logData = null;
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show(string.Format(culture, "Unable to download log: {0}", ex.Message), "Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }));
                    }
                    finally
                    {
                        this.BeginInvoke((MethodInvoker)(() =>
                            {
                                fakeProgress.CloseForReal();
                            }));
                    }

                    if (logData != null)
                    {
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            InterpretLogs(log, logData);
                        }));
                    }
                });
            return future;
        }

        /// <summary>
        /// Determines how the downloaded binary should be processed
        /// </summary>
        /// <param name="log">The session the data relates to</param>
        /// <param name="logData">The binary log data</param>
        private void InterpretLogs(Session log, byte[] logData)
        {
            switch (this.directory.FormatVersion)
            {
                case "0003":
                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (1 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateStandardCSVFiles(log, logData);
                    }

                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (2 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateStandardDaDispFiles(log, logData);
                    }

                    break;

                case "0004":
                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (1 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateECGCSVFiles(log, logData);
                    }

                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (2 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateECGDaDispFiles(log, logData);
                    }

                    break;

                case "0005":
                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (1 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateAccelCSVFiles(log, logData);
                    }

                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (2 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateAccelDaDispFiles(log, logData);
                    }

                    break;

                case "0006":
                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (1 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateBioRICSCSVFiles(log, logData);
                    }

                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (2 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateBioRICSDaDispFiles(log, logData);
                    }

                    break;

                case "0007":
                case "0008": /* Intentional Fallthrough */
                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (1 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateAccelMagnitudeCSVFiles(log, logData);
                    }

                    if ((0 == this.saveAsTypeComboBox.SelectedIndex) ||
                        (2 == this.saveAsTypeComboBox.SelectedIndex))
                    {
                        this.CreateAccelMagnitudeDaDispFiles(log, logData);
                    }

                    break;
                
                default:
                    throw new NotImplementedException("Log Format Not Implimented.");
                ////break; /* Unreachable */
            }

            MessageBox.Show("Download Complete", "Information", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Standard log format export data to CSV files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateStandardCSVFiles(Session session, byte[] data)
        {
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;
            int rawBreathingWaveform;
            float intervalRR;
            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            var timestamp = session.Timestamp;
            using (StreamWriter generalFile = new StreamWriter(filePathAndPrefix + "_General.csv"))
            {
                using (StreamWriter breathingAndRRFile = new StreamWriter(filePathAndPrefix + "_BR_RR.csv"))
                {
                    generalFile.WriteLine("Timestamp,HR,BR,Temp,Posture,Activity,Acceleration,Battery,BRAmplitude,ECGAmplitude,ECGNoise,XMin,XPeak,YMin,YPeak,ZMin,ZPeak");
                    breathingAndRRFile.WriteLine("Timestamp,BR,RtoR");

                    int offset = 0;
                    while (offset < data.Length)
                    {
                        // Generate the General CSV string
                        heartRate = BitConverter.ToInt16(data, offset);
                        breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                        temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                        posture = BitConverter.ToInt16(data, offset + 6);
                        activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                        acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                        battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                        breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                        ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                        ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                        axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                        axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                        axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                        axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                        axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                        axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;
                        generalFile.WriteLine(
                            string.Format(
                                culture,
                                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                                timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                heartRate.ToString(culture),
                                breathingRate.ToString("F1", culture),
                                temperature.ToString("F1", culture),
                                posture.ToString(culture),
                                activity.ToString("F2", culture),
                                acceleration.ToString("F2", culture),
                                battery.ToString("F3", culture),
                                breathingAmplitude.ToString("F3", culture),
                                ecgAmplitude.ToString("F6", culture),
                                ecgNoise.ToString("F6", culture),
                                axisXMin.ToString("F2", culture),
                                axisXPeak.ToString("F2", culture),
                                axisYMin.ToString("F2", culture),
                                axisYPeak.ToString("F2", culture),
                                axisZMin.ToString("F2", culture),
                                axisZPeak.ToString("F2", culture)));

                        /* Generate BR/RR CSV string */
                        var brrrTimestamp = timestamp;
                        for (int sample = 0; sample < 36; sample += 2)
                        {
                            rawBreathingWaveform = BitConverter.ToInt16(data, offset + 36 + sample);
                            intervalRR = BitConverter.ToInt16(data, offset + 72 + sample) / 1000f;
                            /* Check if sample is marked as invalid */
                            if (rawBreathingWaveform != 32767 && intervalRR != 32.767)
                            {
                                breathingAndRRFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1},{2}",
                                        brrrTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        rawBreathingWaveform.ToString(culture),
                                        intervalRR.ToString(culture)));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }
                        }

                        offset += session.Channels * 2;
                        timestamp += TimeSpan.FromMilliseconds(session.Period);
                    }
                }
            }
        }

        /// <summary>
        /// Standard log format export data to DaDisp files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateStandardDaDispFiles(Session session, byte[] data)
        {
            // Create the header files
            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            using (StreamWriter generalHeaderFile = File.CreateText(filePathAndPrefix + "_General.hed"))
            {
                generalHeaderFile.WriteLine("DATASET\tZephyr");
                generalHeaderFile.WriteLine("VERSION\t1.0");
                generalHeaderFile.WriteLine("SERIES\tHeartRate,BreathingRate,SkinTemperature,Posture,VectorMagnitude,PeakAcceleration,BatteryVoltage,BreathingWaveAmplitude,EcgAmplitude,EcgNoise,XMin,XMax,YMin,YMax,ZMin,ZMax");
                generalHeaderFile.WriteLine("RATE\t0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206");
                generalHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time");
                generalHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units");
                generalHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                generalHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                generalHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                generalHeaderFile.WriteLine("NUM_SERIES\t16");
            }

            using (StreamWriter breathingAndRRheaderFile = File.CreateText(filePathAndPrefix + "_BR_RR.hed"))
            {
                breathingAndRRheaderFile.WriteLine("DATASET\tZephyr");
                breathingAndRRheaderFile.WriteLine("VERSION\t1.0");
                breathingAndRRheaderFile.WriteLine("SERIES\tBreathingData,RtoRData");
                breathingAndRRheaderFile.WriteLine("RATE\t17.85714");
                breathingAndRRheaderFile.WriteLine("HORIZ_UNITS\tTime");
                breathingAndRRheaderFile.WriteLine("VERT_UNITS\tNo Units");
                breathingAndRRheaderFile.WriteLine("FILE_TYPE\tSINTEGER");
                breathingAndRRheaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                breathingAndRRheaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                breathingAndRRheaderFile.WriteLine("NUM_SERIES\t2");
            }

            // Create the data files
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int rawBreathingWaveform;
            float intervalRR;

            var timestamp = session.Timestamp;
            var brrrTimestamp = timestamp;
            using (BinaryWriter generalBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_General.dat")))
            {
                using (BinaryWriter breathingAndRRBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_BR_RR.dat")))
                {
                    int offset = 0;
                    while (offset < data.Length)
                    {
                        // Generate the General binary values
                        heartRate = BitConverter.ToInt16(data, offset);
                        breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                        temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                        posture = BitConverter.ToInt16(data, offset + 6);
                        activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                        acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                        battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                        breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                        ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                        ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                        axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                        axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                        axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                        axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                        axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                        axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;

                        generalBinFile.Write(Convert.ToSingle(heartRate));
                        generalBinFile.Write(Convert.ToSingle(breathingRate));
                        generalBinFile.Write(Convert.ToSingle(temperature));
                        generalBinFile.Write(Convert.ToSingle(posture));
                        generalBinFile.Write(Convert.ToSingle(activity));
                        generalBinFile.Write(Convert.ToSingle(acceleration));
                        generalBinFile.Write(Convert.ToSingle(battery));
                        generalBinFile.Write(Convert.ToSingle(breathingAmplitude));
                        generalBinFile.Write(Convert.ToSingle(ecgAmplitude));
                        generalBinFile.Write(Convert.ToSingle(ecgNoise));
                        generalBinFile.Write(Convert.ToSingle(axisXMin));
                        generalBinFile.Write(Convert.ToSingle(axisXPeak));
                        generalBinFile.Write(Convert.ToSingle(axisYMin));
                        generalBinFile.Write(Convert.ToSingle(axisYPeak));
                        generalBinFile.Write(Convert.ToSingle(axisZMin));
                        generalBinFile.Write(Convert.ToSingle(axisZPeak));

                        /* Generate BR/RR binaries values */
                        for (int sample = 0; sample < 36; sample += 2)
                        {
                            rawBreathingWaveform = BitConverter.ToInt16(data, offset + 36 + sample);
                            intervalRR = BitConverter.ToInt16(data, offset + 72 + sample);
                            /* Check if sample is marked as invalid */
                            if (rawBreathingWaveform != 32767 && intervalRR != 32767)
                            {
                                breathingAndRRBinFile.Write(Convert.ToInt16(rawBreathingWaveform));
                                breathingAndRRBinFile.Write(Convert.ToInt16(intervalRR));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }
                        }

                        offset += session.Channels * 2;
                        timestamp += TimeSpan.FromMilliseconds(session.Period);
                    }
                }
            }
        }

        /// <summary>
        /// ECG log format export data to CSV files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateECGCSVFiles(Session session, byte[] data)
        {
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int rawBreathingWaveform;
            float intervalRR;

            int ecg1;
            int ecg2;

            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            var timestamp = session.Timestamp;
            using (StreamWriter generalFile = new StreamWriter(filePathAndPrefix + "_General.csv"))
            {
                using (StreamWriter breathingAndRRFile = new StreamWriter(filePathAndPrefix + "_BR_RR.csv"))
                {
                    using (StreamWriter ecgFile = new StreamWriter(filePathAndPrefix + "_ECG.csv"))
                    {
                        generalFile.WriteLine("Timestamp,HR,BR,Temp,Posture,Activity,Acceleration,Battery,BRAmplitude,ECGAmplitude,ECGNoise,XMin,XPeak,YMin,YPeak,ZMin,ZPeak");
                        breathingAndRRFile.WriteLine("Timestamp,BR,RtoR");
                        ecgFile.WriteLine("Timestamp,ECG");

                        int offset = 0;
                        while (offset < data.Length)
                        {
                            // Generate the General CSV string
                            heartRate = BitConverter.ToInt16(data, offset);
                            breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                            temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                            posture = BitConverter.ToInt16(data, offset + 6);
                            activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                            acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                            battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                            breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                            ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                            ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                            axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                            axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                            axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                            axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                            axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                            axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;
                            generalFile.WriteLine(
                                string.Format(
                                    culture,
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                                    timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                    heartRate.ToString(culture),
                                    breathingRate.ToString("F1", culture),
                                    temperature.ToString("F1", culture),
                                    posture.ToString(culture),
                                    activity.ToString("F2", culture),
                                    acceleration.ToString("F2", culture),
                                    battery.ToString("F3", culture),
                                    breathingAmplitude.ToString("F3", culture),
                                    ecgAmplitude.ToString("F6", culture),
                                    ecgNoise.ToString("F6", culture),
                                    axisXMin.ToString("F2", culture),
                                    axisXPeak.ToString("F2", culture),
                                    axisYMin.ToString("F2", culture),
                                    axisYPeak.ToString("F2", culture),
                                    axisZMin.ToString("F2", culture),
                                    axisZPeak.ToString("F2", culture)));

                            // Generate BR/RR CSV string
                            var brrrTimestamp = timestamp;
                            for (int sample = 0; sample < 36; sample += 2)
                            {
                                rawBreathingWaveform = BitConverter.ToInt16(data, offset + 36 + sample);
                                intervalRR = BitConverter.ToInt16(data, offset + 72 + sample) / 1000f;
                                if ((rawBreathingWaveform == 32767) && (intervalRR == 32.767f))
                                {
                                    continue;
                                }

                                breathingAndRRFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1},{2}",
                                        brrrTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        rawBreathingWaveform.ToString(culture),
                                        intervalRR.ToString(culture)));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }

                            // Generate ECG CSV string
                            var ecgTimestamp = timestamp;
                            for (int sample = 0; sample < 378; sample += 3)
                            {
                                if (session.Period < ((sample + 3) / 1.5) * 4)
                                {
                                    break;
                                }

                                ecg1 = data[offset + 128 + sample] + ((data[offset + 128 + sample + 1] & 0x0f) << 8);
                                ecg2 = ((data[offset + 128 + sample + 1] & 0xf0) >> 4) + (data[offset + 128 + sample + 2] << 4);
                                ecgFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1}",
                                        ecgTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        ecg1.ToString(culture)));
                                ecgTimestamp += TimeSpan.FromMilliseconds(4);
                                ecgFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1}",
                                        ecgTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        ecg2.ToString(culture)));
                                ecgTimestamp += TimeSpan.FromMilliseconds(4);
                            }

                            offset += session.Channels * 2;
                            timestamp += TimeSpan.FromMilliseconds(session.Period);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ECG log format export data to DaDisp files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateECGDaDispFiles(Session session, byte[] data)
        {
            // Create the header files
            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            using (StreamWriter generalHeaderFile = File.CreateText(filePathAndPrefix + "_General.hed"))
            {
                generalHeaderFile.WriteLine("DATASET\tZephyr");
                generalHeaderFile.WriteLine("VERSION\t1.0");
                generalHeaderFile.WriteLine("SERIES\tHeartRate,BreathingRate,SkinTemperature,Posture,VectorMagnitude,PeakAcceleration,BatteryVoltage,BreathingWaveAmplitude,EcgAmplitude,EcgNoise,XMin,XMax,YMin,YMax,ZMin,ZMax");
                generalHeaderFile.WriteLine("RATE\t0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206");
                generalHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time");
                generalHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units");
                generalHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                generalHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                generalHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                generalHeaderFile.WriteLine("NUM_SERIES\t16");
            }

            using (StreamWriter ecgHeaderFile = File.CreateText(filePathAndPrefix + "_ECG.hed"))
            {
                ecgHeaderFile.WriteLine("DATASET\tZephyr");
                ecgHeaderFile.WriteLine("VERSION\t1.0");
                ecgHeaderFile.WriteLine("SERIES\tECG");
                ecgHeaderFile.WriteLine("RATE\t250");
                ecgHeaderFile.WriteLine("HORIZ_UNITS\tTime");
                ecgHeaderFile.WriteLine("VERT_UNITS\tNo Units");
                ecgHeaderFile.WriteLine("FILE_TYPE\tSINTEGER");
                ecgHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                ecgHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                ecgHeaderFile.WriteLine("NUM_SERIES\t1");
            }

            using (StreamWriter breathingAndRRheaderFile = File.CreateText(filePathAndPrefix + "_BR_RR.hed"))
            {
                breathingAndRRheaderFile.WriteLine("DATASET\tZephyr");
                breathingAndRRheaderFile.WriteLine("VERSION\t1.0");
                breathingAndRRheaderFile.WriteLine("SERIES\tBreathingData,RtoRData");
                breathingAndRRheaderFile.WriteLine("RATE\t17.85714");
                breathingAndRRheaderFile.WriteLine("HORIZ_UNITS\tTime");
                breathingAndRRheaderFile.WriteLine("VERT_UNITS\tNo Units");
                breathingAndRRheaderFile.WriteLine("FILE_TYPE\tSINTEGER");
                breathingAndRRheaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                breathingAndRRheaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                breathingAndRRheaderFile.WriteLine("NUM_SERIES\t2");
            }

            // Create the data files
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int rawBreathingWaveform;
            float intervalRR;

            int ecg1;
            int ecg2;

            var timestamp = session.Timestamp;
            using (BinaryWriter generalBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_General.dat")))
            {
                using (BinaryWriter ecgBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_ECG.dat")))
                {
                    using (BinaryWriter breathingAndRRBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_BR_RR.dat")))
                    {
                        int offset = 0;

                        // NOTE: session.Period is not necessarally 1000 (can be 1008 et al).
                        double ec = session.Duration.TotalSeconds / ((double)session.Period / 1000);
                        int elementCount = (int)ec;
                        foreach (var index in Enumerable.Range(0, elementCount))
                        {
                            // Generate the General binary values
                            heartRate = BitConverter.ToInt16(data, offset);
                            breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                            temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                            posture = BitConverter.ToInt16(data, offset + 6);
                            activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                            acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                            battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                            breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                            ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                            ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                            axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                            axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                            axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                            axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                            axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                            axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;

                            generalBinFile.Write(Convert.ToSingle(heartRate));
                            generalBinFile.Write(Convert.ToSingle(breathingRate));
                            generalBinFile.Write(Convert.ToSingle(temperature));
                            generalBinFile.Write(Convert.ToSingle(posture));
                            generalBinFile.Write(Convert.ToSingle(activity));
                            generalBinFile.Write(Convert.ToSingle(acceleration));
                            generalBinFile.Write(Convert.ToSingle(battery));
                            generalBinFile.Write(Convert.ToSingle(breathingAmplitude));
                            generalBinFile.Write(Convert.ToSingle(ecgAmplitude));
                            generalBinFile.Write(Convert.ToSingle(ecgNoise));
                            generalBinFile.Write(Convert.ToSingle(axisXMin));
                            generalBinFile.Write(Convert.ToSingle(axisXPeak));
                            generalBinFile.Write(Convert.ToSingle(axisYMin));
                            generalBinFile.Write(Convert.ToSingle(axisYPeak));
                            generalBinFile.Write(Convert.ToSingle(axisZMin));
                            generalBinFile.Write(Convert.ToSingle(axisZPeak));

                            // Generate BR/RR binaries values
                            var brrrTimestamp = timestamp;
                            for (int sample = 0; sample < 36; sample += 2)
                            {
                                rawBreathingWaveform = BitConverter.ToInt16(data, offset + 36 + sample);
                                intervalRR = BitConverter.ToInt16(data, offset + 72 + sample);
                                if ((rawBreathingWaveform == 32767) && (intervalRR == 32767))
                                {
                                    continue;
                                }

                                breathingAndRRBinFile.Write(Convert.ToInt16(rawBreathingWaveform));
                                breathingAndRRBinFile.Write(Convert.ToInt16(intervalRR));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }

                            // Generate ECG binary values
                            var ecgTimestamp = timestamp;
                            for (int sample = 0; sample < 378; sample += 3)
                            {
                                if (session.Period < ((sample + 3) / 1.5) * 4)
                                {
                                    break;
                                }

                                ecg1 = data[offset + 128 + sample] + ((data[offset + 128 + sample + 1] & 0x0f) << 8);
                                ecg2 = ((data[offset + 128 + sample + 1] & 0xf0) >> 4) + (data[offset + 128 + sample + 2] << 4);
                                ecgBinFile.Write(Convert.ToInt16(ecg1));
                                ecgTimestamp += TimeSpan.FromMilliseconds(4);
                                ecgBinFile.Write(Convert.ToInt16(ecg2));
                                ecgTimestamp += TimeSpan.FromMilliseconds(4);
                            }

                            offset += session.Channels * 2;
                            timestamp += TimeSpan.FromMilliseconds(session.Period);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Accelerometer log format export data to CSV files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateAccelCSVFiles(Session session, byte[] data)
        {
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int[] accel = new int[3];

            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            var timestamp = session.Timestamp;
            using (StreamWriter generalFile = new StreamWriter(filePathAndPrefix + "_General.csv"))
            {
                using (StreamWriter accelerometerFile = new StreamWriter(filePathAndPrefix + "_ACCEL.csv"))
                {
                    generalFile.WriteLine("Timestamp,HR,BR,Temp,Posture,Activity,Acceleration,Battery,BRAmplitude,ECGAmplitude,ECGNoise,XMin,XPeak,YMin,YPeak,ZMin,ZPeak");
                    accelerometerFile.WriteLine("Timestamp,Accel_x,Accel_y,Accel_z");

                    int offset = 0;
                    while (offset < data.Length)
                    {
                        // Generate the General CSV string
                        heartRate = BitConverter.ToInt16(data, offset);
                        breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                        temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                        posture = BitConverter.ToInt16(data, offset + 6);
                        activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                        acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                        battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                        breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                        ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                        ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                        axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                        axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                        axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                        axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                        axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                        axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;
                        generalFile.WriteLine(
                            string.Format(
                                culture,
                                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                                timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                heartRate.ToString(culture),
                                breathingRate.ToString("F1", culture),
                                temperature.ToString("F1", culture),
                                posture.ToString(culture),
                                activity.ToString("F2", culture),
                                acceleration.ToString("F2", culture),
                                battery.ToString("F3", culture),
                                breathingAmplitude.ToString("F3", culture),
                                ecgAmplitude.ToString("F6", culture),
                                ecgNoise.ToString("F6", culture),
                                axisXMin.ToString("F2", culture),
                                axisXPeak.ToString("F2", culture),
                                axisYMin.ToString("F2", culture),
                                axisYPeak.ToString("F2", culture),
                                axisZMin.ToString("F2", culture),
                                axisZPeak.ToString("F2", culture)));

                        // Generate Accel CSV string
                        var accelTimestamp = timestamp;
                        for (int sample = 0; sample < 126; sample++)
                        {
                            if (session.Period < (sample + 1) * 8)
                            {
                                break;
                            }

                            /* Work out which byte to packing starts in */
                            int sampleOffset = (30 * sample) / 8;

                            /* for all three accelerometer axes in each sample */
                            for (int accelChannel = 0; accelChannel < 3; accelChannel++)
                            {
                                /* Figure out how many bits have already been used in the current byte */
                                int usedBits = ((30 * sample) + (accelChannel * 10)) % 8;

                                /* Value read from the queue, add to the packet (bit packed) */
                                switch (usedBits)
                                {
                                    case 0:
                                    default:
                                        accel[accelChannel] = data[offset + 36 + sampleOffset] + ((data[offset + 36 + sampleOffset + 1] & 0x03) << 8);
                                        sampleOffset++;
                                        break;
                                    case 2:
                                        accel[accelChannel] = ((data[offset + 36 + sampleOffset] & 0xFC) >> 2) + ((data[offset + 36 + sampleOffset + 1] & 0x0F) << 6);
                                        sampleOffset++;
                                        break;
                                    case 4:
                                        accel[accelChannel] = ((data[offset + 36 + sampleOffset] & 0xF0) >> 4) + ((data[offset + 36 + sampleOffset + 1] & 0x3F) << 4);
                                        sampleOffset++;
                                        break;
                                    case 6:
                                        accel[accelChannel] = ((data[offset + 36 + sampleOffset] & 0xC0) >> 6) + (data[offset + 36 + sampleOffset + 1] << 2);
                                        sampleOffset++;
                                        sampleOffset++;
                                        break;
                                }
                            }

                            accelerometerFile.WriteLine(
                                string.Format(
                                    culture,
                                    "{0},{1},{2},{3}",
                                    accelTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                    ((accel[0] - ZeroGoffset) / Conversion).ToString(culture),
                                    ((accel[1] - ZeroGoffset) / Conversion).ToString(culture),
                                    ((accel[2] - ZeroGoffset) / Conversion).ToString(culture)));
                            accelTimestamp += TimeSpan.FromMilliseconds(8);
                        }

                        offset += session.Channels * 2;
                        timestamp += TimeSpan.FromMilliseconds(session.Period);
                    }
                }
            }
        }

        /// <summary>
        /// Accelerometer log format export data to DaDisp files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateAccelDaDispFiles(Session session, byte[] data)
        {
            // Create the header files
            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            using (StreamWriter generalDataHeaderFile = File.CreateText(filePathAndPrefix + "_General.hed"))
            {
                generalDataHeaderFile.WriteLine("DATASET\tZephyr");
                generalDataHeaderFile.WriteLine("VERSION\t1.0");
                generalDataHeaderFile.WriteLine("SERIES\tHeartRate,BreathingRate,SkinTemperature,Posture,VectorMagnitude,PeakAcceleration,BatteryVoltage,BreathingWaveAmplitude,EcgAmplitude,EcgNoise,XMin,XMax,YMin,YMax,ZMin,ZMax");
                generalDataHeaderFile.WriteLine("RATE\t0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206");
                generalDataHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time");
                generalDataHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units");
                generalDataHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                generalDataHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                generalDataHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                generalDataHeaderFile.WriteLine("NUM_SERIES\t16");
            }

            using (StreamWriter accelerometerDataHeaderFile = File.CreateText(filePathAndPrefix + "_ACCEL.hed"))
            {
                accelerometerDataHeaderFile.WriteLine("DATASET\tZephyr");
                accelerometerDataHeaderFile.WriteLine("VERSION\t1.0");
                accelerometerDataHeaderFile.WriteLine("SERIES\tACCEL_X,ACCEL_Y,ACCEL_Z");
                accelerometerDataHeaderFile.WriteLine("RATE\t125.0,125.0,125.0");
                accelerometerDataHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time");
                accelerometerDataHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units");
                accelerometerDataHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                accelerometerDataHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                accelerometerDataHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                accelerometerDataHeaderFile.WriteLine("NUM_SERIES\t3");
            }

            // Create the data files
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int[] accel = new int[3];

            var timestamp = session.Timestamp;
            using (BinaryWriter bw = new BinaryWriter(File.Create(filePathAndPrefix + "_General.dat")))
            {
                using (BinaryWriter accelw = new BinaryWriter(File.Create(filePathAndPrefix + "_ACCEL.dat")))
                {
                    int offset = 0;

                    // NOTE: session.Period is not necessarally 1000 (can be 1008 et al).
                    double ec = session.Duration.TotalSeconds / ((double)session.Period / 1000);
                    int elementCount = (int)ec;
                    foreach (var index in Enumerable.Range(0, elementCount))
                    {
                        // Generate the General binary values
                        heartRate = BitConverter.ToInt16(data, offset);
                        breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                        temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                        posture = BitConverter.ToInt16(data, offset + 6);
                        activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                        acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                        battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                        breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                        ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                        ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                        axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                        axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                        axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                        axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                        axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                        axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;

                        bw.Write(Convert.ToSingle(heartRate));
                        bw.Write(Convert.ToSingle(breathingRate));
                        bw.Write(Convert.ToSingle(temperature));
                        bw.Write(Convert.ToSingle(posture));
                        bw.Write(Convert.ToSingle(activity));
                        bw.Write(Convert.ToSingle(acceleration));
                        bw.Write(Convert.ToSingle(battery));
                        bw.Write(Convert.ToSingle(breathingAmplitude));
                        bw.Write(Convert.ToSingle(ecgAmplitude));
                        bw.Write(Convert.ToSingle(ecgNoise));
                        bw.Write(Convert.ToSingle(axisXMin));
                        bw.Write(Convert.ToSingle(axisXPeak));
                        bw.Write(Convert.ToSingle(axisYMin));
                        bw.Write(Convert.ToSingle(axisYPeak));
                        bw.Write(Convert.ToSingle(axisZMin));
                        bw.Write(Convert.ToSingle(axisZPeak));

                        // Generate Accel CSV string
                        var accelTimestamp = timestamp;
                        for (int sample = 0; sample < 126; sample++)
                        {
                            if (session.Period < (sample + 1) * 8)
                            {
                                break;
                            }

                            /* Work out which byte to packing starts in */
                            int sampleOffset = (30 * sample) / 8;

                            /* for all three accelerometer axes in each sample */
                            for (int accelChannel = 0; accelChannel < 3; accelChannel++)
                            {
                                /* Figure out how many bits have already been used in the current byte */
                                int usedBits = ((30 * sample) + (accelChannel * 10)) % 8;

                                /* Value read from the queue, add to the packet (bit packed) */
                                switch (usedBits)
                                {
                                    case 0:
                                    default:
                                        accel[accelChannel] = data[offset + 36 + sampleOffset] + ((data[offset + 36 + sampleOffset + 1] & 0x03) << 8);
                                        sampleOffset++;
                                        break;
                                    case 2:
                                        accel[accelChannel] = ((data[offset + 36 + sampleOffset] & 0xFC) >> 2) + ((data[offset + 36 + sampleOffset + 1] & 0x0F) << 6);
                                        sampleOffset++;
                                        break;
                                    case 4:
                                        accel[accelChannel] = ((data[offset + 36 + sampleOffset] & 0xF0) >> 4) + ((data[offset + 36 + sampleOffset + 1] & 0x3F) << 4);
                                        sampleOffset++;
                                        break;
                                    case 6:
                                        accel[accelChannel] = ((data[offset + 36 + sampleOffset] & 0xC0) >> 6) + (data[offset + 36 + sampleOffset + 1] << 2);
                                        sampleOffset++;
                                        sampleOffset++;
                                        break;
                                }
                            }

                            accelw.Write((float)((accel[0] - ZeroGoffset) / Conversion));
                            accelw.Write((float)((accel[1] - ZeroGoffset) / Conversion));
                            accelw.Write((float)((accel[2] - ZeroGoffset) / Conversion));
                            accelTimestamp += TimeSpan.FromMilliseconds(8);
                        }

                        offset += session.Channels * 2;
                        timestamp += TimeSpan.FromMilliseconds(session.Period);
                    }
                }
            }
        }

        /// <summary>
        /// Accelerometer Magnitude log format export data to CSV files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateAccelMagnitudeCSVFiles(Session session, byte[] data)
        {
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int rawBreathingWaveform;
            float intervalRR;

            float accelMag;

            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            var timestamp = session.Timestamp;
            using (StreamWriter generalFile = new StreamWriter(filePathAndPrefix + "_General.csv"))
            {
                using (StreamWriter breathingAndRRFile = new StreamWriter(filePathAndPrefix + "_BR_RR.csv"))
                {
                    using (StreamWriter accelerometerMagFile = new StreamWriter(filePathAndPrefix + "_ACCELMAG.csv"))
                    {
                        generalFile.WriteLine("Timestamp,HR,BR,Temp,Posture,Activity,Acceleration,Battery,BRAmplitude,ECGAmplitude,ECGNoise,XMin,XPeak,YMin,YPeak,ZMin,ZPeak");
                        breathingAndRRFile.WriteLine("Timestamp,BR,RtoR");
                        accelerometerMagFile.WriteLine("Timestamp,Accel Mag(g)");

                        int offset = 0;
                        while (offset < data.Length)
                        {
                            // Generate the General CSV string
                            heartRate = BitConverter.ToInt16(data, offset);
                            breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                            temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                            posture = BitConverter.ToInt16(data, offset + 6);
                            activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                            acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                            battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                            breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                            ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                            ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                            axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                            axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                            axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                            axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                            axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                            axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;
                            generalFile.WriteLine(
                                string.Format(
                                    culture,
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                                    timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                    heartRate.ToString(culture),
                                    breathingRate.ToString("F1", culture),
                                    temperature.ToString("F1", culture),
                                    posture.ToString(culture),
                                    activity.ToString("F2", culture),
                                    acceleration.ToString("F2", culture),
                                    battery.ToString("F3", culture),
                                    breathingAmplitude.ToString("F3", culture),
                                    ecgAmplitude.ToString("F6", culture),
                                    ecgNoise.ToString("F6", culture),
                                    axisXMin.ToString("F2", culture),
                                    axisXPeak.ToString("F2", culture),
                                    axisYMin.ToString("F2", culture),
                                    axisYPeak.ToString("F2", culture),
                                    axisZMin.ToString("F2", culture),
                                    axisZPeak.ToString("F2", culture)));

                            // Generate BR/RR CSV string
                            var brrrTimestamp = timestamp;
                            for (int sample = 0; sample < 36; sample += 2)
                            {
                                rawBreathingWaveform = BitConverter.ToInt16(data, offset + 36 + sample);
                                intervalRR = BitConverter.ToInt16(data, offset + 72 + sample) / 1000f;
                                if ((rawBreathingWaveform == 32767) && (intervalRR == 32.767f))
                                {
                                    continue;
                                }

                                breathingAndRRFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1},{2}",
                                        brrrTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        rawBreathingWaveform.ToString(culture),
                                        intervalRR.ToString(culture)));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }

                            // Generate Accel magnitude CSV string
                            var accelTimestamp = timestamp;
                            for (int sample = 0; sample < 126; sample++)
                            {
                                if (session.Period < (sample + 1) * 8)
                                {
                                    break;
                                }

                                accelMag = data[offset + 128 + sample] / 10f;
                                accelerometerMagFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1}",
                                        accelTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        accelMag.ToString(culture)));

                                /* Format 0008 has 100Hz Accelerometer data, so we need to adjust the timestamp increment */
                                if (this.directory.FormatVersion == "0008")
                                {
                                    accelTimestamp += TimeSpan.FromMilliseconds(10);
                                }
                                else
                                {
                                    /* Default to 8ms, which is true for all other formats */
                                    accelTimestamp += TimeSpan.FromMilliseconds(8);
                                }
                            }

                            offset += session.Channels * 2;
                            timestamp += TimeSpan.FromMilliseconds(session.Period);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Accelerometer Magnitude log format export data to DaDisp files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateAccelMagnitudeDaDispFiles(Session session, byte[] data)
        {
            // Create the header files
            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            using (StreamWriter generalDataHeaderFile = File.CreateText(filePathAndPrefix + "_General.hed"))
            {
                generalDataHeaderFile.WriteLine("DATASET\tZephyr");
                generalDataHeaderFile.WriteLine("VERSION\t1.0");
                generalDataHeaderFile.WriteLine("SERIES\tHeartRate,BreathingRate,SkinTemperature,Posture,VectorMagnitude,PeakAcceleration,BatteryVoltage,BreathingWaveAmplitude,EcgAmplitude,EcgNoise,XMin,XMax,YMin,YMax,ZMin,ZMax");
                generalDataHeaderFile.WriteLine("RATE\t0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206,0.99206");
                generalDataHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time,Time");
                generalDataHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units,No Units");
                generalDataHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                generalDataHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                generalDataHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                generalDataHeaderFile.WriteLine("NUM_SERIES\t16");
            }

            using (StreamWriter accelerometerDataHeaderFile = File.CreateText(filePathAndPrefix + "_ACCELMAG.hed"))
            {
                accelerometerDataHeaderFile.WriteLine("DATASET\tZephyr");
                accelerometerDataHeaderFile.WriteLine("VERSION\t1.0");
                accelerometerDataHeaderFile.WriteLine("SERIES\tAccel_Mag");
                accelerometerDataHeaderFile.WriteLine("RATE\t125.0");
                accelerometerDataHeaderFile.WriteLine("HORIZ_UNITS\tTime");
                accelerometerDataHeaderFile.WriteLine("VERT_UNITS\tNo Units");
                accelerometerDataHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                accelerometerDataHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                accelerometerDataHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                accelerometerDataHeaderFile.WriteLine("NUM_SERIES\t1");
            }

            using (StreamWriter breathingAndRRheaderFile = File.CreateText(filePathAndPrefix + "_BR_RR.hed"))
            {
                breathingAndRRheaderFile.WriteLine("DATASET\tZephyr");
                breathingAndRRheaderFile.WriteLine("VERSION\t1.0");
                breathingAndRRheaderFile.WriteLine("SERIES\tBreathingData,RtoRData");
                breathingAndRRheaderFile.WriteLine("RATE\t17.85714");
                breathingAndRRheaderFile.WriteLine("HORIZ_UNITS\tTime");
                breathingAndRRheaderFile.WriteLine("VERT_UNITS\tNo Units");
                breathingAndRRheaderFile.WriteLine("FILE_TYPE\tSINTEGER");
                breathingAndRRheaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                breathingAndRRheaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                breathingAndRRheaderFile.WriteLine("NUM_SERIES\t2");
            }

            // Create the data files
            int heartRate;
            float breathingRate;
            float temperature;
            int posture;
            float activity;
            float acceleration;
            float battery;
            float breathingAmplitude;
            float ecgAmplitude;
            float ecgNoise;
            float axisXMin;
            float axisXPeak;
            float axisYMin;
            float axisYPeak;
            float axisZMin;
            float axisZPeak;

            int rawBreathingWaveform;
            float intervalRR;

            float accelMag;

            var timestamp = session.Timestamp;
            using (BinaryWriter bw = new BinaryWriter(File.Create(filePathAndPrefix + "_General.dat")))
            {
                using (BinaryWriter accelw = new BinaryWriter(File.Create(filePathAndPrefix + "_ACCELMAG.dat")))
                {
                    using (BinaryWriter breathingAndRRBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_BR_RR.dat")))
                    {
                        int offset = 0;
                        while (offset < data.Length)
                        {
                            // Generate the General binary values
                            heartRate = BitConverter.ToInt16(data, offset);
                            breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                            temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                            posture = BitConverter.ToInt16(data, offset + 6);
                            activity = BitConverter.ToInt16(data, offset + 8) / 100f;
                            acceleration = BitConverter.ToInt16(data, offset + 10) / 100f;
                            battery = BitConverter.ToInt16(data, offset + 12) / 1000f;
                            breathingAmplitude = BitConverter.ToInt16(data, offset + 14) / 1000f;
                            ecgAmplitude = BitConverter.ToInt16(data, offset + 18) / 1000000f;
                            ecgNoise = BitConverter.ToInt16(data, offset + 20) / 1000000f;
                            axisXMin = BitConverter.ToInt16(data, offset + 24) / 100f;
                            axisXPeak = BitConverter.ToInt16(data, offset + 26) / 100f;
                            axisYMin = BitConverter.ToInt16(data, offset + 28) / 100f;
                            axisYPeak = BitConverter.ToInt16(data, offset + 30) / 100f;
                            axisZMin = BitConverter.ToInt16(data, offset + 32) / 100f;
                            axisZPeak = BitConverter.ToInt16(data, offset + 34) / 100f;

                            bw.Write(Convert.ToSingle(heartRate));
                            bw.Write(Convert.ToSingle(breathingRate));
                            bw.Write(Convert.ToSingle(temperature));
                            bw.Write(Convert.ToSingle(posture));
                            bw.Write(Convert.ToSingle(activity));
                            bw.Write(Convert.ToSingle(acceleration));
                            bw.Write(Convert.ToSingle(battery));
                            bw.Write(Convert.ToSingle(breathingAmplitude));
                            bw.Write(Convert.ToSingle(ecgAmplitude));
                            bw.Write(Convert.ToSingle(ecgNoise));
                            bw.Write(Convert.ToSingle(axisXMin));
                            bw.Write(Convert.ToSingle(axisXPeak));
                            bw.Write(Convert.ToSingle(axisYMin));
                            bw.Write(Convert.ToSingle(axisYPeak));
                            bw.Write(Convert.ToSingle(axisZMin));
                            bw.Write(Convert.ToSingle(axisZPeak));

                            // Generate BR/RR binaries values
                            var brrrTimestamp = timestamp;
                            for (int sample = 0; sample < 36; sample += 2)
                            {
                                rawBreathingWaveform = BitConverter.ToInt16(data, offset + 36 + sample);
                                intervalRR = BitConverter.ToInt16(data, offset + 72 + sample);
                                if ((rawBreathingWaveform == 32767) && (intervalRR == 32767))
                                {
                                    continue;
                                }

                                breathingAndRRBinFile.Write(Convert.ToInt16(rawBreathingWaveform));
                                breathingAndRRBinFile.Write(Convert.ToInt16(intervalRR));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }

                            // Generate Accel magnitude binary values
                            var accelTimestamp = timestamp;
                            for (int sample = 0; sample < 126; sample++)
                            {
                                if (session.Period < (sample + 1) * 8)
                                {
                                    break;
                                }

                                accelMag = data[offset + 128 + sample] / 10f;
                                accelw.Write(Convert.ToSingle(accelMag));
                                accelTimestamp += TimeSpan.FromMilliseconds(8);
                            }

                            offset += session.Channels * 2;
                            timestamp += TimeSpan.FromMilliseconds(session.Period);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// BioRICs Accelerometer log format export data to CSV files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateBioRICSCSVFiles(Session session, byte[] data)
        {
            int heartRate;
            float breathingRate;
            float temperature;
            float battery;
            int intervalRR;
            float intervalRRf;

            int[] accel = new int[3];

            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            var timestamp = session.Timestamp;
            using (StreamWriter bioRICSGenfile = new StreamWriter(filePathAndPrefix + "_BioRICSGeneral.csv"))
            {
                using (StreamWriter breathingAndRRFile = new StreamWriter(filePathAndPrefix + "_BR_RR.csv"))
                {
                    using (StreamWriter accelerometerFile = new StreamWriter(filePathAndPrefix + "_ACCEL.csv"))
                    {
                        bioRICSGenfile.WriteLine("Timestamp,HR,BR,Temp,Battery");
                        accelerometerFile.WriteLine("Timestamp,Accel_x,Accel_y,Accel_z");
                        breathingAndRRFile.WriteLine("Timestamp,BR,RtoR");

                        int offset = 0;

                        // NOTE: session.Period is not necessarally 1000 (can be 1008 et al).
                        double ec = session.Duration.TotalSeconds / ((double)session.Period / 1000);
                        int elementCount = (int)ec;
                        foreach (var index in Enumerable.Range(0, elementCount))
                        {
                            // Generate the General CSV string
                            heartRate = BitConverter.ToInt16(data, offset);
                            breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                            temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                            battery = BitConverter.ToInt16(data, offset + 6) / 1000f;

                            bioRICSGenfile.WriteLine(
                                string.Format(
                                    culture,
                                    "{0},{1},{2},{3},{4}",
                                    timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                    heartRate.ToString(culture),
                                    breathingRate.ToString("F1", culture),
                                    temperature.ToString("F1", culture),
                                    battery.ToString("F3", culture)));

                            // Generate BR/RR CSV string - NOTE Breathing wave is a placeholder to keep consistent file format
                            var brrrTimestamp = timestamp;
                            for (int sample = 0; sample < 18; sample++)
                            {
                                int sampleOffset = 13 * sample;     /* Number of sample bits stored so far */
                                sampleOffset += sample / 3;         /* Number of 'filler' bits stored so far */

                                /* Figure out how many bits have already been used in the current byte */
                                var usedBits = sampleOffset % 8;

                                sampleOffset /= 8;             /* Convert to bytes */
                                sampleOffset += offset;        /* Align with current log */
                                sampleOffset += 8;             /* Starting location for RR data */

                                /* reconstruct the bitpacked intervals  */
                                switch (usedBits)
                                {
                                    case 0:
                                    default:
                                        intervalRR = data[sampleOffset++];
                                        intervalRR += (data[sampleOffset] * 256) & 0x1F00;
                                        break;
                                    case 5:
                                        intervalRR = (data[sampleOffset++] / 32) & 0x0007;
                                        intervalRR += data[sampleOffset++] * 8;
                                        intervalRR += (data[sampleOffset] * 2048) & 0x1800;
                                        break;
                                    case 2:
                                        intervalRR = (data[sampleOffset++] / 4) & 0x003F;
                                        intervalRR += (data[sampleOffset++] * 64) & 0x1FC0;
                                        break;
                                }

                                /* Convert to the floating point version, change from 4ms units to seconds and mask of the detect toggle bit */
                                intervalRRf = ((intervalRR & 0x0FFF) * 4) / 1000.0f;

                                /* Use the detect toggle bit to set the sign */
                                if ((intervalRR & 0x1000) != 0)
                                {
                                    intervalRRf *= -1.0f;
                                }

                                /* Write to file, filling the breathing waveform colum with zeros to preserve the file format */
                                breathingAndRRFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1},{2}",
                                        brrrTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        "0",
                                        intervalRRf.ToString(culture)));
                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }

                            // Generate Accel CSV string
                            var accelTimestamp = timestamp;
                            for (int sample = 0; sample < 126; sample++)
                            {
                                /* Work out which byte to packing starts in */
                                int sampleOffset = (30 * sample) / 8;

                                /* for all three accelerometer axes in each sample */
                                for (int accelChannel = 0; accelChannel < 3; accelChannel++)
                                {
                                    /* Figure out how many bits have already been used in the current byte */
                                    int usedBits = ((30 * sample) + (accelChannel * 10)) % 8;

                                    /* Value read from the queue, add to the packet (bit packed) */
                                    switch (usedBits)
                                    {
                                        case 0:
                                        default:
                                            accel[accelChannel] = data[offset + 38 + sampleOffset] + ((data[offset + 38 + sampleOffset + 1] & 0x03) << 8);
                                            sampleOffset++;
                                            break;
                                        case 2:
                                            accel[accelChannel] = ((data[offset + 38 + sampleOffset] & 0xFC) >> 2) + ((data[offset + 38 + sampleOffset + 1] & 0x0F) << 6);
                                            sampleOffset++;
                                            break;
                                        case 4:
                                            accel[accelChannel] = ((data[offset + 38 + sampleOffset] & 0xF0) >> 4) + ((data[offset + 38 + sampleOffset + 1] & 0x3F) << 4);
                                            sampleOffset++;
                                            break;
                                        case 6:
                                            accel[accelChannel] = ((data[offset + 38 + sampleOffset] & 0xC0) >> 6) + (data[offset + 38 + sampleOffset + 1] << 2);
                                            sampleOffset++;
                                            sampleOffset++;
                                            break;
                                    }
                                }

                                accelerometerFile.WriteLine(
                                    string.Format(
                                        culture,
                                        "{0},{1},{2},{3}",
                                        accelTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff", culture),
                                        ((accel[0] - ZeroGoffset) / Conversion).ToString(culture),
                                        ((accel[1] - ZeroGoffset) / Conversion).ToString(culture),
                                        ((accel[2] - ZeroGoffset) / Conversion).ToString(culture)));
                                accelTimestamp += TimeSpan.FromMilliseconds(8);
                            }

                            offset += session.Channels * 2;
                            timestamp += TimeSpan.FromMilliseconds(session.Period);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// BioRICs Accelerometer log format export data to DaDisp files
        /// </summary>
        /// <param name="session">The log session the data relates to</param>
        /// <param name="data">The binary data for the log.</param>
        private void CreateBioRICSDaDispFiles(Session session, byte[] data)
        {
            // Create the header files
            string filePathAndPrefix = this.saveFolderBrowserDialog.SelectedPath + "\\" + session.Timestamp.ToString("yyyy_MM_dd__HH_mm_ss", culture);

            using (StreamWriter bioRICSGenHeaderFile = File.CreateText(filePathAndPrefix + "_BioRICSGeneral.hed"))
            {
                bioRICSGenHeaderFile.WriteLine("DATASET\tZephyr");
                bioRICSGenHeaderFile.WriteLine("VERSION\t1.0");
                bioRICSGenHeaderFile.WriteLine("SERIES\tHeartRate,BreathingRate,SkinTemperature,BatteryVoltage");
                bioRICSGenHeaderFile.WriteLine("RATE\t0.99206,0.99206,0.99206,0.99206");
                bioRICSGenHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time,Time");
                bioRICSGenHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units,No Units");
                bioRICSGenHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                bioRICSGenHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                bioRICSGenHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                bioRICSGenHeaderFile.WriteLine("NUM_SERIES\t4");
            }

            using (StreamWriter breathingAndRRheaderFile = File.CreateText(filePathAndPrefix + "_BR_RR.hed"))
            {
                breathingAndRRheaderFile.WriteLine("DATASET\tZephyr");
                breathingAndRRheaderFile.WriteLine("VERSION\t1.0");
                breathingAndRRheaderFile.WriteLine("SERIES\tBreathingData,RtoRData");
                breathingAndRRheaderFile.WriteLine("RATE\t17.85714");
                breathingAndRRheaderFile.WriteLine("HORIZ_UNITS\tTime");
                breathingAndRRheaderFile.WriteLine("VERT_UNITS\tNo Units");
                breathingAndRRheaderFile.WriteLine("FILE_TYPE\tSINTEGER");
                breathingAndRRheaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                breathingAndRRheaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                breathingAndRRheaderFile.WriteLine("NUM_SERIES\t2");
            }

            using (StreamWriter accelerometerDataHeaderFile = File.CreateText(filePathAndPrefix + "_ACCEL.hed"))
            {
                accelerometerDataHeaderFile.WriteLine("DATASET\tZephyr");
                accelerometerDataHeaderFile.WriteLine("VERSION\t1.0");
                accelerometerDataHeaderFile.WriteLine("SERIES\tACCEL_X,ACCEL_Y,ACCEL_Z");
                accelerometerDataHeaderFile.WriteLine("RATE\t125.0,125.0,125.0");
                accelerometerDataHeaderFile.WriteLine("HORIZ_UNITS\tTime,Time,Time");
                accelerometerDataHeaderFile.WriteLine("VERT_UNITS\tNo Units,No Units,No Units");
                accelerometerDataHeaderFile.WriteLine("FILE_TYPE\tFLOAT");
                accelerometerDataHeaderFile.WriteLine("DATE\t{0}-{1}-{2}", session.Timestamp.Day.ToString(culture), session.Timestamp.Month.ToString(culture), session.Timestamp.Year.ToString(culture));
                accelerometerDataHeaderFile.WriteLine("TIME\t{0}:{1}:{2}", session.Timestamp.Hour.ToString(culture), session.Timestamp.Minute.ToString(culture), session.Timestamp.Second.ToString(culture));
                accelerometerDataHeaderFile.WriteLine("NUM_SERIES\t3");
            }

            // Create the data files
            int heartRate;
            float breathingRate;
            float temperature;
            float battery;
            int intervalRR;

            int[] accel = new int[3];

            var timestamp = session.Timestamp;
            using (BinaryWriter bioRICSgenw = new BinaryWriter(File.Create(filePathAndPrefix + "_BioRICSGeneral.dat")))
            {
                using (BinaryWriter accelw = new BinaryWriter(File.Create(filePathAndPrefix + "_ACCEL.dat")))
                {
                    using (BinaryWriter breathingAndRRBinFile = new BinaryWriter(File.Create(filePathAndPrefix + "_BR_RR.dat")))
                    {
                        int offset = 0;

                        // NOTE: session.Period is not necessarally 1000 (can be 1008 et al).
                        double ec = session.Duration.TotalSeconds / ((double)session.Period / 1000);
                        int elementCount = (int)ec;
                        foreach (var index in Enumerable.Range(0, elementCount))
                        {
                            // Generate the General binary values
                            heartRate = BitConverter.ToInt16(data, offset);
                            breathingRate = BitConverter.ToInt16(data, offset + 2) / 10f;
                            temperature = BitConverter.ToInt16(data, offset + 4) / 10f;
                            battery = BitConverter.ToInt16(data, offset + 6) / 1000f;

                            bioRICSgenw.Write(Convert.ToSingle(heartRate));
                            bioRICSgenw.Write(Convert.ToSingle(breathingRate));
                            bioRICSgenw.Write(Convert.ToSingle(temperature));
                            bioRICSgenw.Write(Convert.ToSingle(battery));

                            // Generate BR/RR CSV string - NOTE Breathing wave is a placeholder to keep consistent file format
                            var brrrTimestamp = timestamp;
                            for (int sample = 0; sample < 18; sample++)
                            {
                                int sampleOffset = 13 * sample;     /* Number of sample bits stored so far */
                                sampleOffset += sample / 3;         /* Number of 'filler' bits stored so far */

                                /* Figure out how many bits have already been used in the current byte */
                                var usedBits = sampleOffset % 8;

                                sampleOffset /= 8;             /* Convert to bytes */
                                sampleOffset += offset;        /* Align with current log */
                                sampleOffset += 8;             /* Starting location for RR data */

                                /* reconstruct the bitpacked intervals  */
                                switch (usedBits)
                                {
                                    case 0:
                                    default:
                                        intervalRR = data[sampleOffset++];
                                        intervalRR += (data[sampleOffset] * 256) & 0x1F00;
                                        break;
                                    case 5:
                                        intervalRR = (data[sampleOffset++] / 32) & 0x0007;
                                        intervalRR += data[sampleOffset++] * 8;
                                        intervalRR += (data[sampleOffset] * 2048) & 0x1800;
                                        break;
                                    case 2:
                                        intervalRR = (data[sampleOffset++] / 4) & 0x003F;
                                        intervalRR += (data[sampleOffset++] * 64) & 0x1FC0;
                                        break;
                                }

                                /* Use the detect toggle bit to set the sign */
                                if ((intervalRR & 0x1000) != 0)
                                {
                                    intervalRR = (intervalRR & 0x0FFF) * -4;
                                }
                                else
                                {
                                    intervalRR = (intervalRR & 0x0FFF) * 4;
                                }

                                /* Write to file, filling the breathing waveform colum with zeros to preserve the file format */
                                breathingAndRRBinFile.Write(Convert.ToInt16(0));
                                breathingAndRRBinFile.Write(Convert.ToInt16(intervalRR));

                                brrrTimestamp += TimeSpan.FromMilliseconds(56);
                            }

                            // Generate Accel binaries values
                            var accelTimestamp = timestamp;
                            for (int sample = 0; sample < 126; sample++)
                            {
                                /* Work out which byte to packing starts in */
                                int sampleOffset = (30 * sample) / 8;

                                /* for all three accelerometer axes in each sample */
                                for (int accelChannel = 0; accelChannel < 3; accelChannel++)
                                {
                                    /* Figure out how many bits have already been used in the current byte */
                                    int usedBits = ((30 * sample) + (accelChannel * 10)) % 8;

                                    /* Value read from the queue, add to the packet (bit packed) */
                                    switch (usedBits)
                                    {
                                        case 0:
                                        default:
                                            accel[accelChannel] = data[offset + 38 + sampleOffset] + ((data[offset + 38 + sampleOffset + 1] & 0x03) << 8);
                                            sampleOffset++;
                                            break;
                                        case 2:
                                            accel[accelChannel] = ((data[offset + 38 + sampleOffset] & 0xFC) >> 2) + ((data[offset + 38 + sampleOffset + 1] & 0x0F) << 6);
                                            sampleOffset++;
                                            break;
                                        case 4:
                                            accel[accelChannel] = ((data[offset + 38 + sampleOffset] & 0xF0) >> 4) + ((data[offset + 38 + sampleOffset + 1] & 0x3F) << 4);
                                            sampleOffset++;
                                            break;
                                        case 6:
                                            accel[accelChannel] = ((data[offset + 38 + sampleOffset] & 0xC0) >> 6) + (data[offset + 38 + sampleOffset + 1] << 2);
                                            sampleOffset++;
                                            sampleOffset++;
                                            break;
                                    }
                                }

                                accelw.Write(Convert.ToSingle((accel[0] - ZeroGoffset) / Conversion));
                                accelw.Write(Convert.ToSingle((accel[1] - ZeroGoffset) / Conversion));
                                accelw.Write(Convert.ToSingle((accel[2] - ZeroGoffset) / Conversion));
                                accelTimestamp += TimeSpan.FromMilliseconds(8);
                            }

                            offset += session.Channels * 2;
                            timestamp += TimeSpan.FromMilliseconds(session.Period);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the clicking of the Exit button
        /// </summary>
        /// <param name="sender">The exit button</param>
        /// <param name="e">Dummy event args</param>
        private void ExitButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Toggles between available view styles
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e">Dummy Eventargs</param>
        private void StyleButtonClick(object sender, EventArgs e)
        {
            switch (this.logListView.View)
            {
                case View.Details:
                    this.logListView.View = View.LargeIcon;
                    break;
                case View.LargeIcon:
                    this.logListView.View = View.List;
                    break;
                case View.List:
                    this.logListView.View = View.Details;
                    break;
            }

            this.logListView.ShowItemToolTips = this.logListView.View != View.Details;
        }

        /// <summary>
        /// Form closing, so unhook device monitoring
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">FormClosingEventArgs, enabling us to cancel close if req.</param>
        private void DownloadFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // No more add / remove USB hardware notifications for this form
            Zephyr.IO.USB.WinUsbDeviceManagement.StopReceivingDeviceNotifications();
        }
    }
}
