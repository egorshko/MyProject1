using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.World
{
    public class WorldEntity : BaseDisposable
    {
        public struct Ctx
        {
            public Vector3 WorldPos;
            public Quaternion WorldRot;
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public WorldEntryPoint.Data Data;
            public Action ShowMenu;
            public IReadOnlyReactiveValue<bool> IsShowMenu;
            public Action<(WorldEntryPoint.World world, Vector3 worldPos, Vector3 worldRot)> LoadWorld;
        }

        private Ctx _ctx;

        public WorldEntity(Ctx ctx)
        {
            _ctx = ctx;

            _ = new WorldLogic(new WorldLogic.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                ShowMenu = _ctx.ShowMenu,
            }).AddTo(this);

            _ = new WorldCameraLogic(new WorldCameraLogic.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                PlayerTransform = _ctx.Data.PlayerData.PlayerGO.transform,
                Data = _ctx.Data.CameraData,
            }).AddTo(this);

            _ = new WorldCharacterLogic(new WorldCharacterLogic.Ctx
            {
                WorldPos = _ctx.WorldPos,
                WorldRot = _ctx.WorldRot,
                OnUpdate = _ctx.OnUpdate,
                Data = _ctx.Data.PlayerData,
                IsShowMenu = _ctx.IsShowMenu,
                LoadWorld = _ctx.LoadWorld,
            }).AddTo(this);
        }
    }
}

