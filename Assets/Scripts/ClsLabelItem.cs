using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ClsLabelItem : MonoBehaviour
{
    public TMP_Text Text_ClsLabelItemOrderIndex;
    public TMP_Text Text_ClsLabelItemIndex;
    public TMP_Text Text_ClsLabelItemTime;
    public TMP_Text Text_ClsLabelItemArrythimia;

    public ClsLabel m_ClsLabel;

    public void SetByData(int orderIndex, int timeIndex, string arrythmia)
    {
        Text_ClsLabelItemOrderIndex.text = orderIndex.ToString();
        Text_ClsLabelItemIndex.text = timeIndex.ToString();
        Text_ClsLabelItemTime.text = DataManager.TransSecondToHMSp2(timeIndex / m_ClsLabel.dataManager.AnnotationFs);
        Text_ClsLabelItemArrythimia.text = arrythmia;
    }

    public void OnValueChanged_Toggle_ClsOutputItem(bool value)
    {
        if (value)
        {
            m_ClsLabel.selectedTimeIndex = int.Parse(Text_ClsLabelItemIndex.text);
            m_ClsLabel.LocateSelectedClsLabelOnChart();
        }
        else
        {
            if (m_ClsLabel.selectedTimeIndex == int.Parse(Text_ClsLabelItemIndex.text))
            {
                m_ClsLabel.selectedTimeIndex = -1;
            }
        }
    }

    private void Awake()
    {
        m_ClsLabel = GameObject.Find("ClsLabelWindowBackground").GetComponent<ClsLabel>();
        gameObject.GetComponent<Toggle>().group = GameObject.Find("Content_ClsLabel").GetComponent<ToggleGroup>();
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
