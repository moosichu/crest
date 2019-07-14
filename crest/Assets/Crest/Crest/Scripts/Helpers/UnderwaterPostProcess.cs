using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crest
{

    [RequireComponent(typeof(Camera))]
    public class UnderwaterPostProcess : MonoBehaviour
    {
        public Material _underWaterPostProcMat;

        private Camera _mainCamera;
        RenderTexture _textureMask;
        static int sp_HorizonHeight = Shader.PropertyToID("_HorizonHeight");
        static int sp_HorizonOrientation = Shader.PropertyToID("_HorizonOrientation");

        // Start is called before the first frame update
        void Start()
        {
            // hack - push forward so the geometry wont be frustum culled. there might be better ways to draw
            // this stuff.
            _mainCamera = GetComponent<Camera>();
            if (_mainCamera == null)
            {
                Debug.LogError("Underwater effects expect to be attached to a camera", this);
                enabled = false;

                return;
            }

            OceanRenderer.Instance.OceanMaterial.EnableKeyword("_UNDERWATER2_ON");
        }

        void Update()
        {
            // TODO: Make front facing quad occupy camera frustrum

            // TODO: Make the horizon marker fill up half the screen
        }

        void OnRenderImage(RenderTexture source, RenderTexture target)
        {
            if (_textureMask == null)
            {
                _textureMask = new RenderTexture(source);
                _textureMask.format = RenderTextureFormat.R8;
                _textureMask.enableRandomWrite = true;
                _textureMask.Create();
                OceanRenderer.Instance.HACK_MaskToTransfer = _textureMask;

                // _mainCamera.SetTargetBuffers(new RenderBuffer[] {
                //     Graphics.activeColorBuffer, _textureMask.colorBuffer
                // }, Graphics.activeDepthBuffer);
            }
            {
                float horizonRoll = 0.0f;
                float horizonHeight = 0.3f;

                // Transform oceanTransform = OceanRenderer.Instance.transform;

                // //horizonHeight = oceanTransform.position.y - _mainCamera.transform.position.y;

                float halfFov = _mainCamera.fieldOfView * 0.5f;
                Vector3 cameraForward = _mainCamera.transform.forward;
                // TODO(UPP): handle Roll
                float cameraRotation = Mathf.Atan2(-1.0f * cameraForward.y, (new Vector2(cameraForward.x, cameraForward.z)).magnitude);
                float halfProp = Mathf.Tan(cameraRotation * 0.5f) / Mathf.Tan(halfFov * Mathf.Deg2Rad);
                horizonHeight = halfProp + 0.5f;

                _underWaterPostProcMat.SetFloat(sp_HorizonHeight, horizonHeight);
                _underWaterPostProcMat.SetFloat(sp_HorizonOrientation, horizonRoll);
                _underWaterPostProcMat.SetTexture("_MaskTex", _textureMask);
            }
            Graphics.Blit(source, target, _underWaterPostProcMat);
        }
    }

}
