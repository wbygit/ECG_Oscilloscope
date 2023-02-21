using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClsCommentData : IComparable<ClsCommentData>, IEquatable<ClsCommentData>
{
    public int timeIndex;
    public string newArrythmia;
    public string note;

    public ClsCommentData(int timeIndex)
    {
        this.timeIndex = timeIndex;
        newArrythmia = "";
        note = "";
    }

    public int CompareTo(ClsCommentData other)
    {
        return timeIndex.CompareTo(other.timeIndex);
    }

    public bool Equals(ClsCommentData other)
    {
        return timeIndex == other.timeIndex;
    }
}
