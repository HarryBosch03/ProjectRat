using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Runtime.Rendering.DecalParticleSystem
{
    public class DecalParticleSystem : MonoBehaviour
    {
        public Mesh quad;
        public Material material;
        public float zOffset = 0.01f;
        public float size = 0.2f;
        public int maxParticleCount = 128;

        private int head;
        
        public ParticleData[] particles { get; private set; }
        public Matrix4x4[] evaluatedParticlePositions { get; private set; }

        private void Awake()
        {
            particles = new ParticleData[maxParticleCount];
            evaluatedParticlePositions = new Matrix4x4[maxParticleCount];
        }

        private void OnEnable()
        {
            DecalParticleSystemPass.renderers.Add(this);
        }

        private void OnDisable()
        {
            DecalParticleSystemPass.renderers.Remove(this);
        }

        public void Spawn(Transform parent, Vector3 position, Vector3 normal)
        {
            normal.Normalize();

            ref var data = ref particles[head];
            data.parent = parent;
            data.pose = (parent != null ? parent.worldToLocalMatrix : Matrix4x4.identity) * Matrix4x4.TRS(position + normal * zOffset, Quaternion.LookRotation(-normal), Vector3.one * size);
            
            head = (head + 1) % particles.Length;
        }

        private void Update()
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var particle = particles[i];
                evaluatedParticlePositions[i] = (particle.parent != null ? particle.parent.localToWorldMatrix : Matrix4x4.identity) * particle.pose;
            }
        }

        public struct ParticleData
        {
            public Transform parent;
            public Matrix4x4 pose;
        }
    }
}