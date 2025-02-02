using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Game.World
{
    public class WorldCameraLogic : BaseDisposable
    {
        [Serializable]
        public struct Data
        {
            [SerializeField] private Vector3 _cameraOffset;
            [SerializeField] private float _cameraSense;

            public readonly Vector3 CameraOffset => _cameraOffset;
            public readonly float CameraSense => _cameraSense;
        }

        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Transform PlayerTransform;
            public Data Data;
        }

        private Ctx _ctx;

        public WorldCameraLogic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnUpdate.Subscribe(deltaTime => 
            {
                var camTr = Camera.allCameras[0].transform;
                var playerTr = _ctx.PlayerTransform;
                var target = playerTr.position + _ctx.Data.CameraOffset;
                var camSense = deltaTime * _ctx.Data.CameraSense;

                camTr.position = Vector3.Lerp(camTr.position, target, camSense);
                var currentAngle = camTr.rotation;
                camTr.LookAt(playerTr);
                var newAngle = camTr.rotation;
                camTr.rotation = Quaternion.Lerp(currentAngle, newAngle, camSense);
            }).AddTo(this);
        }
    }
}

