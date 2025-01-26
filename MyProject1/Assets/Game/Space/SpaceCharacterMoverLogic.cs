using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Space 
{
    public class SpaceCharacterMoverLogic : BaseDisposable
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
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public Data Data;
            public IReadOnlyReactiveValue<bool> IsShowMenu;
            public GameObject[] Cities;
            public Action<GameObject> LoadCity;
        }

        private const string PlayerPosPrefsKey = "SpacePlayerPos";
        private const string PlayerRotPrefsKey = "SpacePlayerRot";

        private Ctx _ctx;

        private bool _onUnloading;

        public SpaceCharacterMoverLogic(Ctx ctx)
        {
            const float doneDistance = 1f;
            const float cityDistance = 5f;
            const float citySpawnDistance = 6f;
            const float checkDistance = 1000f;

            _ctx = ctx;

            _onUnloading = false;

            var textPlayerPos = PlayerPrefs.GetString(PlayerPosPrefsKey, JsonUtility.ToJson(_ctx.Data.PlayerGO.transform.position));
            var playerPos = JsonUtility.FromJson<Vector3>(textPlayerPos);
            var playerRot = PlayerPrefs.GetFloat(PlayerRotPrefsKey, 0f);

            var playerTr = _ctx.Data.PlayerGO.transform;
            playerTr.SetPositionAndRotation(playerPos, Quaternion.Euler(Vector3.up * playerRot));

            foreach (var city in _ctx.Cities)
            {
                var distance = Vector3.Distance(playerTr.position, city.transform.position);
                if (distance > citySpawnDistance) continue;

                var heading = playerTr.position - city.transform.position;
                var direction = heading / heading.magnitude;
                playerTr.position = city.transform.position;
                var newPosition = playerTr.position + direction * citySpawnDistance;
                playerTr.LookAt(newPosition);
                playerTr.position = newPosition;
                break;
            }

            var characterNavAgent = _ctx.Data.PlayerGO.GetComponent<NavMeshAgent>();
            characterNavAgent.speed = _ctx.Data.Speed;
            characterNavAgent.angularSpeed = _ctx.Data.AngularSpeed;
            characterNavAgent.acceleration = _ctx.Data.Acceleration;

            characterNavAgent.destination = GetDestinationPoint(playerTr.position);
            characterNavAgent.isStopped = true;

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

                foreach (var city in _ctx.Cities) 
                {
                    var distance = Vector3.Distance(playerTr.position, city.transform.position);
                    if (distance > cityDistance) continue;

                    _onUnloading = true;

                    _ctx.LoadCity.Invoke(city);
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

        protected override Task OnAsyncDispose()
        {
            PlayerPrefs.SetString(PlayerPosPrefsKey, JsonUtility.ToJson(_ctx.Data.PlayerGO.transform.position));
            PlayerPrefs.SetFloat(PlayerRotPrefsKey, _ctx.Data.PlayerGO.transform.eulerAngles.y);
            return base.OnAsyncDispose();
        }
    }
}

