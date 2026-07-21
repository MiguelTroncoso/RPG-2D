using System.Collections.Generic;
using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    public enum H7VfxKind
    {
        Attack,
        Defense,
        Area,
        Wave,
        Impact,
        Death,
        LevelUp,
        Mission,
        Equip
    }

    public sealed class H7VfxPool : MonoBehaviour
    {
        [SerializeField, Min(1)] private int prewarmCount = 18;
        [SerializeField, Min(1)] private int maxActive = 24;

        private readonly List<H7PooledVfx> _items = new List<H7PooledVfx>();
        private Transform _container;

        public int PrewarmedCount => _items.Count;
        public int ActiveCount { get; private set; }
        public bool IsConfigured => _items.Count > 0 && maxActive >= _items.Count;

        public void Configure(int count, int activeLimit)
        {
            prewarmCount = Mathf.Max(1, count);
            maxActive = Mathf.Max(prewarmCount, activeLimit);
            if (UnityEngine.Application.isPlaying)
            {
                Prewarm();
            }
        }

        private void Awake()
        {
            Prewarm();
        }

        public H7PooledVfx Play(H7VfxKind kind, Vector3 position, Color color)
        {
            if (ActiveCount >= maxActive)
            {
                return null;
            }

            for (var index = 0; index < _items.Count; index++)
            {
                if (_items[index].IsPlaying)
                {
                    continue;
                }

                ActiveCount++;
                _items[index].Play(this, kind, position, color);
                return _items[index];
            }

            return null;
        }

        internal void Release(H7PooledVfx item)
        {
            if (item == null)
            {
                return;
            }

            item.StopPlaying();
            ActiveCount = Mathf.Max(0, ActiveCount - 1);
        }

        private void Prewarm()
        {
            if (_items.Count > 0)
            {
                return;
            }

            _container = new GameObject("H7_VfxPool_Items").transform;
            _container.SetParent(transform, false);
            for (var index = 0; index < prewarmCount; index++)
            {
                var itemObject = new GameObject("H7_PooledVfx_" + index);
                itemObject.transform.SetParent(_container, false);
                var item = itemObject.AddComponent<H7PooledVfx>();
                item.Initialize();
                _items.Add(item);
            }
        }
    }

    public sealed class H7PooledVfx : MonoBehaviour
    {
        private ParticleSystem _particles;
        private H7VfxPool _owner;

        public bool IsPlaying { get; private set; }

        internal void Initialize()
        {
            _particles = gameObject.AddComponent<ParticleSystem>();
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = _particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.42f;
            main.startLifetime = 0.32f;
            main.startSpeed = 1.2f;
            main.startSize = 0.16f;
            main.maxParticles = 24;
            main.stopAction = ParticleSystemStopAction.Callback;
            var emission = _particles.emission;
            emission.rateOverTime = 0f;
            var burst = new ParticleSystem.Burst(0f, 8);
            emission.SetBursts(new[] { burst });
            var shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.18f;
            var renderer = _particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default"));
            gameObject.SetActive(false);
        }

        internal void Play(H7VfxPool owner, H7VfxKind kind, Vector3 position, Color color)
        {
            _owner = owner;
            transform.position = position;
            transform.localScale = Vector3.one * (kind == H7VfxKind.LevelUp ? 1.4f : 1f);
            var main = _particles.main;
            main.startColor = color;
            IsPlaying = true;
            gameObject.SetActive(true);
            _particles.Clear(true);
            _particles.Play(true);
        }

        internal void StopPlaying()
        {
            IsPlaying = false;
            _owner = null;
            if (_particles != null)
            {
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            gameObject.SetActive(false);
        }

        private void OnParticleSystemStopped()
        {
            _owner?.Release(this);
        }
    }
}
