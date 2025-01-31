using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.LoadingScreen
{
    public class LoadingScreenEntryPoint : BaseDisposableMB
    {
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

        public struct Ctx 
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
        }

        [SerializeField] private Data _data;

        private Ctx _ctx;

        private LoadingScreenEntity _loadingScreenEntity;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _loadingScreenEntity = new LoadingScreenEntity(new LoadingScreenEntity.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
            }).AddTo(this);
        }

        public void ShowImmediate() => _loadingScreenEntity.ShowImmediate();
        public void HideImmediate() => _loadingScreenEntity.HideImmediate();

        public async UniTask Show() => await _loadingScreenEntity.Show();
        public async UniTask Hide() => await _loadingScreenEntity.Hide();
    }
}

