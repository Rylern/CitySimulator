using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;

/*
 * 
 *  Update location and date UI
 * 
*/
public class DateLocationManager : MonoBehaviour
{
    [SerializeField] private GameObject location_panel = null;
    [SerializeField] private GameObject date_panel = null;
    [SerializeField] private GameObject time_panel = null;
    [SerializeField] private TMP_Text date_text = null;
    [SerializeField] private TMP_Text time_text = null;
    [SerializeField] private TMP_InputField[] location_inputs = null;
    [SerializeField] private TMP_InputField[] date_inputs = null;
    [SerializeField] private TMP_InputField[] time_inputs = null;

    private SunRotation sunRotation;


    void Awake()
    {
        sunRotation = GameObject.Find("Sun").GetComponent<SunRotation>();
    }

    void Update()
    {
        // Update date and time texts
        DateTime dateTime = sunRotation.GetDateTime();
        date_text.text = dateTime.ToString("d", CultureInfo.CreateSpecificCulture("en-US"));
        time_text.text = dateTime.ToString("t", CultureInfo.CreateSpecificCulture("fr-FR"));
    }

    /*
    * Panels
    */
    public void HidePanels()
    {
        location_panel.SetActive(false);
        date_panel.SetActive(false);
        time_panel.SetActive(false);
    }
    private void DisplayPanel(GameObject panel)
    {
        bool panel_active = panel.activeSelf;
        HidePanels();

        if (!panel_active)
            sunRotation.PauseSpeed();

        UpdateInputs(panel);
    
        panel.SetActive(!panel_active);
    }
    public void DisplayLocationPanel()
    {
        DisplayPanel(location_panel);
    }
    public void DisplayDatePanel()
    {
        DisplayPanel(date_panel);
    }
    public void DisplayTimePanel()
    {
        DisplayPanel(time_panel);
    }

    /*
    * Inputs
    */
    private void UpdateInputs(GameObject panel)
    {
        if (panel == location_panel)
        {
            location_inputs[0].text = sunRotation.GetLatitude().ToString();
            location_inputs[1].text = sunRotation.GetLongitude().ToString();
            location_inputs[2].text = sunRotation.GetUTC().ToString();
        }
        else if (panel == date_panel)
        {
            DateTime dateTime = sunRotation.GetDateTime();
            date_inputs[0].text = dateTime.Day.ToString();
            date_inputs[1].text = dateTime.Month.ToString();
            date_inputs[2].text = dateTime.Year.ToString();
        }
        else
        {
            DateTime dateTime = sunRotation.GetDateTime();
            time_inputs[0].text = dateTime.Hour.ToString();
            time_inputs[1].text = dateTime.Minute.ToString();
        }
    }

    private void SetCoord(int type, string value)
    {
        double val;
        if (Double.TryParse(value, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-US"), out val))
        {
            if (type == 0)
                sunRotation.SetLatitude(val);
            else if (type == 1)
                sunRotation.SetLongitude(val);
        }
        UpdateInputs(location_panel);
    }
    public void SetLatitude(string latitude)
    {
        SetCoord(0, latitude);
    }
    public void SetLongitude(string longitude)
    {
        SetCoord(1, longitude);
    }
    public void SetUTC(string UTC)
    {
        float utc;
        if (Single.TryParse(UTC, NumberStyles.Float, CultureInfo.CreateSpecificCulture("en-US"), out utc))
            sunRotation.SetUTC(utc);
        UpdateInputs(location_panel);
    }

    private void SetDateTime(int type, string value)
    {
        DateTime dateTime = sunRotation.GetDateTime();
        int val;
        if (Int32.TryParse(value, out val))
        {
            try
            {
                if (type == 0)
                    dateTime = new DateTime(val, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                else if (type == 1)
                    dateTime = new DateTime(dateTime.Year, val, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                else if (type == 2)
                    dateTime = new DateTime(dateTime.Year, dateTime.Month, val, dateTime.Hour, dateTime.Minute, dateTime.Second);
                else if (type == 3)
                    dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, val, dateTime.Minute, dateTime.Second);
                else if (type == 4)
                    dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, val, dateTime.Second);

                sunRotation.SetDateTime(dateTime);
            }
            catch (ArgumentOutOfRangeException)
            {}
        }
        if (type < 3)
            UpdateInputs(date_panel);
        else
            UpdateInputs(time_panel);
    }
    public void SetYear(string year)
    {
        SetDateTime(0, year);
    }
    public void SetMonth(string month)
    {
        SetDateTime(1, month);
    }
    public void SetDay(string day)
    {
        SetDateTime(2, day);
    }
    public void SetHour(string hour)
    {
        SetDateTime(3, hour);
    }
    public void SetMinute(string minute)
    {
        SetDateTime(4, minute);
    }
}
