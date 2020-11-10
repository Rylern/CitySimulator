using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 
 *  Behaviour of the buttons on the white strip
 * 
*/
public class ButtonStrip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string tooltipString = "";

    private Tooltip tooltip;
    

    void Awake()
    {
        tooltip = GameObject.Find("Tooltip").GetComponent<Tooltip>();
    }


    public void ChangeTooltip(string newTooltip)
    {
        tooltipString = newTooltip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(tooltipString);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();
    }
}
