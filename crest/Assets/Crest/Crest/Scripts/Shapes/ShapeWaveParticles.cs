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
            public WaveParticles()
            {
                _properties = new PropertyWrapperCompute[]
                {
                    new PropertyWrapperCompute(),
                    new PropertyWrapperCompute()
                };
            }

            public PropertyWrapperCompute GetProperty(int isTransition) => _properties[isTransition];

            // Two materials because as batch may be rendered twice if it has large wavelengths that are being transitioned back
            // and forth across the last 2 lods.
            PropertyWrapperCompute[] _properties;

            public float Wavelength { get; set; }
            public bool Enabled { get; set; }

            public void Draw(CommandBuffer buf, float weight, int isTransition)
            {
                if (Enabled && weight > 0f)
                {
                    PropertyWrapperCompute property = GetProperty(isTransition);
                    // TODO(WP): Initialise property wrapper properly
                    property.SetFloat(RegisterLodDataInputBase.sp_Weight, weight);
                    // TODO(WP): dispatch on property wrapper
                    //property.DispatchShader();
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

            _waveParticleBatches = new WaveParticles[LodDataMgr.MAX_LOD_COUNT];
            for (int i = 0; i < _waveParticleBatches.Length; i++)
            {
                _waveParticleBatches[i] = new WaveParticles();
            }
        }

        void UpdateBatch(int lodIdx, int firstComponent, int lastComponentNonInc, WaveParticles batch)
        {
            batch.Enabled = false;
            // apply the data to the shape property
            for (int i = 0; i < 2; i++)
            {
                var property = batch.GetProperty(i);
                // TODO(WP): Bind properties properly
                // TODO(WP): work out how this is gonna work with compute shaders (we need cmdbuff access).
                property.SetFloat(OceanRenderer.sp_LD_SliceIndex, lodIdx);
                OceanRenderer.Instance._lodDataAnimWaves.BindResultData(property);

                if (OceanRenderer.Instance._lodDataSeaDepths)
                {
                    OceanRenderer.Instance._lodDataSeaDepths.BindResultData(property, false);
                }
            }
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
