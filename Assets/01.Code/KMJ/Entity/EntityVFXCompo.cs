using System.Collections.Generic;
using System.Linq;
using _Code.EntityCompo;
using UnityEngine;

namespace Code.Effects
{
    public class EntityVFXCompo : MonoBehaviour, IEntityComponent
    {
        private Dictionary<string, IPlayableVFX> _vfxDict = new();

        public void Initialize(Entity owner)
        {
            _vfxDict = new Dictionary<string, IPlayableVFX>();
            GetComponentsInChildren<IPlayableVFX>().ToList()
                .ForEach(playable => _vfxDict.Add(playable.VFXName, playable));
        }
        
        public void PlayVFX(string vfxName, Vector3 position, Quaternion rotation)
        {
            IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);
            Debug.Assert(vfx != null, $"{vfxName} is not exists in dictionary");
            
            vfx.PlayVFX(position, rotation);
        }

        public void PlayVFX(string vfxName)
        {
            if (CheckExistVFX(vfxName))
            {
                IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);
                vfx.PlayVFX(Vector3.zero , Quaternion.identity);
            }   
        }

        public bool CheckExistVFX(string vfxName)
        {
            IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);

            if (vfx != null)
                return true;
            
            Debug.LogWarning($"{vfxName} is not exists in dictionary");
            return false;
        }
        
        public void StopVFX(string vfxName)
        {
            if (CheckExistVFX(vfxName))
            {
                IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);
                vfx.StopVFX();
            }
        }
    }
}