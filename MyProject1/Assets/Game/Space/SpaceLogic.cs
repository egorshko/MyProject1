using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.Space
{
    public class SpaceLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Action ShowMenu;
        }

        private Ctx _ctx;

        public SpaceLogic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnUpdate.Subscribe(deltaTime =>
            {
                if (Input.GetKeyUp(KeyCode.Escape)) _ctx.ShowMenu.Invoke();
            }).AddTo(this);
        }
    }
}

