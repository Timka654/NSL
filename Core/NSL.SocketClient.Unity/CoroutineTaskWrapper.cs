using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace NSL.SocketClient.Unity
{
    public static class CoroutineTaskWrapper
    {
        public static CoroutineTaskAwaiter<TResult> GetAwaiter<TResult>(this TResult yieldInstruction)
            where TResult : AsyncOperation
        {
            return new CoroutineTaskAwaiter<TResult>(yieldInstruction);
        }
    }

    public class CoroutineTaskAwaiter<TResult> : INotifyCompletion
            where TResult : AsyncOperation
    {
        public CoroutineTaskAwaiter(TResult operation)
        {
            Operation = operation;

            operation.completed += (_) =>
            {
                continuation();
            };

            ThreadHelper.StartCoroutine(GetCoroutineEnumerator());
        }

        private IEnumerator GetCoroutineEnumerator()
        {
            yield return Operation;
        }

        public TResult Operation { get; }

        public bool IsCompleted => Operation.isDone;

        Action continuation = () => { };

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
                continuation();

            this.continuation = continuation;
        }

        public TResult GetResult()
        {
            if (!IsCompleted)
                return default;

            return Operation;
        }
    }
}
