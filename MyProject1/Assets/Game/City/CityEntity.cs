using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.City 
{
    public class CityEntity : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public CityEntryPoint.Data Data;
            public Action ToSpace;
        }

        private Ctx _ctx;

        public CityEntity(Ctx ctx)
        {
            _ctx = ctx;

            var nextBuild = new ReactiveCommand<bool>().AddTo(this);
            _ = new CityLogic(new CityLogic.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                ToSpace = _ctx.ToSpace,
            }).AddTo(this);

            var cameraTargetPos = new ReactiveValue<Vector3>().AddTo(this);
            var cameraTargetRot = new ReactiveValue<Quaternion>().AddTo(this);
            _ = new CityBuildLogic(new CityBuildLogic.Ctx
            {
                CurrentCameraPos = cameraTargetPos,
                CurrentCameraRot = cameraTargetRot,
                UseBuild = buildType =>
                {
                    switch ( buildType )
                    {
                        case CityBuildLogic.BuildType.Exit:
                            _ctx.ToSpace.Invoke();
                            break;
                        case CityBuildLogic.BuildType.Ships: 
                            break;
                        case CityBuildLogic.BuildType.Store:
                            break;
                        case CityBuildLogic.BuildType.Bar:
                            break;
                    }
                },
                Data = _ctx.Data.BuildData,
            }).AddTo(this);

            _ = new CityCameraLogic(new CityCameraLogic.Ctx
            {
                OnUpdate = _ctx.OnUpdate,
                Data = _ctx.Data.CameraData,
                CameraTargetPos = cameraTargetPos,
                CameraTargetRot = cameraTargetRot,
            }).AddTo(this);
        }
    }
}

