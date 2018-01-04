﻿using UnityEngine;
using UnityEngine.UI;

public class DayOfWeek : IDayView
{
    public Text AF;
    
    protected override void Start()
    {
        base.Start();
        gManager.OnLanguageChange += SetLanguage;
    }

    public override void Refresh()
    {
        base.Refresh();
        AF.text = "";
    }
    
    public override void RequestView()
    {
        calendarController.RequestView(CalendarViewController.State.DAILY, assignedDate);
    }

    protected override void SetHeader()
    {
        int dayIndex = (byte)assignedDate.DayOfWeek - 1;
        if (dayIndex < 0) dayIndex = 6;
        header.text = gManager.language.GetDay(dayIndex) + " " + assignedDate.Day.ToString() + "/" + assignedDate.Month.ToString() + "/" + assignedDate.Year.ToString();
    }

    protected override void OnSetView()
    {
        base.OnSetView();
        RequestData();
        DisplayInfo();
        AF.text = info.GetOfficer();
    }

    protected override void DisplayInfo()
    {
        if (!isClosed)
        { 
            base.DisplayInfo();
        }
        if (info.Alarms.Count > 0)
        {
            AlarmIndicatorPanel.SetActive(true);
            flagAlrm = true;
        }
    }

    public override void SetLanguage()
    {
        SetHeader();
    }
    
    protected override void AssignInfo(GameObject o, Event n)
    {
        IItemListView<Event> o_view = o.GetComponent<IItemListView<Event>>();
        if (o_view != null)
            o_view.Allocate(n);
    } 
}
