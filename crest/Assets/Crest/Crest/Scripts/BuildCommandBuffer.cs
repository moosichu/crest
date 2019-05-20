// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

#if ENABLE_COMPUTE_SHADERS
#define USE_ASYNC_COMPUTE
#endif

using UnityEngine;
using UnityEngine.Rendering;


namespace Crest
{
    /// <summary>
    /// Base class for the command buffer builder, which takes care of updating all ocean-related data. If you wish to provide your
    /// own update logic, you can create a new component that inherits from this class and attach it to the same GameObject as the
    /// OceanRenderer script. The new component should be set to update after the Default bucket, similar to BuildCommandBuffer.
    /// </summary>
    public abstract class BuildCommandBufferBase : MonoBehaviour
    {
        /// <summary>
        /// Used to validate update order
        /// </summary>
        public static int _lastUpdateFrame = -1;
    }

    /// <summary>
    /// The default builder for the ocean update command buffer which takes care of updating all ocean-related data, for
    /// example rendering animated waves and advancing sims. This runs in LateUpdate after the Default bucket, after the ocean
    /// system been moved to an up to date position and frame processing is done.
    /// </summary>
    public class BuildCommandBuffer : BuildCommandBufferBase
    {
        CommandBuffer _buf;
#if USE_ASYNC_COMPUTE
        CommandBuffer _bufAsync;
#endif

        void Build(OceanRenderer ocean)
        {
#if USE_ASYNC_COMPUTE
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Ocean depths
            if (ocean._lodDataSeaDepths)
            {
                ocean._lodDataSeaDepths.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Flow data
            if (ocean._lodDataFlow)
            {
                ocean._lodDataFlow.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Dynamic wave simulations
            if (ocean._lodDataDynWaves)
            {
                ocean._lodDataDynWaves.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Animated waves next
            if (ocean._lodDataAnimWaves)
            {
                ocean._lodDataAnimWaves.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Foam simulation
            if (ocean._lodDataFoam)
            {
                var fence = _buf.CreateAsyncGraphicsFence(SynchronisationStage.PixelProcessing);
                _bufAsync.WaitOnAsyncGraphicsFence(fence);
                ocean._lodDataFoam.BuildCommandBuffer(ocean, _bufAsync);
            }
#else
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Ocean depths
            if (ocean._lodDataSeaDepths)
            {
                ocean._lodDataSeaDepths.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Flow data
            if (ocean._lodDataFlow)
            {
                ocean._lodDataFlow.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Dynamic wave simulations
            if (ocean._lodDataDynWaves)
            {
                ocean._lodDataDynWaves.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Animated waves next
            if (ocean._lodDataAnimWaves)
            {
                ocean._lodDataAnimWaves.BuildCommandBuffer(ocean, _buf);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // --- Foam simulation
            if (ocean._lodDataFoam)
            {
                ocean._lodDataFoam.BuildCommandBuffer(ocean, _buf);
            }
#endif
        }

        /// <summary>
        /// Construct the command buffer and attach it to the camera so that it will be executed in the render.
        /// </summary>
        public void LateUpdate()
        {
            if (OceanRenderer.Instance == null) return;

            if (_buf == null)
            {
                _buf = new CommandBuffer();
                _buf.name = "CrestLodData";
            }

            _buf.Clear();

#if USE_ASYNC_COMPUTE
            if (_bufAsync == null)
            {
                _bufAsync = new CommandBuffer();
                _bufAsync.name = "CrestLodDataAsync";
            }

            _bufAsync.Clear();
            _bufAsync.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
#endif

            Build(OceanRenderer.Instance);

#if USE_ASYNC_COMPUTE
            Graphics.ExecuteCommandBufferAsync(_bufAsync, ComputeQueueType.Default);
#endif
            // This will execute at the beginning of the frame before the graphics queue
            Graphics.ExecuteCommandBuffer(_buf);

            _lastUpdateFrame = Time.frameCount;
        }
    }
}
