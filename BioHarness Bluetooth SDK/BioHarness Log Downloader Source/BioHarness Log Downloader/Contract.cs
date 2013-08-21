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
//     file="Contract.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       Contract.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Performs tests and throws exceptions in event of error.
*/
////////////////////////////////////////////////////////////

namespace Zephyr
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Performs tests and throws exceptions in event of error.
    /// </summary>
    [CLSCompliant(true)]
    public static class Contract
    {
        /// <summary>
        /// Throw a Contract exception with supplied message
        /// </summary>
        /// <param name="message">Exception message</param>
        public static void Violation(string message)
        {
            ViolationCore(message);
        }

        /// <summary>
        /// Throw a Contract exception with supplied formatted message and parameters
        /// </summary>
        /// <param name="format">The format string used to format the message</param>
        /// <param name="args">The arguments to include in the message</param>
        public static void Violation(string format, params object[] args)
        {
            ViolationCore(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        /// <summary>
        /// Throw a Contract exception with supplied formatted message and parameters
        /// </summary>
        /// <param name="provider"> An System.IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="format">The format string used to format the message</param>
        /// <param name="args">The arguments to include in the message</param>
        public static void Violation(IFormatProvider provider, string format, params object[] args)
        {
            ViolationCore(string.Format(provider, format, args));
        }

        #region Object Null?

        /// <summary>
        /// Throw exception with the default message if the supplied object is not null
        /// </summary>
        /// <param name="value">Object to test</param>
        public static void ThrowIfNotNull(object value)
        {
            ThrowIfNotNull(value, "Value should be null.");
        }

        /// <summary>
        /// Throw exception with the supplied message if the supplied object is not null
        /// </summary>
        /// <param name="value">Object to test</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfNotNull(object value, string message)
        {
            if (value != null)
            {
                ViolationCore(message);
            }
        }

        /// <summary>
        /// Throw exception with the default message if the supplied object is null
        /// </summary>
        /// <param name="value">Object to test</param>
        public static void ThrowIfNull(object value)
        {
            ThrowIfNull(value, "Value should not be null.");
        }

        /// <summary>
        /// Throw exception with the supplied message if the supplied object is null
        /// </summary>
        /// <param name="value">Object to test</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfNull(object value, string message)
        {
            if (value == null)
            {
                ViolationCore(message);
            }
        }
        #endregion

        #region True or False ?
        /// <summary>
        /// Throw exception with the default message if the supplied bool is false
        /// </summary>
        /// <param name="value">Bool to test</param>
        public static void ThrowIfFalse(bool value)
        {
            ThrowIfFalse(value, "Unexpected false (should not be false)");
        }

        /// <summary>
        /// Throw exception with the default message if the supplied bool is false
        /// </summary>
        /// <param name="value">Bool to test</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfFalse(bool value, string message)
        {
            if (value == false)
            {
                ViolationCore(message);
            }
        }

        /// <summary>
        /// Throw exception with the default message if the supplied bool is true
        /// </summary>
        /// <param name="value">Bool to test</param>
        public static void ThrowIfTrue(bool value)
        {
            ThrowIfTrue(value, "Unexpected true (should not be true)");
        }

        /// <summary>
        /// Throw exception with the default message if the supplied bool is true
        /// </summary>
        /// <param name="value">Bool to test</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfTrue(bool value, string message)
        {
            if (value == true)
            {
                ViolationCore(message);
            }
        }
        #endregion

        #region String Null Or Empty ?
        /// <summary>
        /// Throw exception with the default message if the supplied string is null or empty
        /// </summary>
        /// <param name="value">String to test</param>
        public static void ThrowIfNullOrEmpty(string value)
        {
            ThrowIfNullOrEmpty(value, "String is null or empty.");
        }

        /// <summary>
        /// Throw exception with the default message if the supplied string is null or empty
        /// </summary>
        /// <param name="value">String to test</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfNullOrEmpty(string value, string message)
        {
            if (string.IsNullOrEmpty(value) == true)
            {
                ViolationCore(message);
            }
        }
        #endregion

        #region Equal or NotEqual ?
        /// <summary>
        /// Throw exception with the default message if the supplied objects are equal
        /// </summary>
        /// <typeparam name="T">Type to compare</typeparam>
        /// <param name="left">First object to compare</param>
        /// <param name="right">Second object to compare</param>
        public static void ThrowIfEqual<T>(T left, T right)
        {
            ThrowIfEqual<T>(left, right, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Throw exception with the default message if the supplied objects are equal
        /// </summary>
        /// <typeparam name="T">Type to compare</typeparam>
        /// <param name="left">First object to compare</param>
        /// <param name="right">Second object to compare</param>
        /// <param name="comparer">Equality Comparer - defining methods to support the comparison</param>
        public static void ThrowIfEqual<T>(T left, T right, IEqualityComparer<T> comparer)
        {
            ThrowIfEqual<T>(left, right, comparer, "Values should not be equal.");
        }

        /// <summary>
        /// Throw exception with the supplied message if the supplied objects are equal
        /// </summary>
        /// <typeparam name="T">Type to compare</typeparam>
        /// <param name="left">First object to compare</param>
        /// <param name="right">Second object to compare</param>
        /// <param name="comparer">Equality Comparer - defining methods to support the comparison</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfEqual<T>(T left, T right, IEqualityComparer<T> comparer, string message)
        {
            if (comparer.Equals(left, right) == true)
            {
                ViolationCore(message);
            }
        }

        /// <summary>
        /// Throw exception with the default message if the supplied objects are not equal
        /// </summary>
        /// <typeparam name="T">Type to compare</typeparam>
        /// <param name="left">First object to compare</param>
        /// <param name="right">Second object to compare</param>
        public static void ThrowIfNotEqual<T>(T left, T right)
        {
            ThrowIfNotEqual<T>(left, right, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Throw exception with the default message if the supplied objects are not equal
        /// </summary>
        /// <typeparam name="T">Type to compare</typeparam>
        /// <param name="left">First object to compare</param>
        /// <param name="right">Second object to compare</param>
        /// <param name="comparer">Equality Comparer - defining methods to support the comparison</param>
        public static void ThrowIfNotEqual<T>(T left, T right, IEqualityComparer<T> comparer)
        {
            ThrowIfNotEqual<T>(left, right, comparer, "Values should be equal.");
        }

        /// <summary>
        /// Throw exception with the supplied message if the supplied objects are not equal
        /// </summary>
        /// <typeparam name="T">Type to compare</typeparam>
        /// <param name="left">First object to compare</param>
        /// <param name="right">Second object to compare</param>
        /// <param name="comparer">Equality Comparer - defining methods to support the comparison</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfNotEqual<T>(T left, T right, IEqualityComparer<T> comparer, string message)
        {
            if (comparer.Equals(left, right) == false)
            {
                ViolationCore(message);
            }
        }
        #endregion

        #region Type ?
        /// <summary>
        /// Throw exception with the default message if the supplied object is not the specified type
        /// </summary>
        /// <typeparam name="T">Type to check for</typeparam>
        /// <param name="value">Object to check</param>
        /// <returns>Object as the supplied type</returns>
        public static T ThrowIfNotType<T>(object value)
        {
            return ThrowIfNotType<T>(value, String.Format(System.Globalization.CultureInfo.InvariantCulture, "Value ({1}) is not the correct type {0}.", typeof(T).Name, value.GetType().Name));
        }

        /// <summary>
        /// Throw exception with the default message if the supplied object is not the specified type
        /// </summary>
        /// <typeparam name="T">Type to check for</typeparam>
        /// <param name="value">Object to check</param>
        /// <param name="message">Message to include in exception</param>
        /// <returns>Object as the supplied type</returns>
        public static T ThrowIfNotType<T>(object value, string message)
        {
            T temp = default(T);
            try
            {
                temp = (T)value;
            }
            catch (InvalidCastException)
            {
                ViolationCore(message);
            }

            return temp;
        }
        #endregion

        #region On the correct thread ?

        /// <summary>
        /// Throw exception with the default message if the supplied threadId does not match the current threadId
        /// </summary>
        /// <param name="threadId">ThreadId to check</param>
        public static void ThrowIfNotOnThread(int threadId)
        {
            ThrowIfNotOnThread(threadId, String.Format(System.Globalization.CultureInfo.InvariantCulture, "Called on the wrong thread {0}.", threadId.ToString(CultureInfo.CurrentCulture)));
        }

        /// <summary>
        /// Throw exception with the default message if the supplied threadId does not match the current threadId
        /// </summary>
        /// <param name="threadId">ThreadId to check</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfNotOnThread(int threadId, string message)
        {
            if (Thread.CurrentThread.ManagedThreadId != threadId)
            {
                ViolationCore(message);
            }
        }

        /// <summary>
        /// Throw exception with the default message if the supplied threadId matchs the current threadId
        /// </summary>
        /// <param name="threadId">ThreadId to check</param>
        public static void ThrowIfOnThread(int threadId)
        {
            ThrowIfOnThread(threadId, String.Format(System.Globalization.CultureInfo.InvariantCulture, "Called on the wrong thread {0}.", threadId.ToString(CultureInfo.CurrentCulture)));
        }

        /// <summary>
        /// Throw exception with the default message if the supplied threadId matchs the current threadId
        /// </summary>
        /// <param name="threadId">ThreadId to check</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfOnThread(int threadId, string message)
        {
            if (Thread.CurrentThread.ManagedThreadId == threadId)
            {
                ViolationCore(message);
            }
        }
        #endregion

        #region Thread call marshalling ?
        /// <summary>
        /// Throw exception with the default message if the supplied invokation requires syncronisation (cross thread call) 
        /// </summary>
        /// <param name="invoke">Mechanism to execute a delegate</param>
        public static void ThrowIfSynchronizeRequired(ISynchronizeInvoke invoke)
        {
            ThrowIfSynchronizeRequired(invoke, "cross-thread call (synchronization required).");
        }

        /// <summary>
        /// Throw exception with the default message if the supplied invokation requires syncronisation (cross thread call) 
        /// </summary>
        /// <param name="invoke">Mechanism to execute a delegate</param>
        /// <param name="message">Message to include in exception</param>
        public static void ThrowIfSynchronizeRequired(ISynchronizeInvoke invoke, string message)
        {
            if (invoke == null)
            {
                throw new ArgumentNullException("invoke");
            }

            if (invoke.InvokeRequired == true)
            {
                ViolationCore(message);
            }
        }
        #endregion

        #region Value enumeration value ?
        /// <summary>
        /// Throw exception with the default message if the supplied object is not an enum or the sypplied type
        /// or if the supplied object value does not match a defined constant in the supplied enum type.
        /// </summary>
        /// <typeparam name="T">Supplied emum type</typeparam>
        /// <param name="value">object which may represent an enum of the suplied type</param>
        public static void ThrowIfInvalidEnumValue<T>(object value)
            where T : struct
        {
            ThrowIfFalse(typeof(T).IsEnum, String.Format(System.Globalization.CultureInfo.InvariantCulture, "Expected an enum type {0}.", typeof(T).Name));

            if (Enum.IsDefined(typeof(T), value) == false)
            {
                Violation(CultureInfo.CurrentCulture, "Invalid Enum value of Type {0}:{1}", typeof(T).Name, value.ToString());
            }
        }
        #endregion

        /// <summary>
        /// Private method to throw a ContractException with the supplied message
        /// </summary>
        /// <param name="message">Message to include in exception</param>
        private static void ViolationCore(string message)
        {
            throw new ContractException(message);
        }
    }
}
