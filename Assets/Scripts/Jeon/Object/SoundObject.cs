using UnityEngine;
using Dev.cheol.Model;

namespace Dev.cheol.Manager
{
    public class SoundObject : BaseObject
    {
        private AudioSource _source;
        public AudioSource Source
        {
            get
            {
                if (_source == null) _source = GetComponent<AudioSource>();
                return _source;
            }
        }

        public override void ObjectUpdate() { }

        public override void OnDespawn()
        {
            Source.Stop();
            Source.clip = null;
        }
    }
}