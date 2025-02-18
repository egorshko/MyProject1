using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Linq;
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

                private Touch? _lastFirstTouch;
                public IReactiveCommand<(TouchPhase touchPhase, Vector3 pos)> _onTap;

                private float _zoomValue;
                private Vector3 _startTargetPos;
                private Vector3 _startInputPos;
                private readonly UnityEngine.Camera _camera;
                private readonly Transform _cameraTarget;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _onTap = new ReactiveCommand<(TouchPhase touchPhase, Vector3 pos)>().AddTo(this);
                    _onTap.Subscribe(UpdateCameraTargetPos).AddTo(this);

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
                    UpdateTouches();

                    var sense = deltaTime * _ctx.Data.CameraSense;

                    var targetPos = _cameraTarget.position + GetCameraOffset();
                    _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, sense);
                }

                private void UpdateTouches()
                {
                    if (!Input.touchSupported) return;

                    if (!_lastFirstTouch.HasValue) 
                    {
                        foreach (var touch in Input.touches)
                        {
                            if (touch.phase != TouchPhase.Began)
                                continue;

                            _lastFirstTouch = touch;
                            return;
                        }
                    }
                    else 
                    {
                        foreach (var touch in Input.touches)
                        {
                            if (touch.fingerId != _lastFirstTouch.Value.fingerId)
                                continue;

                            _onTap.Execute((touch.phase, touch.position));

                            if (touch.phase == TouchPhase.Ended)
                                _lastFirstTouch = null;

                            return;
                        }
                    }
                }

                private Vector3 GetCameraOffset()
                {
                    _zoomValue += Input.GetAxis("Mouse ScrollWheel") * _ctx.Data.ZoomSense;

                    _zoomValue = Mathf.Clamp01(_zoomValue);
                    return Vector3.Lerp(_ctx.Data.CameraOffsetNear, _ctx.Data.CameraOffsetFar, _zoomValue);
                }

                private void UpdateCameraTargetPos((TouchPhase touchPhase, Vector3 pos) data)
                {
                    if (data.touchPhase == TouchPhase.Began && RayCast(_camera.ScreenPointToRay(data.pos), out var startHit))
                    {
                        _startInputPos = startHit.point;
                        _startTargetPos = _cameraTarget.position;
                    }
                    else if (data.touchPhase == TouchPhase.Moved && RayCast(_camera.ScreenPointToRay(data.pos), out var hit))
                    {
                        _cameraTarget.position = _startTargetPos - (hit.point - _startInputPos);
                        var cameraTargetPos = _cameraTarget.position;
                        cameraTargetPos.y = 0f;
                        _cameraTarget.position = cameraTargetPos;

                        PlayerPrefs.SetFloat("CamTargetPosX", _cameraTarget.position.x);
                        PlayerPrefs.SetFloat("CamTargetPosZ", _cameraTarget.position.z);
                    }
                    else 
                    {
                        return;
                    }
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
