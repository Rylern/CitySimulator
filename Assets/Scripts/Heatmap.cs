using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


/*
 * 
 *  Compute the shadow variation / Change Frequency Heatmap and display the heatmap
 * 
*/
public class Heatmap : MonoBehaviour
{
    private const float SUN_DISTANCE = 1000;
    private const int DEFAULT_BEGIN_HOUR = 8;
    private const int DEFAULT_END_HOUR = 19;
    private const int NB_SUN_POSITION = 50;

    private int nbPointsParkX;
    private int nbPointsParkZ;
    private int metric;                                             // Comparison function for the CFH: 0: increase / 1: equality / 2: decrease
    private int beginningHourCFH;
    private int endingHourCFH;
    private int beginningHourSV;
    private int endingHourSV;
    private SunRotation sunRotation;


    private void Awake()
    {
        // Move the game object just above the park
        transform.localPosition = new Vector3(-0.5f, 1f, -0.5f);

        // Initialize variables
        nbPointsParkX = 50;
        nbPointsParkZ = 100;
        metric = 0;
        beginningHourCFH = DEFAULT_BEGIN_HOUR;
        endingHourCFH = DEFAULT_END_HOUR;
        beginningHourSV = DEFAULT_BEGIN_HOUR;
        endingHourSV = DEFAULT_END_HOUR;
        sunRotation = GameObject.Find("Sun").GetComponent<SunRotation>();
    }


    /*
     * Setters & getters
    */
    public void SetNbPointsParkX(string res)
    {
        int.TryParse(res, out nbPointsParkX);
    }
    public void SetNbPointsParkZ(string res)
    {
        int.TryParse(res, out nbPointsParkZ);
    }
    public void SetMetric(int i)
    {
        metric = i;
    }
    public void SetBeginningHourCFH(int index)
    {
        beginningHourCFH = index;
    }
    public void SetEndingHourCFH(int index)
    {
        endingHourCFH = index;
    }
    public void SetBeginningHourSV(int index)
    {
        beginningHourSV = index;
    }
    public void SetEndingHourSV(int index)
    {
        endingHourSV = index;
    }

    public int GetNbPointsParkX()
    {
        return nbPointsParkX;
    }
    public int GetNbPointsParkZ()
    {
        return nbPointsParkZ;
    }


    public void ResetMap()
    {
        GetComponent<MeshFilter>().mesh = new Mesh();
    }


    private float[,] Normalize(float[,] values)
    {
        int n = values.GetUpperBound(0) + 1;
        int m = values.GetUpperBound(1) + 1;
        float max = 0;

        for (int i=0; i<n; i++)
        {
            for (int j=0; j<m; j++)
            {
                if (values[i, j] > max)
                    max = values[i, j];
            }
        }
        if (max == 0)
            return values;
            
        for (int i=0; i<n; i++)
        {
            for (int j=0; j<m; j++)
            {
                values[i, j] /= max;
            }
        }
        return values;
    }


    public void ComputeShadowVariation(bool normalize)
    {
        float[,] intensities = new float[nbPointsParkX+1, nbPointsParkZ+1];

        DateTime time = sunRotation.GetDateTime().Date;
        DateTime endTime = time.AddHours(endingHourSV);
        time = time.AddHours(beginningHourSV);
        float secondsStep = (float) endTime.Subtract(time).TotalSeconds / NB_SUN_POSITION;
        float caseWidth = 1 / (float)nbPointsParkX;
        float caseHeight = 1 / (float)nbPointsParkZ;
        for (int j = 0; j < nbPointsParkZ+1; j++)
        {
            for (int i = 0; i < nbPointsParkX+1; i++)
            {
                intensities[i, j] = GetAverageLightIntensity(transform.TransformPoint(new Vector3(i * caseWidth, 0, j * caseHeight)), time, secondsStep);
            }
        }
        if (normalize)
            intensities = Normalize(intensities);
        DisplayHeatmap(intensities);
    }


    /*
     * Render the heatmap given the values
    */
    private void DisplayHeatmap(float[,] values)
    {
        // Create a mesh and initialize its variables
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4 * nbPointsParkX * nbPointsParkZ];      // Vertices of the mesh
        Vector2[] uv = new Vector2[4 * nbPointsParkX * nbPointsParkZ];            // Color on each vertex
        int[] triangles = new int[6 * nbPointsParkX * nbPointsParkZ];             // Definition of triangles

        // Initialize variables
        float caseWidth = 1 / (float)nbPointsParkX;
        float caseHeight = 1 / (float)nbPointsParkZ;
        int index = 0;

        for (int j = 0; j < nbPointsParkZ; j++)
        {
            for (int i = 0; i < nbPointsParkX; i++)
            {
                // For each cell in the grid, there are 4 vertices
                vertices[index * 4 + 0] = new Vector3(i * caseWidth, 0, j * caseHeight);
                vertices[index * 4 + 1] = new Vector3(i * caseWidth, 0, (j + 1) * caseHeight);
                vertices[index * 4 + 2] = new Vector3((i + 1) * caseWidth, 0, (j + 1) * caseHeight);
                vertices[index * 4 + 3] = new Vector3((i + 1) * caseWidth, 0, j * caseHeight);

                uv[index * 4 + 0] = new Vector2(values[i, j], 0);
                uv[index * 4 + 1] = new Vector2(values[i, j+1], 0);
                uv[index * 4 + 2] = new Vector2(values[i+1, j+1], 0);
                uv[index * 4 + 3] = new Vector2(values[i+1, j], 0);

                // Define the vertices of the 2 triangles of the cell
                triangles[index * 6 + 0] = index * 4;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;
                triangles[index * 6 + 3] = index * 4;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;

                index++;
            }
        }

        // Apply the new mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    /*
     * Returns the average light intensity of one point (at position coord) during the specified time
    */
    private float GetAverageLightIntensity(Vector3 coord, DateTime time, float secondsStep)
    {
        float sumLightIntensity = 0;

        for (int i = 0; i < NB_SUN_POSITION; i++)
        {
            time = time.AddSeconds(secondsStep);
            float[] angles = sunRotation.GetAngles(time);
            float altitude = angles[0];
            float azimuth = angles[1];
            sumLightIntensity += GetLightIntensity(coord, altitude, azimuth);
        }
        return sumLightIntensity / NB_SUN_POSITION;
    }

    /*
     * Returns the light intensity of one point (at position coord) at a certain time of the day (indicated with the index i)
    */
    private float GetLightIntensity(Vector3 coord, float altitude, float azimuth)
    {
        // Send a ray to the position of the sun and check if something is between. Uses horizontal coordinates
        Vector3 sunPos = SUN_DISTANCE * new Vector3(Mathf.Cos(azimuth + 3*Mathf.PI/2) * Mathf.Cos(altitude), Mathf.Sin(altitude), Mathf.Cos(altitude) * Mathf.Sin(azimuth + 3*Mathf.PI/2));

        int layerMask = ~(1 << 9);      // Ignore the bars
            
        if (Physics.Linecast(coord, sunPos, out RaycastHit hit, layerMask))
        {
            // There is a building between the point and the sun: no visibility
            return 0;
        }
        else
        {
            // There is nothing between the point and the sun
            // We compute the amount of light that point receives
            // The illumination is equal to the cosine of the angle between the surface normal and the light source: cos(azimuth - pi/2) here
            return Mathf.Sin(altitude);	// Because cos(angle - pi/2)=sin(angle)
        }
    }

    /*
     * Compute the Change Frequency Heatmap
    */
    public void ComputeCFH(string pattern, bool normalize)
    {
        // Initialize variables
        int[,,] D = new int[NB_SUN_POSITION - 1, nbPointsParkX+1, nbPointsParkZ+1];
        float caseWidth = 1 / (float)nbPointsParkX;
        float caseHeight = 1 / (float)nbPointsParkZ;
        DateTime time = sunRotation.GetDateTime().Date;
        DateTime endTime = time.AddHours(endingHourCFH);
        time.AddHours(beginningHourCFH);
        float secondsStep = endTime.Subtract(time).Seconds / NB_SUN_POSITION;

        // Loop over each time of the day and each point of the park to compute the binary change map
        for (int k = 0; k < NB_SUN_POSITION - 1; k++)
        {
            time = time.AddSeconds(secondsStep);
            float[] angles = sunRotation.GetAngles(time);
            float altitude1 = angles[0];
            float azimuth1 = angles[1];
            angles = sunRotation.GetAngles(time.AddSeconds(secondsStep));
            float altitude2 = angles[0];
            float azimuth2 = angles[1];

            for (int j = 0; j < nbPointsParkZ+1; j++)
            {
                for (int i = 0; i < nbPointsParkX+1; i++)
                {
                    Vector3 point = transform.TransformPoint(new Vector3(i * caseWidth, 0, j * caseHeight));

                    bool comparison;
                    if (metric == 0)
                    {
                        comparison = GetLightIntensity(point, altitude1, azimuth1) > GetLightIntensity(point, altitude2, azimuth2);
                    }
                    else if (metric == 1)
                    {
                        comparison = GetLightIntensity(point, altitude1, azimuth1) == GetLightIntensity(point, altitude2, azimuth2);
                    }
                    else
                    {
                        comparison = GetLightIntensity(point, altitude1, azimuth1) < GetLightIntensity(point, altitude2, azimuth2);
                    }

                    if (comparison)
                    {
                        D[k, i, j] = 0;
                    }
                    else
                    {
                        D[k, i, j] = 1;
                    }
                }
            }
        }

        // Create the motion history diagram for the given pattern
        float[,] CFH = new float[nbPointsParkX+1, nbPointsParkZ+1];
        int pattern_length = pattern.Length - 1;
        for (int j = 0; j < nbPointsParkZ+1; j++)
        {
            for (int i = 0; i < nbPointsParkX+1; i++)
            {
                CFH[i, j] = 0f;

                // Computer how many times the pattern is present at a certain point
                for (int k = pattern_length; k < NB_SUN_POSITION - 1; k++)
                {
                    int p = 0;
                    while (p < pattern_length+1 && D[k - pattern_length + p, i, j] == pattern[p] - '0')
                    {
                        p++;
                    }
                    if (p == pattern_length+1)
                    {
                        CFH[i, j] += 1;
                    }
                }

                // Normalize the value
                CFH[i, j] /= NB_SUN_POSITION - 1 - pattern_length;

                // Avoid 0 and 1 values that are not well represented by the mesh
                if (CFH[i, j] == 0)
                {
                    CFH[i, j] += 0.01f;
                }
                else if (CFH[i, j] == 1)
                {
                    CFH[i, j] -= 0.01f;
                }
            }
        }

        if (normalize)
            CFH = Normalize(CFH);

        // Display the associated heatmap
        DisplayHeatmap(CFH);
    }
}
