﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayViewManager : IDayView {

    public Text[] headers;
    public GameObject weeklyButton;

    public string[] GetEmptySlots() {
        List<string>  EmptySlots = new List<string>();
        for (int i = 0; i < info.guides.Count(); i++)
        {
            NewEntry n;
            info.guides.TryGet(i, out n);
            if (n.filler)
            {
                EmptySlots.Add(n.attributes[0] + " - " + n.attributes[1]);
            }
        }
        return EmptySlots.ToArray();
    }

    protected override void Start()
    {
        base.Start();
        weeklyButton.SetActive(false);
    }

    protected override void SetHeader() {
        int dayIndex = (byte)assignedDate.DayOfWeek - 1;
        if (dayIndex < 0) dayIndex = 6;
        header.text = gManager.language.GetDay(dayIndex) + " " + assignedDate.Day.ToString() + "/" + assignedDate.Month.ToString() + "/" + assignedDate.Year.ToString();
    }

    public override void Refresh()
    {
        base.Refresh();
        weeklyButton.GetComponentInChildren<Text>().text = gManager.language.WeeklyButton;
        weeklyButton.SetActive(false);

        headers[1].GetComponentInParent<InputField>().text = "";
    }

    public override void SetLanguage()
    {
        headers[0].text = gManager.language.OfficerOnDuty;
        headers[2].text = gManager.language.Time;
        headers[3].text = gManager.language.Details;
        SetHeader();
    }

    protected override void OnSetView()
    {
        base.OnSetView();
        RequestData();
        if (isMonday) {
            weeklyButton.SetActive(true);
        }
        else
        {
            DisplayInfo();
        }
        headers[1].GetComponentInParent<InputField>().text = info.officer;
    }
    
    protected override void AssignInfo(GameObject o, NewEntry n)
    {
        DayGuideView o_view = o.GetComponent<DayGuideView>();
        if (o_view != null)
            o_view.Allocate(n);
    }

    protected override void DisplayInfo()
    {
        if (info == null || info.guides.Count() == 0)
        {
            for (int i = 0; i < setTime.Length - 1; i++)
            {
                AddFiller(setTime[i], setTime[i + 1]);
            }
        }
        else {
            if (info.guides.Count() < setTime.Length - 1) {
                FillEmptySlots();
            }
        }
        base.DisplayInfo();
        if (info.events.Count > 0)
        {
            AlarmIndicatorPanel.SetActive(true);
            flagAlrm = true;
        }
    }

    public void SetOfficerOnDuty()
    {
        dataManager.RequestWriteOfficer(_tag, headers[1].text);
    }

    public void OnClickWeeklyButton()
    {
        calendarController.RequestView(CalendarViewController.State.WEEKLY, assignedDate);
    }
}
