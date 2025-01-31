using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;

namespace Game.LoadingScreen 
{
    public class LoadingScreenEntity : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public LoadingScreenEntryPoint.Data Data;
        }

        private Ctx _ctx;

        private LoadingScreenLogic _loadingScreenLogic;

        public LoadingScreenEntity(Ctx ctx)
        {
            _ctx = ctx;

            _loadingScreenLogic = new LoadingScreenLogic(new LoadingScreenLogic.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,
                Data = _ctx.Data,
            }).AddTo(this);
        }

        public void ShowImmediate() => _loadingScreenLogic.ShowImmediate();
        public void HideImmediate() => _loadingScreenLogic.HideImmediate();

        public async UniTask Show() => await _loadingScreenLogic.Show();
        public async UniTask Hide() => await _loadingScreenLogic.Hide();
    }
}

