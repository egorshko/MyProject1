using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Disposable 
{
    public static class BaseDisposableEx 
    {
        public static T AddTo<T>(this T disposable, IBaseDisposable owner) where T : ITaskDisposable
        {
            owner.AddDisposable(disposable);
            return disposable;
        }
    }

    public interface IBaseDisposable 
    {
        public void AddDisposable(ITaskDisposable disposable);
    }

    public abstract class BaseDisposable : ITaskDisposable, IBaseDisposable
    {
        private readonly Stack<ITaskDisposable> _disposables = new ();

        public async UniTask AsyncDispose()
        {
            while (_disposables.Count > 0)
                await _disposables.Pop().AsyncDispose();
            await OnAsyncDispose();
        }

        public void AddDisposable(ITaskDisposable disposable) => _disposables.Push(disposable);

        protected virtual async UniTask OnAsyncDispose() => await UniTask.CompletedTask;
    }

    public abstract class BaseDisposableMB : MonoBehaviour, ITaskDisposable, IBaseDisposable
    {
        private readonly Stack<ITaskDisposable> _disposables = new();

        protected virtual async void OnDisable() => await AsyncDispose();

        public async UniTask AsyncDispose()
        {
            while (_disposables.Count > 0)
                await _disposables.Pop().AsyncDispose();
            await OnAsyncDispose();
        }

        public void AddDisposable(ITaskDisposable disposable) => _disposables.Push(disposable);

        protected virtual async UniTask OnAsyncDispose() => await UniTask.CompletedTask;
    }
}

