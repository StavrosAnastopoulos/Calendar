﻿public class DayGuideView : IItemListView
{
    
    public override void Allocate(NewEntry n)
    {
        base.Allocate(n);
        SetTime(n.attributes[0] + " - " + n.attributes[1]);
        if (!n.filler)
            SetDetails(n.attributes[2] + ", #" + n.attributes[3] + ", " + n.attributes[7]);
    }
    
}
