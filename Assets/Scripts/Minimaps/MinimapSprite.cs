using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MinimapSprite : MonoBehaviour
{
    public MinimapCamera minimapCamera;
    SpriteRenderer spriteRenderer;

    public float iconSize;
    public float depth;
    float sizeReciprocal;

    [SerializeField]
    bool showBorderIndicator = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sizeReciprocal = iconSize / minimapCamera.GetCameraViewSize();
        depth *= 0.01f;
    }
    void Update()
    {
        transform.rotation = Quaternion.Euler(90, transform.parent.eulerAngles.y, 0);
        transform.position = new Vector3(transform.parent.position.x, 0, transform.parent.position.z);

        if (spriteRenderer.isVisible == false)
        {
            minimapCamera.ShowBorderIndicator(transform.position);
        }
        else
        {
            minimapCamera.HideBorderIncitator();
        }
        float scale = sizeReciprocal * minimapCamera.GetCameraViewSize();
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
