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
//     file="ProcessingDialog.Designer.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       ProcessingDialog.Designer.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      This class is a simple modal progress bar
*/
////////////////////////////////////////////////////////////

namespace BioHarnessLogDownloader
{
    /// <summary>
    /// Simple progress bar
    /// </summary>
    public partial class ProcessingDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Progressbar to put the fake progress indication onto.
        /// </summary>
        private System.Windows.Forms.ProgressBar fakeProgressBar;

        /// <summary>
        /// Lable for the fake progressbar.
        /// </summary>
        private System.Windows.Forms.Label downloadingLabel;

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
            this.fakeProgressBar = new System.Windows.Forms.ProgressBar();
            this.downloadingLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // fakeProgressBar
            // 
            this.fakeProgressBar.Location = new System.Drawing.Point(12, 25);
            this.fakeProgressBar.Name = "fakeProgressBar";
            this.fakeProgressBar.Size = new System.Drawing.Size(299, 23);
            this.fakeProgressBar.TabIndex = 0;
            // 
            // downloadingLabel
            // 
            this.downloadingLabel.AutoSize = true;
            this.downloadingLabel.Location = new System.Drawing.Point(12, 9);
            this.downloadingLabel.Name = "downloadingLabel";
            this.downloadingLabel.Size = new System.Drawing.Size(70, 13);
            this.downloadingLabel.TabIndex = 1;
            this.downloadingLabel.Text = "Please wait...";
            // 
            // ProcessingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 68);
            this.Controls.Add(this.downloadingLabel);
            this.Controls.Add(this.fakeProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProcessingDialog";
            this.ShowInTaskbar = false;
            this.Text = "Download In Progress";
            this.Load += new System.EventHandler(this.ProcessingDialog_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessingDialogFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}