using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class CoroutineAwaiter
{
    public static CoroutineAwaiterWrapper GetAwaiter(this IEnumerator coroutine)
    {
        return new CoroutineAwaiterWrapper(coroutine);
    }

    public class CoroutineAwaiterWrapper : INotifyCompletion
    {
        private IEnumerator coroutine;
        private Action continuation;

        public CoroutineAwaiterWrapper(IEnumerator coroutine)
        {
            this.coroutine = coroutine;
            RunCoroutine();
        }

        private async void RunCoroutine()
        {
            while (coroutine.MoveNext())
            {
                await Task.Yield();
            }

            continuation?.Invoke();
        }

        public bool IsCompleted => coroutine.Current == null;

        public void GetResult() { }

        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
    }
}
