using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeadingUIController : MonoBehaviour
{
    public RectTransform[] texts;
    public float padding;
    public RectTransform maskingArea;

    public TextMeshProUGUI headingText;
    public UVController headingUV;

    float reciprocal;
    RectTransform textRectTransform;

    // Update is called once per frame
    void Awake()
    {
        textRectTransform = headingText.GetComponent<RectTransform>();
        reciprocal = (1.0f / 360.0f) * padding * texts.Length;
    }
    public void SetHeading(float heading)
    {
        // Main Text
        headingText.text = string.Format("{0:0}", heading);
        headingUV.SetUV(heading);

        // Texts
        int index = 0;
        float passBackThreshold = padding * (texts.Length * 0.5f);

        foreach (RectTransform text in texts)
        {
            float localPosX = index * padding - heading * reciprocal;

            // Adjust Text position
            if (localPosX > passBackThreshold)
            {
                localPosX -= padding * (texts.Length);
            }
            else if (localPosX < -passBackThreshold)
            {
                localPosX += padding * (texts.Length);
            }
            text.anchoredPosition = new Vector2(localPosX, 0);

            // Set Visible
            bool visible = (maskingArea.rect.width * -0.5f < localPosX && localPosX < maskingArea.rect.width * 0.5f) &&
                           (localPosX < textRectTransform.rect.width * -0.5f || textRectTransform.rect.width * 0.5f < localPosX);
            text.gameObject.SetActive(visible);

            index++;
        }
    }
}
