using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class CoroutineAwaiter
{
    // �ڷ�ƾ�� Task�� ��ȯ�ϴ� �޼���
    public static Task ToTask(this IEnumerator coroutine, MonoBehaviour monoBehaviour)
    {
        var tcs = new TaskCompletionSource<bool>();
        monoBehaviour.StartCoroutine(RunCoroutine(coroutine, tcs));
        return tcs.Task;
    }

    private static IEnumerator RunCoroutine(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return coroutine;  // �ڷ�ƾ �Ϸ� ���
        tcs.SetResult(true);     // �Ϸ� �� Task�� ��� ����
    }

    // awaiter�� �����ϴ� ���� �ڵ�
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
                await Task.Yield();  // ���� �����ӱ��� ���
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
