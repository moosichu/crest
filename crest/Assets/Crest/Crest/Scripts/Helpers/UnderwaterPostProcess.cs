using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterPostProcess : MonoBehaviour
{
    public Material _underWaterPostProcMat;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnRenderImage(RenderTexture source, RenderTexture target)
    {
        Graphics.Blit(source, target, _underWaterPostProcMat);
    }
}
