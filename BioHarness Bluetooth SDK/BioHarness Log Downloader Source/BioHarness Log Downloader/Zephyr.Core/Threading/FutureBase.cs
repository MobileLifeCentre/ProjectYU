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
//     file="FutureBase.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       FutureBase.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Represents an operation executing concurrently
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Represents an operation executing concurrently
    /// Abstract class
    /// </summary>
    [CLSCompliant(true)]
    public abstract class FutureBase : IFuture
    {
        // Manage the completed Action
        private readonly object _completedActionListLock = new object();
        private List<Action> _completedActionList;
        private Exception _error;
        private int _aborted;

        // Waiting process
        private readonly ActiveOperation _operation;

        private int _started;

        // Internal ctor
        internal FutureBase()
        {
            this._operation = new ActiveOperation();
        }

        /// <summary>
        /// Gets a value indicating whether the Future started running 
        /// </summary>
        public bool HasStarted
        {
            get { return this._started == 1; }
        }

        internal Exception Error
        {
            get { return this._error; }
        }

        #region IFuture
        /// <summary>
        /// Gets a value indicating whether or not the future has completed
        /// </summary>
        public bool HasCompleted
        {
            get { return this._operation.HasCompleted; }
        }

        /// <summary>
        /// Waits for the ActiveOperation to complete and throws as necessary
        /// </summary>
        public void Wait()
        {
            this._operation.Wait();

            if (this._error != null)
            {
                throw new FutureException(this._error.Message, this._error);
            }
            else if (this._aborted != 0)
            {
                throw new FutureAbortedException("Future was aborted before being run.");
            }
        }

        #endregion

        #region Run

        /// <summary>
        /// Run this future asynchrnously in the thread pool
        /// </summary>
        public void RunAsync()
        {
            this.CheckForDoubleRun();
            ThreadPool.QueueUserWorkItem((x) => this.RunFutureWrapper(), null);
        }

        /// <summary>
        /// Run the future synchronously immediately
        /// </summary>
        public void RunSync()
        {
            this.CheckForDoubleRun();
            this.RunFutureWrapper();
        }

        /// <summary>
        /// Called when the running of a Future is aborted for whatever reason
        /// </summary>
        public void Abort()
        {
            // Prevent it's not already started
            this.CheckForDoubleRun();
            Interlocked.Exchange(ref this._aborted, 1);
            this._operation.Completed();
            this.FutureCompleted();
        }

        private void RunFutureWrapper()
        {
            try
            {
                this.RunFuture();
            }
            catch (Exception ex)
            {
                Interlocked.Exchange(ref this._error, ex);
            }
            finally
            {
                this._operation.Completed();
                this.FutureCompleted();
            }
        }

        private void CheckForDoubleRun()
        {
            if (Interlocked.CompareExchange(ref this._started, 1, 0) != 0)
            {
                throw new InvalidOperationException("Multiple calls to Run are not allowed.");
            }
        }

        /// <summary>
        /// Called when the future finishes running
        /// </summary>
        private void FutureCompleted()
        {
            Contract.ThrowIfFalse(this.HasCompleted);

            // Create a local list for the completed action and clear the list
            List<Action> localList = null;
            lock (this._completedActionListLock)
            {
                if (this._completedActionList != null)
                {
                    localList = new List<Action>(this._completedActionList);
                    this._completedActionList.Clear();
                }
            }

            if (localList != null)
            {
                foreach (var completedAction in localList)
                {
                    completedAction();
                }
            }

            // Call derived complementary method
            this.OnFutureCompleted();
        }

        /// <summary>
        /// Used in a derived class to actually run a Future
        /// </summary>
        protected abstract void RunFuture();

        protected virtual void OnFutureCompleted()
        {
            // Nothing by default
        }

        #endregion

        #region Callback

        //// Note useful for the moment - I prefer removed this function - useful only in a IAsyncResult context
        /////// <summary>
        /////// Creates a WaitHandle which will be set once the future is completed.  No
        /////// caching is involved here and it's the callers responsibility to free up
        /////// the handle
        /////// </summary>
        /////// <returns></returns>
        ////public ManualResetEvent CreateWaitHandle()
        ////{
        ////    // Create the WaitHandle
        ////    var handle = new ManualResetEvent(false);

        ////    // Add the signal action as completed action
        ////    Action action = delegate
        ////    {
        ////        try
        ////        {
        ////            handle.Set();
        ////        }
        ////        catch (ObjectDisposedException)
        ////        {
        ////            // No need to error out if the caller has already freed up 
        ////            // the event
        ////        }
        ////    };
        ////    AddCompletedCallback(action);

        ////    return handle;
        ////}

        /// <summary>
        /// internal access redefine into the derived class
        /// Add a delegate to be called once the future is completed.  If the future is 
        /// already completed this may be invoked immediately
        /// </summary>
        /// <param name="completedAction"></param>
        internal void AddCompletedCallback(Action completedAction)
        {
            Contract.ThrowIfNull(completedAction);

            bool runNow = false;
            lock (this._completedActionListLock)
            {
                if (this.HasCompleted)
                {
                    runNow = true;
                }
                else
                {
                    if (this._completedActionList == null)
                    {
                        this._completedActionList = new List<Action>();
                    }

                    this._completedActionList.Add(completedAction);
                }
            }

            if (runNow)
            {
                completedAction();
            }
        }

        #endregion
    }
}
