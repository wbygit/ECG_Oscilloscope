using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ECGTooltip : MyTooltipBase
{
    public const float k_TimeThreshold = 0.1f; // �������λ������Ҫ��ʶλ�õĴ�����ֵ�������ʱ����

    [SerializeField]
    private DataManager dataManager;
    private SegCommentData m_CurSegCommentData;

    // ��ÿһ֡��ʼ���ø���Add����ǰ����
    public void ClearData()
    {
        m_CurSegCommentData = null;
    }

    public void AddSegComment(SegCommentData data)
    {
        if (dataManager.PointerTimeAnnotationIndex == -1)
        {
            return;
        }
        if (Math.Abs(data.timeIndex - dataManager.PointerTimeAnnotationIndex) < (int)(k_TimeThreshold * dataManager.AnnotationFs))
        {
            if (m_CurSegCommentData == null || Math.Abs(data.timeIndex - dataManager.PointerTimeAnnotationIndex) < Math.Abs(m_CurSegCommentData.timeIndex - dataManager.PointerTimeAnnotationIndex))
            {
                m_CurSegCommentData = data;
            }
        }
    }

    // ���������ݺ����
    public void UpdateECGTooltip()
    {
        string newTip = "";
        if (m_CurSegCommentData != null)
        {
            newTip += m_CurSegCommentData.GetTip(dataManager);
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
