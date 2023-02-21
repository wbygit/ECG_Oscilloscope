using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassificationOutputContainer
{
    private SortedList<int, ClassificationOutputData> m_ClsOutputList;
    private SortedList<int, ClassificationOutputData> m_CurClsOutputList;
    private ArrythmiaDict m_ArrythmiaDict;
    private SortedDictionary<int, ClsCommentData> m_ClsCommentDict;
    private string m_CurArrythmia;
    private bool m_CurOnlyMarked;

    public ClassificationOutputContainer(ArrythmiaDict arrythmiaDict)
    {
        m_ArrythmiaDict = arrythmiaDict;
        m_ClsCommentDict = new SortedDictionary<int, ClsCommentData>();
        m_ClsOutputList = new SortedList<int, ClassificationOutputData>();
        m_CurClsOutputList = new SortedList<int, ClassificationOutputData>();
        m_CurArrythmia = "";
        m_CurOnlyMarked = false;
    }

    public SortedList<int, ClassificationOutputData> curClsOutputList
    {
        get => m_CurClsOutputList;
    }

    public SortedDictionary<int, ClsCommentData> clsCommentDict
    {
        get => m_ClsCommentDict;
        set
        {
            foreach (KeyValuePair<int, ClsCommentData> pair in value)
            {
                if (!m_ClsOutputList.ContainsKey(pair.Key))
                {
                    throw new Exception(string.Format("分类输出复核项采样位置与分类输出不对应{0}", pair.Key));
                }
                if (m_ArrythmiaDict.GetIndex(pair.Value.newArrythmia) == -1)
                {
                    throw new Exception(string.Format("分类输出复核文件中存在于心律类别文件不匹配的心律类型：{0}", pair.Value.newArrythmia));
                }
            }
            m_ClsCommentDict = value;
        }
    }

    public string curArrythmia
    {
        get => m_CurArrythmia;
    }

    public bool curOnlyMarked
    {
        get => m_CurOnlyMarked;
    }
    public void Clear()
    {
        m_ClsOutputList.Clear();
        m_CurClsOutputList.Clear();
        m_ClsCommentDict.Clear();
        m_CurArrythmia = "";
        m_CurOnlyMarked = false;
    }

    public void ClearCurOutputList()
    {
        m_CurClsOutputList.Clear();
        m_CurArrythmia = "";
        m_CurOnlyMarked = false;
    }

    public void ClearClsCommentDict()
    {
        m_ClsCommentDict.Clear();
    }

    public void AddClsOutput(ClassificationOutputData data)
    {
        if (!m_ArrythmiaDict.ContainsArrythmia(data.arrythmia))
        {
            throw new Exception(string.Format("模型分类输出文件中存在于心律类别文件不匹配的心律类型：{0}", data.arrythmia));
        }
        m_ClsOutputList.Add(data.timeIndex, data);
    }

    public void AddClsComment(int timeIndex)
    {
        if (!m_ClsOutputList.ContainsKey(timeIndex))
        {
            throw new Exception(string.Format("分类输出复核项采样位置与分类输出不对应{0}", timeIndex));
        }
        m_ClsCommentDict.Add(timeIndex, new ClsCommentData(timeIndex));
        if (curOnlyMarked && (curArrythmia == ArrythmiaDict.k_EmptyArrythmia || curArrythmia == m_ClsOutputList[timeIndex].arrythmia))
        {
            curClsOutputList.Add(timeIndex, m_ClsOutputList[timeIndex]);
        }
    }

    public bool RemoveClsComment(int timeIndex)
    {
        bool removed = m_ClsCommentDict.Remove(timeIndex);
        if (removed && curOnlyMarked && (curArrythmia == ArrythmiaDict.k_EmptyArrythmia || curArrythmia == m_ClsOutputList[timeIndex].arrythmia))
        {
            curClsOutputList.Remove(timeIndex);
        }
        return removed;
    }

    public bool SetClsCommentNewArrythmia(int timeIndex, string newArrythmia)
    {
        if (m_ClsCommentDict.ContainsKey(timeIndex))
        {
            if (m_ClsCommentDict[timeIndex].newArrythmia != newArrythmia)
            {
                m_ClsCommentDict[timeIndex].newArrythmia = newArrythmia;
                return true;
            }
        }
        return false;
    }

    public bool SetClsCommentNote(int timeIndex, string note)
    {
        if (m_ClsCommentDict.ContainsKey(timeIndex))
        {
            if (m_ClsCommentDict[timeIndex].note != note)
            {
                m_ClsCommentDict[timeIndex].note = note;
                return true;
            }
        }
        return false;
    }
    

    public void InitCurClsOutputList(string arrythmia = ArrythmiaDict.k_EmptyArrythmia, bool onlyMarked = false)
    {
        if (curClsOutputList.Count == 0 || arrythmia != m_CurArrythmia || onlyMarked != m_CurOnlyMarked)
        {
            m_CurClsOutputList.Clear();
            m_CurArrythmia = arrythmia;
            m_CurOnlyMarked = onlyMarked;
            foreach (ClassificationOutputData data in m_ClsOutputList.Values)
            {
                if (string.IsNullOrEmpty(arrythmia) || arrythmia == data.arrythmia)
                {
                    if (!onlyMarked || m_ClsCommentDict.ContainsKey(data.timeIndex))
                    {
                        m_CurClsOutputList.Add(data.timeIndex, data);
                    }
                }
            }
        }
    }

    // C++风格，返回CurOutputList大于等于的第一个位置（>=0），不考虑容器中是否包含该值
    public int GetLowerBoundIndexOfCurClsOutputList(int timeIndex)
    {
        return Tool.GetLowerBoundIndex(m_CurClsOutputList.Keys, timeIndex);
    }
}
