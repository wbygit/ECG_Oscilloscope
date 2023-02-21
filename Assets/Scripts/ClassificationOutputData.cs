using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassificationOutputData : IComparable<ClassificationOutputData>, IEquatable<ClassificationOutputData>
{
    public int timeIndex;
    public int rPeakTimeIndex;
    public string arrythmia;

    public ClassificationOutputData(int timeIndex, int rPeakTimeIndex, string arrythmia)
    {
        this.timeIndex = timeIndex;
        this.rPeakTimeIndex = rPeakTimeIndex;
        this.arrythmia = arrythmia;
    }

    public int CompareTo(ClassificationOutputData other)
    {
        return timeIndex.CompareTo(other.timeIndex);
    }

    public bool Equals(ClassificationOutputData other)
    {
        return timeIndex == other.timeIndex;
    }
}
