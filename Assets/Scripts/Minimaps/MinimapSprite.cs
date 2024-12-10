using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MinimapSprite : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(90, transform.parent.eulerAngles.y, 0);
        transform.position = new Vector3(transform.parent.position.x, 0, transform.parent.position.z);
    }
}
