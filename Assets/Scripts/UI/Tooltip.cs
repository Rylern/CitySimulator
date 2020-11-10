using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;

/*
 * 
 *  Move and modify the tooltip
 * 
*/
public class Tooltip : MonoBehaviour
{
    const float TEXT_PADDING_SIZE = 4f;

    private TMP_Text toolTipText;
    private RectTransform backgroundRectTransform;
    private bool started;


    void Awake()
    {
        backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        toolTipText = transform.Find("Text").GetComponent<TMP_Text>();
        started = false;
    }

    void Update()
    {
        if (started)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, null, out localPoint);
            transform.localPosition = localPoint;
        }
    }


    public void ShowTooltip(string toolTipString)
    {
        started = true;
        gameObject.SetActive(true);

        toolTipText.text = toolTipString;
        backgroundRectTransform.sizeDelta = new Vector2(toolTipText.preferredWidth + TEXT_PADDING_SIZE * 2f, toolTipText.preferredHeight + TEXT_PADDING_SIZE * 2f);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
