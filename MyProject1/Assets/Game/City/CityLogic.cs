using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.City 
{
    public class CityLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Action ToSpace;
        }

        private Ctx _ctx;

        public CityLogic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnUpdate.Subscribe(deltaTime =>
            {
                if (Input.GetKeyUp(KeyCode.Escape)) _ctx.ToSpace.Invoke();
            }).AddTo(this);
        }
    }
}

