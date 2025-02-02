using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using static Game.World.WorldEntryPoint;

namespace Game.World 
{
    public class WorldEntryPoint : BaseDisposableMB
    {
        public enum World : int
        {
            World_Test_0 = -1000,
            World_Test_1,
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private WorldCharacterLogic.Data _playerData;
            [SerializeField] private WorldCameraLogic.Data _cameraData;

            public readonly WorldCharacterLogic.Data PlayerData => _playerData;
            public readonly WorldCameraLogic.Data CameraData => _cameraData;
        }

        public struct Ctx
        {
            public Vector3 WorldPos;
            public Quaternion WorldRot;
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Action ShowMenu;
            public IReadOnlyReactiveValue<bool> IsShowMenu;
            public Action<(World world, Vector3 worldPos, Vector3 worldRot)> LoadWorld;
        }

        [SerializeField] private Data _data;

        private Ctx _ctx;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _ = new WorldEntity(new WorldEntity.Ctx
            {
                WorldPos = _ctx.WorldPos,
                WorldRot = _ctx.WorldRot,
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
                ShowMenu = _ctx.ShowMenu,
                IsShowMenu = _ctx.IsShowMenu,
                LoadWorld = _ctx.LoadWorld,
            }).AddTo(this);
        }
    }
}

