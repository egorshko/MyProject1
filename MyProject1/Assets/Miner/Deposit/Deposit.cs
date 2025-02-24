using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Miner.Deposit
{
    public sealed class Deposit
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

                private Dictionary<string, (NavMeshAgent shipNavAgent, Vector3 depositPoint, float checkDepositRadius)> _deposits;
                private Dictionary<string, bool> _shipsStates;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    UpdateDeposits();

                    var dirtyIds = new List<string>();

                    _ctx.OnUpdate.Subscribe(updateTime =>
                    {
                        dirtyIds.Clear();
                        foreach (var ship in _shipsStates)
                        {
                            var target = ship.Value ? _ctx.Data.StorageView.transform.position : _deposits[ship.Key].depositPoint;

                            _deposits[ship.Key].shipNavAgent.SetDestination(target);
                            _deposits[ship.Key].shipNavAgent.isStopped = false;

                            if (Vector3.Distance(_deposits[ship.Key].shipNavAgent.transform.position, target) <= _ctx.Data.StorageCheckRadius)
                                dirtyIds.Add(ship.Key);
                        }
                        foreach (var dirtyId in dirtyIds) 
                        {
                            _shipsStates[dirtyId] = !_shipsStates[dirtyId];
                        }
                    }).AddTo(this);
                }

                private void UpdateDeposits(string id = null)
                {
                    //load or update deposits here...

                    if (id == null) 
                    {
                        _deposits = new();
                        _shipsStates = new();
                    }
                    
                    foreach (var source in _ctx.Data.Sources)
                    {
                        if (id != null && source.Id != id) continue;

                        var shipNavAgent = source.ShipView.GetComponent<NavMeshAgent>();
                        _deposits[source.Id] = (shipNavAgent, source.DepositeView.transform.position, source.CheckRadius);
                        _shipsStates[source.Id] = false;
                    }
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
            }

            private Ctx _ctx;

            private readonly Logic _logic;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _logic = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data,
                }).AddTo(this);
            }
        }

        [Serializable]
        public struct Data
        {
            [Serializable]
            public struct Source 
            {
                [SerializeField] private string _id;
                [SerializeField] private GameObject _depositeView;
                [SerializeField] private float _checkRadius;
                [SerializeField] private GameObject _shipView;

                public readonly string Id => _id;
                public readonly GameObject DepositeView => _depositeView;
                public readonly float CheckRadius => _checkRadius;
                public readonly GameObject ShipView => _shipView;
            }

            [SerializeField] private GameObject _storageView;
            [SerializeField] private float _storageCheckRadius;
            [SerializeField] private Source[] _sources;

            public readonly GameObject StorageView => _storageView;
            public readonly float StorageCheckRadius => _storageCheckRadius;
            public readonly Source[] Sources => _sources;
        }
    }
}

