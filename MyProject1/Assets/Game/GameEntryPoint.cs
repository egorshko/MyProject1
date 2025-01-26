using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game 
{
    public class GameEntryPoint : BaseDisposableMB
    {
        [Serializable]
        public struct Data
        {

        }

        [SerializeField] private Data _data;

        private IReactiveCommand<float> _onUpdate;

        private void OnEnable()
        {
            _onUpdate = new ReactiveCommand<float>().AddTo(this);
            _ = new GameEntity(new GameEntity.Ctx
            {
                OnUpdate = _onUpdate,
                Data = _data,
            }).AddTo(this);
        }

        private void Update()
        {
            _onUpdate.Execute(Time.deltaTime);
        }
    }
}

