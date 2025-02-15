using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Miner.LoadingScreen 
{
    public sealed class LoadingScreen
    {
        public class Entity : BaseDisposable
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;

                    public Data Data;
                }

                private Ctx _ctx;

                public Logic(Ctx ctx)
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
                    _ctx.Data.CanvasGroup.gameObject.SetActive(true);

                    var delayMs = 50;
                    var deltaTime = delayMs / 1000f;

                    var timer = _ctx.Data.ShowHideDuration;
                    while (timer >= 0f)
                    {
                        _ctx.Data.CanvasGroup.alpha = 1f - (timer / _ctx.Data.ShowHideDuration);
                        timer -= deltaTime;
                        await UniTask.Delay(delayMs, true);
                    }

                    _ctx.Data.CanvasGroup.alpha = 1f;
                    _ctx.Data.CanvasGroup.gameObject.SetActive(false);
                }

                public void HideImmediate()
                {
                    _ctx.Data.CanvasGroup.alpha = 0f;
                }

                public async UniTask Hide()
                {
                    _ctx.Data.CanvasGroup.alpha = 1f;
                    _ctx.Data.CanvasGroup.gameObject.SetActive(true);

                    var delayMs = 50;
                    var deltaTime = delayMs / 1000f;

                    var timer = _ctx.Data.ShowHideDuration;
                    while (timer >= 0f)
                    {
                        _ctx.Data.CanvasGroup.alpha = timer / _ctx.Data.ShowHideDuration;
                        timer -= deltaTime;
                        await UniTask.Delay(delayMs, true);
                    }

                    _ctx.Data.CanvasGroup.alpha = 0f;
                    _ctx.Data.CanvasGroup.gameObject.SetActive(false);
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
            }

            private Ctx _ctx;

            private readonly Logic _loadingScreenLogic;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _loadingScreenLogic = new Logic(new Logic.Ctx
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

        [Serializable]
        public struct Data
        {
            [SerializeField] private float _showHideDuration;
            [SerializeField] private CanvasGroup _canvasGroup;
            [SerializeField] private Transform _spinnerView;
            [SerializeField] private float _rotateSpeed;

            public readonly float ShowHideDuration => _showHideDuration;
            public readonly CanvasGroup CanvasGroup => _canvasGroup;
            public readonly Transform SpinnerView => _spinnerView;
            public readonly float RotationSpeed => _rotateSpeed;
        }
    }
}

