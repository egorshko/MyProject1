using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using UnityEngine;

namespace Game.LoadingScreen 
{
    public class LoadingScreenLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public LoadingScreenEntryPoint.Data Data;
        }

        private Ctx _ctx;

        public LoadingScreenLogic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnUpdate.Subscribe(updateTime => 
            {
                _ctx.Data.SpinnerView.rotation *= Quaternion.Euler(_ctx.Data.RotationSpeed * updateTime * Vector3.forward);
            }).AddTo(this);
        }

        public void ShowImmediate()
        {
            _ctx.Data.CanvasGroup.alpha = 1f;
        }

        public async UniTask Show()
        {
            _ctx.Data.CanvasGroup.alpha = 0f;

            var timer = _ctx.Data.ShowHideDuration;
            while(timer >= 0f)
            {
                _ctx.Data.CanvasGroup.alpha = 1f - (timer / _ctx.Data.ShowHideDuration);
                timer -= 50 / 1000f;
                await UniTask.Delay(50, true);
            }

            _ctx.Data.CanvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            _ctx.Data.CanvasGroup.alpha = 0f;
        }

        public async UniTask Hide()
        {
            _ctx.Data.CanvasGroup.alpha = 1f;

            var timer = _ctx.Data.ShowHideDuration;
            while(timer >= 0f)
            {
                _ctx.Data.CanvasGroup.alpha = timer / _ctx.Data.ShowHideDuration;
                timer -= 50 / 1000f;
                await UniTask.Delay(50, true);
            }

            _ctx.Data.CanvasGroup.alpha = 0f;
        }
    }
}

