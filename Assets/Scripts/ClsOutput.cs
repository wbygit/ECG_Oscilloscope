using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;

public class ClsOutput : MonoBehaviour
{
    const int k_MaxVirualListLength = 30;
    const int k_VirtualListLoad = 10; // 靠近边缘时一次加载多少物体
    const double k_VirtualEdgeRatio = 0.1; // 当当前位置距边缘的比例低于该值时触发列表重新加载

    public DataManager dataManager;
    public GameObject m_Content;
    public TMP_Text Text_ClsCurOutputListLength;
    public ScrollRect m_ScrollView;
    public TMP_Text Text_ClsCommentSaveStatus;
    public TMP_Dropdown Dropdown_ClsOutputCurArrythmia;
    public Toggle Toggle_ClsOutputOnlyMarked;
    public TMP_InputField InputField_SelectClsOutputIndex;
    GameObject m_ItemPrefab;

    public int selectedTimeIndex = -1; // -1表示未选中，否则用采样位置标记
    public int startTimeIndex = -1; // 虚拟列表中第一个元素的采样位置

    public void OnClick_Button_LoadClsComment()
    {
        string filepath = OpenFileDialog.GetFilePath("JSON", "选择分类输出复核文件");
        if (filepath != null)
        {
            try
            {
                string jsonStr = System.IO.File.ReadAllText(filepath);
                SortedDictionary<int, ClsCommentData> clsCommentDict = JsonConvert.DeserializeObject<SortedDictionary<int, ClsCommentData>>(jsonStr);
                dataManager.ClsOutputContainer.ClearCurOutputList();
                dataManager.ClsOutputContainer.clsCommentDict = clsCommentDict;
                SetClsCommentSavedFlag(true);
            }
            catch (Exception e)
            {
                dataManager.ClsOutputContainer.ClearCurOutputList();
                dataManager.ClsOutputContainer.ClearClsCommentDict();
                MessageBox.DisplayMessageBox("错误", "文件读取出错。", true, null);
                Debug.LogError(e);
            }
            finally
            {
                selectedTimeIndex = -1;
                startTimeIndex = -1;
                dataManager.ClsOutputContainer.InitCurClsOutputList();
                OnEnable();
            }
        }
    }
    public void OnClick_Button_SaveClsComment()
    {
        string defaultFileName = string.Format("{0}-ClsComment.json", System.IO.Path.GetFileNameWithoutExtension(dataManager.ClassificationOutputPath));
        string filepath = OpenFileDialog.GetSavePath("JSON", "保存分类输出复核文件", System.IO.Path.GetDirectoryName(dataManager.ClassificationOutputPath), defaultFileName);
        if (filepath != null)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(dataManager.ClsOutputContainer.clsCommentDict);
                System.IO.File.WriteAllText(filepath, jsonStr);
                SetClsCommentSavedFlag(true);
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("错误", "文件保存出错。", true, null);
                Debug.LogError(e);
            }
        }
    }

    public void OnClick_Button_ClearClsComment()
    {
        MessageBox.DisplayMessageBox("是否清空分类输出复核项", "正在编辑的所有复核项将被清空，不会影响本地文件。", dismissable: true, dissmissText: "取消"
            , confirm: true, confirmText: "确认", confirmButton:
        () =>
        {
            dataManager.ClsOutputContainer.ClearClsCommentDict();
            dataManager.ClsOutputContainer.ClearCurOutputList();
            dataManager.ClsOutputContainer.InitCurClsOutputList();
            selectedTimeIndex = -1;
            startTimeIndex = -1;
            OnEnable();
            SetClsCommentSavedFlag(false);
        });
    }

    public void OnClick_Button_SelectClsOutputIndex()
    {
        int index = -1;
        try
        {
            index = int.Parse(InputField_SelectClsOutputIndex.text) - 1;
            if (index >= dataManager.ClsOutputContainer.curClsOutputList.Count || index < 0)
            {
                throw new Exception(string.Format("无法跳转，超出范围。序号：{0}", index + 1));
            }
            selectedTimeIndex = dataManager.ClsOutputContainer.curClsOutputList.Keys[index];
            startTimeIndex = selectedTimeIndex;
            startTimeIndex = ClampStartTimeIndex();
            m_ScrollView.verticalNormalizedPosition = GetScrollPositionFocusSelected();
            UpdateWindow();
            LocateSelectedClsOutputOnChart();
        }
        catch (Exception e)
        {
            MessageBox.DisplayMessageBox("跳转失败", "目标序号为空或不在范围内");
            Debug.LogError(e);
        }
    }

    public void OnValueChanged_Dropdown_ClsOutputCurArrythmia()
    {
        dataManager.ClsOutputContainer.InitCurClsOutputList(dataManager.ArrythmiaDict.GetArrythmia(Dropdown_ClsOutputCurArrythmia.value), dataManager.ClsOutputContainer.curOnlyMarked);
        selectedTimeIndex = -1;
        startTimeIndex = -1;
        UpdateWindow();
        UpdateLength();
    }

    public void OnValueChanged_Toggle_ClsOutputOnlyMarked()
    {
        dataManager.ClsOutputContainer.InitCurClsOutputList(dataManager.ClsOutputContainer.curArrythmia, Toggle_ClsOutputOnlyMarked.isOn);
        selectedTimeIndex = -1;
        startTimeIndex = -1;
        UpdateWindow();
        UpdateLength();
    }

    public void OnValueChanged_ScrollView(Vector2 position)
    {
        var curClsOutputList = dataManager.ClsOutputContainer.curClsOutputList;
        if (curClsOutputList.Count == 0)
        {
            return;
        }
        int curStartTimeIndex = startTimeIndex;
        int curStartIndex = curClsOutputList.IndexOfKey(startTimeIndex);
        int startIndex = curStartIndex;
        if (position.y > 1 - k_VirtualEdgeRatio)
        {
            startIndex = Math.Max(0, startIndex - k_VirtualListLoad);
        }
        else if (position.y < k_VirtualEdgeRatio)
        {
            startIndex = Math.Min(startIndex + k_MaxVirualListLength + k_VirtualListLoad, curClsOutputList.Count) - k_MaxVirualListLength;
            startIndex = Math.Max(0, startIndex);
        }
        startTimeIndex = curClsOutputList.Keys[startIndex];
        if (curStartTimeIndex != startTimeIndex)
        {
            float y = position.y + (float)(startIndex - curStartIndex) / k_MaxVirualListLength;
            y = Math.Clamp(y, 0f, 1f);
            m_ScrollView.verticalNormalizedPosition = y;
            UpdateWindow();
        }
    }
    public void UpdateWindow()
    {
        int curSelectedTimeIndex = selectedTimeIndex;
        bool setSelectedTimeIndex = false;
        Queue<GameObject> itemQueue = new Queue<GameObject>();
        while (m_Content.transform.childCount > 0)
        {
            Transform childTransform = m_Content.transform.GetChild(0);
            childTransform.SetParent(null);
            childTransform.gameObject.GetComponent<Toggle>().SetIsOnWithoutNotify(false);
            itemQueue.Enqueue(childTransform.gameObject);
        }
        var curClsOutputList = dataManager.ClsOutputContainer.curClsOutputList;
        if (curClsOutputList.Count > 0)
        {
            int startIndex = 0;
            if (startTimeIndex == -1)
            {
                startTimeIndex = curClsOutputList.Keys[0];
                m_ScrollView.verticalNormalizedPosition = 1f;
            }
            else if (!curClsOutputList.ContainsKey(startTimeIndex))
            {
                startIndex = dataManager.ClsOutputContainer.GetLowerBoundIndexOfCurClsOutputList(startTimeIndex);
                m_ScrollView.verticalNormalizedPosition = 1f;
                ClampStartTimeIndex();
            }
            else
            {
                startIndex = curClsOutputList.IndexOfKey(startTimeIndex);
            }
            int endIndex = Math.Min(startIndex + k_MaxVirualListLength, curClsOutputList.Count);
            for (int i = startIndex; i < endIndex; i++)
            {
                GameObject item;
                if (itemQueue.Count > 0)
                {
                    item = itemQueue.Dequeue();
                    item.transform.SetParent(m_Content.transform);
                }
                else
                {
                    item = GameObject.Instantiate(m_ItemPrefab, m_Content.transform);
                }
                item.GetComponent<ClsOutputItem>().SetByData(i + 1, curClsOutputList.Values[i]);
                if (curClsOutputList.Keys[i] == curSelectedTimeIndex)
                {
                    item.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
                    //item.GetComponent<Toggle>().isOn = true;
                    setSelectedTimeIndex = true;
                }
            }
        }
        if (!setSelectedTimeIndex)
        {
            selectedTimeIndex = -1;
        }

        while (itemQueue.Count > 0)
        {
            GameObject item = itemQueue.Dequeue();
            GameObject.Destroy(item);
        }
        UpdateContentSize();
        OnValueChanged_ScrollView(m_ScrollView.normalizedPosition);
    }

    public void UpdateLength()
    {
        Text_ClsCurOutputListLength.text = string.Format("数量：{0}", dataManager.ClsOutputContainer.curClsOutputList.Count);
    }

    public void SetClsCommentSavedFlag(bool flag)
    {
        dataManager.ClsCommentSavedFlag = flag;
        if (flag)
        {
            Text_ClsCommentSaveStatus.text = "";
            //Text_SegCommentSaveStatus.color = Color.black;
        }
        else
        {
            Text_ClsCommentSaveStatus.text = "有修改，请及时保存 ";
            Text_ClsCommentSaveStatus.color = Color.red;
        }
    }

    public void LocateSelectedClsOutputOnChart()
    {
        if (selectedTimeIndex != -1)
        {
            int newStTime = (int)(selectedTimeIndex / dataManager.AnnotationFs - 0.5 * DataManager.k_MainChartPeriod_s);
            newStTime = Math.Clamp(newStTime, 0, (int)(dataManager.ECGData.Count / dataManager.ECGDataFs) - DataManager.k_MainChartPeriod_s);
            dataManager.SetMainChartFocusTimeIndex(selectedTimeIndex, newStTime);
            dataManager.mainChart.NeedUpdate();
        }
    }

    private void OnEnable()
    {
        if (dataManager.IsValidPlotClsOutput())
        {
            Tool.InitArrythmiaDropdown(Dropdown_ClsOutputCurArrythmia, dataManager.ArrythmiaDict, dataManager.ClsOutputContainer.curArrythmia);
            Toggle_ClsOutputOnlyMarked.SetIsOnWithoutNotify(dataManager.ClsOutputContainer.curOnlyMarked);
            UpdateWindow();
            UpdateLength();
        }
    }

    private void OnDisable()
    {   
        ClearItem();
    }

    void Awake()
    {
        m_ItemPrefab = Resources.Load<GameObject>("Toggle_ClsOutputItem");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ClearItem()
    {
        while (m_Content.transform.childCount > 0)
        {
            var curItem = m_Content.transform.GetChild(0).gameObject;
            curItem.transform.SetParent(null);
            GameObject.Destroy(curItem);
        }
        selectedTimeIndex = -1;
        startTimeIndex = -1;
    }

    private void UpdateContentSize()
    {
        m_Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1, m_Content.transform.childCount) * m_ItemPrefab.GetComponent<RectTransform>().rect.height);
    }
    private int ClampStartTimeIndex()
    {
        if (startTimeIndex == -1)
        {
            return -1;
        }
        int startIndex = dataManager.ClsOutputContainer.GetLowerBoundIndexOfCurClsOutputList(startTimeIndex);
        int maxx = Math.Max(0, dataManager.ClsOutputContainer.curClsOutputList.Count - k_MaxVirualListLength);
        startIndex = Math.Clamp(startIndex, 0, maxx);
        return dataManager.ClsOutputContainer.curClsOutputList.Keys[startIndex];
    }

    private float GetScrollPositionFocusSelected()
    {
        var curClsOutputList = dataManager.ClsOutputContainer.curClsOutputList;
        int startIndex = curClsOutputList.IndexOfKey(startTimeIndex);
        int selectedIndex = curClsOutputList.IndexOfKey(selectedTimeIndex);
        return 1f - (float)(selectedIndex - startIndex) / Math.Min(k_MaxVirualListLength, curClsOutputList.Count);
    }
}
