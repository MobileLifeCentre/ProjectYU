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
//     file="FutureException.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       FutureException.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Exception relating to a future opperation.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Threading
{
    using System;
    using System.Runtime.Serialization;
    #region FutureException

    [Serializable]
    [CLSCompliant(true)]
    public class FutureException : Exception
    {
        public FutureException() : this("Future Exception")
        {
        }

        public FutureException(string message)
            : base(message) 
        {
        }

        public FutureException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FutureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    [CLSCompliant(true)]
    public class FutureAbortedException : FutureException
    {
        public FutureAbortedException() : this("Future Aborted Exception")
        {
        }

        public FutureAbortedException(string message)
            : base(message)
        {
        }

        public FutureAbortedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FutureAbortedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
    #endregion
}
