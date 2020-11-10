using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * 
 *  Compute sky exposure
 * 
*/
public class SkyExposure : MonoBehaviour
{
    private int rayNumber;
    private bool computationMode;
    private GameUIManager gameUIManager;


    void Awake()
    {
        rayNumber = 50;
        computationMode = false;
        gameUIManager = GameObject.Find("GameUI").GetComponent<GameUIManager>();
    }

    void Update()
    {
        if (computationMode && Input.GetMouseButtonDown(0))
        {
            ComputeSkyExposure();
        }
    }

    public void SetRayNumber(string newRayNumber)
    {
        int.TryParse(newRayNumber, out rayNumber);
    }
    public int GetRayNumber()
    {
        return rayNumber;
    }

    public void StartComputationMode()
    {
        gameUIManager.SetExposureMode(-1f);
        computationMode = true;
    }

    public void EscComputationMode()
    {
        computationMode = false;
    }

    private void ComputeSkyExposure()
    {
        int layerMask = 1 << 8;     // Mask that will make the ray goes through all entities except the terrain
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Vector3 position = hit.point;
            position.y += 2f;

            float theta;
            float phi;
            float resTheta = 0.5f * Mathf.PI / rayNumber;
            float resPhi = Mathf.PI / rayNumber;
            int totalRays = 0;
            int totalHits = 0;

            for (int i=-rayNumber; i<rayNumber; i++)
            {
                for (int j= -rayNumber; j<rayNumber; j++)
                {
                    theta = i * resTheta;
                    phi = j * resPhi;
                    Vector3 direction = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));

                    int layerMaskBar = ~(1 << 9);      // Ignore the bars
                    if (Physics.Raycast(position, direction, out RaycastHit hitSky, Mathf.Infinity, layerMaskBar))
                    {
                        totalHits++;
                    }
                    totalRays++;
                }
            }
            float result = (totalRays - totalHits) / (float)totalRays;
            gameUIManager.SetExposureMode(result);
            computationMode = true;
        }
    }
}
