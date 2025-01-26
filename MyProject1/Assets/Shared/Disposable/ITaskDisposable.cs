using System.Threading.Tasks;

namespace Shared.Disposable
{
    public interface ITaskDisposable
    {
        public Task AsyncDispose();
    }
}

