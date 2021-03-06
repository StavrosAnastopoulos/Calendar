﻿using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;

public class DataReader
{

    XmlDocument doc;
    
    private string GetElementID(XmlElement e)
    {
        string[] temp = e.GetAttribute("id").Split('_');
        return temp[1];
    }

    private XmlElement GetElementById(string id)
    {
        return doc.GetElementById("_" + id);
    }

    private T ReadItem<T>(XmlElement element, T item) where T : Item
    {
        item.dataGroupID = element.GetAttribute("dataGroup");
        item.XMLToObject(element.InnerText);
        item.filler = false;
        item.id = GetElementID(element);
        
        return item;
    }

    private DAY GetDayInfo(XmlElement day) {
        DAY dayInfo = new DAY()
        {
            id = GetElementID(day)
        };
        {
            XmlNodeList entries = day.GetElementsByTagName(DataStrings.Event);
            foreach (XmlElement entry in entries)
            {
                Event newGuide = new Event();
                newGuide = ReadItem(entry, newGuide);
                newGuide.SetDate(dayInfo.id);
                dayInfo.AddEvent(newGuide);
            }
        }
        {
            XmlNodeList entries = day.GetElementsByTagName(DataStrings.Alarm);
            foreach (XmlElement entry in entries)
            {
                Alarm alarm = new Alarm();
                alarm = ReadItem(entry, alarm);
                alarm.SetDate(dayInfo.id);
                dayInfo.AddAlarm(alarm);
            }
        }
        dayInfo.SetOfficer(day.GetAttribute("officer"));
        dayInfo.SetTourGuides(day.GetAttribute("guides"));
		
        return dayInfo;
    }

    // Read By Day
    public DAY Read(string filename, string ID) {
        DAY dayInfo = new DAY();
        if (File.Exists(filename)) {
            doc = new XmlDocument();
            doc.Load(filename);
            XmlElement day = GetElementById(ID);
            if(day != null)
                dayInfo = GetDayInfo(day);
        }
        return dayInfo;
    }

    // Read By Month
    public Dictionary<string, DAY> Read(string filename)
    {
        Dictionary<string, DAY> monthInfo = new Dictionary<string, DAY>();
        if (File.Exists(filename))
        {
            doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList entries = doc.GetElementsByTagName(DataStrings.Day);
            foreach(XmlElement day in entries)
            {
                string id = GetElementID(day);
                monthInfo.Add(id, GetDayInfo(day));
            }
        }
        return monthInfo;
    }

    private string InitialiseDoc(string filePath)
    {
        string files = filePath.Substring(0, filePath.LastIndexOf('/'));
        if (!Directory.Exists(files))
            Directory.CreateDirectory(files);

        doc = new XmlDocument();

        if (!File.Exists(filePath))
        {
            doc.Save(filePath);
            XmlTextWriter writer = new XmlTextWriter(filePath, null);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteDocType("Entries", null, null, DataStrings.doctype);
            writer.WriteStartElement("Entries");
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }
        return filePath;
    }
    
    public void WriteAttribute(string filePath, string id, string atttribute, string value)
    {
        string filename = InitialiseDoc(filePath);

        doc = new XmlDocument();

        doc.Load(filename);
        XmlElement day = GetElementById(id);
        if (day == null)
        {
            day = doc.CreateElement(DataStrings.Day);
            day.SetAttribute("id", "_" + id);

        }
        day.SetAttribute(atttribute, value);
        XmlElement root = doc.DocumentElement;
        root.AppendChild(day);
        doc.Save(filename);
    }

    // write element
    public void Write(string filePath, Item item) {
        string filename = InitialiseDoc(filePath);

        doc = new XmlDocument();

        doc.Load(filename);
        XmlElement day = GetElementById(item.Date);
        if (day == null)
        {
            day = doc.CreateElement(DataStrings.Day);
            day.SetAttribute("id", "_" + item.Date);

            XmlElement root = doc.DocumentElement;
            root.AppendChild(day);
        }
        if (!item.filler)
        {
            XmlNodeList NList = day.GetElementsByTagName(item.Type.ToString());
            XmlElement NE = null;
            bool exists = false;
            if (NList != null)
            {
                foreach (XmlElement element in NList)
                {
                    if (GetElementID(element) == item.id)
                    {
                        NE = element;
                        NE.IsEmpty = true;
                        exists = true;
                        break;
                    }
                }
            }
            if (!exists)
            {
                NE = doc.CreateElement(item.Type.ToString());
                NE.SetAttribute("id", item.id);
            }
            NE.SetAttribute("dataGroup", item.dataGroupID);
            NE.InnerText = item.ObjectToXML();
            day.AppendChild(NE);
        }
        doc.Save(filename);
    }
    
    public void DeleteItem(string filePath, Item item)
    {
        if (File.Exists(filePath))
        {
            doc = new XmlDocument();
            doc.Load(filePath);
            XmlElement element = GetElementById(item.id);
            if (element != null)
            {
                XmlNode parent = element.ParentNode;
                parent.RemoveChild(element);
            }
            doc.Save(filePath);
        }
    }
    
    public DAY SearchItem(string searchTerm, string dataPath)
    {
        searchTerm = searchTerm.ToLower();
        DAY result = new DAY();
        
        string[] years = Directory.GetDirectories(dataPath);
        foreach(string year in years)
        {
            string[] months = Directory.GetDirectories(year);
            foreach(string month in months)
            {
                string[] files = Directory.GetFiles(month);
                foreach(string file in files)
                {
                    if(file.EndsWith(".xml"))
                        if (File.Exists(file))
                        {
                            doc = new XmlDocument();
                            doc.Load(file);
                            XmlNodeList days = doc.GetElementsByTagName(DataStrings.Day);
                            foreach(XmlElement day in days)
                            {
                                string[] temp = { "officer", "guides" };
                                foreach(string s in temp)
                                {
                                    string value = day.GetAttribute(s);
                                    if (value.ToLower().Contains(searchTerm))
                                    {
                                        Event n = new Event();
                                        n.SetDate(GetElementID(day));
                                        result.AddEvent(n);
                                    }
                                }
                                
                                XmlNodeList teams = day.GetElementsByTagName(DataStrings.Event);

                                foreach (XmlElement entry in teams)
                                {
                                    if (entry.InnerText.ToLower().Contains(searchTerm))
                                    {
                                        Event guide = new Event();
                                        guide = ReadItem(entry, guide);
                                        guide.filler = false;
                                        guide.SetDate(GetElementID(day));
                                        guide.id = GetElementID(entry);
                                        result.AddEvent(guide);
                                    }
                                }
                            }
                        }
                }
            }
        }

        return result;
    }
    
    public static void BackUp(string source, string destination, bool cleanDestination)
    {
        if (cleanDestination && Directory.Exists(destination))
            DeleteFolder(destination);
        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);
        foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, destination));
        foreach (string file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories).Where(name => !name.EndsWith(".meta")))
            File.Copy(file, file.Replace(source, destination), true);
    }

    public static void DeleteFolder(string path)
    {
        Directory.Delete(path, true);
    }
    
}
