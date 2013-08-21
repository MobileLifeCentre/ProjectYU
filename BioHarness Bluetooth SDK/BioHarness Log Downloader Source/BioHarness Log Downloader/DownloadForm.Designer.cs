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
//     file="DownloadForm.Designer.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       DownloadForm.Designer.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Main form of the log downloader.
*/
////////////////////////////////////////////////////////////

namespace BioHarnessLogDownloader
{
    /// <summary>
    /// Main form of the log downloader.
    /// </summary>
    public partial class DownloadForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Combobox to select the device
        /// </summary>
        private System.Windows.Forms.ComboBox deviceSelectionComboBox;

        /// <summary>
        /// Label for the device select combo box
        /// </summary>
        private System.Windows.Forms.Label deviceSelectLabel;

        /// <summary>
        /// Listview to show the current directory of log sessions
        /// </summary>
        private System.Windows.Forms.ListView logListView;

        /// <summary>
        /// Collection of larger icons for the loglistview box, when in tile mode.
        /// </summary>
        private System.Windows.Forms.ImageList logLargeImageList;

        /// <summary>
        /// Lable for the log record select text box
        /// </summary>
        private System.Windows.Forms.Label logRecordSelectlabel;

        /// <summary>
        /// Text box containing the log record select box
        /// </summary>
        private System.Windows.Forms.TextBox selectedLogRecordTextBox;

        /// <summary>
        /// Combobox for the various save options
        /// </summary>
        private System.Windows.Forms.ComboBox saveAsTypeComboBox;

        /// <summary>
        /// Label for the type save box
        /// </summary>
        private System.Windows.Forms.Label saveAsTypeLabel;

        /// <summary>
        /// Button to start the log save procedure
        /// </summary>
        private System.Windows.Forms.Button saveButton;

        /// <summary>
        /// Button to exit the application
        /// </summary>
        private System.Windows.Forms.Button exitButton;

        /// <summary>
        /// Collection of smaller icons for the loglistview box, when in detail or list mode.
        /// </summary>
        private System.Windows.Forms.ImageList logSmallImageList;

        /// <summary>
        /// Standard folder browser dialog box, to select the place to save the log session
        /// </summary>
        private System.Windows.Forms.FolderBrowserDialog saveFolderBrowserDialog;

        /// <summary>
        /// Button to change the view style
        /// </summary>
        private System.Windows.Forms.Button styleButton;

        /// <summary>
        /// Side Panel for device information
        /// </summary>
        private System.Windows.Forms.Panel deviceInfoPanel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadForm));
            this.deviceSelectionComboBox = new System.Windows.Forms.ComboBox();
            this.deviceSelectLabel = new System.Windows.Forms.Label();
            this.logListView = new System.Windows.Forms.ListView();
            this.logLargeImageList = new System.Windows.Forms.ImageList(this.components);
            this.logSmallImageList = new System.Windows.Forms.ImageList(this.components);
            this.logRecordSelectlabel = new System.Windows.Forms.Label();
            this.selectedLogRecordTextBox = new System.Windows.Forms.TextBox();
            this.saveAsTypeComboBox = new System.Windows.Forms.ComboBox();
            this.saveAsTypeLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.saveFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.styleButton = new System.Windows.Forms.Button();
            this.deviceInfoPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // deviceSelectionComboBox
            // 
            this.deviceSelectionComboBox.FormattingEnabled = true;
            this.deviceSelectionComboBox.Location = new System.Drawing.Point(101, 6);
            this.deviceSelectionComboBox.Name = "deviceSelectionComboBox";
            this.deviceSelectionComboBox.Size = new System.Drawing.Size(254, 21);
            this.deviceSelectionComboBox.TabIndex = 0;
            this.deviceSelectionComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceSelectionComboBoxSelectedIndexChanged);
            this.deviceSelectionComboBox.DropDown += new System.EventHandler(this.DeviceSelectionComboBoxDropDown);
            // 
            // deviceSelectLabel
            // 
            this.deviceSelectLabel.AutoSize = true;
            this.deviceSelectLabel.Location = new System.Drawing.Point(21, 9);
            this.deviceSelectLabel.Name = "deviceSelectLabel";
            this.deviceSelectLabel.Size = new System.Drawing.Size(77, 13);
            this.deviceSelectLabel.TabIndex = 1;
            this.deviceSelectLabel.Text = "Select Device:";
            // 
            // logListView
            // 
            this.logListView.LargeImageList = this.logLargeImageList;
            this.logListView.Location = new System.Drawing.Point(101, 33);
            this.logListView.MultiSelect = false;
            this.logListView.Name = "logListView";
            this.logListView.Size = new System.Drawing.Size(449, 283);
            this.logListView.SmallImageList = this.logSmallImageList;
            this.logListView.TabIndex = 2;
            this.logListView.UseCompatibleStateImageBehavior = false;
            this.logListView.View = System.Windows.Forms.View.Details;
            this.logListView.SelectedIndexChanged += new System.EventHandler(this.LogListViewSelectedIndexChanged);
            this.logListView.DoubleClick += new System.EventHandler(this.LogListViewDoubleClick);
            // 
            // logLargeImageList
            // 
            this.logLargeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("logLargeImageList.ImageStream")));
            this.logLargeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.logLargeImageList.Images.SetKeyName(0, "page_blank_32.png");
            this.logLargeImageList.Images.SetKeyName(1, "activity_monitor.png");
            this.logLargeImageList.Images.SetKeyName(2, "heart_32.png");
            this.logLargeImageList.Images.SetKeyName(3, "activity_monitor_add.png");
            this.logLargeImageList.Images.SetKeyName(4, "user_business_chart_32.png");
            // 
            // logSmallImageList
            // 
            this.logSmallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("logSmallImageList.ImageStream")));
            this.logSmallImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.logSmallImageList.Images.SetKeyName(0, "page_blank_32.png");
            this.logSmallImageList.Images.SetKeyName(1, "activity_monitor.png");
            this.logSmallImageList.Images.SetKeyName(2, "heart_32.png");
            this.logSmallImageList.Images.SetKeyName(3, "activity_monitor_add.png");
            this.logSmallImageList.Images.SetKeyName(4, "user_business_chart_32.png");
            // 
            // logRecordSelectlabel
            // 
            this.logRecordSelectlabel.AutoSize = true;
            this.logRecordSelectlabel.Location = new System.Drawing.Point(113, 332);
            this.logRecordSelectlabel.Name = "logRecordSelectlabel";
            this.logRecordSelectlabel.Size = new System.Drawing.Size(66, 13);
            this.logRecordSelectlabel.TabIndex = 3;
            this.logRecordSelectlabel.Text = "Log Record:";
            // 
            // selectedLogRecordTextBox
            // 
            this.selectedLogRecordTextBox.AcceptsReturn = true;
            this.selectedLogRecordTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.selectedLogRecordTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.selectedLogRecordTextBox.Location = new System.Drawing.Point(194, 329);
            this.selectedLogRecordTextBox.Name = "selectedLogRecordTextBox";
            this.selectedLogRecordTextBox.Size = new System.Drawing.Size(245, 20);
            this.selectedLogRecordTextBox.TabIndex = 4;
            this.selectedLogRecordTextBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.SelectedLogRecordTextBoxPreviewKeyDown);
            // 
            // saveAsTypeComboBox
            // 
            this.saveAsTypeComboBox.FormattingEnabled = true;
            this.saveAsTypeComboBox.Location = new System.Drawing.Point(194, 355);
            this.saveAsTypeComboBox.Name = "saveAsTypeComboBox";
            this.saveAsTypeComboBox.Size = new System.Drawing.Size(245, 21);
            this.saveAsTypeComboBox.TabIndex = 5;
            // 
            // saveAsTypeLabel
            // 
            this.saveAsTypeLabel.AutoSize = true;
            this.saveAsTypeLabel.Location = new System.Drawing.Point(113, 358);
            this.saveAsTypeLabel.Name = "saveAsTypeLabel";
            this.saveAsTypeLabel.Size = new System.Drawing.Size(72, 13);
            this.saveAsTypeLabel.TabIndex = 6;
            this.saveAsTypeLabel.Text = "Save as type:";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(475, 327);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "&Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(475, 353);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 8;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
            // 
            // saveFolderBrowserDialog
            // 
            this.saveFolderBrowserDialog.Description = "Location to save log data";
            // 
            // styleButton
            // 
            this.styleButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.styleButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.styleButton.Image = global::BioHarnessLogDownloader.Properties.Resources.ViewButton;
            this.styleButton.Location = new System.Drawing.Point(438, 6);
            this.styleButton.Name = "styleButton";
            this.styleButton.Size = new System.Drawing.Size(37, 23);
            this.styleButton.TabIndex = 9;
            this.styleButton.UseVisualStyleBackColor = true;
            this.styleButton.Click += new System.EventHandler(this.StyleButtonClick);
            // 
            // deviceInfoPanel
            // 
            this.deviceInfoPanel.Location = new System.Drawing.Point(6, 33);
            this.deviceInfoPanel.Name = "deviceInfoPanel";
            this.deviceInfoPanel.Size = new System.Drawing.Size(89, 342);
            this.deviceInfoPanel.TabIndex = 10;
            // 
            // DownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 382);
            this.Controls.Add(this.deviceInfoPanel);
            this.Controls.Add(this.styleButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.saveAsTypeLabel);
            this.Controls.Add(this.saveAsTypeComboBox);
            this.Controls.Add(this.selectedLogRecordTextBox);
            this.Controls.Add(this.logRecordSelectlabel);
            this.Controls.Add(this.logListView);
            this.Controls.Add(this.deviceSelectLabel);
            this.Controls.Add(this.deviceSelectionComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DownloadForm";
            this.Text = "BioHarness Log Downloader  ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DownloadFormFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

