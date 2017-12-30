﻿using UnityEngine;
using UnityEngine.UI;

public class SettingsField : MonoBehaviour {

    public enum ValueType { INT, FLOAT, STRING };
    public string id;
    public ValueType valueType;
    InputField field;    
    GameManager gmanager;

    public void OnStart()
    {
        if (!gmanager)
            gmanager = FindObjectOfType<GameManager>();
        if (!field)
            field = GetComponent<InputField>();
        
        field.text = SettingsManager.Read(id); 
    }

    public void OnEndEdit()
    {
        gmanager.Command(new string[] { "", id, field.text });
    }
    
}
