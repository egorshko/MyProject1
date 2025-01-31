using Cysharp.Threading.Tasks;

namespace Shared.Disposable
{
    public interface ITaskDisposable
    {
        public UniTask AsyncDispose();
    }
}

