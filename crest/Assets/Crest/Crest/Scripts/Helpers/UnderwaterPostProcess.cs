using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crest
{

    public class UnderwaterPostProcess : MonoBehaviour
    {
        public Material _underWaterPostProcMat;
        public Camera _mainCamera;
        static int sp_HorizonHeight = Shader.PropertyToID("_HorizonHeight");
        static int sp_HorizonOrientation = Shader.PropertyToID("_HorizonOrientation");

        // Start is called before the first frame update
        void Start()
        {
        }

        void OnRenderImage(RenderTexture source, RenderTexture target)
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            float horizonHeight = 0.3f;
            float horizonRoll = 0.0f;

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
            // TOOD(UPP): Disable post-proc iff we don't have water intersecting the camera.
            Graphics.Blit(source, target, _underWaterPostProcMat);
        }
    }

}
