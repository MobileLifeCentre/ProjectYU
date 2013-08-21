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
//     file="ContractException.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       ContractException.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Represents errors that occur during application execution
*/
////////////////////////////////////////////////////////////

namespace Zephyr
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents errors that occur during application execution
    /// </summary>
    [Serializable]
    [CLSCompliant(true)]
    public class ContractException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ContractException class.
        /// Exception message set to default.
        /// </summary>
        public ContractException() : this("Contract Violation") 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the ContractException class.
        /// </summary>
        /// <param name="message">Message to include in exception</param>
        public ContractException(string message)
            : base(message) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the ContractException class.
        /// </summary>
        /// <param name="message">Message to include in exception</param>
        /// <param name="inner">Inner Exception</param>
        public ContractException(string message, Exception inner)
            : base(message, inner) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the ContractException class.
        /// </summary>
        /// <param name="info">Store of all data required to serialise and deserialise object</param>
        /// <param name="context">Describes the source and destination of a given serialized stream, and provides an additional caller-defined context.</param>
        protected ContractException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        { 
        }
    }
}
