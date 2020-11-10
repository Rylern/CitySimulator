using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;


/*
 * 
 *  Handle the placement of buildings when creating a new one
 * 
*/
public class BuildingPlacement : MonoBehaviour
{
    private const float ROTATION_SPEED = 10f;
    private const int NB_RAYS_PLACABLE = 5;

    [SerializeField] private Material placableMaterial = null;
    [SerializeField] private Material nonPlacableMaterial = null;
    [SerializeField] private GameObject[] buildingPrefabs = null;
    [SerializeField] private Transform parentCity = null;

    private BuildingChoice buildingChoice;
    private GameUIManager gameUIManager;
    private GameObject buildingSelected;
    private Material[] buildingMaterial;
    private bool dragging;
    private Vector3 buildingSize;
    private bool placable;


    void Awake()
    {
        dragging = false;
        placable = false;
        gameUIManager = GameObject.Find("GameUI").GetComponent<GameUIManager>();
    }


    void Update()
    {
        if (dragging)
        {
            // Move building to cursor position
            int layerMask = 1 << 8;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Vector3 pos = hit.point;
                pos.y += buildingSize.y/2;
                buildingSelected.transform.position = pos;

                // Check if building is placable
                bool isPlacable = IsPlacable(pos.x, pos.z);
                if (isPlacable != placable)
                {
                    placable = isPlacable;
                    if (placable)
                    {
                        buildingSelected.GetComponent<Renderer>().materials = new Material[] { placableMaterial, placableMaterial, placableMaterial, placableMaterial };
                    }
                    else
                    {
                        buildingSelected.GetComponent<Renderer>().materials = new Material[] { nonPlacableMaterial, nonPlacableMaterial, nonPlacableMaterial, nonPlacableMaterial };
                    }
                }
            }

            // Rotate building with the mouse wheel
            buildingSelected.transform.Rotate(Vector3.up, Input.mouseScrollDelta.y * ROTATION_SPEED);

            // Release building if left clicked
            if (Input.GetMouseButtonDown(0))
            {
                dragging = false;
                gameUIManager.SetBuildingCreationMode(false);
                if (!placable)
                {
                    Destroy(buildingSelected);
                }
                else
                {
                    buildingSelected.GetComponent<Renderer>().materials = buildingMaterial;
                    buildingSelected.layer = 0;
                    buildingSelected = null;
                    buildingMaterial = null;
                }
            }

            // Cancel if Esc clicked
            if (Input.GetKey(KeyCode.Escape))
            {
                ChangeDragging();
            }
        }
    }


    /*
     * Getter
    */
    public bool IsDraggingOn()
    {
        return dragging;
    }


    /*
     * Launch or exit dragging mode
    */
    public void ChangeDragging()
    {
        if (dragging)
        {
            Destroy(buildingSelected);
            dragging = false;
            gameUIManager.SetBuildingCreationMode(false);
        }
        else
        {
            gameUIManager.SetBuildingCreationMode(true);
            if (buildingChoice == null)
            {
                buildingChoice = GameObject.Find("Create Building Panel").GetComponent<BuildingChoice>();
            }

            buildingSelected = Instantiate(buildingPrefabs[buildingChoice.GetChoice()], parentCity);

            buildingMaterial = buildingSelected.GetComponent<Renderer>().materials;
            buildingSize = buildingSelected.GetComponent<Collider>().bounds.size;

            buildingSelected.GetComponent<Renderer>().materials = new Material[] { nonPlacableMaterial, nonPlacableMaterial, nonPlacableMaterial, nonPlacableMaterial };
            placable = false;
            buildingSelected.layer = 2;
            dragging = true;
        }
    }


    /*
     * Check if the current selected building is placable given its coordinates
    */
    private bool IsPlacable(float x, float z)
    {
        int layerMask = ~((1 << 8) | (1 << 2) | (1 << 9));
        Vector3 position = new Vector3(x, -10f, z);

        float angle = buildingSelected.transform.eulerAngles.y * Mathf.Deg2Rad;

        position.x -= (Mathf.Cos(angle) * buildingSize.x + Mathf.Sin(angle) * buildingSize.z) / 2;
        position.z -= (Mathf.Cos(angle) * buildingSize.z + Mathf.Sin(angle) * buildingSize.x) / 2;

        Vector3 basePosition = position;
        float stepX = buildingSize.x / (NB_RAYS_PLACABLE - 1);
        float stepZ = buildingSize.z / (NB_RAYS_PLACABLE - 1);

        for (int i=0; i < NB_RAYS_PLACABLE; i++)
        {
            for (int j=0; j < NB_RAYS_PLACABLE; j++)
            {
                position = basePosition + new Vector3(stepX*i*Mathf.Cos(angle) + stepZ * j * Mathf.Sin(angle), 0f, stepZ * j * Mathf.Cos(angle) + stepX * i * Mathf.Sin(angle));
                if (Physics.Raycast(position, Vector3.up, Mathf.Infinity, layerMask))
                {
                    return false;
                }
            }
        }
        
        return true;
    }


}
