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

                private float _zoomValue;
                private Vector3 _startTargetPos;
                private Vector3 _startInputPos;
                private readonly UnityEngine.Camera _camera;
                private readonly Transform _cameraTarget;

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

                    var targetPos = _cameraTarget.position + GetCameraOffset();
                    _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, sense);
                }

                private Vector3 GetCameraOffset()
                {
                    _zoomValue += Input.GetAxis("Mouse ScrollWheel") * _ctx.Data.ZoomSense;

                    _zoomValue = Mathf.Clamp01(_zoomValue);
                    return Vector3.Lerp(_ctx.Data.CameraOffsetNear, _ctx.Data.CameraOffsetFar, _zoomValue);
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
            [SerializeField] private float _zoomSense;
            [SerializeField] private Vector3 _cameraOffsetFar;
            [SerializeField] private Vector3 _cameraOffsetNear;

            public readonly float CameraSense => _cameraSense;
            public readonly float ZoomSense => _zoomSense;
            public readonly Vector3 CameraOffsetFar => _cameraOffsetFar;
            public readonly Vector3 CameraOffsetNear => _cameraOffsetNear;
        }
    }
}
