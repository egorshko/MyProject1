using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;

namespace Shared.Reactive 
{
    public interface IReadOnlyReactiveValue<T> : ITaskDisposable
    {
        public T Value { get; }
        public ITaskDisposable Subscribe(Action<T> onChange);
    }

    public interface IReactiveValue<T> : IReadOnlyReactiveValue<T>
    {
        public new T Value { get; set; }
    }

    public class ReactiveValue<T> : IReactiveValue<T>
    {
        private Action<T> _onChange;

        private T _value;
        public T Value 
        {
            get => _value;
            set 
            {
                _value = value;
                _onChange?.Invoke(_value);
            }
        }

        public ReactiveValue(T value) => _value = value;
        public ReactiveValue() => _value = default;

        public ITaskDisposable Subscribe(Action<T> onChange) 
        {
            _onChange += onChange;
            return new DisposeObserver(() => 
            {
                _onChange -= onChange;
                return UniTask.CompletedTask;
            });
        }

        public async UniTask AsyncDispose() 
        { 
            _onChange = null;
            await UniTask.CompletedTask;
        }
    }
}

