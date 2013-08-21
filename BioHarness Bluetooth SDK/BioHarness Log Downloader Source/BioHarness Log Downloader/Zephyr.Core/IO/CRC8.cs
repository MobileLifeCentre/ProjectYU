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
//     file="CRC8.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       CRC8.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Calculates an 8bit CRC on the supplied list of bytes.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.IO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Calculates an 8bit CRC on the supplied list of bytes.
    /// </summary>
    [CLSCompliant(true)]
    public class CRC8 : ICRC<byte>
    {
        #region Fields
        /// <summary>
        /// Readonly CRC polynomial
        /// </summary>
        private readonly byte internalCrc8Poly;
        #endregion

        #region Consructors
        /// <summary>
        /// Initializes a new instance of the CRC8 class.
        /// </summary>
        /// <param name="crc8Poly">The CRC polynomial to use</param>
        public CRC8(byte crc8Poly)
        {
            this.internalCrc8Poly = crc8Poly;
        }
        #endregion

        #region ICRC<byte> Members

        /// <summary>
        /// Calculates the CRC of the supplied list of bytes.
        /// </summary>
        /// <param name="data">List of bytes to be CRCed</param>
        /// <returns>The CRC value</returns>
        public byte Calculate(IList<byte> data)
        {
            byte crc = 0;

            if (data == null)
            {
                return crc;
            }

            return data.Aggregate(
                crc,
                (currentCRC, item) =>
                {
                    byte loop;

                    currentCRC = (byte)(currentCRC ^ item);
                    for (loop = 0; loop < 8; loop++)
                    {
                        if ((currentCRC & 1) == 1)
                        {
                            currentCRC = (byte)((currentCRC >> 1) ^ internalCrc8Poly);
                        }
                        else
                        {
                            currentCRC = (byte)(currentCRC >> 1);
                        }
                    }

                    return currentCRC;
                });
        }
        #endregion
    }
}
