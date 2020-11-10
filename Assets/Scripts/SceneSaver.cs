using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TMPro;
using UnityEngine;


/*
 * 
 *  Save, load and delete scenes
 * 
*/
public class SceneSaver : MonoBehaviour
{
    [SerializeField] private GameObject[] buildingPrefabs = null;
    [SerializeField] private GameObject barPrefab = null;
    [SerializeField] private Transform barParent = null;

    private LandmarkVisibility landmarkVisibility;
    private SunRotation sunRotation;
    private Heatmap heatmap;


    void Awake()
    {
        landmarkVisibility = GameObject.Find("City").GetComponent<LandmarkVisibility>();
        sunRotation = GameObject.Find("Sun").GetComponent<SunRotation>();
        heatmap = GameObject.Find("Heatmap").GetComponent<Heatmap>();
    }


    public void SaveScene(string save_name)
    {
        if (save_name == "")
            return;

        SceneData data = GetSceneData();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path.Combine(Application.persistentDataPath, save_name + ".dat"));

        bf.Serialize(file, data);
        file.Close();
    }

    public void LoadScene(TMP_Dropdown load_dropdown)
    {
        string path = Path.Combine(Application.persistentDataPath, load_dropdown.options[load_dropdown.value].text + ".dat");
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            SceneData data = (SceneData)bf.Deserialize(file);
            file.Close();

            CreateScene(data);
        }
    }

    public void DeleteScene(TMP_Dropdown load_dropdown)
    {
        string path = Path.Combine(Application.persistentDataPath, load_dropdown.options[load_dropdown.value].text + ".dat");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }


    private SceneData GetSceneData()
    {
        int n = transform.childCount;
        int[] buildingsIndexes = new int[n];
        Vector3[] positions = new Vector3[n];
        Quaternion[] rotations = new Quaternion[n];
        Vector3[] scales = new Vector3[n];
        Vector3 landmarkPosition = landmarkVisibility.GetLandmarkPosition();
        int n_bar = barParent.childCount;
        Vector3[] barPositions = new Vector3[n_bar];
        float sunSpeed = sunRotation.GetSpeed();
        double latitude = sunRotation.GetLatitude();
        double longitude = sunRotation.GetLongitude();
        float UTCHours = sunRotation.GetUTC();
        DateTime dateTime = sunRotation.GetDateTime();

        for (int i=0; i<n; i++)
        {
            Transform child = transform.GetChild(i);
            int index = child.name.IndexOf('(');
            string indexStr;
            if (index != -1)
                indexStr = child.name.Substring(0, index);
            else
                indexStr = child.name;

            buildingsIndexes[i] = int.Parse(indexStr);
            positions[i] = child.position;
            rotations[i] = child.rotation;
            scales[i] = child.localScale;
        }
        for (int i=0; i<n_bar; i++)
        {
            barPositions[i] = barParent.GetChild(i).position;
        }

        return new SceneData(buildingsIndexes, positions, rotations, scales, landmarkPosition, barPositions, sunSpeed, latitude, longitude, UTCHours, dateTime);
    }

    private void CreateScene(SceneData data)
    {
        // Reset landmark visibility and heatmap
        landmarkVisibility.ResetColors();
        heatmap.ResetMap();

        // Delete all existing buildings
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Delete all existing bars
        foreach (Transform child in barParent)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Load saved buildings and bars, and location and time
        int[] buildingsIndexes = data.GetBuildingsIndexes();
        Vector3[] positions = data.GetPositions();
        Quaternion[] rotations = data.GetRotations();
        Vector3[] scales = data.GetScales();
        Vector3 landmarkPosition = data.GetLandmarkPosition();
        Vector3[] barPositions = data.GetBarPositions();
        float sunSpeed = data.GetSunSpeed();
        double latitude = data.GetLatitude();
        double longitude = data.GetLongitude();
        float UTCHours = data.GetUTCHours();
        DateTime dateTime = data.GetDateTime();

        // Create saved buildings and bars
        bool isTowerLandmark = true;
        for (int i = 0; i < buildingsIndexes.Length; i++)
        {
            GameObject building = Instantiate(buildingPrefabs[buildingsIndexes[i]], positions[i], rotations[i]);
            building.transform.SetParent(transform);
            building.transform.localScale = scales[i];
            if (building.transform.position == landmarkPosition)
            {
                landmarkVisibility.SetLandmark(building);
                isTowerLandmark = false;
            }
        }
        if (isTowerLandmark)
        {
            landmarkVisibility.SetLandmark(GameObject.Find("Eiffel Tower"));
        }
        for (int i = 0; i < barPositions.Length; i++)
        {
            GameObject bar = Instantiate(barPrefab, barPositions[i], Quaternion.identity);
            bar.transform.SetParent(barParent);
        }

        // Set time and location
        sunRotation.SetSpeed(sunSpeed);
        sunRotation.SetLatitude(latitude);
        sunRotation.SetLongitude(longitude);
        sunRotation.SetUTC(UTCHours);
        sunRotation.SetDateTime(dateTime);
    }
}


/*
 * 
 *  Data class representing the values saved in files
 * 
*/
[Serializable]
class SceneData
{
    private int[] buildingsIndexes;

    private float[] positionsX;
    private float[] positionsY;
    private float[] positionsZ;

    private float[] rotationsX;
    private float[] rotationsY;
    private float[] rotationsZ;
    private float[] rotationsW;

    private float[] scalesX;
    private float[] scalesY;
    private float[] scalesZ;

    private float landmarkPositionX;
    private float landmarkPositionY;
    private float landmarkPositionZ;

    private float[] barPositionsX;
    private float[] barPositionsY;
    private float[] barPositionsZ;

    private float sunSpeed;
    private double latitude;
    private double longitude;
    private float UTCHours;
    private long dateTimeTicks;


    public SceneData(
        int[] bldgsI, Vector3[] pos, Quaternion[] rot, Vector3[] scl, Vector3 landmarkPosition, Vector3[] barPos, float speed, double lat, double lon, float UTC, DateTime dateTime)
    {

        buildingsIndexes = bldgsI;

        int n = pos.Length;
        positionsX = new float[n];
        positionsY = new float[n];
        positionsZ = new float[n];

        rotationsX = new float[n];
        rotationsY = new float[n];
        rotationsZ = new float[n];
        rotationsW = new float[n];

        scalesX = new float[n];
        scalesY = new float[n];
        scalesZ = new float[n];

        int bar_n = barPos.Length;
        barPositionsX = new float[bar_n];
        barPositionsY = new float[bar_n];
        barPositionsZ = new float[bar_n];

        for (int i=0; i<n; i++)
        {
            positionsX[i] = pos[i].x;
            positionsY[i] = pos[i].y;
            positionsZ[i] = pos[i].z;

            rotationsX[i] = rot[i].x;
            rotationsY[i] = rot[i].y;
            rotationsZ[i] = rot[i].z;
            rotationsW[i] = rot[i].w;

            scalesX[i] = scl[i].x;
            scalesY[i] = scl[i].y;
            scalesZ[i] = scl[i].z;
        }

        landmarkPositionX = landmarkPosition.x;
        landmarkPositionY = landmarkPosition.y;
        landmarkPositionZ = landmarkPosition.z;

        for (int i=0; i<bar_n; i++)
        {
            barPositionsX[i] = barPos[i].x;
            barPositionsY[i] = barPos[i].y;
            barPositionsZ[i] = barPos[i].z;
        }

        sunSpeed = speed;
        latitude = lat;
        longitude = lon;
        UTCHours = UTC;
        dateTimeTicks = dateTime.Ticks;
    }

    public int[] GetBuildingsIndexes()
    {
        return buildingsIndexes;
    }

    public Vector3[] GetPositions()
    {
        int n = positionsX.Length;
        Vector3[] res = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            res[i] = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);
        }
        return res;
    }

    public Quaternion[] GetRotations()
    {
        int n = positionsX.Length;
        Quaternion[] res = new Quaternion[n];
        for (int i = 0; i < n; i++)
        {
            res[i] = new Quaternion(rotationsX[i], rotationsY[i], rotationsZ[i], rotationsW[i]);
        }
        return res;
    }

    public Vector3[] GetScales()
    {
        int n = positionsX.Length;
        Vector3[] res = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            res[i] = new Vector3(scalesX[i], scalesY[i], scalesZ[i]);
        }
        return res;
    }

    public Vector3 GetLandmarkPosition()
    {
        return new Vector3(landmarkPositionX, landmarkPositionY, landmarkPositionZ);
    }

    public Vector3[] GetBarPositions()
    {
        int n = barPositionsX.Length;
        Vector3[] res = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            res[i] = new Vector3(barPositionsX[i], barPositionsY[i], barPositionsZ[i]);
        }
        return res;
    }
    
    public float GetSunSpeed()
    {
        return sunSpeed;
    }
    public double GetLatitude()
    {
        return latitude;
    }
    public double GetLongitude()
    {
        return longitude;
    }
    public float GetUTCHours()
    {
        return UTCHours;
    }
    public DateTime GetDateTime()
    {
        return new DateTime(dateTimeTicks);
    }
}