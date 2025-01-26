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
            [SerializeField] private CityCameraLogic.Data _cameraData;
            [SerializeField] private CityBuildLogic.Data _buildData;

            public readonly CityCameraLogic.Data CameraData => _cameraData;
            public readonly CityBuildLogic.Data BuildData => _buildData;
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

            _ = new CityEntity(new CityEntity.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
                ToSpace = _ctx.ToSpace,
            }).AddTo(this);
        }
    }
}

