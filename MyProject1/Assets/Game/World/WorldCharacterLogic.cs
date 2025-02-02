using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace Game.World 
{
    public class WorldCharacterLogic : BaseDisposable
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
            public Action<(WorldEntryPoint.World world, Vector3 worldPos, Vector3 worldRot)> LoadWorld;
        }

        private Ctx _ctx;

        private bool _onUnloading;

        public WorldCharacterLogic(Ctx ctx)
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
}

