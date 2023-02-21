using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ClsLabel : MonoBehaviour
{
    const int k_MaxVirualListLength = 30;
    const int k_VirtualListLoad = 10; // 靠近边缘时一次加载多少物体
    const double k_VirtualEdgeRatio = 0.1; // 当当前位置距边缘的比例低于该值时触发列表重新加载

    public DataManager dataManager;
    public GameObject m_Content;
    public TMP_Text Text_ClsCurLabelListLength;
    public ScrollRect m_ScrollView;
    public TMP_Dropdown Dropdown_ClsLabelCurArrythmia;
    public TMP_InputField InputField_SelectClsLabelIndex;
    GameObject m_ItemPrefab;

    public int selectedTimeIndex = -1; // -1表示未选中，否则用采样位置标记
    public int startTimeIndex = -1; // 虚拟列表中第一个元素的采样位置

    public void OnClick_Button_SelectClsLabelIndex()
    {
        int index = -1;
        try
        {
            index = int.Parse(InputField_SelectClsLabelIndex.text) - 1;
            if (index >= dataManager.ClsLabelContainer.curClsLabelList.Count || index < 0)
            {
                throw new Exception(string.Format("无法跳转，超出范围。序号：{0}", index + 1));
            }
            selectedTimeIndex = dataManager.ClsLabelContainer.curClsLabelList.Keys[index];
            startTimeIndex = selectedTimeIndex;
            startTimeIndex = ClampStartTimeIndex();
            m_ScrollView.verticalNormalizedPosition = GetScrollPositionFocusSelected();
            UpdateWindow();
            LocateSelectedClsLabelOnChart();
        }
        catch (Exception e)
        {
            MessageBox.DisplayMessageBox("跳转失败", "目标序号为空或不在范围内");
            Debug.LogError(e);
        }
    }

    public void OnValueChanged_Dropdown_ClsLabelCurArrythmia()
    {
        dataManager.ClsLabelContainer.InitCurClsLabelList(dataManager.ClsLabelContainer.arrythmiaDict.GetArrythmia(Dropdown_ClsLabelCurArrythmia.value));
        selectedTimeIndex = -1;
        startTimeIndex = -1;
        UpdateWindow();
        UpdateLength();
    }

    public void OnValueChanged_ScrollView(Vector2 position)
    {
        var curClsLabelList = dataManager.ClsLabelContainer.curClsLabelList;
        if (curClsLabelList.Count == 0)
        {
            return;
        }
        int curStartTimeIndex = startTimeIndex;
        int curStartIndex = curClsLabelList.IndexOfKey(startTimeIndex);
        int startIndex = curStartIndex;
        if (position.y > 1 - k_VirtualEdgeRatio)
        {
            startIndex = Math.Max(0, startIndex - k_VirtualListLoad);
        }
        else if (position.y < k_VirtualEdgeRatio)
        {
            startIndex = Math.Min(startIndex + k_MaxVirualListLength + k_VirtualListLoad, curClsLabelList.Count) - k_MaxVirualListLength;
            startIndex = Math.Max(0, startIndex);
        }
        startTimeIndex = curClsLabelList.Keys[startIndex];
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
        var curClsLabelList = dataManager.ClsLabelContainer.curClsLabelList;
        if (curClsLabelList.Count > 0)
        {
            int startIndex = 0;
            if (startTimeIndex == -1)
            {
                startTimeIndex = curClsLabelList.Keys[0];
                m_ScrollView.verticalNormalizedPosition = 1f;
            }
            else if (!curClsLabelList.ContainsKey(startTimeIndex))
            {
                startIndex = dataManager.ClsLabelContainer.GetLowerBoundIndexOfCurClsLabelList(startTimeIndex);
                m_ScrollView.verticalNormalizedPosition = 1f;
                ClampStartTimeIndex();
            }
            else
            {
                startIndex = curClsLabelList.IndexOfKey(startTimeIndex);
            }
            int endIndex = Math.Min(startIndex + k_MaxVirualListLength, curClsLabelList.Count);
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
                item.GetComponent<ClsLabelItem>().SetByData(i + 1, curClsLabelList.Keys[i], curClsLabelList.Values[i]);
                if (curClsLabelList.Keys[i] == curSelectedTimeIndex)
                {
                    item.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
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
        Text_ClsCurLabelListLength.text = string.Format("数量：{0}", dataManager.ClsLabelContainer.curClsLabelList.Count);
    }

    public void LocateSelectedClsLabelOnChart()
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
        if (dataManager.IsValidPlotClsLabel())
        {
            Tool.InitArrythmiaDropdown(Dropdown_ClsLabelCurArrythmia, dataManager.ClsLabelContainer.arrythmiaDict, dataManager.ClsLabelContainer.curArrythmia);
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
        m_ItemPrefab = Resources.Load<GameObject>("Toggle_ClsLabelItem");
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
        int startIndex = dataManager.ClsLabelContainer.GetLowerBoundIndexOfCurClsLabelList(startTimeIndex);
        int maxx = Math.Max(0, dataManager.ClsLabelContainer.curClsLabelList.Count - k_MaxVirualListLength);
        startIndex = Math.Clamp(startIndex, 0, maxx);
        return dataManager.ClsLabelContainer.curClsLabelList.Keys[startIndex];
    }

    private float GetScrollPositionFocusSelected()
    {
        var curClsLabelList = dataManager.ClsLabelContainer.curClsLabelList;
        int startIndex = curClsLabelList.IndexOfKey(startTimeIndex);
        int selectedIndex = curClsLabelList.IndexOfKey(selectedTimeIndex);
        return 1f - (float)(selectedIndex - startIndex) / Math.Min(k_MaxVirualListLength, curClsLabelList.Count);
    }

}
