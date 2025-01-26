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
            [SerializeField] private SpaceCharacterMoverLogic.Data _playerData;
            [SerializeField] private SpaceCameraMoverLogic.Data _cameraData;
            [SerializeField] private GameObject[] _cities;

            public readonly SpaceCharacterMoverLogic.Data PlayerData => _playerData;
            public readonly SpaceCameraMoverLogic.Data CameraData => _cameraData;
            public readonly GameObject[] Cities => _cities;
        }

        public struct Ctx 
        {
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
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
                ShowMenu = _ctx.ShowMenu,
                IsShowMenu = _ctx.IsShowMenu,
                LoadCity = _ctx.LoadCity,
            }).AddTo(this);
        }
    }
}

