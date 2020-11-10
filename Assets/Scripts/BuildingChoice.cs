using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * 
 *  Handle interactions with the selection of buildings in the creation building panel
 * 
*/
public class BuildingChoice : MonoBehaviour
{
    [SerializeField] private Image buildingPreview = null;
    [SerializeField] private Sprite[] previews = null;
    
    private int nbBuildings;
    private int indexBuildingSelected;                                   // Current building selected, from 0 to nbBuildings


    void Awake()
    {
        indexBuildingSelected = 0;
        nbBuildings = previews.Length;
    }

    public void ChangeSelection(bool increase)
    {
        if (increase)
        {
            indexBuildingSelected += 1;
        }
        else
        {
            indexBuildingSelected -= 1;
        }
        indexBuildingSelected %= nbBuildings;
        if (indexBuildingSelected == -1)
        {
            indexBuildingSelected = nbBuildings - 1;
        }

        buildingPreview.sprite = previews[indexBuildingSelected];
    }

    public int GetChoice()
    {
        return indexBuildingSelected;
    }
}
