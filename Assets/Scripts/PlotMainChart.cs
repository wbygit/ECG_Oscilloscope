using System;
using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

[DisallowMultipleComponent]
public class PlotMainChart : MonoBehaviour
{
    LineChart chart;
    Line serieECG;
    const string k_serieECGName = "ECG";

    Scatter serieRPeak;
    const string k_serieRPeakName = "RPeak";

    // 只是为了显示图例而已
    Scatter seriePWave;
    const string k_seriePWaveName = "PWave";
    Scatter serieRWave;
    const string k_serieRWaveName = "QRSWave";
    Scatter serieTWave;
    const string k_serieTWaveName = "TWave";

    const int k_Width = 1920;
    const int k_Height = 300;
    const string k_Title = "动态心电图";
    const float k_GridLeft = 0.05f;  // 左边距
    const float k_GridRight = 0.04f; // 右边距

    const double k_MaxVal = 5;
    const double k_MinVal = -5;

    public DataManager dataManager;

    void Awake()
    {
        dataManager = GameObject.Find("AppConfig").GetComponent<DataManager>();

        chart = gameObject.AddComponent<LineChart>();
        chart.Init(false);
        chart.SetSize(k_Width, k_Height);
        
        Title title = chart.AddChartComponent<Title>();
        title.text = k_Title;
        title.show = true;

        GridCoord girdCoord = chart.AddChartComponent<GridCoord>();
        girdCoord.left = k_GridLeft;
        girdCoord.right = k_GridRight;

        Legend legend = chart.GetOrAddChartComponent<Legend>();
        legend.show = true;

        XAxis xAxis = chart.AddChartComponent<XAxis>();
        YAxis yAxis = chart.AddChartComponent<YAxis>();
        xAxis.show = true;
        yAxis.show = true;
        //xAxis.type = Axis.AxisType.Time;
        xAxis.type = Axis.AxisType.Value;
        yAxis.type = Axis.AxisType.Value;
        yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        yAxis.min = 0;
        yAxis.max = 2;
        xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        xAxis.min = 0;
        xAxis.max = DataManager.k_MainChartPeriod_s;

        serieECG = chart.AddSerie<Line>(k_serieECGName, true);
        serieECG.AnimationEnable(false);
        serieECG.sampleDist = 0.5f;
        serieECG.sampleType = SampleType.Peak;
        serieECG.lineStyle.width = 0.65f;

        serieRPeak = chart.AddSerie<Scatter>(k_serieRPeakName, false);
        serieRPeak.AnimationEnable(false);
        serieRPeak.colorBy = SerieColorBy.Serie;
        serieRPeak.itemStyle.color = Color.red;
        serieRPeak.symbol.size = 3f;
        serieRPeak.itemStyle.opacity = 0.8f; // 不透明度

        // 只是为了显示图例而已
        seriePWave = chart.AddSerie<Scatter>(k_seriePWaveName, false);
        seriePWave.AnimationEnable(false);
        seriePWave.colorBy = SerieColorBy.Serie;
        seriePWave.itemStyle.color = Color.green;
        seriePWave.show = false;

        serieRWave = chart.AddSerie<Scatter>(k_serieRWaveName, false);
        serieRWave.AnimationEnable(false);
        serieRWave.colorBy = SerieColorBy.Serie;
        serieRWave.itemStyle.color = Color.yellow;
        serieRWave.show = false;

        serieTWave = chart.AddSerie<Scatter>(k_serieTWaveName, false);
        serieTWave.AnimationEnable(false);
        serieTWave.colorBy = SerieColorBy.Serie;
        serieTWave.itemStyle.color = Color.cyan;
        serieTWave.show = false;

        yAxis.axisLabel.formatter = "{value:f1}";
        yAxis.interval = 0.5; // 设置Y轴刻度间隔为0.5mV
        yAxis.axisLabel.showStartLabel = true;
        yAxis.axisLabel.showEndLabel = true;
        yAxis.minorSplitLine.show = true;
        yAxis.minorTick.splitNumber = 5;

        xAxis.axisLabel.formatter = "{value:f1}";
        xAxis.interval = 0.2; // 设置X轴刻度间隔为0.2s
        xAxis.axisLabel.showStartLabel = true;
        xAxis.axisLabel.showEndLabel = true;
        xAxis.splitLine.show = true;
        xAxis.minorSplitLine.show = true;
        yAxis.minorTick.splitNumber = 5;


        AxisTick xAxisTick = xAxis.axisTick;
        AxisTick yAxisTick = yAxis.axisTick;
        xAxisTick.showStartTick = true;
        xAxisTick.showEndTick = true;
        yAxisTick.showStartTick = true;
        yAxisTick.showEndTick = true;
    }

    void Update()
    {
        serieECG.ClearData();
        serieRPeak.ClearData();
        chart.RemoveChartComponents<MarkArea>();
        if (dataManager.ECGData.Count > 0 && dataManager.ECGDataFs > 0)
        {
            dataManager.Slider_MainChartTime.interactable = true;
            dataManager.Button_MainChartMoveForward.interactable = true;
            dataManager.Button_MainChartFastMoveForward.interactable = true;
            dataManager.Button_MainChartMoveBackward.interactable = true;
            dataManager.Button_MainChartFastMoveBackward.interactable = true;

            int stPos = (int)(dataManager.MainChartStTime_s * dataManager.ECGDataFs);
            int length = (int)(DataManager.k_MainChartPeriod_s * dataManager.ECGDataFs);
            XAxis xAxis = chart.GetOrAddChartComponent<XAxis>();
            YAxis yAxis = chart.GetOrAddChartComponent<YAxis>();
            double maxVal = dataManager.ECGData[stPos];
            double minVal = dataManager.ECGData[stPos];
            for (int i = 0; i < length; i++)
            {
                serieECG.AddXYData(i / dataManager.ECGDataFs, dataManager.ECGData[stPos + i]);
                //serieECG.AddXYData((st_pos + i) / dataManager.ECGDataFs, dataManager.ECGData[st_pos + i]);
                maxVal = Math.Max(maxVal, dataManager.ECGData[stPos + i]);
                minVal = Math.Min(minVal, dataManager.ECGData[stPos + i]);
            }
            yAxis.min = Math.Max(minVal, k_MinVal);
            yAxis.max = Math.Min(maxVal, k_MaxVal);

            // 绘制R波波峰
            if (dataManager.AnnotationFs > 0 && dataManager.RPeakList.Count > 0 && dataManager.Toggle_ShowRPeakAnnotation.isOn)
            {
                serieRPeak.show = true;
                int anStPos = dataManager.RPeakList.BinarySearch((int)(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
                if (anStPos < 0)
                {
                    anStPos = ~anStPos;
                }
                for (int i = anStPos; i < dataManager.RPeakList.Count && dataManager.RPeakList[i] <= (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs; i++)
                {
                    int ecgidx = (int)((dataManager.RPeakList[i] - dataManager.MainChartStTime_s * dataManager.AnnotationFs) * dataManager.ECGDataFs / dataManager.AnnotationFs);
                    serieRPeak.AddXYData((double)ecgidx / dataManager.ECGDataFs, dataManager.ECGData[ecgidx + stPos]);
                }
            }
            else
            {
                serieRPeak.show = false;
            }

            // 绘制分割标记
            if (dataManager.AnnotationFs > 0 && dataManager.SegFilePath != null && dataManager.Toggle_ShowSegAnnotation.isOn)
            {
                seriePWave.show = true;
                serieRWave.show = true;
                serieTWave.show = true;
                AddPQRSTSegAnnotation(dataManager.POnList, dataManager.POffList, Color.green);
                AddPQRSTSegAnnotation(dataManager.ROnList, dataManager.ROffList, Color.yellow);
                AddPQRSTSegAnnotation(dataManager.TOnList, dataManager.TOffList, Color.cyan);
            }
            else
            {
                seriePWave.show = false;
                serieRWave.show = false;
                serieTWave.show = false;
            }
        }
        else
        {
            dataManager.Slider_MainChartTime.interactable = false;
            dataManager.Slider_MainChartTime.value = 0;
            dataManager.Button_MainChartMoveForward.interactable = false;
            dataManager.Button_MainChartFastMoveForward.interactable = false;
            dataManager.Button_MainChartMoveBackward.interactable = false;
            dataManager.Button_MainChartFastMoveBackward.interactable = false;
        }
    }

    private void AddPQRSTSegAnnotation(in List<int> onList, in List<int> offList, Color32 color)
    {
        int anStPos = offList.BinarySearch((int)(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
        if (anStPos < 0)
        {
            anStPos = ~anStPos;
        }
        for (int i = anStPos; i < onList.Count && onList[i] <= (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs; i++)
        {
            float stTime = Math.Max(dataManager.MainChartStTime_s, onList[i] / (float)dataManager.AnnotationFs);
            float edTime = Math.Min(dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s, offList[i] / (float)dataManager.AnnotationFs);
            AddSegAnnotationMarkArea(stTime - dataManager.MainChartStTime_s, edTime - dataManager.MainChartStTime_s, color);
        }
    }

    // 此处时间为相对时间，而非从整个心电图开始的时间，单位（秒）
    private void AddSegAnnotationMarkArea(float stTime, float edTime, Color32 color)
    {
        MarkArea markArea = chart.AddChartComponent<MarkArea>();
        markArea.serieIndex = serieECG.index;
        markArea.itemStyle.color = color;

        markArea.start.type = MarkAreaType.None;
        stTime = Math.Max(0.001f, stTime);
        markArea.start.xPosition = stTime / DataManager.k_MainChartPeriod_s * (1 - k_GridLeft - k_GridRight) * k_Width;
        markArea.start.yPosition = 0;

        markArea.end.type = MarkAreaType.None;
        markArea.end.xPosition = edTime / DataManager.k_MainChartPeriod_s * (1 - k_GridLeft - k_GridRight) * k_Width;
        markArea.end.yPosition = 0.665f * k_Height;
    }
}
