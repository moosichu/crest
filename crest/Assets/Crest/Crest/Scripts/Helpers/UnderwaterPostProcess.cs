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

            float fieldOfView = _mainCamera.fieldOfView;
            Vector3 cameraForward = _mainCamera.transform.forward;
            // TODO(UPP): handle Roll
            // float cameraRotation = Mathf.Atan2(-1.0f * cameraForward.y, cameraForward.x);
            // horizonHeight = 0.5f + cameraRotation / (Mathf.Deg2Rad * fieldOfView);

            // float tanCameraRotation = cameraForward.y * (new Vector2(cameraForward.x, cameraForward.z)).magnitude;
            // horizonHeight = tanCameraRotation / Mathf.Tan(Mathf.Deg2Rad * fieldOfView);
            // horizonHeight = (horizonHeight * -2.0f) + 0.5f;

            float cameraRotation = Mathf.Atan2(-1.0f * cameraForward.y, cameraForward.x);

            float halfProp = Mathf.Tan(cameraRotation * 0.5f) / Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f);
            horizonHeight = halfProp + 0.5f;

            // Matrix4x4 cameraMatrix = _mainCamera.projectionMatrix * _mainCamera.worldToCameraMatrix;
            // Vector3 planeNormalCS = cameraMatrix.MultiplyVector(Vector3.up);
            // horizonHeight = 0.5f - planeNormalCS.z;// * (planeNormalCS.y / planeNormalCS.magnitude);
            // horizonHeight = Mathf.Asin(horizonHeight);

            Debug.Log(horizonHeight);
            // Debug.Log(new Vector2(horizonHeight, h2));

            _underWaterPostProcMat.SetFloat(sp_HorizonHeight, horizonHeight);
            _underWaterPostProcMat.SetFloat(sp_HorizonOrientation, horizonRoll);


            // TOOD(UPP): Disable post-proc iff we don't have water intersecting the camera.
            Graphics.Blit(source, target, _underWaterPostProcMat);
        }
    }

}
