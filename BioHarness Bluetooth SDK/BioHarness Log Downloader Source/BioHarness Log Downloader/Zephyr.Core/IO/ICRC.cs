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
//     file="ICRC.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       ICRC.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Interface for CRC classes.
*/
////////////////////////////////////////////////////////////
namespace Zephyr.IO
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for CRC classes
    /// </summary>
    /// <typeparam name="T">The return type of the CRC</typeparam>
    [CLSCompliant(true)]
    public interface ICRC<T>
    {
        /// <summary>
        /// Calculates the CRC of a supplied list of bytes.
        /// </summary>
        /// <param name="data">A list of bytes to calculate the CRC of</param>
        /// <returns>The CRC value</returns>
        T Calculate(IList<T> data);
    }
}
