using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace NSL.Utils.Unity
{
    public class ThreadHelper : MonoBehaviour
    {
        private static ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

        private static Action temp;

        private static ThreadHelper Instance;

        void Start()
        {
            Instance = this;
        }

        public static void InvokeOnMain(Action action)
        {
            mainThreadActions.Enqueue(action);
        }

        public void FixedUpdate()
        {
            InvokeQueue();
        }

        public static void StartCoroutine(IEnumerator enumerator)
        {
            ((MonoBehaviour)Instance).StartCoroutine(enumerator);
        }

        public static void InvokeQueue()
        {
            while (mainThreadActions.TryDequeue(out temp))
            {
                temp.Invoke();
            }
        }
    }
}