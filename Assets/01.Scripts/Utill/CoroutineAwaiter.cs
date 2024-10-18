using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class CoroutineAwaiter
{
    // 코루틴을 Task로 변환하는 메서드
    public static Task ToTask(this IEnumerator coroutine, MonoBehaviour monoBehaviour)
    {
        var tcs = new TaskCompletionSource<bool>();
        monoBehaviour.StartCoroutine(RunCoroutine(coroutine, tcs));
        return tcs.Task;
    }

    private static IEnumerator RunCoroutine(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return coroutine;  // 코루틴 완료 대기
        tcs.SetResult(true);     // 완료 후 Task의 결과 설정
    }

    // awaiter를 제공하는 기존 코드
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
                await Task.Yield();  // 다음 프레임까지 대기
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
