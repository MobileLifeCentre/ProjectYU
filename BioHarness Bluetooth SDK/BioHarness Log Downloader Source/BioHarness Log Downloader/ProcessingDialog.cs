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
//     file="ProcessingDialog.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       ProcessingDialog.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      This class is a simple modal progress bar
*/
////////////////////////////////////////////////////////////

namespace BioHarnessLogDownloader
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Zephyr.Threading;

    /// <summary>
    /// Simple progress bar
    /// </summary>
    [CLSCompliant(true)]
    public partial class ProcessingDialog : Form
    {
        /// <summary>
        /// Internal bool that indicates if the animation should continue, and that the close should be aborted.
        /// </summary>
        private bool downloadInProgress;

        /// <summary>
        /// Reference to parent form so we can co-locate with the parent form.
        /// </summary>
        private Form parent;

        /// <summary>
        /// Initializes a new instance of the ProcessingDialog class.
        /// </summary>
        /// <param name="parent">A reference to the form to center on.</param>
        public ProcessingDialog(Form parent)
        {
            this.InitializeComponent();
            this.downloadInProgress = true;
            this.parent = parent;
        }
        
        /// <summary>
        /// Markes the download as finished, and closes the form.
        /// </summary>
        public void CloseForReal()
        {
            this.downloadInProgress = false;
            this.Close();
        }

        /// <summary>
        /// Runs an animated repeated filling of the bar - in other words, fake progress.
        /// </summary>
        /// <returns>A ref to the future class.</returns>
        private Future Animation()
        {
            var future = Future.Create(() =>
                {
                    while (downloadInProgress)
                    {
                        this.BeginInvoke((MethodInvoker)(() =>
                            {
                                if (fakeProgressBar.Value + 10 > fakeProgressBar.Maximum)
                                {
                                    fakeProgressBar.Value = fakeProgressBar.Minimum;
                                }

                                fakeProgressBar.Value += 10;
                            }));

                        System.Threading.Thread.Sleep(300);
                    }
                });
            this.Close();
            return future;
        }

        /// <summary>
        /// Intercept the closing event - if the animation is still running, abort closing.
        /// </summary>
        /// <param name="sender">The close source, could be the X in the corner or an external call</param>
        /// <param name="e">Arguments relating to the sender</param>
        private void ProcessingDialogFormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.downloadInProgress)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// On load, centers the form on the parent and runs the animation
        /// </summary>
        /// <param name="sender">Sening object, this form probably</param>
        /// <param name="e">Dummy event args</param>
        private void ProcessingDialog_Load(object sender, EventArgs e)
        {
            int parentCenterX = this.parent.Location.X + (this.parent.Size.Width / 2);
            int parentCenterY = this.parent.Location.Y + (this.parent.Size.Height / 2);
            this.Location = new Point(parentCenterX - (this.Size.Width / 2), parentCenterY - (this.Size.Height / 2));
            this.Animation();
        }
    }
}
