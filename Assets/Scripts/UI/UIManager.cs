using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 *  Game UI manager
 * 
*/
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameUI = null;
    [SerializeField] private GameObject pauseMenuUI = null;

    private DateLocationManager dateLocationManager;
    private GameUIManager gameUIManager;


    void Awake()
    {
        dateLocationManager = GameObject.Find("Sun Strip").GetComponent<DateLocationManager>();
        gameUIManager = gameUI.GetComponent<GameUIManager>();
    }

    public void DisplayPauseMenu()
    {
        // Stops the time
        Time.timeScale = 0f;

        // Hide panels
        dateLocationManager.HidePanels();
        gameUIManager.ClearModes();
        gameUIManager.HidePanels();

        gameUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void DisplayGame()
    {
        Time.timeScale = 1f;
        gameUI.SetActive(true);
        pauseMenuUI.SetActive(false);
    }
}
