using UnityEngine;

namespace Game.World 
{
    internal class WorldLoadPoint : MonoBehaviour
    {
        [SerializeField] private World.WorldName _world;
        [SerializeField] private float _useDistance = 5f;
        [SerializeField] private Vector3 _worldPos;
        [SerializeField] private Vector3 _worldRot;

        internal World.WorldName World => _world;
        internal float UseDistance => _useDistance;
        internal Vector3 WorldPos => _worldPos;
        internal Vector3 WorldRot => _worldRot;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _useDistance);
        }
    }
}

