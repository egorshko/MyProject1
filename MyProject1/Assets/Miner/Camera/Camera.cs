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
                public IReactiveCommand<(TouchPhase touchPhase, Vector3 pos, Vector3 deltaPos)> _onTouch;
                public IReactiveCommand<float> _onPinch;

                private float _zoomValue;
                private readonly UnityEngine.Camera _camera;
                private readonly Transform _cameraTarget;
                private readonly Transform _cameraLafet;

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

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _onTouch = new ReactiveCommand<(TouchPhase touchPhase, Vector3 pos, Vector3 deltaPos)>().AddTo(this);
                    _onTouch.Subscribe(UpdateCameraTargetPos).AddTo(this);

                    _onPinch = new ReactiveCommand<float>().AddTo(this);
                    _onPinch.Subscribe(UpdateCameraOffset).AddTo(this);

                    _camera = UnityEngine.Camera.allCameras[0];
                    _cameraTarget = new GameObject(nameof(_cameraTarget)).transform;
                    _cameraLafet = new GameObject(nameof(_cameraLafet)).transform;
                    _cameraLafet.SetParent(_cameraTarget);
                    _cameraLafet.localPosition = _ctx.Data.CameraOffsetFar;
                    _camera.transform.SetParent(_cameraLafet);
                    _camera.transform.localPosition = Vector3.zero;

                    var cameraTargetPos = _cameraTarget.position;
                    //cameraTargetPos.x = PlayerPrefs.GetFloat("CamTargetPosX", 0f);
                    //cameraTargetPos.z = PlayerPrefs.GetFloat("CamTargetPosZ", 0f);
                    _cameraTarget.position = cameraTargetPos;

                    _ctx.OnUpdate.Subscribe(OnUpdate).AddTo(this);
                }

                private void OnUpdate(float deltaTime) 
                {
                    if (Input.touchSupported)
                    {
                        //UpdateTouch();
                        UpdateClick();
                        UpdatePinch();
                    }
                    else
                    {
                        UpdateClick();
                        UpdateScroll();
                    }

                    _camera.transform.LookAt(_cameraTarget);

                    //var sense = deltaTime * _ctx.Data.CameraSense;

                    //var targetPos = _cameraTarget.position + Vector3.Lerp(_ctx.Data.CameraOffsetNear, _ctx.Data.CameraOffsetFar, _zoomValue);
                    _cameraLafet.localPosition = Vector3.Lerp(_ctx.Data.CameraOffsetNear, _ctx.Data.CameraOffsetFar, _zoomValue);
                }

                private void UpdatePinch()
                {
                    if (Input.touchCount != 2) return;

                    var touch1 = Input.GetTouch(1);
                    var touch2 = Input.GetTouch(0);

                    if (touch1.phase != TouchPhase.Moved || touch2.phase != TouchPhase.Moved) return;

                    var pinchAmount = (touch2.deltaPosition - touch1.deltaPosition).magnitude;
                    var zoomingOut = (touch2.position - touch1.position).sqrMagnitude < ((touch2.position - touch2.deltaPosition) - (touch1.position - touch1.deltaPosition)).sqrMagnitude;
                    if (zoomingOut) pinchAmount = -pinchAmount;
                    _onPinch.Execute(pinchAmount * ResolutionMultiplier);
                }

                private void UpdateTouch()
                {
                    if (Input.touchCount > 1) 
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

                            _onTouch.Execute((touch.phase, touch.position, touch.deltaPosition));

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

                            _onTouch.Execute((touch.phase, touch.position, touch.deltaPosition));

                            if (touch.phase == TouchPhase.Ended)
                                _lastFirstTouch = null;

                            return;
                        }
                    }
                }

                private void UpdateScroll()
                {
                    _onPinch.Execute(Input.GetAxis("Mouse ScrollWheel"));
                }

                private void UpdateClick()
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _onTouch.Execute((TouchPhase.Began, Input.mousePosition, Input.mousePositionDelta));
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        _onTouch.Execute((TouchPhase.Moved, Input.mousePosition, Input.mousePositionDelta));
                    }
                }

                private void UpdateCameraOffset(float pinch)
                {
                    _zoomValue += pinch * _ctx.Data.ZoomSense;
                    _zoomValue = Mathf.Clamp01(_zoomValue);
                }

                private void UpdateCameraTargetPos((TouchPhase touchPhase, Vector3 pos, Vector3 deltaPos) data)
                {
                    _cameraTarget.position -= _cameraTarget.TransformDirection(new Vector3(data.deltaPos.x, 0f, data.deltaPos.y) * _ctx.Data.CameraSense);
                    
                    //_cameraTarget.position += _cameraTarget.forward * data.deltaPos.y * _ctx.Data.CameraSense;
                    //_cameraTarget.right -= Vector3.right * data.deltaPos.x * _ctx.Data.CameraSense;
                    //PlayerPrefs.SetFloat("CamTargetPosX", _cameraTarget.position.x);
                    //PlayerPrefs.SetFloat("CamTargetPosZ", _cameraTarget.position.z);
                }

                private bool RayCast(Ray ray, out RaycastHit hit) => Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Ground"));

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
