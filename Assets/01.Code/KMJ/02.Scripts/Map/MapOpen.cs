using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class MapOpen : MonoBehaviour
    {
        public static MapOpen Instance { get; private set; }
        [SerializeField] private List<int> doorEnemyCnt;

        private bool _isCanOpen = false;
        private int _listCnt = 0;
        private int _doorCnt = 0;
        private int _enemyCnt = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;

            _isCanOpen = false;
            _doorCnt = 0;
            _listCnt = 0;
        }

        public void CanCollectEnemies()
        {
            _isCanOpen = true;
        }

        public void CantCollectEnemies()
        {
            _isCanOpen = false;
        }

        public void GetOpenCnt()
        {
            if (_isCanOpen)
            {
                _enemyCnt += 1;

                if (_enemyCnt >= doorEnemyCnt[_listCnt])
                {
                    _doorCnt += 1;
                    _listCnt += 1;
                    _isCanOpen = false;
                }   
            }
        }
    }
}