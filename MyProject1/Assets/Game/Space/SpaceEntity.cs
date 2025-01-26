using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.Space 
{
    public class SpaceEntity : BaseDisposable
    {
        public struct Ctx
        {
            public IReactiveValue<Vector3> SpacePos;
            public IReactiveValue<Quaternion> SpaceRot;
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public SpaceEntryPoint.Data Data;
            public Action ShowMenu;
            public IReadOnlyReactiveValue<bool> IsShowMenu;
            public Action<GameObject> LoadCity;
        }

        private Ctx _ctx;

        public SpaceEntity(Ctx ctx)
        {
            _ctx = ctx;

            _ = new SpaceLogic(new SpaceLogic.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,
                ShowMenu = _ctx.ShowMenu,
            }).AddTo(this);

            _ = new SpaceCameraLogic(new SpaceCameraLogic.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,
                PlayerTransform = _ctx.Data.PlayerData.PlayerGO.transform,
                Data = _ctx.Data.CameraData,
            }).AddTo(this);

            _ = new SpaceCharacterLogic(new SpaceCharacterLogic.Ctx
            {
                SpacePos = _ctx.SpacePos,
                SpaceRot = _ctx.SpaceRot,
                OnUpdate = _ctx.OnUpdate,
                Data = _ctx.Data.PlayerData,
                IsShowMenu = _ctx.IsShowMenu,
                LoadCity = _ctx.LoadCity,
                Cities = _ctx.Data.Cities,
            }).AddTo(this);
        }
    }
}

