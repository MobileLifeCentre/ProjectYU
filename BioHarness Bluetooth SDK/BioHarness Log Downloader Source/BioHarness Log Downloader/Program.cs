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
//     file="Program.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       Program.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Entry point into the application.
*/
////////////////////////////////////////////////////////////

namespace BioHarnessLogDownloader
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Entry point into the application.
    /// </summary>
    [CLSCompliant(true)]
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            Application.Run(new DownloadForm());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                var message = e.Exception.ToString();
                MessageBox.Show("Error occured while downloading " + Environment.NewLine + Environment.NewLine +
                    message);
            }
        }
    }
}
