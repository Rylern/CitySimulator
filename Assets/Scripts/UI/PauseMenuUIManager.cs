using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
 * 
 *  Handle the interaction with the pause menu
 * 
*/
public class PauseMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject main_menu_panel = null;
    [SerializeField] private GameObject save_menu_panel = null;
    [SerializeField] private GameObject load_menu_panel = null;

    // Settings menu panel and settings components
    [SerializeField] private GameObject settings_menu = null;               
    [SerializeField] private TMP_Dropdown keyboard_dropdown = null;
    [SerializeField] private TMP_InputField camera_speed_input = null;
    [SerializeField] private TMP_InputField camera_sensitivity_input = null;
    [SerializeField] private TMP_InputField heatmap_X_input = null;
    [SerializeField] private TMP_InputField heatmap_Z_input = null;
    [SerializeField] private TMP_InputField landmark_height_input = null;
    [SerializeField] private TMP_InputField landmark_angle_input = null;
    [SerializeField] private TMP_InputField landmark_nbRays_input = null;
    [SerializeField] private TMP_InputField skyExposure_nbRays_input = null;

    // Save & load components
    [SerializeField] private TMP_Text save_status_success = null;
    [SerializeField] private TMP_Text save_status_error = null;
    [SerializeField] private TMP_InputField save_input = null;
    [SerializeField] private TMP_Dropdown load_dropdown = null;
    [SerializeField] private TMP_Text load_status_success = null;

    private SceneSaver sceneSaver;
    private UIManager uiManager;
    private CameraController cameraController;
    private Heatmap heatmap;
    private LandmarkVisibility landmarkVisibility;
    private SkyExposure skyExposure;


    void Awake()
    {
        sceneSaver = GameObject.Find("Buildings").GetComponent<SceneSaver>();
        uiManager = GameObject.Find("UI").GetComponent<UIManager>();
        cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
        heatmap = GameObject.Find("Heatmap").GetComponent<Heatmap>();
        landmarkVisibility = GameObject.Find("City").GetComponent<LandmarkVisibility>();
        skyExposure = GameObject.Find("Main Camera").GetComponent<SkyExposure>();
    }


    /*
     * Menus
    */
    public void DisplayMainMenu()
    {
        HideMenus();
        main_menu_panel.SetActive(true);
    }

    public void DisplaySaveMenu()
    {
        HideMenus(); 
        save_status_success.enabled = false;
        save_status_error.enabled = false;
        save_menu_panel.SetActive(true);
    }

    public void DisplayLoadMenu()
    {
        HideMenus();
        UpdateLoadDropdown();
        load_status_success.enabled = false;
        load_menu_panel.SetActive(true);
    }

    public void DisplaySettingsMenu()
    {
        HideMenus();
        UpdateSettings();
        settings_menu.SetActive(true);
    }

    private void HideMenus()
    {
        main_menu_panel.SetActive(false);
        save_menu_panel.SetActive(false);
        load_menu_panel.SetActive(false);
        settings_menu.SetActive(false);
    }


    /*
     * Save menu
    */
    public void SaveScene()
    {
        string save_name = save_input.text;

        if (save_name == "")
        {
            save_status_error.enabled = true;
            return;
        }
        try
        {
            FileStream file = File.Create(Path.Combine(Application.persistentDataPath, save_name + ".dat"));
            file.Close();
        }
        catch (ArgumentException)
        {
            save_status_error.enabled = true;
            return;
        }

        save_status_error.enabled = false;
        save_status_success.enabled = true;
        sceneSaver.SaveScene(save_input.text);
    }


    /*
     * Load menu
    */
    public void LoadScene()
    {
        if (load_dropdown.value >= load_dropdown.options.Count)
            return;

        sceneSaver.LoadScene(load_dropdown);
        DisplayMainMenu();
        uiManager.DisplayGame();
    }
    public void DeleteScene()
    {
        if (load_dropdown.options.Count < 1)
            return;

        sceneSaver.DeleteScene(load_dropdown);
        load_status_success.enabled = true;
        UpdateLoadDropdown();
    }
    private void UpdateLoadDropdown()
    {
        load_dropdown.ClearOptions();
        List<string> simSaved = new List<string>();
        string sim;
        foreach (string file in System.IO.Directory.GetFiles(Application.persistentDataPath))
        {
            sim = Path.GetFileName(file);
            if (sim.Contains(".dat"))
            {
                simSaved.Add(sim.Replace(".dat", ""));
            }
        }
        load_dropdown.AddOptions(simSaved);
    }


    /*
     * Settings menu
    */
    private void UpdateSettings()
    {
        if (cameraController.IsQwerty())
        {
            keyboard_dropdown.value = 0;
        }
        else
        {
            keyboard_dropdown.value = 1;
        }

        camera_speed_input.text = cameraController.GetSpeed().ToString();
        camera_sensitivity_input.text = cameraController.GetSensitivity().ToString();

        heatmap_X_input.text = heatmap.GetNbPointsParkX().ToString();
        heatmap_Z_input.text = heatmap.GetNbPointsParkZ().ToString();

        landmark_height_input.text = landmarkVisibility.GetNbPositionsHeight().ToString();
        landmark_angle_input.text = landmarkVisibility.GetNbPositionsAngle().ToString();
        landmark_nbRays_input.text = landmarkVisibility.GetNbRays().ToString();

        skyExposure_nbRays_input.text = skyExposure.GetRayNumber().ToString();
    }

    /*
     * Quit
    */
    public void Quit()
    {
        Application.Quit();
    }
}
