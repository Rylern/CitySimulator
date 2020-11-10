using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/*
 * 
 *  Compute and display the landmark visibility
 * 
*/
public class LandmarkVisibility : MonoBehaviour
{
    [SerializeField] private Material visibleMaterial = null;
    [SerializeField] private GameObject landmark = null;

    private int nbPositionsHeight;
    private int nbPositionsAngle;
    private int nbRays;
    private float radius;
    private float height;
    private bool alreadyComputed;
    private List<GameObject> buildingsSeen;
    private List<Material[]> buildingsMaterials;



    void Awake()
    {
        // Initialize variables
        radius = landmark.GetComponent<Collider>().bounds.size.x;
        height = landmark.GetComponent<Collider>().bounds.size.y;
        nbPositionsHeight = 50;
        nbPositionsAngle = 250;
        nbRays = 20;
        alreadyComputed = false;
        buildingsSeen = new List<GameObject>();
        buildingsMaterials = new List<Material[]>();
    }

    /*
     * Setters & getters
    */
    public void SetNbPositionsHeight(string res)
    {
        int.TryParse(res, out nbPositionsHeight);
    }
    public void SetNbPositionsAngle(string res)
    {
        int.TryParse(res, out nbPositionsAngle);
    }
    public void SetNbRays(string res)
    {
        int.TryParse(res, out nbRays);
    }
    public void SetLandmark(GameObject gameObject)
    {
        landmark = gameObject;
        radius = landmark.GetComponent<Collider>().bounds.size.x;
        if (landmark.name != "Eiffel Tower")
        {
            radius /= 2;
        }
        height = landmark.GetComponent<Collider>().bounds.size.y;
    }

    public int GetNbPositionsHeight()
    {
        return nbPositionsHeight;
    }
    public int GetNbPositionsAngle()
    {
        return nbPositionsAngle;
    }
    public int GetNbRays()
    {
        return nbRays;
    }
    public Vector3 GetLandmarkPosition()
    {
        return landmark.transform.position;
    }

    /*
     * Compute and display the landmark visibility
    */
    public void ComputeVisibility()
    {
        if (alreadyComputed)
        {
            ResetColors();
        }
        else
        {
            // Get the position of the landmark
            Vector3 basePosition = landmark.transform.position;
            if (landmark.name != "Eiffel Tower")
            {
                basePosition.y -= height / 2;
            }


            // Set the steps
            float stepAngle = 2 * Mathf.PI / nbPositionsAngle;
            float stepAngleY = 1 / (float)nbRays;
            float stepHeight = height / nbPositionsHeight;

            Material[] materials;
            Renderer renderer;
            // Loop over each point
            for (int i = 0; i < nbPositionsHeight; i++)
            {
                for (int j = 0; j < nbPositionsAngle; j++)
                {
                    // Starting point of the rays
                    Vector3 position = basePosition + new Vector3(radius * Mathf.Cos(j * stepAngle), (i + 1) * stepHeight, radius * Mathf.Sin(j * stepAngle));

                    // Send rays at different direction on the y-axis
                    for (int k = 0; k < nbRays; k++)
                    {
                        Vector3 direction = new Vector3(Mathf.Cos(j * stepAngle), k * stepAngleY - 0.5f, Mathf.Sin(j * stepAngle));

                        // Detect if a building is touched by the ray
                        int layerMask = ~(1 << 9);      // Ignore the bars
                        if (Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
                        {
                            GameObject target = hit.collider.gameObject;
                            if (target.CompareTag("Building") && !(buildingsSeen.Contains(target)) && target != landmark)
                            {
                                renderer = target.GetComponent<Renderer>();
                                materials = renderer.materials;
                                buildingsSeen.Add(target);
                                buildingsMaterials.Add(materials);

                                // Override the material of the building with a green one
                                renderer.materials = new Material[] { visibleMaterial, visibleMaterial, visibleMaterial, visibleMaterial };
                            }
                        }
                    }
                }
            }
            alreadyComputed = true;
        }
    }

    /*
     * Reset the materials on all buildings
    */
    public void ResetColors()
    {
        int i = 0;
        foreach (GameObject building in buildingsSeen)
        {
            if (building != null)
            {
                building.GetComponent<Renderer>().materials = buildingsMaterials[i];
            }
            i++;
        }
        buildingsSeen = new List<GameObject>();
        buildingsMaterials = new List<Material[]>();

        alreadyComputed = false;
    }
}
