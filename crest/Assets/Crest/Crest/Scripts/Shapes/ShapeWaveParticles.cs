// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    public class ShapeWaveParticles : MonoBehaviour, ICollProvider, IFloatingOrigin
    {
        public class WaveParticles : ILodDataInput
        {
            private int _lodIdx;
            private ComputeShader _waveShader;
            private int _waveKernel;
            public WaveParticles(int lodIdx, ComputeShader waveShader, int waveKernel)
            {
                _properties = new PropertyWrapperCompute();
                _lodIdx = lodIdx;
                _waveShader = waveShader;
                _waveKernel = waveKernel;
            }

            private PropertyWrapperCompute _properties;
            public float Wavelength { get; set; }
            public bool Enabled { get; set; }

            public void Draw(CommandBuffer buf, float weight, int isTransition)
            {
                if (Enabled && weight > 0f)
                {
                    _properties.Initialise(buf, _waveShader, _waveKernel);
                    _properties.SetFloat(OceanRenderer.sp_LD_SliceIndex, _lodIdx - isTransition);
                    if (OceanRenderer.Instance._lodDataSeaDepths)
                    {
                        OceanRenderer.Instance._lodDataSeaDepths.BindResultData(_properties, false);
                    }
                    _properties.DispatchShader();
                }
            }
        }

        WaveParticles[] _waveParticleBatches = null;


        static int sp_TODO = Shader.PropertyToID("_TODO"); // TODO(WP)
        private const string ShaderName = "SplatWaveParticles";
        private ComputeShader _waveShader;
        private int _waveKernel = -1;

        void Start()
        {
            // TODO(WP): Start
        }

        public void SetOrigin(Vector3 newOrigin)
        {
            // TODO(WP): Handle new origin
        }

        void Update()
        {
            if (OceanRenderer.Instance == null) return;

            // TODO(WP): Everything

            // this is done every frame for flexibility/convenience, in case the lod count changes
            if (_waveParticleBatches == null)
            {
                InitBatches();
            }
        }

        void InitBatches()
        {
            if (_waveShader == null)
            {
                _waveShader = Resources.Load<ComputeShader>(ShaderName);
                Debug.Assert(_waveShader, "Could not load the wave particles, make sure it is packaged in the build.");
                if (_waveShader == null)
                {
                    return;
                }
                _waveKernel = _waveShader.FindKernel(ShaderName);
            }

            // TODO(WP): Use ocean count?
            _waveParticleBatches = new WaveParticles[LodDataMgr.MAX_LOD_COUNT];
            for (int i = 0; i < _waveParticleBatches.Length; i++)
            {
                _waveParticleBatches[i] = new WaveParticles(i, _waveShader, _waveKernel);
            }
        }

        void UpdateBatch(int lodIdx, int firstComponent, int lastComponentNonInc, WaveParticles batch)
        {
            batch.Enabled = false;
            // TODO(WP): work out what we need to check with the batch
            batch.Enabled = true;
        }

        void OnEnable()
        {
            if (_waveParticleBatches == null)
            {
                InitBatches();
            }

            foreach (var batch in _waveParticleBatches)
            {
                OceanRenderer.Instance._lodDataAnimWaves.AddDraw(batch);
            }
        }

        void OnDisable()
        {
            foreach (var batch in _waveParticleBatches)
            {
                OceanRenderer.Instance._lodDataAnimWaves.RemoveDraw(batch);
            }
        }

        // TODO(WP): Share this code with shape Gerstner batched :)
        float ComputeWaveSpeed(float wavelength/*, float depth*/)
        {
            // wave speed of deep sea ocean waves: https://en.wikipedia.org/wiki/Wind_wave
            // https://en.wikipedia.org/wiki/Dispersion_(water_waves)#Wave_propagation_and_dispersion
            float g = 9.81f;
            float k = 2f * Mathf.PI / wavelength;
            //float h = max(depth, 0.01);
            //float cp = sqrt(abs(tanh_clamped(h * k)) * g / k);
            float cp = Mathf.Sqrt(g / k);
            return cp;
        }

        public bool GetSamplingData(ref Rect i_displacedSamplingArea, float i_minSpatialLength, SamplingData o_samplingData)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public void ReturnSamplingData(SamplingData i_data)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public bool SampleDisplacement(ref Vector3 i_worldPos, SamplingData i_samplingData, out Vector3 o_displacement)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public void SampleDisplacementVel(ref Vector3 i_worldPos, SamplingData i_samplingData, out Vector3 o_displacement, out bool o_displacementValid, out Vector3 o_displacementVel, out bool o_velValid)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public bool SampleHeight(ref Vector3 i_worldPos, SamplingData i_samplingData, out float o_height)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public bool SampleNormal(ref Vector3 i_undisplacedWorldPos, SamplingData i_samplingData, out Vector3 o_normal)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public bool ComputeUndisplacedPosition(ref Vector3 i_worldPos, SamplingData i_samplingData, out Vector3 undisplacedWorldPos)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }

        public AvailabilityResult CheckAvailability(ref Vector3 i_worldPos, SamplingData i_samplingData)
        {
            // TODO(WP):
            throw new System.NotImplementedException();
        }
    }
}
