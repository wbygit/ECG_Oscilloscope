using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ECGTooltip : MyTooltipBase
{
    public const float k_TimeThreshold = 0.1f; // 鼠标所在位置与想要标识位置的触发阈值，即最大时间间隔

    [SerializeField]
    private DataManager dataManager;
    private List<SegCommentData> m_SegCommentDataList = new List<SegCommentData>();

    // 在每一帧开始调用各种Add函数前调用
    public void ClearData()
    {
        m_SegCommentDataList.Clear();
    }

    public void AddSegComment(SegCommentData data)
    {
        m_SegCommentDataList.Add(data);
    }

    public SegCommentData UpdateSegComment()
    {
        if (dataManager.PointerTimeAnnotationIndex == -1)
        {
            return null;
        }
        SegCommentData curSegCommentData = null;
        foreach (SegCommentData data in m_SegCommentDataList)
        {
            if (Math.Abs(data.timeIndex - dataManager.PointerTimeAnnotationIndex) < (int)(k_TimeThreshold * dataManager.AnnotationFs))
            {
                if (curSegCommentData == null || Math.Abs(data.timeIndex - dataManager.PointerTimeAnnotationIndex) < Math.Abs(curSegCommentData.timeIndex - dataManager.PointerTimeAnnotationIndex))
                {
                    curSegCommentData = data;
                }
            }
        }
        return curSegCommentData;
    }

    // 加载完数据后调用
    public void UpdateECGTooltip()
    {
        string newTip = "";
        SegCommentData curSegCommentData = UpdateSegComment();
        if (curSegCommentData != null)
        {
            newTip += curSegCommentData.GetTip(dataManager);
        }
        if (newTip != "")
        {
            if (!m_IsShowingTooltip || newTip != m_tipText)
            {
                m_tipText = newTip;
                ShowTooltip();
            }
        }
        else
        {
            if (m_IsShowingTooltip)
            {
                m_tipText = "";
                m_IsShowingTooltip = false;
                MoveOutsideScreen();
            }
        }
    }

    private void Awake()
    {
        m_TooltipGameObject = GameObject.Find("GUITooltipBackground");
        m_Text_Tooltip = m_TooltipGameObject.GetComponentInChildren<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
