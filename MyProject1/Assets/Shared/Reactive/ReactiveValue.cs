using Shared.Disposable;
using System;
using System.Threading.Tasks;

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
                return Task.CompletedTask;
            });
        }

        public async Task AsyncDispose() 
        { 
            _onChange = null;
            await Task.CompletedTask;
        }
    }
}

