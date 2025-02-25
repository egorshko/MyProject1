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
                private class DepositState
                {
                    private NavMeshAgent _shipNavAgent;
                    private Vector3 _depositPoint;
                    private float _checkRadius;

                    public bool ToStorage { get; set; }
                    public NavMeshAgent ShipNavAgent => _shipNavAgent;
                    public Vector3 DepositPoint => _depositPoint;

                    public DepositState(Data.Source source)
                    {
                        _shipNavAgent = source.ShipView.GetComponent<NavMeshAgent>();
                        _depositPoint = source.DepositeView.transform.position;
                        _checkRadius = source.CheckRadius;

                        ToStorage = false;
                    }

                    public bool CheckDistance(Vector3 targetPoint)
                        => Vector3.Distance(_shipNavAgent.transform.position, targetPoint) <= _checkRadius;
                }

                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;

                    public Data Data;
                }

                private Ctx _ctx;

                private Dictionary<string, DepositState> _depositStates;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    UpdateDeposits();

                    _ctx.OnUpdate.Subscribe(updateTime =>
                    {
                        foreach (var deposit in _depositStates)
                        {
                            var target = deposit.Value.ToStorage ? _ctx.Data.StorageView.transform.position : deposit.Value.DepositPoint;

                            deposit.Value.ShipNavAgent.SetDestination(target);
                            deposit.Value.ShipNavAgent.isStopped = false;
                            if (deposit.Value.CheckDistance(target))
                                deposit.Value.ToStorage = !deposit.Value.ToStorage;
                        }
                    }).AddTo(this);
                }

                private void UpdateDeposits(string id = null)
                {
                    //load or update deposits here...

                    if (id == null) 
                    {
                        _depositStates = new();
                    }
                    
                    foreach (var source in _ctx.Data.Sources)
                    {
                        if (id != null && source.Id != id) continue;

                        _depositStates[source.Id] = new DepositState(source);
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

