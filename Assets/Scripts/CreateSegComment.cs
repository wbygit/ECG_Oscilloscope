using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using XCharts.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class CreateSegComment : MonoBehaviour
{
    public DataManager dataManager;
    public PlotMainChart mainChart;
    public SegComments segComments;
    public TMP_Dropdown Dropdown_CreateSegCommentType;
    public TMP_InputField InputField_CreateSegCommentNote;
    public TMP_Text Text_CreateSegCommentIndex;
    public TMP_Text Text_CreateSegCommentTime;
    public Toggle Toggle_CreateSegCommentSnapToRPeak;

    public void OnClick_Button_CreateSegComment()
    {
        gameObject.SetActive(true);
    }

    public void OnClick_Button_CreateSegCommentCancel()
    {
        gameObject.SetActive(false);
    }

    public void OnClick_Button_CreateSegCommentConfirm()
    {
        if (dataManager.CreateSegCommentTimeIndex == -1)
        {
            MessageBox.DisplayMessageBox("新增分割修改项错误", "未指定位置。", true, null);
            return;
        }

        SegCommentData data = new SegCommentData(dataManager.CreateSegCommentTimeIndex, SegCommentType.GetType(Dropdown_CreateSegCommentType.value), InputField_CreateSegCommentNote.text);
        int insertIndex = dataManager.GetLowerBoundIndexOfSegCommentList(dataManager.CreateSegCommentTimeIndex);
        if (insertIndex >= 0)
        {
            MessageBox.DisplayMessageBox("新增分割修改项错误", "相同位置已有修改项。", true, null);
            return;
        }
        insertIndex = ~insertIndex;
        segComments.selectedTimeIndex = dataManager.CreateSegCommentTimeIndex;
        segComments.startTimeIndex = dataManager.CreateSegCommentTimeIndex;
        dataManager.SegCommentList.Insert(insertIndex, data);
        segComments.UpdateSegCommentListLength();
        segComments.UpdateSegCommentsWindow();
        segComments.SetSegCommentSavedFlag(false);
        gameObject.SetActive(false);
    }

    public void OnClick_Button_CreateSegCommentMoveForward()
    {
        if (dataManager.CreateSegCommentTimeIndex != -1 && !Toggle_CreateSegCommentSnapToRPeak.isOn)
        {
            dataManager.CreateSegCommentTimeIndex += (int)Math.Round(DataManager.k_CreateSegCommentMovePeroid_s * dataManager.AnnotationFs);
            dataManager.CreateSegCommentTimeIndex = Math.Min(dataManager.CreateSegCommentTimeIndex, (int)((dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs));
            UpdateCreateSegCommentTimeAndIndex();
        }
    }

    public void OnClick_Button_CreateSegCommentMoveBackward()
    {
        if (dataManager.CreateSegCommentTimeIndex != -1 && !Toggle_CreateSegCommentSnapToRPeak.isOn)
        {
            dataManager.CreateSegCommentTimeIndex -= (int)Math.Round(DataManager.k_CreateSegCommentMovePeroid_s * dataManager.AnnotationFs);
            dataManager.CreateSegCommentTimeIndex = Math.Max(dataManager.CreateSegCommentTimeIndex, (int)Math.Ceiling(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
            UpdateCreateSegCommentTimeAndIndex();
        }
    }

    public void OnValueChanged_Toggle_CreateSegCommentSnapToRPeak()
    {
        if (Toggle_CreateSegCommentSnapToRPeak.isOn)
        {
            SnapCreateSegCommentTimeIndexToRPeak();
        }
    }

    public void OnValueChanged_Dropdown_CreateSegCommentType()
    {
        string createType = SegCommentType.GetType(Dropdown_CreateSegCommentType.value);
        if (createType == SegCommentType.Add)
        {
            Toggle_CreateSegCommentSnapToRPeak.isOn = false;
            Toggle_CreateSegCommentSnapToRPeak.interactable = false;
        }
        else if(createType == SegCommentType.Remove)
        {
            Toggle_CreateSegCommentSnapToRPeak.isOn = true;
            Toggle_CreateSegCommentSnapToRPeak.interactable = false;
        }
        else if (createType == SegCommentType.Other)
        {
            Toggle_CreateSegCommentSnapToRPeak.interactable = false;
            Toggle_CreateSegCommentSnapToRPeak.interactable = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        Init();
        //gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SetPointerActionForCreateSegComment();
    }

    private void OnDisable()
    {
        Init();
        ClearPointerActionForCreateSegComment();
    }
    
    private void Init()
    {
        Dropdown_CreateSegCommentType.value = SegCommentType.GetIndex(SegCommentType.Add);
        dataManager.CreateSegCommentTimeIndex = -1;
        InputField_CreateSegCommentNote.text = "";
        dataManager.PointerTimeAnnotationIndex = -1;
        Text_CreateSegCommentIndex.text = "采样位置：";
        Text_CreateSegCommentTime.text = "时间：";
    }
    private void SetPointerActionForCreateSegComment()
    {
        mainChart.chart.onAxisPointerValueChanged = delegate (Axis axis, double value)
        {
            if (axis is XAxis)
            {
                dataManager.PointerTimeAnnotationIndex = (int)Math.Round((value + dataManager.MainChartStTime_s) * dataManager.AnnotationFs);
            }
        };

        mainChart.chart.onPointerExit = delegate (PointerEventData eventData, BaseGraph baseGraph)
        {
            dataManager.PointerTimeAnnotationIndex = -1;
        };

        mainChart.chart.onPointerClick = delegate (PointerEventData eventData, BaseGraph baseGraph)
        {
            if (dataManager.PointerTimeAnnotationIndex != -1)
            {
                dataManager.CreateSegCommentTimeIndex = dataManager.PointerTimeAnnotationIndex;
                if (Toggle_CreateSegCommentSnapToRPeak.isOn)
                {
                    SnapCreateSegCommentTimeIndexToRPeak();
                }
                UpdateCreateSegCommentTimeAndIndex();
            }
        };
    }

    private void ClearPointerActionForCreateSegComment()
    {
        if (mainChart.chart != null)
        {
            mainChart.chart.onAxisPointerValueChanged = null;
            mainChart.chart.onPointerExit = null;
            mainChart.chart.onPointerClick = null;
        }
    }

    private void SnapCreateSegCommentTimeIndexToRPeak()
    {
        int curCreateSegCommentTimeIndex = dataManager.CreateSegCommentTimeIndex;
        int rpeakIndex = dataManager.RPeakList.BinarySearch(curCreateSegCommentTimeIndex);
        if (rpeakIndex >= 0)
        {
            return;
        }
        rpeakIndex = ~rpeakIndex;
        int minDist = int.MaxValue;
        if (rpeakIndex < dataManager.RPeakList.Count)
        {
            int dist = Math.Abs(curCreateSegCommentTimeIndex - dataManager.RPeakList[rpeakIndex]);
            if (dist < minDist)
            {
                minDist = dist;
                dataManager.CreateSegCommentTimeIndex = dataManager.RPeakList[rpeakIndex];
            }
        }
        if (rpeakIndex - 1 >= 0)
        {
            int dist = Math.Abs(curCreateSegCommentTimeIndex - dataManager.RPeakList[rpeakIndex - 1]);
            if (dist < minDist)
            {
                minDist = dist;
                dataManager.CreateSegCommentTimeIndex = dataManager.RPeakList[rpeakIndex - 1];
            }
        }
        if (minDist == int.MaxValue || dataManager.CreateSegCommentTimeIndex < dataManager.MainChartStTime_s * dataManager.AnnotationFs || dataManager.CreateSegCommentTimeIndex >= (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs)
        {
            dataManager.CreateSegCommentTimeIndex = -1;
            MessageBox.DisplayMessageBox("选择标记修改位置失败", "开启吸附R波波峰选项但心电图当前显示范围内无R波波峰");
        }
        UpdateCreateSegCommentTimeAndIndex();
    }

    private void UpdateCreateSegCommentTimeAndIndex()
    {
        if (dataManager.CreateSegCommentTimeIndex != -1)
        {
            Text_CreateSegCommentIndex.text = String.Format("采样位置：{0}", dataManager.CreateSegCommentTimeIndex);
            Text_CreateSegCommentTime.text = "时间：" + DataManager.TransSecondToHMSp2((double)dataManager.CreateSegCommentTimeIndex / dataManager.AnnotationFs);
        }
        else
        {
            Text_CreateSegCommentIndex.text = "采样位置：";
            Text_CreateSegCommentTime.text = "时间：";
        }
    }
}
