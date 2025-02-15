using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;

namespace Miner.Camera 
{
    public sealed class Camera
    {
        public class Entity : BaseDisposable
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;

                    public Data Data;
                }

                private Ctx _ctx;

                private Vector3 _startTargetPos;
                private Vector3 _startInputPos;
                private UnityEngine.Camera _camera;
                private Transform _cameraTarget;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _camera = UnityEngine.Camera.allCameras[0];
                    _cameraTarget = new GameObject(nameof(_cameraTarget)).transform;

                    var cameraTargetPos = _cameraTarget.position;
                    cameraTargetPos.x = PlayerPrefs.GetFloat("CamTargetPosX", 0f);
                    cameraTargetPos.z = PlayerPrefs.GetFloat("CamTargetPosZ", 0f);
                    _cameraTarget.position = cameraTargetPos;

                    _ctx.OnUpdate.Subscribe(OnUpdate).AddTo(this);
                }

                private void OnUpdate(float deltaTime) 
                {
                    UpdateCameraTargetPos();

                    var sense = deltaTime * _ctx.Data.CameraSense;

                    var targetPos = _cameraTarget.position + _ctx.Data.CameraOffset;
                    _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, sense);
                }

                private void UpdateCameraTargetPos()
                {
                    var ray = _camera.ScreenPointToRay(Input.mousePosition);
                    if (Input.GetMouseButtonDown(0) && RayCast(ray, out var startHit))
                    { 
                        _startInputPos = startHit.point;
                        _startTargetPos = _cameraTarget.position;
                    }

                    if (Input.GetMouseButton(0) && RayCast(ray, out var hit))
                    { 
                        _cameraTarget.position = _startTargetPos - (hit.point - _startInputPos);
                        PlayerPrefs.SetFloat("CamTargetPosX", _cameraTarget.position.x);
                        PlayerPrefs.SetFloat("CamTargetPosZ", _cameraTarget.position.z);
                    }

                    var cameraTargetPos = _cameraTarget.position;
                    cameraTargetPos.y = 0f;
                    _cameraTarget.position = cameraTargetPos;
                }
                private bool RayCast(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground"));

                protected override async UniTask OnAsyncDispose()
                {
                    if (_cameraTarget != null) UnityEngine.Object.Destroy(_cameraTarget.gameObject);
                    await base.OnAsyncDispose();
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
            }

            private Ctx _ctx;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _ = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data,
                }).AddTo(this);
            }
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private float _cameraSense;
            [SerializeField] private Vector3 _cameraOffset;

            public readonly float CameraSense => _cameraSense;
            public readonly Vector3 CameraOffset => _cameraOffset;
        }
    }
}
