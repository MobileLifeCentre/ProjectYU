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
//     file="Serialization.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       Serialization.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Performs Serialization on supplied object
*/
////////////////////////////////////////////////////////////

namespace Zephyr
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Performs XML Serialization on supplied object
    /// </summary>
    [CLSCompliant(true)]
    public static class Serialization
    {
        /// <summary>
        /// Performs XML Serialization on supplied object
        /// </summary>
        /// <param name="obj">Object to serialise</param>
        /// <returns>XML string</returns>
        public static string XmlSerialize(object obj)
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            XmlSerializer xs = new XmlSerializer(obj.GetType());
            xs.Serialize(sw, obj);

            return sw.ToString();
        }
    }
}
