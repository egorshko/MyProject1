using System;
using System.Threading.Tasks;

namespace Shared.Disposable
{
    public class DisposeObserver : ITaskDisposable
    {
        private Func<Task> _onDispose;

        public DisposeObserver(Func<Task> onDispose)
        {
            _onDispose = onDispose;
        }

        public async Task AsyncDispose()
        {
            await _onDispose.Invoke();
        }
    }
}

