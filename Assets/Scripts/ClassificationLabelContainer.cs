using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassificationLabelContainer
{
    private SortedList<int, string> m_ClsLabelList;
    private SortedList<int, string> m_CurClsLabelList;
    private ArrythmiaDict m_ArrythmiaDict;
    private string m_CurArrythmia;
    
    public ClassificationLabelContainer()
    {
        m_ArrythmiaDict = new ArrythmiaDict();
        m_CurClsLabelList = new SortedList<int, string>();
        m_ClsLabelList = new SortedList<int, string>();
        m_CurArrythmia = "";
    }

    public string curArrythmia
    {
        get => m_CurArrythmia;
    }

    public SortedList<int, string> curClsLabelList
    {
        get => m_CurClsLabelList;
    }

    public ArrythmiaDict arrythmiaDict
    {
        get => m_ArrythmiaDict;
    }

    public void Clear()
    {
        m_ClsLabelList.Clear();
        m_CurClsLabelList.Clear();
        m_ArrythmiaDict.Clear();
    }

    public void AddClsLabel(int timeIndex, string arrythmia)
    {
        m_ClsLabelList.Add(timeIndex, arrythmia);
        m_ArrythmiaDict.AddArrythmia(arrythmia);
    }

    public void InitCurClsLabelList(string arrythmia = "")
    {
        if (curClsLabelList.Count == 0 || arrythmia != m_CurArrythmia)
        {
            m_CurClsLabelList.Clear();
            m_CurArrythmia = arrythmia;
            foreach (KeyValuePair<int, string> pair in m_ClsLabelList)
            {
                if (string.IsNullOrEmpty(arrythmia) || arrythmia == pair.Value)
                {
                    m_CurClsLabelList.Add(pair.Key, pair.Value);
                }
            }
        }
    }

    // C++风格，返回CurLabelList大于等于的第一个位置（>=0），不考虑容器中是否包含该值
    public int GetLowerBoundIndexOfCurClsLabelList(int timeIndex)
    {
        return Tool.GetLowerBoundIndex(m_CurClsLabelList.Keys, timeIndex);
    }
}
