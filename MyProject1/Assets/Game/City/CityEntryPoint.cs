using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.City 
{
    public class CityEntryPoint : BaseDisposableMB
    {
        [Serializable]
        public struct Data
        {

        }

        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Action ToSpace;
        }

        [SerializeField] private Data _data;

        private Ctx _ctx;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnUpdate.Subscribe(deltaTime =>
            {
                if (Input.GetKeyUp(KeyCode.Escape)) _ctx.ToSpace.Invoke();
            }).AddTo(this);
        }
    }
}

