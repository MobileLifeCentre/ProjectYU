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
//     file="Session.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       Session.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      This class represents a log session.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Logging
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// This struct represents the collection of logs on a particular device. Incorporates some additional information on the file.
    /// </summary>
    [CLSCompliant(true)]
    public struct SessionDirectory
    {
        /// <summary>
        /// Internal store for the 
        /// </summary>
        private Session[] sessions;

        /// <summary>
        /// Initializes a new instance of the SessionDirectory struct.
        /// </summary>
        /// <param name="sessionData">The array of session information structures</param>
        /// <param name="formatVersion">The format for the logs in this device.</param>
        public SessionDirectory(Session[] sessionData, string formatVersion)
            : this()
        {
            this.sessions = sessionData;
            this.FormatVersion = formatVersion;
        }

        /// <summary>
        /// Gets the File Format version string for this device.
        /// </summary>
        public string FormatVersion { get; private set; }

        /// <summary>
        /// Gets a read only collection of sessions
        /// </summary>
        public ReadOnlyCollection<Session> SessionData
        {
            get
            {
                return new ReadOnlyCollection<Session>(this.sessions);
            }
        }
    }

    /// <summary>
    /// Represents a log session
    /// </summary>
    [CLSCompliant(true)]
    public class Session
    {
        /// <summary>
        /// Gets or sets the number of data channels present in the log
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// Gets or sets the padding data used to alighn the log to the next block boundary.
        /// </summary>
        public int PadBytes { get; set; }

        /// <summary>
        /// Gets or sets the offset into the memory to find this session at.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the duration of the log session.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the time at which this session was recoreded.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the length in bytes of the log record
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the period in bytes of the log record
        /// </summary>
        public int Period { get; set; }

        public Session(DateTime timestamp, int channels, int pad)
        {
            Timestamp = timestamp;
            Channels = channels;
            PadBytes = pad;
        }
    }
}
