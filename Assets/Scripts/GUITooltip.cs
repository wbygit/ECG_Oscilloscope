using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class GUITooltip : MyTooltipBase, IPointerEnterHandler, IPointerExitHandler
{
    //private static GameObject s_TooltipPrefab = Resources.Load<GameObject>("Toggle_SegCommentItem");
    //private static GameObject s_TooltipObject = GameObject.Instantiate(s_TooltipPrefab, null);

    public const float k_DefaultWaitTime = 0.5f; // 鼠标悬停多久后显示提示框
    private float m_WaitTime = 0f; // 已经悬停了多长时间
    private bool m_IsPointEnter; // 鼠标是否在物体范围内
    private Toggle m_Toggle_ShowGUITooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_IsPointEnter = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Init();
    }

    private void Awake()
    {
        m_TooltipGameObject = GameObject.Find("GUITooltipBackground");
        m_Text_Tooltip = m_TooltipGameObject.GetComponentInChildren<TMP_Text>();
        m_Toggle_ShowGUITooltip = GameObject.Find("Toggle_ShowGUITooltip").GetComponent<Toggle>();
        Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Toggle_ShowGUITooltip.isOn && m_IsPointEnter)
        {
            if (m_WaitTime <= k_DefaultWaitTime)
            {
                m_WaitTime += Time.deltaTime;
            }
            else if (!m_IsShowingTooltip)
            {
                ShowTooltip();
            }
        }
    }

    private void Init()
    {
        m_WaitTime = 0f;
        m_IsPointEnter = false;
        //m_TooltipGameObject.SetActive(false);
        MoveOutsideScreen();
        m_IsShowingTooltip = false;
    }
}
