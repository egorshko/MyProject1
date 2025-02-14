using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace Game.World
{
    public class World : BaseDisposableMB
    {
        public class Entity : BaseDisposable
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;
                    public Action ShowMenu;
                }

                private Ctx _ctx;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _ctx.OnUpdate.Subscribe(deltaTime =>
                    {
                        if (Input.GetKeyUp(KeyCode.Escape)) _ctx.ShowMenu.Invoke();
                    }).AddTo(this);
                }
            }

            public class CameraLogic : BaseDisposable
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

                public CameraLogic(Ctx ctx)
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

            public class CharacterLogic : BaseDisposable
            {
                [Serializable]
                public struct Data
                {
                    [SerializeField] private GameObject _targetMarkerGO;
                    [SerializeField] private GameObject _playerGO;
                    [SerializeField] private float _speed;
                    [SerializeField] private float _angularSpeed;
                    [SerializeField] private float _acceleration;

                    public readonly GameObject TargetMarkerGO => _targetMarkerGO;
                    public readonly GameObject PlayerGO => _playerGO;
                    public readonly float Speed => _speed;
                    public readonly float AngularSpeed => _angularSpeed;
                    public readonly float Acceleration => _acceleration;
                }

                public struct Ctx
                {
                    public Vector3 WorldPos;
                    public Quaternion WorldRot;
                    public IReadOnlyReactiveCommand<float> OnUpdate;
                    public Data Data;
                    public IReadOnlyReactiveValue<bool> IsShowMenu;
                    public Action<(World.WorldName world, Vector3 worldPos, Vector3 worldRot)> LoadWorld;
                }

                private Ctx _ctx;

                private bool _onUnloading;

                public CharacterLogic(Ctx ctx)
                {
                    const float doneDistance = 1f;
                    const float checkDistance = 1000f;

                    _ctx = ctx;

                    _onUnloading = false;

                    var playerTr = _ctx.Data.PlayerGO.transform;
                    playerTr.SetPositionAndRotation(_ctx.WorldPos, _ctx.WorldRot);

                    var characterNavAgent = _ctx.Data.PlayerGO.GetComponent<NavMeshAgent>();
                    characterNavAgent.speed = _ctx.Data.Speed;
                    characterNavAgent.angularSpeed = _ctx.Data.AngularSpeed;
                    characterNavAgent.acceleration = _ctx.Data.Acceleration;

                    characterNavAgent.destination = GetDestinationPoint(playerTr.position);
                    characterNavAgent.isStopped = true;

                    var worldPoints = UnityEngine.Object.FindObjectsByType<WorldLoadPoint>(FindObjectsSortMode.None);

                    _ctx.OnUpdate.Subscribe(deltaTime =>
                    {
                        if (_onUnloading) return;
                        if (_ctx.IsShowMenu.Value) return;

                        var layers = Physics.AllLayers;
                        var trigger = QueryTriggerInteraction.Ignore;
                        var ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
                        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out var hitInfo, checkDistance, layers, trigger))
                            characterNavAgent.destination = GetDestinationPoint(hitInfo.point);

                        var targetDistance = Vector3.Distance(playerTr.position, characterNavAgent.destination);
                        characterNavAgent.isStopped = targetDistance <= doneDistance;
                        UpdateMarker(characterNavAgent.destination, !characterNavAgent.isStopped);

                        foreach (var worldPoint in worldPoints)
                        {
                            var distance = Vector3.Distance(playerTr.position, worldPoint.transform.position);
                            if (distance > worldPoint.UseDistance) continue;

                            _onUnloading = true;

                            _ctx.LoadWorld.Invoke((worldPoint.World, worldPoint.WorldPos, worldPoint.WorldRot));
                            break;
                        }
                    }).AddTo(this);

                    Vector3 GetDestinationPoint(Vector3 point)
                    {
                        if (!NavMesh.SamplePosition(point, out var navHit, checkDistance, Physics.AllLayers)) return playerTr.position;
                        return navHit.position;
                    }

                    void UpdateMarker(Vector3 pos, bool isVisible)
                    {
                        _ctx.Data.TargetMarkerGO.transform.position = pos;
                        _ctx.Data.TargetMarkerGO.SetActive(isVisible);
                    }
                }
            }

            public struct Ctx
            {
                public Vector3 WorldPos;
                public Quaternion WorldRot;
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
                public Action ShowMenu;
                public IReadOnlyReactiveValue<bool> IsShowMenu;
                public Action<(WorldName world, Vector3 worldPos, Vector3 worldRot)> LoadWorld;
            }

            private Ctx _ctx;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _ = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    ShowMenu = _ctx.ShowMenu,
                }).AddTo(this);

                _ = new CameraLogic(new CameraLogic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    PlayerTransform = _ctx.Data.PlayerData.PlayerGO.transform,
                    Data = _ctx.Data.CameraData,
                }).AddTo(this);

                _ = new CharacterLogic(new CharacterLogic.Ctx
                {
                    WorldPos = _ctx.WorldPos,
                    WorldRot = _ctx.WorldRot,
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data.PlayerData,
                    IsShowMenu = _ctx.IsShowMenu,
                    LoadWorld = _ctx.LoadWorld,
                }).AddTo(this);
            }
        }

        public enum WorldName : int
        {
            World_Test_0 = -1000,
            World_Test_1,
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private Entity.CharacterLogic.Data _playerData;
            [SerializeField] private Entity.CameraLogic.Data _cameraData;

            public readonly Entity.CharacterLogic.Data PlayerData => _playerData;
            public readonly Entity.CameraLogic.Data CameraData => _cameraData;
        }

        public struct Ctx
        {
            public Vector3 WorldPos;
            public Quaternion WorldRot;
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Action ShowMenu;
            public IReadOnlyReactiveValue<bool> IsShowMenu;
            public Action<(WorldName world, Vector3 worldPos, Vector3 worldRot)> LoadWorld;
        }

        [SerializeField] private Data _data;

        private Ctx _ctx;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _ = new Entity(new Entity.Ctx
            {
                WorldPos = _ctx.WorldPos,
                WorldRot = _ctx.WorldRot,
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
                ShowMenu = _ctx.ShowMenu,
                IsShowMenu = _ctx.IsShowMenu,
                LoadWorld = _ctx.LoadWorld,
            }).AddTo(this);
        }
    }
}

