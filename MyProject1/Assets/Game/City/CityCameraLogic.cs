using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.City 
{
    public class CityCameraLogic : BaseDisposable
    {
        [Serializable]
        public struct Data 
        {
            [SerializeField] private float _sense;

            public readonly float Sense => _sense;
        }

        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public IReadOnlyReactiveValue<Vector3> CameraTargetPos;
            public IReadOnlyReactiveValue<Quaternion> CameraTargetRot;
            public Data Data;
        }

        private Ctx _ctx;

        public CityCameraLogic(Ctx ctx)
        {
            _ctx = ctx;

            var camTr = Camera.allCameras[0].transform;

            camTr.SetPositionAndRotation(_ctx.CameraTargetPos.Value, _ctx.CameraTargetRot.Value);

            _ctx.OnUpdate.Subscribe(deltaTime => 
            {
                var camSense = deltaTime * _ctx.Data.Sense;
                camTr.position = Vector3.Lerp(camTr.position, _ctx.CameraTargetPos.Value, camSense);
                camTr.rotation = Quaternion.Lerp(camTr.rotation, _ctx.CameraTargetRot.Value, camSense);
            }).AddTo(this);
        }
    }
}

