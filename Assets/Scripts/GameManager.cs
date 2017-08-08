﻿using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

public class GameManager : MonoBehaviour {

    public GameObject headerObj;
    [HideInInspector]
    public string DATA_FOLDER, LEGACY_FOLDER;
    [HideInInspector]
    public string DESKTOP;

    public event Action PrintMode;
    public event Action OnLanguageChange;

    public Language language;

    void Awake()
    {
        DATA_FOLDER = Application.dataPath + @"/Calendar Data/Data";
        LEGACY_FOLDER = Application.dataPath + @"/Calendar Data/Legacy";
        DESKTOP = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        int value = PlayerPrefs.GetInt("TimeThreshold");
        if (value == 0)
            PlayerPrefs.SetInt("TimeThreshold", 45);
    }
    
    void Start()
    {
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        headerObj.SetActive(true);
        SetLanguage(PlayerPrefs.GetInt("Language"));
        ReportGeneration.CreatePDF(DESKTOP, new string[] { });
    }

    public void SetLanguage(int value)
    {
        if (value == 0)
            language = new English();
        else if (value == 1)
            language = new Greek();
        PlayerPrefs.SetInt("Language", value);

        if (OnLanguageChange != null)
            OnLanguageChange();
        headerObj.GetComponentInChildren<UnityEngine.UI.Text>().text = language.Title;
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public void SetFullScreen(bool b)
    {
        Screen.fullScreen = b;
        headerObj.SetActive(b);
    }

    public void Print()
    {
        StartCoroutine(PrintProcess());
    }

    IEnumerator PrintProcess()
    {
        bool headerFlag = headerObj.activeSelf;
        if (PrintMode != null)
            PrintMode();
        if (headerFlag)
            headerObj.SetActive(false);
        yield return new WaitForSeconds(1);
        Application.CaptureScreenshot(DESKTOP + "/Calendar.png");
        yield return 0;
        if (PrintMode != null)
            PrintMode();
        if (headerFlag)
            headerObj.SetActive(true);
        yield return 0;
    }

    public void Command(string[] s)
    {
        switch (s[1])
        {
            case "FULLSCREEN":
                SetFullScreen(s[2] == "ON" ? true : false);
                break;
            case "CLEAR_LEGACY":
                break;
            case "SEARCH_LEGACY":
                break;
            case "TIME_THRESHOLD":
                int spacing = 0;
                if(int.TryParse(s[2], out spacing))
                    PlayerPrefs.SetInt("TimeThreshold", spacing);
                break;
            case "EXIT":
                ExitApplication();
                break;
            case "RESET_IDS":
                PlayerPrefs.SetInt("Guide", 0);
                PlayerPrefs.SetInt("Event", 0);
                break;
            case "BACKUP":
                ThreadReader.BackUp(DATA_FOLDER, DESKTOP);
                break;
            case "IMPORT":
                ThreadReader.BackUp(s[2], DATA_FOLDER);
                break;
            default:
                break;
        }
    }
}
