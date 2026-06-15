using GondrLib.Dependencies;
using UnityEngine;

namespace Code.Entities.Managers
{
    [DefaultExecutionOrder(-1)]
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] [Inject] private Player player;
        [SerializeField] private EntityFinderSO playerFinder;

        private void Awake()
        {
            playerFinder.SetTarget(player);
        }
    }
}