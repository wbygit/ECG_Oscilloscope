using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class MyTooltipBase : MonoBehaviour
{
    protected const float k_DistAwayFromPointer = 10f; // 提示框边缘距离鼠标指针的距离

    protected GameObject m_TooltipGameObject;
    protected TMP_Text m_Text_Tooltip;
    [SerializeField]
    protected string m_tipText;
    protected bool m_IsShowingTooltip;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void ShowTooltip()
    {
        //if (!m_TooltipGameObject.activeSelf)
        //{
        //    m_TooltipGameObject.SetActive(true);
        //}
        MoveToPointer();
        m_IsShowingTooltip = true;
        m_Text_Tooltip.text = System.Text.RegularExpressions.Regex.Unescape(m_tipText);
        
    }

    protected virtual void MoveOutsideScreen()
    {
        float x = m_TooltipGameObject.transform.parent.GetComponent<RectTransform>().rect.width + 10f;
        float y = 0f;
        m_TooltipGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    protected virtual void MoveToPointer()
    {
        // 以屏幕左下角为原点，x向右，y向上的坐标系
        Vector2 mousePositionInScreen = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 normalizedMousePositionInScreen = new Vector2(mousePositionInScreen.x / Screen.width, mousePositionInScreen.y / Screen.height);
        Vector2 backgroundSize = m_TooltipGameObject.transform.parent.GetComponent<RectTransform>().rect.size;
        // 以background左下角为原点，x向右，y向上的坐标系
        Vector2 backgroundSizeInScreen = new Vector2(Screen.width, Screen.width * backgroundSize.y / backgroundSize.x);
        Vector2 normalizedMousePosition = new Vector2(normalizedMousePositionInScreen.x, (mousePositionInScreen.y - 0.5f * (Screen.height - backgroundSizeInScreen.y)) / backgroundSizeInScreen.y);
        // Tooltip的锚点在左上角
        Vector2 tooltipPosition = normalizedMousePosition * backgroundSize;
        RectTransform rectTransform = m_TooltipGameObject.GetComponent<RectTransform>();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        if (normalizedMousePosition.x < 0.5 && normalizedMousePosition.y < 0.5)
        {
            // 鼠标在左下，提示框在右上
            tooltipPosition.y += height ;
            tooltipPosition.x += k_DistAwayFromPointer;
            tooltipPosition.y += k_DistAwayFromPointer;
        }
        else if (normalizedMousePosition.x >= 0.5 && normalizedMousePosition.y < 0.5)
        {
            // 鼠标在右下，提示框在左上
            tooltipPosition.x -= width;
            tooltipPosition.y += height;
            tooltipPosition.x -= k_DistAwayFromPointer;
            tooltipPosition.y += k_DistAwayFromPointer;
        }
        else if (normalizedMousePosition.x >= 0.5 && normalizedMousePosition.y >= 0.5)
        {
            // 鼠标在右上，提示框在左下
            tooltipPosition.x -= width;
            tooltipPosition.x -= k_DistAwayFromPointer;
            tooltipPosition.y -= k_DistAwayFromPointer;
        }
        else
        {
            // 鼠标在左上，提示框在右下
            tooltipPosition.x += k_DistAwayFromPointer;
            tooltipPosition.y -= k_DistAwayFromPointer;
        }
        rectTransform.anchoredPosition = tooltipPosition;
    }
}
