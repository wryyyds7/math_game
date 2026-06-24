using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Shooting
{
    public class Bullet : MonoBehaviour
    {
        public BulletInfo Info { get; private set; }
        private ShootingSystem system;
        private TrailRenderer trail;

        [SerializeField] private GameObject explosionEffectPrefab;

        public void Initialize(BulletInfo info, ShootingSystem sys)
        {
            Info = info;
            system = sys;
            trail = GetComponent<TrailRenderer>();
            if (trail) trail.Clear();
        }

        private void Update()
        {
            if (Info == null || Info.HasExploded)
            {
                PlayExplosionEffect();
                Destroy(gameObject, 0.3f);
                return;
            }
            transform.position = Info.CurrentPosition;
        }

        private void PlayExplosionEffect()
        {
            if (explosionEffectPrefab != null && !playedEffect)
            {
                playedEffect = true;
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        private bool playedEffect = false;
    }
}
