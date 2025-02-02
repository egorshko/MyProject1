using UnityEngine;

namespace Game.World 
{
    public class WorldLoadPoint : MonoBehaviour
    {
        [SerializeField] private WorldEntryPoint.World _world;
        [SerializeField] private float _useDistance = 5f;
        [SerializeField] private Vector3 _worldPos;
        [SerializeField] private Vector3 _worldRot;

        public WorldEntryPoint.World World => _world;
        public float UseDistance => _useDistance;
        public Vector3 WorldPos => _worldPos;
        public Vector3 WorldRot => _worldRot;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _useDistance);
        }
    }
}

