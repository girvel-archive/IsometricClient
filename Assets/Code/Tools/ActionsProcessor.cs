using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Tools
{
    public class ActionsProcessor : MonoBehaviour
    {
        private static readonly Queue<Action> SyncActionQueue = new Queue<Action>();
        private static readonly object ActionQueueLock = new object();

        public static void AddActionToQueue(Action action)
        {
            lock (ActionQueueLock)
            {
                SyncActionQueue.Enqueue(action);
            }
        }

        private void FixedUpdate()
        {
            Action syncAction = null;

            lock (ActionQueueLock)
            {
                if (SyncActionQueue.Count > 0)
                {
                    syncAction = SyncActionQueue.Dequeue();
                }
            }

            if (syncAction != null)
            {
                syncAction();
            }
        }
    }
}