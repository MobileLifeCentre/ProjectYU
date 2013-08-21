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
//     file="Future.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       Future.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Future opperations - simplified multithreading.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Threading
{
    using System;

    #region Future

    /// <summary>
    /// Future operation that has no return value
    /// </summary>
    [CLSCompliant(true)]
    public class Future : FutureBase
    {
        private readonly Action _futureDelegate;

        internal Future(Action action)
        {
            this._futureDelegate = action;
        }

        protected override void RunFuture()
        {
            this._futureDelegate();
        }

        public void AddCompletedCallback(Action<Future> completedAction)
        {
            ////Action<FutureBase> completedActionDelegate = (x) => completedAction((Future<T>)x);
            ////((FutureBase)this).AddCompletedCallback(completedActionDelegate);
            this.AddCompletedCallback(() => completedAction(this));
        }

        /// <summary>
        /// Create a Future the the specific action and run it (asynchrnously) immediatly
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Future Create(Action action)
        {
            Future future = new Future(action);
            future.RunAsync();

            return future;
        }

        public static Future Create<TArg>(Action<TArg> action, TArg arg)
        {
            // Action  actionDelegate = delegate { action(arg); };

            // return Create(actionDelegate);
            return Create(() => action(arg));
        }

        public static Future Create<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            return Create(() => action(arg1, arg2));
        }

        public static Future CreateWithoutRunning(Action action)
        {
            return new Future(action);
        }
    }

    #endregion

    #region Future<T>

    /// <summary>
    /// Future operation that returns a value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [CLSCompliant(true)]
    public class Future<T> : FutureBase, IFuture<T>
    {
        // Not typed - because we set it as volatile to avoid a lock but we need to box it ! 
        private volatile object _value;

        private readonly Func<T> _futureDelegate;

        internal Future(Func<T> function)
        {
            this._futureDelegate = function;
        }

        /// <summary>
        /// Gets the result of the future
        /// </summary>
        public T Value
        {
            get
            {
                this.Wait();
                return (T)this._value;
            }
        }

        /// <summary>
        /// Add a completed action on the Future<T>
        /// </summary>
        /// <param name="completedAction"></param>
        public void AddCompletedCallback(Action<Future<T>> completedAction)
        {
            ////Action<FutureBase> completedActionDelegate = (x) => completedAction((Future<T>)x);
            ////((FutureBase)this).AddCompletedCallback(completedActionDelegate);
            this.AddCompletedCallback(() => completedAction(this));
        }

        protected override void RunFuture()
        {
            // Update the value
            this._value = this._futureDelegate();
        }

        /// <summary>
        /// Create a Future the the specific function and run it (asynchrnously) immediatly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Future<T> Create(Func<T> function)
        {
            Future<T> future = new Future<T>(function);
            future.RunAsync();

            return future;
        }

        public static Future<T> Create<TArg>(Func<TArg, T> function, TArg arg)
        {
            ////Func<TReturn> functionDelegate = delegate { return function(arg); };
            ////return Create(functionDelegate);
            return Create(() => function(arg));
        }

        public static Future<T> Create<TArg1, TArg2>(Func<TArg1, TArg2, T> function, TArg1 arg1, TArg2 arg2)
        {
            return Create(() => function(arg1, arg2));
        }

        public static Future<T> CreateWithoutRunning(Func<T> function)
        {
            return new Future<T>(function);
        }

        public static Future<T> CreateWithoutRunning<TArg>(Func<TArg, T> function, TArg arg)
        {
            return new Future<T>(() => function(arg));
        }
    }
    #endregion
}
