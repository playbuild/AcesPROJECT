using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UVController : MonoBehaviour
{
    private RawImage image;
    public float unitValue;
    public float imageUnitCnt;

    float reciprocal;

    void Awake()
    {
        image = GetComponent<RawImage>();
        reciprocal = 1 / unitValue / imageUnitCnt;
    }

    public void SetUV(float value)
    {
        // range[0 - unitValue] -> [0 - 1 / imageUnitCnt]
        float remainder = value % unitValue;
        image.uvRect = new Rect(0, remainder * reciprocal, 1, 1);
    }
}
