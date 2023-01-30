using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XCharts.Runtime;

[DisallowMultipleComponent]
public class PlotMainChart : MonoBehaviour
{
    public LineChart chart;
    Line serieECG;
    const string k_serieECGName = "ECG";

    Scatter serieRPeak;
    const string k_serieRPeakName = "RPeak";

    Scatter serieSegComment;
    const string k_serieSegCommentName = "�ָ��޸ı��";

    // ֻ��Ϊ����ʾͼ������
    Scatter seriePWave;
    const string k_seriePWaveName = "PWave";
    Scatter serieRWave;
    const string k_serieRWaveName = "QRSWave";
    Scatter serieTWave;
    const string k_serieTWaveName = "TWave";

    

    const int k_Width = 1920;
    const int k_Height = 300;
    const string k_Title = "��̬�ĵ�ͼ";
    const float k_GridLeft = 0.05f;  // ��߾�
    const float k_GridRight = 0.04f; // �ұ߾�

    const double k_MaxVal = 5;
    const double k_MinVal = -5;

    public DataManager dataManager;
    public ECGTooltip ecgTooltip;

    void Awake()
    {
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
        serieRPeak.itemStyle.opacity = 0.8f; // ��͸����

        serieSegComment = chart.AddSerie<Scatter>(k_serieSegCommentName, false);
        serieSegComment.AnimationEnable(false);
        serieSegComment.colorBy = SerieColorBy.Serie;
        serieSegComment.itemStyle.color = Color.gray;
        serieSegComment.symbol.size = 4.5f;
        serieSegComment.itemStyle.borderWidth = 1f;
        serieSegComment.itemStyle.borderColor = Color.black;
        serieSegComment.itemStyle.opacity = 0.4f; // ��͸����
        //LabelStyle serieSegCommentLabelstyle = serieSegComment.AddExtraComponent<LabelStyle>();
        //serieSegCommentLabelstyle.show = true;
        //serieSegCommentLabelstyle.formatter = "{b}";
        //serieSegCommentLabelstyle.position = LabelStyle.Position.Default;


        // ֻ��Ϊ����ʾͼ������
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
        yAxis.interval = 0.5; // ����Y��̶ȼ��Ϊ0.5mV
        yAxis.axisLabel.showStartLabel = true;
        yAxis.axisLabel.showEndLabel = true;
        yAxis.minorSplitLine.show = true;
        yAxis.minorTick.splitNumber = 5;

        xAxis.axisLabel.formatter = "{value:f1}";
        xAxis.interval = 0.2; // ����X��̶ȼ��Ϊ0.2s
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

        //tooltip = chart.AddChartComponent<Tooltip>();
        //tooltip.show = false;
        //tooltip.type = Tooltip.Type.Corss;
        //tooltip.trigger = Tooltip.Trigger.Item;
        //tooltip.position = Tooltip.Position.Auto;
        //tooltip.titleFormatter = "{.2}{a2}:{e2}";
        //tooltip.itemFormatter = ""; 

        // ΪECGTooltip��CreateSegComment�ṩ���λ��
        chart.onAxisPointerValueChanged = delegate (Axis axis, double value)
        {
            if (axis is XAxis)
            {
                if (dataManager.IsValidPlotRPeak() || dataManager.IsValidPlotClsAnnotation())
                {
                    dataManager.PointerTimeAnnotationIndex = (int)Math.Round((value + dataManager.MainChartStTime_s) * dataManager.AnnotationFs);
                }
                else
                {
                    dataManager.PointerTimeAnnotationIndex = -1;
                }
            }
        };

        chart.onPointerExit = delegate (PointerEventData eventData, BaseGraph baseGraph)
        {
            dataManager.PointerTimeAnnotationIndex = -1;
        };
    }

    void Update()
    {
        serieECG.ClearData();
        serieRPeak.ClearData();
        serieSegComment.ClearData();
        chart.RemoveChartComponents<MarkArea>();
        //tooltip.ClearData();
        ecgTooltip.ClearData();

        if (dataManager.IsValidPlotMainChart())
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

            // ����R������
            if (dataManager.IsValidPlotRPeak())
            {
                serieRPeak.show = true;
                serieSegComment.show = true;
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

                // �ָ��޸ı��
                anStPos = dataManager.GetLowerBoundIndexOfSegCommentList((int)(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
                if (anStPos < 0)
                {
                    anStPos = ~anStPos;
                }
                for (int i = anStPos; i < dataManager.SegCommentList.Count && dataManager.SegCommentList[i].timeIndex <= (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs; i++)
                {
                    int ecgidx = (int)((dataManager.SegCommentList[i].timeIndex - dataManager.MainChartStTime_s * dataManager.AnnotationFs) * dataManager.ECGDataFs / dataManager.AnnotationFs);
                    SerieData data = serieSegComment.AddXYData((double)ecgidx / dataManager.ECGDataFs, dataManager.ECGData[ecgidx + stPos], SegCommentType.GetTip(dataManager.SegCommentList[i].type));
                    ecgTooltip.AddSegComment(dataManager.SegCommentList[i]);
                    //tooltip.AddSerieDataIndex(serieSegComment.index, data.index);
                    //LabelStyle labelstyle = data.GetOrAddComponent<LabelStyle>();
                    //labelstyle.show = true;
                    //labelstyle.formatter = "{e}";
                    //labelstyle.position = LabelStyle.Position.Inside;
                }

                if (dataManager.CreateSegCommentTimeIndex != -1)
                {
                    if (dataManager.CreateSegCommentTimeIndex >= dataManager.MainChartStTime_s * dataManager.AnnotationFs && dataManager.CreateSegCommentTimeIndex < (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs)
                    {
                        int ecgidx = (int)((dataManager.CreateSegCommentTimeIndex - dataManager.MainChartStTime_s * dataManager.AnnotationFs) * dataManager.ECGDataFs / dataManager.AnnotationFs);
                        SerieData data = serieSegComment.AddXYData((double)ecgidx / dataManager.ECGDataFs, dataManager.ECGData[ecgidx + stPos], "NEW");
                        ecgTooltip.AddSegComment(new SegCommentData(dataManager.CreateSegCommentTimeIndex, SegCommentType.New, "��ǰ�½�λ��"));
                        //tooltip.AddSerieDataIndex(serieSegComment.index, data.index);
                        //data.itemStyle.borderColor = new Color32(255, 0, 255, 1); // ���/�ۺ�
                        //LabelStyle labelstyle = data.GetOrAddComponent<LabelStyle>();
                        //labelstyle.show = true;
                        //labelstyle.formatter = "{e}";
                        //labelstyle.position = LabelStyle.Position.Inside;
                    }
                    
                }
                //ResetTooltip(serieSegComment.dataCount > 0);
            }
            else
            {
                serieRPeak.show = false;
                serieSegComment.show = false;
                //ResetTooltip(false);
            }

            // ���Ʒָ���
            if (dataManager.IsValidPlotSegAnnotation())
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

            // ���Ƹ�������
            if (dataManager.AnnotationFs > 0.01f && dataManager.MainChartFocusTimeIndex != -1 && dataManager.MainChartFocusLeftTime > 0.001f)
            {
                double focusTime = dataManager.MainChartFocusTimeIndex / dataManager.AnnotationFs;
                double alpha = dataManager.MainChartFocusLeftTime / DataManager.k_MainChartDefaultFocusLeftTime_s;
                if (dataManager.IsInMainChartScope((float)focusTime))
                {
                    Color32 orangeYellow = new Color32(255, 128, 0, 255); // �ٻ�ɫ
                    AddFocusMarkArea((float)focusTime - dataManager.MainChartStTime_s, orangeYellow, (float)alpha);
                }
                dataManager.MainChartFocusLeftTime -= Time.deltaTime;
            }
            else
            {
                dataManager.ReSetMainChartFocusTimeIndex();
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
        ecgTooltip.UpdateECGTooltip();
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

    // �˴�ʱ��Ϊ���ʱ�䣬���Ǵ������ĵ�ͼ��ʼ��ʱ�䣬��λ���룩
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

    // �˴�ʱ��Ϊ���ʱ�䣬���Ǵ������ĵ�ͼ��ʼ��ʱ�䣬��λ���룩
    private void AddFocusMarkArea(float midTime, Color32 color, float alpha, float width = 0.2f)
    {
        MarkArea markArea = chart.AddChartComponent<MarkArea>();
        markArea.serieIndex = serieECG.index;
        markArea.itemStyle.color = color;
        markArea.itemStyle.opacity = alpha;

        markArea.start.type = MarkAreaType.None;
        float stTime = Math.Max(0.001f, midTime - width / 2);
        markArea.start.xPosition = stTime / DataManager.k_MainChartPeriod_s * (1 - k_GridLeft - k_GridRight) * k_Width;
        markArea.start.yPosition = 0;

        markArea.end.type = MarkAreaType.None;
        float edTime = Math.Min(DataManager.k_MainChartPeriod_s - 0.001f, midTime + width / 2);
        markArea.end.xPosition = edTime / DataManager.k_MainChartPeriod_s * (1 - k_GridLeft - k_GridRight) * k_Width;
        markArea.end.yPosition = 0.665f * k_Height;
    }

    //private Tooltip ResetTooltip(bool flag)
    //{
    //    if (tooltip != null)
    //    {
    //        chart.RemoveChartComponents<Tooltip>();
    //        tooltip = null;
    //    }
    //    if (flag)
    //    {
    //        tooltip = chart.AddChartComponent<Tooltip>();
    //        tooltip.show = true;
    //        tooltip.type = Tooltip.Type.Corss;
    //        tooltip.trigger = Tooltip.Trigger.Item;
    //        tooltip.position = Tooltip.Position.Auto;
    //        tooltip.titleFormatter = "{.2}{a2}:{e2}";
    //        tooltip.itemFormatter = "";
    //    }
    //    return tooltip;
    //}
}
