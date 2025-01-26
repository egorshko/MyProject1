using System.Threading.Tasks;
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

        public async Task Show()
        {
            _ctx.Data.CanvasGroup.alpha = 0f;

            const int deltaMs = 50;
            var timer = _ctx.Data.ShowHideDuration;
            while(timer >= 0f)
            {
                _ctx.Data.CanvasGroup.alpha = 1f - (timer / _ctx.Data.ShowHideDuration);
                timer -= deltaMs / 1000f;
                await Task.Delay(deltaMs);
            }

            _ctx.Data.CanvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            _ctx.Data.CanvasGroup.alpha = 0f;
        }

        public async Task Hide()
        {
            _ctx.Data.CanvasGroup.alpha = 1f;

            const int deltaMs = 50;
            var timer = _ctx.Data.ShowHideDuration;
            while(timer >= 0f)
            {
                _ctx.Data.CanvasGroup.alpha = timer / _ctx.Data.ShowHideDuration;
                timer -= deltaMs / 1000f;
                await Task.Delay(deltaMs);
            }

            _ctx.Data.CanvasGroup.alpha = 0f;
        }
    }
}

