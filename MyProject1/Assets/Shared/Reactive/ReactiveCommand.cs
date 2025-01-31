using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;

namespace Shared.Reactive 
{
    public interface IReadOnlyReactiveCommand<T> : ITaskDisposable
    {
        public ITaskDisposable Subscribe(Action<T> onChange);
    }

    public interface IReactiveCommand<T> : IReadOnlyReactiveCommand<T>
    {
        public void Execute(T value);
    }

    public class ReactiveCommand<T> : IReactiveCommand<T>
    {
        private Action<T> _onInvoked;

        public void Execute(T value) => _onInvoked?.Invoke(value);

        public ITaskDisposable Subscribe(Action<T> onInvoked)
        {
            _onInvoked += onInvoked;
            return new DisposeObserver(() => 
            {
                _onInvoked -= onInvoked;
                return UniTask.CompletedTask;
            });
        }

        public async UniTask AsyncDispose()
        {
            _onInvoked = null;
            await UniTask.CompletedTask;
        }
    }
}

