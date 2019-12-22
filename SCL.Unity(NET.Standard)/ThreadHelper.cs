using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace SCL.Unity
{
    public class ThreadHelper : MonoBehaviour
    {
        private static ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

        private static Action temp;

        public static void InvokeOnMain(Action action)
        {
            mainThreadActions.Enqueue(action);
        }

        public void FixedUpdate()
        {
            while (mainThreadActions.TryDequeue(out temp))
            {
                temp.Invoke();
            }
        }
    }
}