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

                private Touch? _lastFirstTouch;
                public IReactiveCommand<(TouchPhase touchPhase, Vector3 pos)> _onTouch;
                public IReactiveCommand<float> _onPinch;

                private float _zoomValue;
                private Vector3 _startTargetPos;
                private Vector3 _startInputPos;
                private readonly UnityEngine.Camera _camera;
                private readonly Transform _cameraTarget;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _onTouch = new ReactiveCommand<(TouchPhase touchPhase, Vector3 pos)>().AddTo(this);
                    _onTouch.Subscribe(UpdateCameraTargetPos).AddTo(this);

                    _onPinch = new ReactiveCommand<float>().AddTo(this);
                    _onPinch.Subscribe(UpdateCameraOffset).AddTo(this);

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
                    UpdateTouch();
                    UpdatePinch();

                    var sense = deltaTime * _ctx.Data.CameraSense;

                    var targetPos = _cameraTarget.position + Vector3.Lerp(_ctx.Data.CameraOffsetNear, _ctx.Data.CameraOffsetFar, _zoomValue);
                    _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, sense);
                }

                private void UpdatePinch()
                {
                    if (!Input.touchSupported || Input.touchCount != 2) return;

                    var touch1 = Input.GetTouch(1);
                    var touch2 = Input.GetTouch(2);

                    var pinchAmount = (touch2.deltaPosition - touch1.deltaPosition).magnitude;
                    var zoomingOut = (touch2.position - touch1.position).sqrMagnitude < ((touch2.position - touch2.deltaPosition) - (touch1.position - touch1.deltaPosition)).sqrMagnitude;
                    if (zoomingOut) pinchAmount = -pinchAmount;
                    _onPinch.Execute(pinchAmount * ResolutionMultiplier);
                }

                private void UpdateTouch()
                {
                    if (!Input.touchSupported || Input.touchCount > 1) 
                    {
                        _lastFirstTouch = null;
                        return; 
                    }

                    if (!_lastFirstTouch.HasValue) 
                    {
                        foreach (var touch in Input.touches)
                        {
                            if (touch.phase != TouchPhase.Began)
                                continue;

                            _onTouch.Execute((touch.phase, touch.position));

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

                            _onTouch.Execute((touch.phase, touch.position));

                            if (touch.phase == TouchPhase.Ended)
                                _lastFirstTouch = null;

                            return;
                        }
                    }
                }

                private void UpdateCameraOffset(float pinch)
                {
                    _zoomValue += pinch * _ctx.Data.ZoomSense;
                    _zoomValue = Mathf.Clamp01(_zoomValue);
                    Debug.Log($"{pinch} - {_zoomValue}");
                }

                private void UpdateCameraTargetPos((TouchPhase touchPhase, Vector3 pos) data)
                {
                    if (!RayCast(_camera.ScreenPointToRay(data.pos), out var hit)) return;

                    if (data.touchPhase == TouchPhase.Began)
                    {
                        _startInputPos = hit.point;
                        _startTargetPos = _cameraTarget.position;
                    }
                    else if (data.touchPhase == TouchPhase.Moved)
                    {
                        _cameraTarget.position = _startTargetPos - (hit.point - _startInputPos);
                        var cameraTargetPos = _cameraTarget.position;
                        cameraTargetPos.y = 0f;
                        _cameraTarget.position = cameraTargetPos;

                        PlayerPrefs.SetFloat("CamTargetPosX", _cameraTarget.position.x);
                        PlayerPrefs.SetFloat("CamTargetPosZ", _cameraTarget.position.z);
                    }
                }

                private bool RayCast(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground"));

                private float _resolutionMultiplier;
                private float ResolutionMultiplier
                {
                    get
                    {
                        if (_resolutionMultiplier <= 0f)
                            _resolutionMultiplier = 100f / (Screen.width + Screen.height);

                        return _resolutionMultiplier;
                    }
                }

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
