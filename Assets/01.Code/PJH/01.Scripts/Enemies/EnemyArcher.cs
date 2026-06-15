using Code.Combat;
using Code.Entities.Combat;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.Enemies
{
    public class EnemyArcher : CommonEnemy
    {
        [SerializeField] private BowLineRenderer bowLineRenderer;
        [SerializeField] private Transform firePos;
        [SerializeField] private PoolItemSO arrowPrefab;
        [SerializeField] private GameObject arrowInHand;
        [SerializeField] private float arrowSpeed = 20f;
        [SerializeField] private float arrowDamage = 10f;
        [SerializeField] private float bowstringDuration = 0.5f;

        [Inject] private PoolManagerMono poolManager;

        public void ShootArrow(Vector3 dir)
        {
            var arrow = poolManager.Pop<Arrow>(arrowPrefab);

            arrow.transform.position = firePos.transform.position;
            arrow.transform.rotation = Quaternion.LookRotation(dir);

            arrow.Initialize(dir, arrowSpeed, arrowDamage, this);
            SetActiveArrowInHand(false);
            bowLineRenderer.ReleaseBowstring(bowstringDuration);
        }

        public void LoadBowstring()
            => bowLineRenderer.LoadBowstring(bowstringDuration);

        public void SetActiveArrowInHand(bool isActive)
            => arrowInHand.SetActive(isActive);
    }
}