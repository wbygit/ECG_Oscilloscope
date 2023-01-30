using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegCommentData : IComparable<SegCommentData>, IEquatable<SegCommentData>
{

    public int timeIndex;
    public string type; // ����
    public string note; // ��ע

    public SegCommentData()
    {
        timeIndex = -1;
        type = SegCommentType.Other;
        note = "";
    }

    public SegCommentData(int timeIndex, string type, string note = "")
    {
        this.timeIndex = timeIndex;
        this.type = type;
        this.note = note;
    }

    public int CompareTo(SegCommentData other)
    {
        return timeIndex.CompareTo(other.timeIndex);
    }

    public bool Equals(SegCommentData other)
    {
        return timeIndex == other.timeIndex;
    }

    public string GetTip(DataManager dataManager)
    {
        int index = dataManager.GetLowerBoundIndexOfSegCommentList(timeIndex);
        string tip = string.Format("[�ָ��޸ı��]\n(��ɫ����Բ)\n��ţ�{0}\n���ͣ�{1}\n����λ�ã�{2}\nʱ�䣺{3}\n��ע��{4}\n", index < 0 ? "" : index + 1, type, timeIndex, DataManager.TransSecondToHMSp2(timeIndex / dataManager.AnnotationFs), note);
        return tip;
    }
}

public struct SegCommentType
{
    public const string Add = "ADD";
    public const string Remove = "REMOVE";
    public const string Other = "OTHER";
    public const string New = "New"; // ���ڴ�����

    public static int GetIndex(string str)
    {
        int index = -1;
        switch (str)
        {
            case Add: { index = 0; break; }
            case Remove: { index = 1; break; }
            case Other: { index = 2; break; }
            case New: { index = 3; break; }
        }
        return index;
    }

    public static string GetType(int index)
    {
        string type = "";
        switch (index)
        {
            case 0: { type = Add; break; }
            case 1: { type = Remove; break; }
            case 2: { type = Other; break; }
            case 3: { type = New; break; }
        }
        return type;
    }

    // ��ʾ���ĵ�ͼ�е���ʾ
    public static string GetTip(string type)
    {
        //string tip = "";
        //switch (type)
        //{
        //    case Add: { tip = "+"; break; }
        //    case Remove: { tip = "-"; break; }
        //    case Other: { tip = "O"; break; }
        //}
        //return tip;
        return type;
    }
}