using Cysharp.Threading.Tasks;
using System;

namespace Shared.Disposable
{
    public class DisposeObserver : ITaskDisposable
    {
        private Func<UniTask> _onDispose;

        public DisposeObserver(Func<UniTask> onDispose)
        {
            _onDispose = onDispose;
        }

        public async UniTask AsyncDispose()
        {
            await _onDispose.Invoke();
        }
    }
}

