using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crest
{

    public class UnderwaterPostProcess : MonoBehaviour
    {
        public Material _underWaterPostProcMat;
        public Material _underWaterHorizonMarker;
        public GameObject _horizonStenciller;
        public GameObject _postProcesser;

        private Camera _mainCamera;
        static int sp_HorizonHeight = Shader.PropertyToID("_HorizonHeight");
        static int sp_HorizonOrientation = Shader.PropertyToID("_HorizonOrientation");

        // Start is called before the first frame update
        void Start()
        {
            // hack - push forward so the geometry wont be frustum culled. there might be better ways to draw
            // this stuff.
            _mainCamera = transform.parent.GetComponent<Camera>();
            if (_mainCamera == null)
            {
                Debug.LogError("Underwater effects expect to be parented to a camera.", this);
                enabled = false;

                return;
            }
            transform.localPosition = Vector3.forward;

            {
                MeshRenderer _meshRenderer = _postProcesser.GetComponent<MeshRenderer>();
                _meshRenderer.material = _underWaterPostProcMat;
            }
            {
                MeshRenderer _meshRenderer = _horizonStenciller.GetComponent<MeshRenderer>();
                _meshRenderer.material = _underWaterHorizonMarker;
            }
        }

        void Update()
        {
            // TODO: Make front facing quad occupy camera frustrum

            // TODO: Make the horizon marker fill up half the screen
            {
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

                _underWaterHorizonMarker.SetFloat(sp_HorizonHeight, horizonHeight);
                _underWaterHorizonMarker.SetFloat(sp_HorizonOrientation, horizonRoll);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture target)
        {

        }
    }

}
