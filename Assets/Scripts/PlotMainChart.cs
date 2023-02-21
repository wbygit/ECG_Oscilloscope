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
    const string k_serieSegCommentName = "分割修改标记";

    // 只是为了显示图例而已
    Scatter seriePWave;
    const string k_seriePWaveName = "PWave";
    Scatter serieRWave;
    const string k_serieRWaveName = "QRSWave";
    Scatter serieTWave;
    const string k_serieTWaveName = "TWave";

    Scatter serieClsOutput;
    const string k_serieClsOutput = "分类输出";

    Scatter serieClsLabel;
    const string k_serieClsLabel = "分类标签";

    const int k_Width = 1920;
    const int k_Height = 340;
    const string k_Title = "动态心电图";
    const float k_GridLeft = 0.05f;  // 左边距
    const float k_GridRight = 0.04f; // 右边距

    const double k_MaxVal = 5;
    const double k_MinVal = -5;

    public DataManager dataManager;
    public ECGTooltip ecgTooltip;

    private bool updateInThisFrame = true; // 当前帧是否需要重新绘制

    public void NeedUpdate()
    {
        updateInThisFrame = true;
    }

    void Awake()
    {
        chart = gameObject.AddComponent<LineChart>();
        chart.Init(false);
        chart.SetSize(k_Width, k_Height);

        //Title title = chart.AddChartComponent<Title>();
        //title.text = k_Title;
        //title.show = true;

        GridCoord girdCoord = chart.AddChartComponent<GridCoord>();
        girdCoord.left = k_GridLeft;
        girdCoord.right = k_GridRight;
        girdCoord.top = 0.2f;

        Legend legend = chart.GetOrAddChartComponent<Legend>();
        legend.location.top = 0.02f;
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

        serieSegComment = chart.AddSerie<Scatter>(k_serieSegCommentName, false);
        serieSegComment.AnimationEnable(false);
        serieSegComment.colorBy = SerieColorBy.Serie;
        serieSegComment.itemStyle.color = Color.gray;
        serieSegComment.symbol.size = 4.5f;
        serieSegComment.itemStyle.borderWidth = 1f;
        serieSegComment.itemStyle.borderColor = Color.black;
        serieSegComment.itemStyle.opacity = 0.4f; // 不透明度
        //LabelStyle serieSegCommentLabelstyle = serieSegComment.AddExtraComponent<LabelStyle>();
        //serieSegCommentLabelstyle.show = true;
        //serieSegCommentLabelstyle.formatter = "{b}";
        //serieSegCommentLabelstyle.position = LabelStyle.Position.Default;

        

        #region // 只是为了显示图例而已
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
        #endregion


        serieClsOutput = chart.AddSerie<Scatter>(k_serieClsOutput, false);
        serieClsOutput.AnimationEnable(false);
        serieClsOutput.colorBy = SerieColorBy.Serie;
        serieClsOutput.itemStyle.color = Color.blue;
        serieClsOutput.symbol.type = SymbolType.Triangle;
        serieClsOutput.symbol.size = 10f;
        serieClsOutput.itemStyle.backgroundWidth = 2f;
        serieClsOutput.itemStyle.borderColor = Color.blue;
        serieClsOutput.itemStyle.opacity = 0.45f; // 不透明度
        LabelStyle serieClsOutputLabelstyle = serieClsOutput.AddExtraComponent<LabelStyle>();
        serieClsOutputLabelstyle.show = true;
        serieClsOutputLabelstyle.textStyle.color = Color.blue;
        serieClsOutputLabelstyle.formatter = "{b}";
        serieClsOutputLabelstyle.position = LabelStyle.Position.Top;
        serieClsOutputLabelstyle.textStyle.fontSize = 15;
        serieClsOutputLabelstyle.offset = new Vector3(0, 25, 0); // 文字向上偏移一点

        serieClsLabel = chart.AddSerie<Scatter>(k_serieClsLabel, false);
        serieClsLabel.AnimationEnable(false);
        serieClsLabel.colorBy = SerieColorBy.Serie;
        Color32 brownColor = new Color32(128, 42, 42, 255); // 棕色
        serieClsLabel.itemStyle.color = brownColor;
        serieClsLabel.symbol.type = SymbolType.EmptyRect;
        serieClsLabel.symbol.size = 20f;
        serieClsLabel.itemStyle.opacity = 0.8f; // 不透明度
        LabelStyle serieClsLabelLabelstyle = serieClsLabel.AddExtraComponent<LabelStyle>();
        serieClsLabelLabelstyle.show = true;
        serieClsLabelLabelstyle.textStyle.color = brownColor;
        serieClsLabelLabelstyle.formatter = "{b}";
        serieClsLabelLabelstyle.position = LabelStyle.Position.Bottom;
        serieClsLabelLabelstyle.textStyle.fontSize = 15;
        serieClsLabelLabelstyle.offset = new Vector3(0, -45, 0); // 文字向下偏移一点


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

        //tooltip = chart.AddChartComponent<Tooltip>();
        //tooltip.show = false;
        //tooltip.type = Tooltip.Type.Corss;
        //tooltip.trigger = Tooltip.Trigger.Item;
        //tooltip.position = Tooltip.Position.Auto;
        //tooltip.titleFormatter = "{.2}{a2}:{e2}";
        //tooltip.itemFormatter = ""; 

        // 为ECGTooltip和CreateSegComment提供鼠标位置
        chart.onAxisPointerValueChanged = delegate (Axis axis, double value)
        {
            if (axis is XAxis)
            {
                if (dataManager.IsValidPlotRPeak() || dataManager.IsValidPlotClsOutput() || dataManager.IsValidPlotClsLabel())
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
        if (dataManager.MainChartFocusTimeIndex != -1 || updateInThisFrame)
        {
            chart.RemoveChartComponents<MarkArea>();
            #region // 绘制分割标记
            if (dataManager.IsValidPlotMainChart() && dataManager.IsValidPlotSegAnnotation())
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
            #endregion

            #region // 绘制高亮区域

            if (dataManager.IsValidPlotMainChart() && dataManager.AnnotationFs > 0.01f && dataManager.MainChartFocusTimeIndex != -1 && dataManager.MainChartFocusLeftTime > 0.001f)
            {
                double focusTime = dataManager.MainChartFocusTimeIndex / dataManager.AnnotationFs;
                double alpha = dataManager.MainChartFocusLeftTime / DataManager.k_MainChartDefaultFocusLeftTime_s;
                if (dataManager.IsInMainChartScope((float)focusTime))
                {
                    Color32 orangeYellow = new Color32(255, 128, 0, 255); // 橘黄色
                    AddFocusMarkArea((float)focusTime - dataManager.MainChartStTime_s, orangeYellow, (float)alpha);
                }
                dataManager.MainChartFocusLeftTime -= Time.deltaTime;
            }
            else
            {
                dataManager.ReSetMainChartFocusTimeIndex();
            }
            #endregion
        }


        if (!updateInThisFrame)
        {
            ecgTooltip.UpdateECGTooltip();
            return;
        }
        updateInThisFrame = false;
        serieECG.ClearData();
        serieRPeak.ClearData();
        serieSegComment.ClearData();
        
        serieRWave.ClearData();
        serieTWave.ClearData();
        seriePWave.ClearData();
        //tooltip.ClearData();
        ecgTooltip.ClearData();
        serieClsOutput.ClearData();
        serieClsLabel.ClearData();
        //chart.RemoveChartComponents<MarkLine>();

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

            // 绘制R波波峰
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

                // 分割修改标记
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
                        ecgTooltip.AddSegComment(new SegCommentData(dataManager.CreateSegCommentTimeIndex, SegCommentType.New, "当前新建位置"));
                    }

                }
            }
            else
            {
                serieRPeak.show = false;
                serieSegComment.show = false;
                //ResetTooltip(false);
            }


            #region 绘制模型分类输出
            // 绘制模型分类输出
            if (dataManager.IsValidPlotClsOutput())
            {
                if (!serieClsOutput.show)
                {
                    serieClsOutput.show = true;
                }
                //MarkLine markLine = chart.AddChartComponent<MarkLine>();
                //markLine.serieIndex = serieClsOutput.index;
                //markLine.show = true;
                
                var curClsOutputList = dataManager.ClsOutputContainer.curClsOutputList;
                var clsCommentDcit = dataManager.ClsOutputContainer.clsCommentDict;
                int anStPos = dataManager.ClsOutputContainer.GetLowerBoundIndexOfCurClsOutputList((int)(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
                for (int i = anStPos; i < curClsOutputList.Count && curClsOutputList.Keys[i] <= (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs; i++)
                {
                    int ecgidx = (int)((curClsOutputList.Keys[i] - dataManager.MainChartStTime_s * dataManager.AnnotationFs) * dataManager.ECGDataFs / dataManager.AnnotationFs);
                    double x = (double)ecgidx / dataManager.ECGDataFs;
                    double y = dataManager.ECGData[ecgidx + stPos];
                    string name = string.Format("({0})\n{1}", i+1, curClsOutputList.Values[i].arrythmia);
                    int curTimeIndex = curClsOutputList.Keys[i];
                    if (clsCommentDcit.ContainsKey(curTimeIndex))
                    {
                        if (clsCommentDcit[curTimeIndex].newArrythmia == ArrythmiaDict.k_EmptyArrythmia)
                        {
                            name = string.Format("{0}\n待复核", name);
                        }
                        else
                        {
                            name = string.Format("{0}\n复核为[{1}]", name, clsCommentDcit[curTimeIndex].newArrythmia);
                        }
                    }
                    serieClsOutput.AddXYData(x, y, name);
                }
                serieClsOutput.RefreshLabel();
            }
            else
            {
                serieClsOutput.show = false;
            }
            #endregion

            #region 绘制分类标签
            // 绘制分类标签
            if (dataManager.IsValidPlotClsLabel())
            {
                if (!serieClsLabel.show)
                {
                    serieClsLabel.show = true;
                }

                var curClsLabelList = dataManager.ClsLabelContainer.curClsLabelList;
                int anStPos = dataManager.ClsLabelContainer.GetLowerBoundIndexOfCurClsLabelList((int)(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
                for (int i = anStPos; i < curClsLabelList.Count && curClsLabelList.Keys[i] <= (dataManager.MainChartStTime_s + DataManager.k_MainChartPeriod_s) * dataManager.AnnotationFs; i++)
                {
                    int ecgidx = (int)((curClsLabelList.Keys[i] - dataManager.MainChartStTime_s * dataManager.AnnotationFs) * dataManager.ECGDataFs / dataManager.AnnotationFs);
                    double x = (double)ecgidx / dataManager.ECGDataFs;
                    double y = dataManager.ECGData[ecgidx + stPos];
                    serieClsLabel.AddXYData(x, y, string.Format("({0})\n{1}", i + 1, curClsLabelList.Values[i]));
                }
                serieClsLabel.RefreshLabel();
            }
            else
            {
                serieClsLabel.show = false;
            }
            #endregion
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

    // 此处时间为相对时间，而非从整个心电图开始的时间，单位（秒）
    private void AddSegAnnotationMarkArea(float stTime, float edTime, Color32 color)
    {
        MarkArea markArea = chart.AddChartComponent<MarkArea>();
        markArea.serieIndex = serieRWave.index;
        markArea.itemStyle.color = color;

        markArea.start.type = MarkAreaType.None;
        stTime = Math.Max(0.001f, stTime);
        markArea.start.xPosition = stTime / DataManager.k_MainChartPeriod_s * (1 - k_GridLeft - k_GridRight) * k_Width;
        markArea.start.yPosition = 0;

        markArea.end.type = MarkAreaType.None;
        markArea.end.xPosition = edTime / DataManager.k_MainChartPeriod_s * (1 - k_GridLeft - k_GridRight) * k_Width;
        markArea.end.yPosition = 0.665f * k_Height;
    }

    // 此处时间为相对时间，而非从整个心电图开始的时间，单位（秒）
    private MarkArea AddFocusMarkArea(float midTime, Color32 color, float alpha, float width = 0.2f)
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
        return markArea;
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
