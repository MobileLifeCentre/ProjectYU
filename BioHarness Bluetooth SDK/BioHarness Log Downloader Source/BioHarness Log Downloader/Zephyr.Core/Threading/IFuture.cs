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
//     file="IFuture.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       IFuture.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Interface for Future Operation classes
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Threading
{
    using System;

    /// <summary>
    /// Interface for Future Operation class with no return value
    /// </summary>
    [CLSCompliant(true)]
    public interface IFuture
    {
        /// <summary>
        /// Gets a value indicating whether or not the future has completed
        /// </summary>
        bool HasCompleted
        {
            get;
        }

        /// <summary>
        /// Wait for the future to complete
        /// </summary>
        void Wait();
    }

    /// <summary>
    /// Interface for Future Operation class with return value
    /// </summary>
    /// <typeparam name="T">Type of the return value</typeparam>
    [CLSCompliant(true)]
    public interface IFuture<T> : IFuture
    {
        /// <summary>
        /// Gets the return value. Wait for the future to complete and return the value
        /// </summary>
        /// <returns></returns>
        T Value { get; }
    }
}
