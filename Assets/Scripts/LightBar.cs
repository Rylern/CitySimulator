using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 
 *  Move and change color of bar
 * 
*/
public class LightBar : MonoBehaviour
{
    private const int SUN_DISTANCE = 1000;

    private GameObject sun;
    private Vector3 pointPosition;
    private float height;
    private Mesh mesh;
    private Vector2[] uvs;
    private bool dragging;
    private GameUIManager gameUIManager;


    void Awake()
    {
        sun = GameObject.Find("Sun");
        height = GetComponent<Collider>().bounds.size.y;
        dragging = false;

        pointPosition = DeterminePointPosition();

        mesh = GetComponent<MeshFilter>().mesh;
        uvs = new Vector2[mesh.vertices.Length];
    }

    void Update()
    {
        if (!dragging)
        {
            float lightIntensity = GetLightIntensity();
            Vector3 newPosition = pointPosition;

            newPosition.y += height * (lightIntensity - 0.5f) - 3f;
            transform.position = newPosition;

            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(lightIntensity, 0);
            }
            mesh.uv = uvs;
        }
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

        // Destroy the bar if the delete mode is activated
        if (gameUIManager.GetBarDeleteMode())
        {
            Destroy(gameObject);
        }
    }

    private float GetLightIntensity()
    {
        Vector3 sunAngles = sun.transform.eulerAngles * Mathf.Deg2Rad;
        float theta = Mathf.PI / 2 - sunAngles.x;
        float phi = 3*Mathf.PI / 2 - sunAngles.y;
        Vector3 sunPosition = SUN_DISTANCE * new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));

        int layerMask = ~(1 << 9);      // Ignore the bars
        if (Physics.Linecast(pointPosition, sunPosition, out RaycastHit hit, layerMask))
        {
            return 0;
        }
        else
        {
            return Mathf.Max(0, Mathf.Cos(theta));
        }
    }

    private Vector3 DeterminePointPosition()
    {
        Vector3 pos = transform.position;

        // Place the point 2m above the terrain
        int layerMask = 1 << 8;     // Mask that will make the ray goes through all entities except the terrain
        if (Physics.Raycast(new Vector3(pos.x, height, pos.z), Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            pos.y = hit.point.y + 2f;
        }
        else
        {
            pos.y = 2f;
        }
        return pos;
    }


    public void SetDragging(bool drag)
    {
        if (!drag)
            pointPosition = DeterminePointPosition();

        dragging = drag;
    }
}
