using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ClsOutputItem : MonoBehaviour
{
    public TMP_Text Text_ClsOutputItemOrderIndex;
    public TMP_Text Text_ClsOutputItemIndex;
    public TMP_Text Text_ClsOutputItemTime;
    public TMP_Text Text_ClsOutputItemArrythimia;
    public Toggle Toggle_ClsOutputNeedReviewed;
    public TMP_Dropdown Dropdown_ClsOutputNewArrythmia;
    public TMP_InputField InputField_ClsOutputItemNote;

    public ClsOutput m_ClsOutput;

    public void SetByData(int orderIndex, ClassificationOutputData outputData)
    {
        Text_ClsOutputItemOrderIndex.text = orderIndex.ToString();
        Text_ClsOutputItemIndex.text = outputData.timeIndex.ToString();
        Text_ClsOutputItemTime.text = DataManager.TransSecondToHMSp2(outputData.timeIndex / m_ClsOutput.dataManager.AnnotationFs);
        Text_ClsOutputItemArrythimia.text = outputData.arrythmia;
        Toggle_ClsOutputNeedReviewed.SetIsOnWithoutNotify(m_ClsOutput.dataManager.ClsOutputContainer.clsCommentDict.ContainsKey(outputData.timeIndex));
        if (Toggle_ClsOutputNeedReviewed.isOn)
        {
            Dropdown_ClsOutputNewArrythmia.gameObject.SetActive(true);
            InputField_ClsOutputItemNote.gameObject.SetActive(true);
            ClsCommentData commentData = m_ClsOutput.dataManager.ClsOutputContainer.clsCommentDict[outputData.timeIndex];
            Tool.InitArrythmiaDropdown(Dropdown_ClsOutputNewArrythmia, m_ClsOutput.dataManager.ArrythmiaDict, commentData.newArrythmia);
            InputField_ClsOutputItemNote.text = commentData.note;
        }
        else
        {
            Dropdown_ClsOutputNewArrythmia.gameObject.SetActive(false);
            InputField_ClsOutputItemNote.gameObject.SetActive(false);
        }
    }

    public void OnValueChanged_Toggle_ClsOutputNeedReviewed()
    {
        int timeIndex = int.Parse(Text_ClsOutputItemIndex.text);
        if (Toggle_ClsOutputNeedReviewed.isOn)
        {
            Dropdown_ClsOutputNewArrythmia.gameObject.SetActive(true);
            InputField_ClsOutputItemNote.gameObject.SetActive(true);
            m_ClsOutput.dataManager.ClsOutputContainer.AddClsComment(timeIndex);
            Tool.InitArrythmiaDropdown(Dropdown_ClsOutputNewArrythmia, m_ClsOutput.dataManager.ArrythmiaDict, ArrythmiaDict.k_EmptyArrythmia);
            InputField_ClsOutputItemNote.text = "";
        }
        else
        {
            Dropdown_ClsOutputNewArrythmia.gameObject.SetActive(false);
            InputField_ClsOutputItemNote.gameObject.SetActive(false);
            m_ClsOutput.dataManager.ClsOutputContainer.RemoveClsComment(timeIndex);
        }
        m_ClsOutput.dataManager.mainChart.NeedUpdate();
        m_ClsOutput.UpdateWindow();
        m_ClsOutput.UpdateLength();
        m_ClsOutput.SetClsCommentSavedFlag(false);
    }

    public void OnValueChanged_Toggle_ClsOutputItem(bool value)
    {
        if (value)
        {
            m_ClsOutput.selectedTimeIndex = int.Parse(Text_ClsOutputItemIndex.text);
            m_ClsOutput.LocateSelectedClsOutputOnChart();
        }
        else
        {
            if (m_ClsOutput.selectedTimeIndex == int.Parse(Text_ClsOutputItemIndex.text))
            {
                m_ClsOutput.selectedTimeIndex = -1;
            }
        }
    }

    public void OnValueChanged_Dropdown_ClsOutputNewArrythmia()
    {
        int timeIndex = int.Parse(Text_ClsOutputItemIndex.text);
        bool changed = m_ClsOutput.dataManager.ClsOutputContainer.SetClsCommentNewArrythmia(timeIndex, m_ClsOutput.dataManager.ArrythmiaDict.GetArrythmia(Dropdown_ClsOutputNewArrythmia.value));
        if (changed)
        {
            m_ClsOutput.SetClsCommentSavedFlag(false);
            m_ClsOutput.dataManager.mainChart.NeedUpdate();
        }
    }

    public void OnEndEdit_InputField_ClsOutputItemNote()
    {
        int timeIndex = int.Parse(Text_ClsOutputItemIndex.text);
        bool changed = m_ClsOutput.dataManager.ClsOutputContainer.SetClsCommentNote(timeIndex, InputField_ClsOutputItemNote.text);
        if (changed)
        {
            m_ClsOutput.SetClsCommentSavedFlag(false);
        }
    }

    private void Awake()
    {
        m_ClsOutput = GameObject.Find("ClsOutputWindowBackground").GetComponent<ClsOutput>();
        gameObject.GetComponent<Toggle>().group = GameObject.Find("Content_ClsOutput").GetComponent<ToggleGroup>();
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
