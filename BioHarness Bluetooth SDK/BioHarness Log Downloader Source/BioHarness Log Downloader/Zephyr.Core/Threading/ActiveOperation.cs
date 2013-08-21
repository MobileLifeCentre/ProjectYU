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
//     file="ActiveOperation.cs" 
//     company="Zephyr Technology">
//     Copyright (c) Zephyr Technology. All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////
/*!
    \file       ActiveOperation.cs
    \author     $Author: chriss $:
    \date       $Date: 2008-12-18 17:38:25 +1300 (Thu, 18 Dec 2008) $:
    \version    $Revision: 6719 $:
    \brief      Responsable to Wait the end of a operation
                To wait a thread : Join(not very efficient), ManualResetEvent (better) but cost a handle (not infinite)
                Main idea behind it's to create a handle (waitEvent) only if it's necessary to optimize the use of handles
                Don't create it or shared it if possible.
*/
////////////////////////////////////////////////////////////

namespace Zephyr.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Responsable to Wait the end of a operation
    /// To wait a thread : Join(not very efficient), ManualResetEvent (better) but cost a handle (not infinite)
    /// Main idea behind it's to create a handle (waitEvent) only if it's necessary to optimize the use of handles
    /// Don't create it or shared it if possible.
    /// </summary>
    internal sealed class ActiveOperation
    {
        private int _hasCompleted;
        private ManualResetEvent _waitEvent;

        internal bool HasCompleted
        {
            get { return this._hasCompleted == 1; }
        }

        internal ActiveOperation()
        {
        }

        /// <summary>
        /// Called when the operation complete
        /// Deal with the following cases :
        /// Another thread already called Completed.  
        /// Completed called before another thread calls Wait.  
        /// Completed called while or after another thread calls Wait.   
        /// </summary>
        internal void Completed()
        {
            // Signal it's completed
            if (Interlocked.CompareExchange(ref this._hasCompleted, 1, 0) == 0)
            {
                // if it was not, signal the event (if necessary)
                try
                {
                    ManualResetEvent waitEvent = this._waitEvent;
                    if (waitEvent != null)
                    {
                        waitEvent.Set();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // If another thread is in Wait at the same time and sees the completed flag
                    // it may dispose of the shared event.  In this case there is no need to signal
                    // just return.
                }
            }
        }

        /// <summary>
        /// Wait for the operation to complete
        /// HasCompleted already set 
        /// Second or later thread to call Wait and needs to wait on _waitEvent 
        /// While attempting to create _waitEvent, another thread in Wait finishes first. 
        /// Thread successfully creates and owns the shared _waitEvent variable before Completed() is called. 
        /// During the creation of _waitEvent, another thread calls Completed() in which case there is no guarantee that _waitEvent will be signaled.
        /// </summary>
        internal void Wait()
        {
            // Already completed
            if (this._hasCompleted == 1)
            {
                return;
            }

            // Already create a event used it
            if (this._waitEvent != null)
            {
                WaitOnEvent(this._waitEvent);
            }

            ManualResetEvent createdEvent = null;
            ManualResetEvent originalEvent = null;

            try
            {
                createdEvent = new ManualResetEvent(false);

                // Affect the new created event to the _waitEvent, keep memory of the original state of _waitEvent.
                originalEvent = Interlocked.CompareExchange<ManualResetEvent>(ref this._waitEvent, createdEvent, null);

                // There are 2 race conditions that we can encounter at this point.
                // 1) Two separate threads created event objects.  In that case if we are the second
                //    such thread the "originalEvent" variable will be non-null.  Destroy the created
                //    event and wait on the first event
                // 2) Between our original completion check and the time we set the event OnCompleted
                //    was called.  Now that the event has been created re-check the has completed value 
                //    and destroy the event if necessary
                if (originalEvent != null)
                {
                    // Another thread got here first. Destroy the created event and wait on the original
                    createdEvent.Close();
                    createdEvent = null; // doesn't get reDisposed

                    WaitOnEvent(originalEvent);
                }
                else if (this._hasCompleted == 1)
                {
                    // In between the time we checked for completion and created the event a completion
                    // occurred.  Returning will dispose of _waitEvent and force other threads Wait to complete 
                    createdEvent.Close();
                    createdEvent = null;     // So it doesn't get re-disposed
                }
                else
                {
                    createdEvent.WaitOne();
                }
            }
            finally
            {
                /* If the created event is _waitEvent we need to swap it out for null
                   we are the owner of the handle all other thread are just a shared handle (_waitEVent is not null)
                   As finish reset it to null*/
                if (originalEvent == null)
                {
                    Interlocked.Exchange<ManualResetEvent>(ref this._waitEvent, null);
                }

                // Release the handle
                if (createdEvent != null)
                {
                    createdEvent.Close();
                }
            }
        }

        // Wait on the shared event
        private static void WaitOnEvent(ManualResetEvent evt)
        {
            // There is a race condition where this event can get disposed
            // while we're still in a wait.  Not a problem because all we
            // care about is the event getting hit
            try
            {
                evt.WaitOne();
            }
            catch (ObjectDisposedException) 
            {
            }
        }
    }
}
