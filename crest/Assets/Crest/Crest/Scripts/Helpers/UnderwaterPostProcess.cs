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
            // TODO(UPP): is yaw the right word for this?
            float horizonOrientation = 0.0f;

            Transform oceanTransform = OceanRenderer.Instance.transform;

            //horizonHeight = oceanTransform.position.y - _mainCamera.transform.position.y;

            float fieldOfView = _mainCamera.fieldOfView;
            Vector3 cameraForward = _mainCamera.transform.forward;
            // TODO(UPP): handle orientations
            float cameraRotaion = Mathf.Atan2(-1.0f * cameraForward.y, cameraForward.x);
            horizonHeight = 0.5f + cameraRotaion / (Mathf.Deg2Rad * fieldOfView);

            _underWaterPostProcMat.SetFloat(sp_HorizonHeight, horizonHeight);
            _underWaterPostProcMat.SetFloat(sp_HorizonOrientation, horizonOrientation);

            // TOOD(UPP): Disable post-proc iff we don't have water intersecting the camera.
            Graphics.Blit(source, target, _underWaterPostProcMat);
        }
    }

}
