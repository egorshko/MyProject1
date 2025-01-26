using Shared.Disposable;
using System;
using System.Threading.Tasks;

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
                return Task.CompletedTask;
            });
        }

        public async Task AsyncDispose()
        {
            _onInvoked = null;
            await Task.CompletedTask;
        }
    }
}

