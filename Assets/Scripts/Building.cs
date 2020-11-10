using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/*
 * 
 *  Handle interactions with each building
 * 
*/
public class Building : MonoBehaviour
{
    private GameUIManager gameUIManager;
    private LandmarkVisibility landmarkVisibility;


    private void Start()
    {
        landmarkVisibility = GameObject.Find("City").GetComponent<LandmarkVisibility>();
    }
    
    void OnMouseDown()
    {
        // Skip if the user has clicked on a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Define gameUIManager if needed
        if (gameUIManager == null)
        {
            gameUIManager = GameObject.Find("GameUI").GetComponent<GameUIManager>();
        }

        // Destroy the building if the delete mode is activated and the building is neither the current landmark nor the Eiffel Tower
        if (gameUIManager.GetBuildingDeleteMode() && transform.position != landmarkVisibility.GetLandmarkPosition() && gameObject.name != "Eiffel Tower")
        {
            Destroy(gameObject);
        }

        // Assign the building as the new landmark
        if (gameUIManager.GetLandmarkMode())
        {
            landmarkVisibility.SetLandmark(this.gameObject);
            gameUIManager.ChangeLandmarkMode();
        }
    }
}
