using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 *  Handle the placement of bars when creating a new one
 * 
*/
public class BarPlacement : MonoBehaviour
{
    [SerializeField] private GameObject barPrefab = null;
    [SerializeField] private Transform parentBar = null;

    private GameObject bar;
    private bool dragging;
    private GameUIManager gameUIManager;
    private Vector3 buildingSize;


    void Awake()
    {
        dragging = false;
        gameUIManager = GameObject.Find("GameUI").GetComponent<GameUIManager>();
    }


    void Update()
    {
        if (dragging)
        {
            // Move bar to cursor position
            int layerMask = 1 << 8;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Vector3 pos = hit.point;
                pos.y += buildingSize.y/2;
                bar.transform.position = pos;
            }

            // Release building if left clicked
            if (Input.GetMouseButtonDown(0))
            {
                dragging = false;
                gameUIManager.SetBarCreationMode(false);
                bar.GetComponent<LightBar>().SetDragging(false);

                bar = null;
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
            Destroy(bar);
            dragging = false;
            gameUIManager.SetBarCreationMode(false);
        }
        else
        {
            gameUIManager.SetBarCreationMode(true);

            bar = Instantiate(barPrefab, parentBar);
            bar.GetComponent<LightBar>().SetDragging(true);
            buildingSize = bar.GetComponent<Collider>().bounds.size;

            dragging = true;
        }
    }
}
