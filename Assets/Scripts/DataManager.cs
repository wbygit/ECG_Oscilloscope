using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    public const int k_MainChartPeriod_s = 10; // 主图显示多少秒的心电图
    public const int k_MainChartMovePeriod_s = 5; // 左右移动多少秒的心电图
    public const int k_MainChartFastMovePeriod_s = 60; // 左右快速移动多少秒的心电图

    public string ECGFilePath; // 若filepath为null，则意味着数据未读取，不进行绘图
    public TMP_InputField InputField_ECGDataFilePath;
    public TMP_Text Text_ECGDataLength;
    public List<double> ECGData = new List<double>();

    public double ECGDataFs = 0; // 若采样率为零，则意味着该项未设置，不进行绘图
    public TMP_InputField InputField_ECGDataFs;

    public string RPeakFilePath;
    public TMP_InputField InputField_RPeakFilePath;
    public Toggle Toggle_ShowRPeakAnnotation;
    public List<int> RPeakList = new List<int>(); // R波波峰序列，以AnnotationFs采样率为准的采样点下标

    public string SegFilePath;
    public TMP_InputField InputField_SegFilePath;
    public Toggle Toggle_ShowSegAnnotation;
    public List<int> ROnList = new List<int>();
    public List<int> ROffList = new List<int>();
    public List<int> TOnList = new List<int>();
    public List<int> TOffList = new List<int>();
    public List<int> POnList = new List<int>();
    public List<int> POffList = new List<int>();

    public double AnnotationFs = 0; // 若采样率为零，则意味着该项未设置，不绘制标记
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


    public void OnClick_Button_LoadECGData()
    {
        string filepath = OpenFileDialog.GetFilePath("TXT", "选择心电数据文件");
        if (filepath != null)
        {
            ECGData.Clear();
            try
            {
                //MessageBox.DisplayMessageBox("载入心电数据", "读取文件中，请稍等。", false, null);
                string[] lines = System.IO.File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    string[] arr = line.Split(",");
                    foreach (string vstr in arr)
                    {
                        if (!String.IsNullOrWhiteSpace(vstr))
                        {
                            ECGData.Add(Convert.ToDouble(vstr));
                        }
                    }
                }
                ECGFilePath = filepath;
                InputField_ECGDataFilePath.text = filepath;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("错误", "文件读取出错。", true, null);
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
                    Text_ECGDataLength.text = String.Format("总时长：{0:00}h{1:00}m{:00}s", maxh, maxm, maxs);
                }
                else
                {
                    Text_ECGDataLength.text = "总时长：";
                }
            }
        }
    }

    public void OnClick_Button_LoadRPeakAnnotation()
    {
        string filepath = OpenFileDialog.GetFilePath("TXT", "选择R波波峰标记文件");
        if (filepath != null)
        {
            RPeakList.Clear();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    string[] arr = line.Split(",");
                    for (int i = 1; i < arr.Length; i++) // 跳过第一个，应该是文件名
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
                MessageBox.DisplayMessageBox("错误", "文件读取出错。", true, null);
                Debug.LogError(e);
                RPeakFilePath = null;
                RPeakList.Clear();
                InputField_RPeakFilePath.text = "";
                Toggle_ShowRPeakAnnotation.interactable = false;
            }
            finally
            {
                Toggle_ShowRPeakAnnotation.isOn = true;
            }
        }
    }

    public void OnClick_Button_LoadSegAnnotation()
    {
        string filepath = OpenFileDialog.GetFilePath("JSON", "选择分割标记文件");
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
                    throw new Exception("分割标签On和Off数量不一致");
                }
                if ((ROnList.Count > 0 && (ROnList[0] < 0 || ROffList[0] < 0)) || (TOnList.Count > 0 && (TOnList[0] < 0 || TOffList[0] < 0)) || (POnList.Count > 0 && (POnList[0] < 0 || POffList[0] < 0)))
                {
                    throw new Exception("分割标签有负值");
                }
                ROnList.Sort();
                ROffList.Sort();
                TOnList.Sort();
                TOffList.Sort();
                POnList.Sort();
                POffList.Sort();
                RPeakFilePath = filepath;
                InputField_SegFilePath.text = filepath;
                Toggle_ShowSegAnnotation.interactable = true;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("错误", "文件读取出错。", true, null);
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
        TransSecondToHMS(MainChartStTime_s+k_MainChartPeriod_s, out hh, out mm, out ss);
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
                Text_ECGDataLength.text = String.Format("总时长：{0:00}h{1:00}m{2:00}s", maxh, maxm, maxs);
            }
            else
            {
                Text_ECGDataLength.text = "总时长：";
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
        InputField_ECGDataFilePath = GameObject.Find("InputField_ECGDataFilePath").GetComponent<TMP_InputField>();
        InputField_ECGDataFs = GameObject.Find("InputField_ECGDataFs").GetComponent<TMP_InputField>();
        InputField_MainChartStHour = GameObject.Find("InputField_MainChartStHour").GetComponent<TMP_InputField>();
        InputField_MainChartStMinute = GameObject.Find("InputField_MainChartStMinute").GetComponent<TMP_InputField>();
        InputField_MainChartStSecond = GameObject.Find("InputField_MainChartStSecond").GetComponent<TMP_InputField>();
        InputField_MainChartEdHour = GameObject.Find("InputField_MainChartEdHour").GetComponent<TMP_InputField>();
        InputField_MainChartEdMinute = GameObject.Find("InputField_MainChartEdMinute").GetComponent<TMP_InputField>();
        InputField_MainChartEdSecond = GameObject.Find("InputField_MainChartEdSecond").GetComponent<TMP_InputField>();
        Text_ECGDataLength = GameObject.Find("Text_ECGDataLength").GetComponent<TMP_Text>();
        InputField_AnnotationFs = GameObject.Find("InputField_AnnotationFs").GetComponent<TMP_InputField>();
        InputField_RPeakFilePath = GameObject.Find("InputField_RPeakFilePath").GetComponent<TMP_InputField>();
        Toggle_ShowRPeakAnnotation = GameObject.Find("Toggle_ShowRPeakAnnotation").GetComponent<Toggle>();
        InputField_SegFilePath = GameObject.Find("InputField_SegFilePath").GetComponent<TMP_InputField>();
        Toggle_ShowSegAnnotation = GameObject.Find("Toggle_ShowSegAnnotation").GetComponent<Toggle>();
        Slider_MainChartTime = GameObject.Find("Slider_MainChartTime").GetComponent<Slider>();
        Button_MainChartMoveForward = GameObject.Find("Button_MainChartMoveForward").GetComponent<Button>();
        Button_MainChartFastMoveForward = GameObject.Find("Button_MainChartFastMoveForward").GetComponent<Button>();
        Button_MainChartMoveBackward = GameObject.Find("Button_MainChartMoveBackward").GetComponent<Button>();
        Button_MainChartFastMoveBackward = GameObject.Find("Button_MainChartFastMoveBackward").GetComponent<Button>();
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
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (ECGData.Count > 0 && ECGDataFs > 0.01)
            {
                MainChartStTime_s -= k_MainChartMovePeriod_s;
                MainChartStTime_s = Math.Max(MainChartStTime_s, 0);
                UpdateMainChartStEdTime();
            }
        }
    }

    public static void TransSecondToHMS(in int time_s, out int hh, out int mm, out int ss)
    {
        hh = time_s / 3600;
        mm = time_s / 60 % 60;
        ss = time_s % 60;
    }

    public static int TransHMSToSecond(in int hh, in int mm, in int ss)
    {
        return hh * 3600 + mm * 60 + ss;
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
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileDlg ofd);

    // 打开资源管理器对话框，返回选中文件路径，若未进行选择，返回null
    static public string GetFilePath(string type, string title)
    {

        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);

        pth.filter = "文件(*." + type + ")\0*." + type + "\0";//筛选文件类型
        //pth.filter = "图片文件(*.jpg*.png)\0*.jpg;*.png";
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
        //pth.defExt = "TXT";//显示文件类型
        pth.defExt = "";//显示文件类型
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (OpenFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file;//选择的文件路径;
            return filepath;
            //DirectoryInfo i = new DirectoryInfo(filepath);

            ////上级目录
            //string path = i.Parent.FullName;//返回文件的上级目录

            //ProjectData openprodata = new ProjectData();
            //openprodata.proname = Path.GetFileNameWithoutExtension(path);//返回路径的最后一个文件夹名称
        }
        return null;
    }
}

