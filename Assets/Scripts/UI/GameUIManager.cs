using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;


/*
 * 
 *  Handle the interaction with the UI during the simulation
 * 
*/
public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject shadow_variation_panel = null;
    [SerializeField] private GameObject CFH_panel = null;
    [SerializeField] private GameObject start_panel = null;
    [SerializeField] private GameObject create_building_panel = null;
    [SerializeField] private TMP_Text status_text = null;
    [SerializeField] private TMP_InputField CFH_pattern_input = null;
    [SerializeField] private TMP_Dropdown CFH_beginning_hour_dropdown = null;
    [SerializeField] private TMP_Dropdown CFH_ending_hour_dropdown = null;
    [SerializeField] private TMP_Dropdown SV_beginning_hour_dropdown = null;
    [SerializeField] private TMP_Dropdown SV_ending_hour_dropdown = null;
    [SerializeField] private GameObject bars_parent = null;
    [SerializeField] private GameObject show_bars_button = null;
    [SerializeField] private GameObject create_bars_button = null;
    [SerializeField] private GameObject delete_bars_button = null;
    [SerializeField] private Toggle normalize_toggle_sv = null;
    [SerializeField] private Toggle normalize_toggle_cfh = null;

    private bool start_panel_active;
    private bool buildingDeleteModeOn;
    private bool barDeleteModeOn;
    private bool landmarkModeOn;
    private BuildingPlacement buildingPlacement;
    private BarPlacement barPlacement;
    private SkyExposure skyExposure;
    private Heatmap heatmap;

    private void Awake()
    {
        start_panel_active = true;
        buildingDeleteModeOn = false;
        barDeleteModeOn = false;
        landmarkModeOn = false;
        buildingPlacement = GameObject.Find("Terrain").GetComponent<BuildingPlacement>();
        barPlacement = GameObject.Find("Terrain").GetComponent<BarPlacement>();
        skyExposure = GameObject.Find("Main Camera").GetComponent<SkyExposure>();
        heatmap = GameObject.Find("Heatmap").GetComponent<Heatmap>();
        create_building_panel.SetActive(false);
    }

    private void Update()
    {
        if (start_panel_active)
        {
            if (Input.anyKey)
            {
                start_panel.SetActive(false);
                start_panel_active = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearModes();
        }
    }

    public void ClearModes()
    {
        status_text.text = "";

        buildingDeleteModeOn = false;

        barDeleteModeOn = false;

        create_building_panel.SetActive(false);
        if (buildingPlacement.IsDraggingOn())
            buildingPlacement.ChangeDragging();

        if (barPlacement.IsDraggingOn())
            barPlacement.ChangeDragging();

        skyExposure.EscComputationMode();

        landmarkModeOn = false;
    }


    /*
     * Building delete mode
    */
    public void ChangeBuildingDeleteMode()
    {
        bool activateMode = !buildingDeleteModeOn;
        ClearModes();

        if (activateMode)
        {
            status_text.text = "Building delete mode activated. Left click on a building to delete it or press Esc to return.";
            buildingDeleteModeOn = true;
        }
    }

    public bool GetBuildingDeleteMode()
    {
        return buildingDeleteModeOn;
    }


    /*
     * Bar delete mode
    */
    public void ChangeBarDeleteMode()
    {
        bool activateMode = !barDeleteModeOn;
        ClearModes();

        if (activateMode)
        {
            status_text.text = "Bar delete mode activated. Left click on a bar to delete it or press Esc to return.";
            barDeleteModeOn = true;
        }
    }

    public bool GetBarDeleteMode()
    {
        return barDeleteModeOn;
    }


    /*
     * Landmark mode
    */
    public void ChangeLandmarkMode()
    {
        bool isOff = !landmarkModeOn;
        ClearModes();

        if (isOff)
        {
            status_text.text = "Landmark mode activated. Left click on a building to define it as the new landmark.\nPress Esc to return.";
        }
        else
        {
            status_text.text = "New landmark assigned.";
        }
        landmarkModeOn = isOff;
    }

    public bool GetLandmarkMode()
    {
        return landmarkModeOn;
    }


    /*
     * Building creation mode
    */
    public void SetBuildingCreationMode(bool modeOn)
    {
        bool panel_active = create_building_panel.activeSelf;
        ClearModes();

        if (modeOn)
            status_text.text = "Building creation mode activated. Left click to place the building or use the scroll wheel to rotate it.\nPress Esc to return.";

        create_building_panel.SetActive(panel_active);
    }


    /*
     * Bar creation mode
    */
    public void SetBarCreationMode(bool modeOn)
    {
        ClearModes();
        if (modeOn)
            status_text.text = "Bar creation mode activated. Left click to place the bar.\nPress Esc to return.";
    }


    /*
     * Exposure mode
    */
    public void SetExposureMode(float result)
    {
        ClearModes();

        if (result == -1)
            status_text.text = "Sky Exposure mode activated. Left click on a point of the city to compute the sky exposure.\nPress Esc to return.";
        else
            status_text.text = "Sky Exposure result: " + result.ToString() + "\nPress Esc to return.";
    }


    /*
     * Shadow variation
    */
    public void ComputeSV()
    {
        shadow_variation_panel.SetActive(false);
        heatmap.ComputeShadowVariation(normalize_toggle_sv.isOn);
    }
    public void SetSVBeginningHour(int index)
    {
        if (index > SV_ending_hour_dropdown.value)
        {
            SV_ending_hour_dropdown.value = index;
        }
        heatmap.SetBeginningHourSV(index);
    }
    public void SetSVEndingHour(int index)
    {
        if (index < SV_beginning_hour_dropdown.value)
        {
            SV_beginning_hour_dropdown.value = index;
        }
        heatmap.SetEndingHourSV(index);
    }


    /*
     * CFH
    */
    public void ComputeCFH()
    {
        // Check the pattern
        string pattern = CFH_pattern_input.text;
        foreach (char c in pattern)
        {
            if (c != '0' && c != '1')
            {
                return;
            }
        }

        CFH_panel.SetActive(false);
        heatmap.ComputeCFH(pattern, normalize_toggle_cfh.isOn);
    }
    public void SetCFHBeginningHour(int index)
    {
        if (index > CFH_ending_hour_dropdown.value)
        {
            CFH_ending_hour_dropdown.value = index;
        }
        heatmap.SetBeginningHourCFH(index);
    }
    public void SetCFHEndingHour(int index)
    {
        if (index < CFH_beginning_hour_dropdown.value)
        {
            CFH_beginning_hour_dropdown.value = index;
        }
        heatmap.SetEndingHourCFH(index);
    }


    /*
     * Panels
    */
    public void DisplayShadowVariationPanel()
    {
        bool panel_active = shadow_variation_panel.activeSelf;
        HidePanels();
        shadow_variation_panel.SetActive(!panel_active);
    }

    public void DisplayCFHPanel()
    {
        bool panel_active = CFH_panel.activeSelf;
        HidePanels();
        CFH_panel.SetActive(!panel_active);
    }

    public void DisplayCreatePanel()
    {
        bool panel_active = create_building_panel.activeSelf;
        ClearModes();
        HidePanels();
        create_building_panel.SetActive(!panel_active);
    }

    public void HidePanels()
    {
        shadow_variation_panel.SetActive(false);
        CFH_panel.SetActive(false);
        create_building_panel.SetActive(false);
    }


    /*
     * Show or hide bars
    */
    public void ChangeBars()
    {
        if (bars_parent.activeSelf)
        {
            show_bars_button.GetComponent<ButtonStrip>().ChangeTooltip("Show Bars");
            create_bars_button.GetComponent<ButtonStrip>().ChangeTooltip("Bars hiden");
            delete_bars_button.GetComponent<ButtonStrip>().ChangeTooltip("Bars hiden");
        }
        else
        {
            show_bars_button.GetComponent<ButtonStrip>().ChangeTooltip("Hide Bars");
            create_bars_button.GetComponent<ButtonStrip>().ChangeTooltip("Create a Bar");
            delete_bars_button.GetComponent<ButtonStrip>().ChangeTooltip("Delete a Bar");
        }
        create_bars_button.GetComponent<Button>().interactable = !bars_parent.activeSelf;
        delete_bars_button.GetComponent<Button>().interactable = !bars_parent.activeSelf;
        bars_parent.SetActive(!bars_parent.activeSelf);
    }
}
