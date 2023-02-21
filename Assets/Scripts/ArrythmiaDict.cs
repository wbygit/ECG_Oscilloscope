using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrythmiaDict
{
    public const string k_EmptyArrythmia = "";

    private List<string> m_ArrythmiaList = new List<string>();
    private Dictionary<string, int> m_ArrythmiaDict = new Dictionary<string, int>();
    private List<string> m_ArrythmiaOptions = new List<string>();

    public int count
    {
        get => m_ArrythmiaList.Count;
    }

    public List<string> arrythmiaOptions
    {
        get => m_ArrythmiaOptions;
    }


    public void AddArrythmia(string arrythmia)
    {
        if (!m_ArrythmiaDict.ContainsKey(arrythmia))
        {
            m_ArrythmiaList.Add(arrythmia);
            m_ArrythmiaDict.Add(arrythmia, m_ArrythmiaDict.Count);
            m_ArrythmiaOptions.Insert(m_ArrythmiaOptions.Count - 1, arrythmia);
        }
    }

    public int GetIndex(string arrythmia)
    {
        if (m_ArrythmiaDict.ContainsKey(arrythmia))
        {
            return m_ArrythmiaDict[arrythmia];
        }
        if (arrythmia == k_EmptyArrythmia)
        {
            return count;
        }
        return -1;
    }

    public bool ContainsArrythmia(string arrythmia)
    {
        return m_ArrythmiaDict.ContainsKey(arrythmia);
    }

    public string GetArrythmia(int index)
    {
        if (index < count && index >= 0)
        {
            return m_ArrythmiaList[index];
        }
        if (index == count)
        {
            return k_EmptyArrythmia;
        }
        return null;
    }

    public void Clear()
    {
        m_ArrythmiaDict.Clear();
        m_ArrythmiaList.Clear();
        m_ArrythmiaOptions.Clear();
        m_ArrythmiaOptions.Add(k_EmptyArrythmia);
    }
}
