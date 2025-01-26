using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.Space
{
    public class SpaceEntryPoint : BaseDisposableMB
    {
        [Serializable]
        public struct Data 
        {
            [SerializeField] private SpaceCharacterLogic.Data _playerData;
            [SerializeField] private SpaceCameraLogic.Data _cameraData;
            [SerializeField] private GameObject[] _cities;

            public readonly SpaceCharacterLogic.Data PlayerData => _playerData;
            public readonly SpaceCameraLogic.Data CameraData => _cameraData;
            public readonly GameObject[] Cities => _cities;
        }

        public struct Ctx 
        {
            public IReactiveValue<Vector3> SpacePos;
            public IReactiveValue<Quaternion> SpaceRot;
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Action ShowMenu;
            public IReadOnlyReactiveValue<bool> IsShowMenu;
            public Action<GameObject> LoadCity;
        }

        [SerializeField] private Data _data;

        private Ctx _ctx;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _ = new SpaceEntity(new SpaceEntity.Ctx 
            {
                SpacePos = _ctx.SpacePos,
                SpaceRot = _ctx.SpaceRot,
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
                ShowMenu = _ctx.ShowMenu,
                IsShowMenu = _ctx.IsShowMenu,
                LoadCity = _ctx.LoadCity,
            }).AddTo(this);
        }
    }
}

