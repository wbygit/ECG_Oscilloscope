using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Text; 

public class DataManager : MonoBehaviour
{
    public const int k_MainChartPeriod_s = 10; // ��ͼ��ʾ��������ĵ�ͼ
    public const int k_MainChartMovePeriod_s = 7; // �����ƶ���������ĵ�ͼ
    public const int k_MainChartFastMovePeriod_s = 60; // ���ҿ����ƶ���������ĵ�ͼ
    public const double k_MainChartDefaultFocusLeftTime_s = 1; // Ĭ��ʣ��ĸ�����ʾʱ��
    public const float k_CreateSegCommentMovePeroid_s = 0.01f; // �����ָ��޸ı��λ��ʱ΢��һ���ƶ��೤ʱ��
    public readonly double[] k_JLECGDataFsArr = {348.8, 500, 1000, 697.5, 240, 250, 400};

    public PlotMainChart mainChart;

    public string ECGFilePath; // ��filepathΪnull������ζ������δ��ȡ�������л�ͼ
    public TMP_InputField InputField_ECGDataFilePath;
    public TMP_Text Text_ECGDataLength;
    public List<double> ECGData = new List<double>();

    public double ECGDataFs = 0; // ��������Ϊ�㣬����ζ�Ÿ���δ���ã������л�ͼ
    public TMP_InputField InputField_ECGDataFs;

    public string RPeakFilePath;
    public TMP_InputField InputField_RPeakFilePath;
    public Toggle Toggle_ShowRPeakAnnotation;
    public List<int> RPeakList = new List<int>(); // R���������У���AnnotationFs������Ϊ׼�Ĳ������±�

    public string SegFilePath;
    public TMP_InputField InputField_SegFilePath;
    public Toggle Toggle_ShowSegAnnotation;
    public List<int> ROnList = new List<int>();
    public List<int> ROffList = new List<int>();
    public List<int> TOnList = new List<int>();
    public List<int> TOffList = new List<int>();
    public List<int> POnList = new List<int>();
    public List<int> POffList = new List<int>();

    public List<SegCommentData> SegCommentList = new List<SegCommentData>(); // �ָ����޸����б���AnnotationFs������Ϊ׼�Ĳ������±�
    public bool SegCommentListSavedFlag = false; // ��Ƿָ����޸����Ƿ��޸�δ����

    public string ArrythmiaFliePath;
    public ArrythmiaDict ArrythmiaDict = new ArrythmiaDict();
    public TMP_InputField InputField_ArrythmiaFilePath;
    public string ClassificationOutputPath;
    public ClassificationOutputContainer ClsOutputContainer;
    public bool ClsCommentSavedFlag = false; // ��Ƿָ����޸����Ƿ��޸�δ����
    public TMP_InputField InputField_ClassificationOutputPath;
    public Toggle Toggle_ShowClassificationOutput;


    public string ClassificationLabelPath;
    public ClassificationLabelContainer ClsLabelContainer = new ClassificationLabelContainer();
    public TMP_InputField InputField_ClassificationLabelPath;
    public Toggle Toggle_ShowClassificationLabel;

    public double AnnotationFs = 0; // ��������Ϊ�㣬����ζ�Ÿ���δ���ã������Ʊ��
    public TMP_InputField InputField_AnnotationFs;

    public int MainChartStTime_s = 0;
    public TMP_InputField InputField_MainChartStHour;
    public TMP_InputField InputField_MainChartStMinute;
    public TMP_InputField InputField_MainChartStSecond;
    public TMP_InputField InputField_MainChartEdHour;
    public TMP_InputField InputField_MainChartEdMinute;
    public TMP_InputField InputField_MainChartEdSecond;

    public Slider Slider_MainChartTime;
    public Button Button_MainChartMoveForward;
    public Button Button_MainChartFastMoveForward;
    public Button Button_MainChartMoveBackward;
    public Button Button_MainChartFastMoveBackward;

    public int CreateSegCommentTimeIndex = -1; // �����ָ��޸ı��ʱ��λ��
    public int PointerTimeAnnotationIndex = -1; // ������ڲ���λ�ã��Ա�ǲ����ʣ�
    public int MainChartFocusTimeIndex = -1; // ��Ҫ��ͼ�۽���ʾ��λ�� ���Ա�ǲ����ʣ�
    public double MainChartFocusLeftTime = 0; // �۽�λ�õ�ʣ��ʱ��


    public void OnClick_Button_LoadECGData()
    {
        string filepath = OpenFileDialog.GetFilePath("TXT", "ѡ���ĵ������ļ�");
        if (filepath != null)
        {
            ECGData.Clear();
            StreamReader inputFile = null;
            try
            {
                inputFile = System.IO.File.OpenText(filepath);
                StringBuilder str = new StringBuilder();
                while (inputFile.Peek() != -1)
                {
                    char c = (char)inputFile.Read();
                    if (c == ',')
                    {
                        if (!String.IsNullOrWhiteSpace(str.ToString()))
                        {
                            ECGData.Add(Convert.ToDouble(str.ToString()));
                        }
                        str.Clear();
                    }
                    else
                    {
                        str.Append(c);
                    }
                }
                if (!String.IsNullOrWhiteSpace(str.ToString()))
                {
                    ECGData.Add(Convert.ToDouble(str.ToString()));
                }


                //MessageBox.DisplayMessageBox("�����ĵ�����", "��ȡ�ļ��У����Եȡ�", false, null);
                //string[] lines = System.IO.File.ReadAllLines(filepath);

                //foreach (string line in lines)
                //{
                //    string[] arr = line.Split(",");
                //    foreach (string vstr in arr)
                //    {
                //        if (!String.IsNullOrWhiteSpace(vstr))
                //        {
                //            ECGData.Add(Convert.ToDouble(vstr));
                //        }
                //    }
                //}
                ECGData.Capacity = ECGData.Count;
                ECGFilePath = filepath;
                InputField_ECGDataFilePath.text = filepath;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e);
                ECGFilePath = null;
                ECGData.Clear();
                InputField_ECGDataFilePath.text = "";
            }
            finally
            {
                MainChartStTime_s = 0;
                UpdateMainChartStEdTime();
                if (ECGDataFs >= 0.01)
                {
                    TransSecondToHMS((int)(ECGData.Count / ECGDataFs), out int maxh, out int maxm, out int maxs);
                    Text_ECGDataLength.text = String.Format("��ʱ����{0:00}h{1:00}m{:00}s", maxh, maxm, maxs);
                }
                else
                {
                    Text_ECGDataLength.text = "��ʱ����";
                }
                if (inputFile != null)
                {
                    inputFile.Dispose();
                }
            }
        }
    }

    public void OnClick_Button_SaveCurECGData()
    {
        if (!IsValidPlotMainChart())
        {
            MessageBox.DisplayMessageBox("����ʧ��", "��ǰ��ʾ�ĵ�ͼΪ��", true, null);
            return;
        }
        int fsIndex = Tool.GetSimilarValueIndexInArr(k_JLECGDataFsArr, ECGDataFs);
        if (fsIndex < 0)
        {
            MessageBox.DisplayMessageBox("����ʧ��", string.Format("��֧�ֵ�ǰ���ݲ�����"), true, null);
        }

        string defaultFileName = string.Format(
            "{0}_{1}-{2}-{3}_{4}-{5}-{6}.ecg", System.IO.Path.GetFileNameWithoutExtension(ECGFilePath), InputField_MainChartStHour.text, InputField_MainChartStMinute.text, InputField_MainChartStSecond.text,
            InputField_MainChartEdHour.text, InputField_MainChartEdMinute.text, InputField_MainChartEdSecond.text
            );
        string filepath = OpenFileDialog.GetSavePath("ECG", "���������������ļ�", System.IO.Path.GetDirectoryName(ECGFilePath), defaultFileName);
        if (filepath != null)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filepath, FileMode.Create)))
                {
                    ushort leadType = 101; // 10����ϵͳ�����˸�����
                    writer.Write(leadType);
                    //byte leadsValidBits = 0b00000001; // ��II������������
                    byte leadsValidBits = 0b11111111; // ��II������������
                    writer.Write(leadsValidBits);
                    double mv2ad = 104.8; // ת����ֵ��1ad����mv2ad��mv
                    byte adhz = ((byte)fsIndex); // ��λ��ֵ���ͣ���λ��������𣻷�ֵ���ͱ�ʶΪ0��1.0/104.8 AD2MV��
                    writer.Write(adhz);
                    byte filterBits = 0; // �˲�����
                    byte QRSPaceValidBits = 0; // ������Ϣ
                    writer.Write(filterBits);
                    writer.Write(QRSPaceValidBits);
                    DateTime now = DateTime.UtcNow;
                    TimeSpan timeSpan = now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long beginTime = (long)timeSpan.TotalMilliseconds; // ϵͳ��ǰʱ���
                    writer.Write(beginTime);

                    int frameCount = (int)(k_MainChartPeriod_s * ECGDataFs);
                    writer.Write(frameCount);
                    for (int i = 0; i < 6; i++)
                    {
                        writer.Write((byte)0);
                    }
                    int stECGIndex = (int)(MainChartStTime_s * ECGDataFs);
                    
                    for (int i = 0; i < frameCount; i++)
                    {
                        for (int lead = 0; lead < 8; lead++)
                        {
                            short value = (short)(ECGData[stECGIndex + i] * mv2ad);
                            writer.Write(value);
                        }
                    }
                }
                string patientFilepath = filepath.Substring(0, filepath.Length-4) + ".txt";
                string patientContent = string.Format("����������\n����ע�ͣ�����Դ:{0}�����������ݣ���II������Ч��", defaultFileName.Substring(0, defaultFileName.Length-4));
                File.WriteAllText(patientFilepath, patientContent, new UTF8Encoding(false)); // ����BOMͷ��UTF-8����
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                MessageBox.DisplayMessageBox("����ʧ��", "���������г���", true, null);
            }
        }
    }

    public void OnClick_Button_LoadRPeakAnnotation()
    {
        string filepath = OpenFileDialog.GetFilePath("TXT", "ѡ��R���������ļ�");
        if (filepath != null)
        {
            RPeakList.Clear();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string[] arr = line.Split(",");
                    for (int i = 1; i < arr.Length; i++) // ������һ����Ӧ�����ļ���
                    {
                        if (!String.IsNullOrWhiteSpace(arr[i]))
                        {
                            RPeakList.Add(Convert.ToInt32(arr[i]));
                            if (RPeakList[RPeakList.Count - 1] < 0)
                            {
                                throw new Exception("RPeakIndex < 0");
                            }
                        }
                    }
                }
                RPeakList.Sort();
                RPeakFilePath = filepath;
                InputField_RPeakFilePath.text = filepath;
                Toggle_ShowRPeakAnnotation.interactable = true;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e);
                RPeakFilePath = null;
                RPeakList.Clear();
                InputField_RPeakFilePath.text = "";
                Toggle_ShowRPeakAnnotation.interactable = false;
            }
            finally
            {
                Toggle_ShowRPeakAnnotation.isOn = true;
                Toggle Toggle_SegCommentsTab = GameObject.Find("Toggle_SegCommentsTab").GetComponent<Toggle>();
                Toggle_SegCommentsTab.isOn = false;
                Toggle_SegCommentsTab.interactable = false;
            }
        }
    }

    public void OnClick_Button_LoadSegAnnotation()
    {
        string filepath = OpenFileDialog.GetFilePath("JSON", "ѡ��ָ����ļ�");
        if (filepath != null)
        {
            ROnList.Clear();
            ROffList.Clear();
            TOnList.Clear();
            TOffList.Clear();
            POnList.Clear();
            POnList.Clear();
            try
            {
                string jsonStr = System.IO.File.ReadAllText(filepath);
                Dictionary<string, List<int>> segDict = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(jsonStr);
                ROnList = segDict["R on"];
                ROffList = segDict["R off"];
                TOnList = segDict["T on"];
                TOffList = segDict["T off"];
                POnList = segDict["P on"];
                POffList = segDict["P off"];
                if (ROnList.Count != ROffList.Count || TOnList.Count != TOffList.Count || POnList.Count != POffList.Count)
                {
                    throw new Exception("�ָ��ǩOn��Off������һ��");
                }
                if ((ROnList.Count > 0 && (ROnList[0] < 0 || ROffList[0] < 0)) || (TOnList.Count > 0 && (TOnList[0] < 0 || TOffList[0] < 0)) || (POnList.Count > 0 && (POnList[0] < 0 || POffList[0] < 0)))
                {
                    throw new Exception("�ָ��ǩ�и�ֵ");
                }
                ROnList.Sort();
                ROffList.Sort();
                TOnList.Sort();
                TOffList.Sort();
                POnList.Sort();
                POffList.Sort();
                SegFilePath = filepath;
                InputField_SegFilePath.text = filepath;
                Toggle_ShowSegAnnotation.interactable = true;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e);
                SegFilePath = null;
                ROnList = new List<int>();
                ROffList = new List<int>();
                TOnList = new List<int>();
                TOffList = new List<int>();
                POnList = new List<int>();
                POnList = new List<int>();
                InputField_SegFilePath.text = "";
                Toggle_ShowSegAnnotation.interactable = false;
            }
            finally
            {
                Toggle_ShowSegAnnotation.isOn = true;
            }
        }
    }

    public void OnClick_Button_LoadArrythmiaFile()
    {
        string filepath = OpenFileDialog.GetFilePath("TXT", "ѡ����������ļ�");
        if (filepath != null)
        {
            ArrythmiaDict.Clear();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string arrythmia = line.Trim();
                    if (!string.IsNullOrEmpty(arrythmia))
                    {
                        ArrythmiaDict.AddArrythmia(arrythmia);
                    }
                }
                ArrythmiaFliePath = filepath;
                InputField_ArrythmiaFilePath.text = filepath;
                Toggle_ShowClassificationOutput.interactable = ClassificationOutputPath != null;
                
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e);
                ArrythmiaFliePath = null;
                ArrythmiaDict.Clear();
                InputField_RPeakFilePath.text = "";
                Toggle_ShowClassificationOutput.interactable = false;
            }
            finally
            {
                Toggle_ShowClassificationOutput.isOn = true;
                Toggle Toggle_ClsAnnotationTab = GameObject.Find("Toggle_ClsAnnotationTab").GetComponent<Toggle>();
                Toggle_ClsAnnotationTab.isOn = false;
                Toggle_ClsAnnotationTab.interactable = false;
            }
        }
    }

    public void OnClick_Button_LoadClassificationOutput()
    {
        if (string.IsNullOrEmpty(ArrythmiaFliePath))
        {
            MessageBox.DisplayMessageBox("��ʾ", "����������������ļ���", true, null);
            return;
        }

        string filepath = OpenFileDialog.GetFilePath("TXT", "ѡ��ģ�ͷ�������ļ�");
        if (filepath != null)
        {
            ClsOutputContainer.Clear();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string[] arr = line.Trim().Split(',');
                    if (arr.Length != 4)
                    {
                        throw new Exception("ģ�ͷ�������ļ�һ�в����ĸ�Ԫ��");
                    }
                    int timeIndex = int.Parse(arr[0]);
                    int rPeakTimeIndex = int.Parse(arr[1]);
                    string arrythmia = arr[2];
                    if (!string.IsNullOrEmpty(arrythmia))
                    {
                        ClsOutputContainer.AddClsOutput(new ClassificationOutputData(timeIndex, rPeakTimeIndex, arrythmia));
                    }
                }
                ClsOutputContainer.InitCurClsOutputList();
                ClassificationOutputPath = filepath;
                InputField_ClassificationOutputPath.text = filepath;
                Toggle_ShowClassificationOutput.interactable = true;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e); 
                ClassificationOutputPath = null;
                ClsOutputContainer.Clear();
                InputField_ClassificationOutputPath.text = "";
                Toggle_ShowClassificationOutput.interactable = false;
            }
            finally
            {
                Toggle_ShowClassificationOutput.isOn = true;
                Toggle Toggle_ClsAnnotationTab = GameObject.Find("Toggle_ClsAnnotationTab").GetComponent<Toggle>();
                Toggle_ClsAnnotationTab.isOn = false;
                Toggle_ClsAnnotationTab.interactable = false;
            }
        }
    }

    public void OnClick_Button_LoadClassificationLabel()
    {
        string filepath = OpenFileDialog.GetFilePath("TXT", "ѡ������ǩ�ļ�");
        if (filepath != null)
        {
            ClsLabelContainer.Clear();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    string[] arr = line.Trim().Split(',');
                    if (arr.Length != 2)
                    {
                        throw new Exception("ģ�ͷ�������ļ�һ�в�������Ԫ��");
                    }
                    int timeIndex = int.Parse(arr[0]);
                    string arrythmia = arr[1];
                    ClsLabelContainer.AddClsLabel(timeIndex, arrythmia);
                }
                ClsLabelContainer.InitCurClsLabelList();
                ClassificationLabelPath = filepath;
                InputField_ClassificationLabelPath.text = filepath;
                Toggle_ShowClassificationLabel.interactable = true;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e);
                ClassificationLabelPath = null;
                ClsLabelContainer.Clear();
                InputField_ClassificationLabelPath.text = "";
                Toggle_ShowClassificationLabel.interactable = false;
            }
            finally
            {
                Toggle_ShowClassificationLabel.isOn = true;
                Toggle Toggle_ClsAnnotationTab = GameObject.Find("Toggle_ClsAnnotationTab").GetComponent<Toggle>();
                Toggle_ClsAnnotationTab.isOn = false;
                Toggle_ClsAnnotationTab.interactable = false;
            }
        }
    }

    public void OnEndEdit_InputField_MainChartStHour()
    {
        string str = InputField_MainChartStHour.text;
        try
        {
            int val = Convert.ToInt32(str);
            if (val >= 0)
            {
                int targetStTime_s = TransHMSToSecond(val, Convert.ToInt32(InputField_MainChartStMinute.text), Convert.ToInt32(InputField_MainChartStSecond.text));
                if (targetStTime_s + k_MainChartPeriod_s <= ECGData.Count / ECGDataFs)
                {
                    MainChartStTime_s = targetStTime_s;
                }
            }
        }
        finally
        {
            UpdateMainChartStEdTime();
        }
    }

    public void OnEndEdit_InputField_MainChartStMinute()
    {
        string str = InputField_MainChartStMinute.text;
        try
        {
            int val = Convert.ToInt32(str);
            if (val >= 0 && val < 60)
            {
                int targetStTime_s = TransHMSToSecond(Convert.ToInt32(InputField_MainChartStHour.text), val, Convert.ToInt32(InputField_MainChartStSecond.text));
                if (targetStTime_s + k_MainChartPeriod_s <= ECGData.Count / ECGDataFs)
                {
                    MainChartStTime_s = targetStTime_s;
                }
            }
        }
        finally
        {
            UpdateMainChartStEdTime();
        }
    }

    public void OnEndEdit_InputField_MainChartStSecond()
    {
        string str = InputField_MainChartStSecond.text;
        try
        {
            int val = Convert.ToInt32(str);
            if (val >= 0 && val < 60)
            {
                int targetStTime_s = TransHMSToSecond(Convert.ToInt32(InputField_MainChartStHour.text), Convert.ToInt32(InputField_MainChartStMinute.text), val);
                if (targetStTime_s + k_MainChartPeriod_s <= ECGData.Count / ECGDataFs)
                {
                    MainChartStTime_s = targetStTime_s;
                }
            }
        }
        finally
        {
            UpdateMainChartStEdTime();
        }
    }

    public void OnValueChanged_Slider_MainChartTime()
    {
        MainChartStTime_s = (int)Math.Round(Slider_MainChartTime.value / Slider_MainChartTime.maxValue * ECGData.Count / ECGDataFs);
        MainChartStTime_s = Math.Min(MainChartStTime_s, (int)(ECGData.Count / ECGDataFs) - k_MainChartPeriod_s);
        UpdateMainChartStEdTime();
    }

    public void OnClick_Button_MainChartMoveForward()
    {
        MainChartStTime_s += k_MainChartMovePeriod_s;
        MainChartStTime_s = Math.Min(MainChartStTime_s, (int)(ECGData.Count / ECGDataFs) - k_MainChartPeriod_s);
        UpdateMainChartStEdTime(false);
    }

    public void OnClick_Button_MainChartFastMoveForward()
    {
        MainChartStTime_s += k_MainChartFastMovePeriod_s;
        MainChartStTime_s = Math.Min(MainChartStTime_s, (int)(ECGData.Count / ECGDataFs) - k_MainChartFastMovePeriod_s);
        UpdateMainChartStEdTime();
    }

    public void OnClick_Button_MainChartMoveBackward()
    {
        MainChartStTime_s -= k_MainChartMovePeriod_s;
        MainChartStTime_s = Math.Max(MainChartStTime_s, 0);
        UpdateMainChartStEdTime();
    }

    public void OnClick_Button_MainChartFastMoveBackward()
    {
        MainChartStTime_s -= k_MainChartFastMovePeriod_s;
        MainChartStTime_s = Math.Max(MainChartStTime_s, 0);
        UpdateMainChartStEdTime();
    }

    public void UpdateMainChartStEdTime(bool updateSlider = true)
    {
        TransSecondToHMS(MainChartStTime_s, out int hh, out int mm, out int ss);
        InputField_MainChartStHour.text = String.Format("{0:00}", hh);
        InputField_MainChartStMinute.text = String.Format("{0:00}", mm);
        InputField_MainChartStSecond.text = String.Format("{0:00}", ss);
        TransSecondToHMS(MainChartStTime_s + k_MainChartPeriod_s, out hh, out mm, out ss);
        InputField_MainChartEdHour.text = String.Format("{0:00}", hh);
        InputField_MainChartEdMinute.text = String.Format("{0:00}", mm);
        InputField_MainChartEdSecond.text = String.Format("{0:00}", ss);
        if (ECGData.Count > 0 && ECGDataFs > 0 && updateSlider)
        {
            Slider_MainChartTime.value = MainChartStTime_s * (float)ECGDataFs / ECGData.Count;
        }
    }

    public void OnEndEdit_InputField_ECGDataFs()
    {
        string fsstr = InputField_ECGDataFs.text;
        try
        {
            double fs = Convert.ToDouble(fsstr);
            if (fs > 0.01)
            {
                ECGDataFs = fs;
            }
        }
        finally
        {
            InputField_ECGDataFs.text = ECGDataFs == 0.0 ? "" : String.Format("{0:G}", ECGDataFs);
            if (ECGDataFs >= 0.01 && ECGData.Count > 0)
            {
                TransSecondToHMS((int)(ECGData.Count / ECGDataFs), out int maxh, out int maxm, out int maxs);
                Text_ECGDataLength.text = String.Format("��ʱ����{0:00}h{1:00}m{2:00}s", maxh, maxm, maxs);
            }
            else
            {
                Text_ECGDataLength.text = "��ʱ����";
            }
        }
    }

    public void OnEndEdit_InputField_AnnotationFs()
    {
        string fsstr = InputField_AnnotationFs.text;
        try
        {
            double fs = Convert.ToDouble(fsstr);
            if (fs > 0.01)
            {
                AnnotationFs = fs;
            }
        }
        finally
        {
            InputField_ECGDataFs.text = ECGDataFs == 0.0 ? "" : String.Format("{0:G}", ECGDataFs);
        }
    }

    

    private void Awake()
    {
        ClsOutputContainer = new ClassificationOutputContainer(ArrythmiaDict);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (ECGData.Count > 0 && ECGDataFs > 0.01)
            {
                MainChartStTime_s += k_MainChartMovePeriod_s;
                MainChartStTime_s = Math.Min(MainChartStTime_s, (int)(ECGData.Count / ECGDataFs) - k_MainChartPeriod_s);
                UpdateMainChartStEdTime();
                mainChart.NeedUpdate();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (ECGData.Count > 0 && ECGDataFs > 0.01)
            {
                MainChartStTime_s -= k_MainChartMovePeriod_s;
                MainChartStTime_s = Math.Max(MainChartStTime_s, 0);
                UpdateMainChartStEdTime();
                mainChart.NeedUpdate();
            }
        }
    }

    public static void TransSecondToHMS(in int time_s, out int hh, out int mm, out int ss)
    {
        hh = time_s / 3600;
        mm = time_s / 60 % 60;
        ss = time_s % 60;
    }

    // ������Ϊ��λ�ĸ�������ת��Ϊ����00:00:00.00��ʽ���ַ���
    public static string TransSecondToHMSp2(double time_s)
    {
        int time_s_int = (int)time_s;
        TransSecondToHMS(time_s_int, out int hh, out int mm, out int ss);
        return String.Format("{0:00}:{1:00}:{2:00}.{3:00}", hh, mm, ss, (int)Math.Round((time_s - time_s_int) * 100));
    }

    public static int TransHMSToSecond(in int hh, in int mm, in int ss)
    {
        return hh * 3600 + mm * 60 + ss;
    }

    public bool IsValidPlotMainChart()
    {
        return ECGData.Count > 0 && ECGDataFs > 0;
    }

    public bool IsValidPlotRPeak()
    {
        return AnnotationFs > 0 && RPeakList.Count > 0 && Toggle_ShowRPeakAnnotation.isOn;
    }

    public bool IsValidPlotSegAnnotation()
    {
        return AnnotationFs > 0 && !string.IsNullOrEmpty(SegFilePath) && Toggle_ShowSegAnnotation.isOn;
    }

    // TODO
    public bool IsValidPlotClsOutput()
    {
        return AnnotationFs > 0 && !string.IsNullOrEmpty(ArrythmiaFliePath) && !string.IsNullOrEmpty(ClassificationOutputPath) && Toggle_ShowClassificationOutput.isOn;
    }

    public bool IsValidPlotClsLabel()
    {
        return AnnotationFs > 0 && !string.IsNullOrEmpty(ClassificationLabelPath) && Toggle_ShowClassificationLabel.isOn;
    }

    public int GetLowerBoundIndexOfSegCommentList(int timeIndex)
    {
        return SegCommentList.BinarySearch(new SegCommentData(timeIndex, SegCommentType.Other));
    }

    public void SetMainChartFocusTimeIndex(int timeIndex, int? mainChartStTime_s = null)
    {
        MainChartFocusTimeIndex = timeIndex;
        MainChartFocusLeftTime = k_MainChartDefaultFocusLeftTime_s;
        if (mainChartStTime_s != null)
        {
            MainChartStTime_s = (int)mainChartStTime_s;
            UpdateMainChartStEdTime();
        }
    }

    public void ReSetMainChartFocusTimeIndex()
    {
        MainChartFocusTimeIndex = -1;
        MainChartFocusLeftTime = 0;
    }

    // �ж�ʱ���Ƿ񳬳���ͼ��ʾʱ��
    public bool IsInMainChartScope(float time)
    {
        if (!IsValidPlotMainChart())
        {
            return false;
        }
        return time >= MainChartStTime_s && time < MainChartStTime_s + k_MainChartPeriod_s;
    }
}


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileDlg
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public class OpenFileDialog
{
    // ����ָ��ϵͳ����       ���ļ��Ի���
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileDlg ofd);

    // ����ָ��ϵͳ����        ���Ϊ�Ի���
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileDlg ofn);

    // ����Դ�������Ի��򣬷���ѡ���ļ�·������δ����ѡ�񣬷���null
    // multiFile �Ƿ�ɴ򿪶���ļ�
    static public string GetFilePath(string type, string title, bool multiFile = false)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);

        pth.filter = "�ļ�(*." + type + ")\0*." + type + "\0";//ɸѡ�ļ�����
        //pth.filter = "ͼƬ�ļ�(*.jpg*.png)\0*.jpg;*.png";
        pth.file = new string(new char[1024]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[1024]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = "E:\\Work\\ECG_AI\\Long_term_ECG";  // default path
        if (!Directory.Exists(pth.initialDir))
        {
            pth.initialDir = Application.streamingAssetsPath.Replace('/', '\\');  // default path
        }
        pth.title = title;
        //pth.defExt = "TXT";//��ʾ�ļ�����
        pth.defExt = "";//��ʾ�ļ�����
        //pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        pth.flags = 0x00080000 | 0x00000800 | 0x00000008;
        if (multiFile)
        {
            pth.flags |= 0x00000200;
        }
        if (OpenFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file;//ѡ����ļ�·��;
            Directory.SetCurrentDirectory(originalDirectory);
            return filepath;
            //DirectoryInfo i = new DirectoryInfo(filepath);

            ////�ϼ�Ŀ¼
            //string path = i.Parent.FullName;//�����ļ����ϼ�Ŀ¼

            //ProjectData openprodata = new ProjectData();
            //openprodata.proname = Path.GetFileNameWithoutExtension(path);//����·�������һ���ļ�������
            
        }
        Directory.SetCurrentDirectory(originalDirectory);
        return null;
    }

    // ����Դ�������Ի��򣬷������ڱ�����ļ�·������δ����ѡ�񣬷���null
    static public string GetSavePath(string type, string dlgTitle, string initialDir, string initialFileName)
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = "�ļ�(*." + type + ")\0*." + type + "\0";//ɸѡ�ļ�����
        //pth.filter = "ͼƬ�ļ�(*.jpg*.png)\0*.jpg;*.png";
        pth.file = new string(new char[1024]);
        char[] initialFileNameCharArr = new char[1024];
        for (int i = 0; i < initialFileName.Length; i++)
        {
            initialFileNameCharArr[i] = initialFileName[i];
        }
        pth.file = new string(initialFileNameCharArr);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[1024]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = initialDir;
        pth.title = dlgTitle;
        //pth.defExt = "TXT";//��ʾ�ļ�����
        pth.defExt = "";//��ʾ�ļ�����
        // OFN_OVERWRITEPROMPT 0x00000002 �����ѡ�ļ��Ѵ��ڣ���ᵼ�¡� ���Ϊ ���Ի���������Ϣ�� �û�����ȷ���Ƿ񸲸��ļ���
        // OFN_PATHMUSTEXIST 0x00000800 �û�ֻ�ܼ�����Ч��·�����ļ����� ���ʹ�ô˱�־�������û��� ���ļ��� ����Ŀ�ֶ��м�����Ч��·�����ļ������Ի�����������Ϣ������ʾ���档
        pth.flags = 0x00000002 | 0x00000800;
        if (OpenFileDialog.GetSaveFileName(pth))
        {
            string filepath = pth.file;//ѡ����ļ�·��;
            Directory.SetCurrentDirectory(originalDirectory);
            return filepath;
            //DirectoryInfo i = new DirectoryInfo(filepath);
            ////�ϼ�Ŀ¼
            //string path = i.Parent.FullName;//�����ļ����ϼ�Ŀ¼

            //ProjectData openprodata = new ProjectData();
            //openprodata.proname = Path.GetFileNameWithoutExtension(path);//����·�������һ���ļ�������
            
        }
        Directory.SetCurrentDirectory(originalDirectory);
        return null;
    }
}

