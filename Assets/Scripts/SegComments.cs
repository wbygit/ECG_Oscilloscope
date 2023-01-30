using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegComments : MonoBehaviour
{
    const int k_MaxVirualListLength = 30;
    const int k_VirtualListLoad = 10; // ������Եʱһ�μ��ض�������
    const double k_VirtualEdgeRatio = 0.1; // ����ǰλ�þ��Ե�ı������ڸ�ֵʱ�����б����¼���

    public DataManager dataManager;
    public GameObject Content_SegComments;
    public TMP_Text Text_SegCommentListLength;
    public ScrollRect ScrollView_SegComments;
    public TMP_Text Text_SegCommentSaveStatus;
    public TMP_InputField InputField_SelectSegCommentIndex;
    GameObject segCommentItemPrefab;

    public int selectedTimeIndex = -1; // -1��ʾδѡ�У������ò���λ�ñ��
    public int startTimeIndex = -1; // �����б��е�һ��Ԫ�صĲ���λ��

    private void Awake()
    {
        segCommentItemPrefab = Resources.Load<GameObject>("Toggle_SegCommentItem");
    }

    private void OnEnable()
    {
        UpdateSegCommentsWindow();
        UpdateSegCommentListLength();
    }

    private void OnDisable()
    {
        ClearSegCommentItem();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick_Button_ClearSegComments()
    {
        MessageBox.DisplayMessageBox("�Ƿ���շָ����޸���", "���ڱ༭�����зָ��ǽ�����գ�����Ӱ�챾���ļ���", dismissable: true, dissmissText: "ȡ��"
            , confirm: true, confirmText: "ȷ��", confirmButton:
        () =>
        {
            dataManager.SegCommentList.Clear();
            SetSegCommentSavedFlag(false);
            UpdateSegCommentListLength();
            ClearSegCommentItem();
        });
        
    }

    public void OnClick_Button_DeleteSegComment()
    {
        if (selectedTimeIndex == -1)
        {
            return;
        }
        int selectedIndex = dataManager.GetLowerBoundIndexOfSegCommentList(selectedTimeIndex);
        dataManager.SegCommentList.RemoveAt(selectedIndex);
        if (dataManager.SegCommentList.Count > 0)
        {
            int startIndex = dataManager.GetLowerBoundIndexOfSegCommentList(startTimeIndex);
            if (startIndex < 0)
            {
                startIndex = ~startIndex;
            }
            startTimeIndex = dataManager.SegCommentList[startIndex].timeIndex;
        }
        else
        {
            startTimeIndex = -1;
        }
        selectedTimeIndex = -1;
        UpdateSegCommentsWindow();
        UpdateSegCommentListLength();
        SetSegCommentSavedFlag(false);
    }

    public void OnClick_Button_SaveSegComments()
    {
        string defaultFileName = string.Format("{0}-SegComment.json", System.IO.Path.GetFileNameWithoutExtension(dataManager.RPeakFilePath));
        string filepath = OpenFileDialog.GetSavePath("JSON", "����ָ����޸��ļ�", System.IO.Path.GetDirectoryName(dataManager.RPeakFilePath), defaultFileName);
        if (filepath != null)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(dataManager.SegCommentList);
                System.IO.File.WriteAllText(filepath, jsonStr);
                SetSegCommentSavedFlag(true);
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ��������", true, null);
                Debug.LogError(e);
            }
        }
    }

    public void OnClick_Button_LoadSegComments()
    {
        string filepath = OpenFileDialog.GetFilePath("JSON", "ѡ��ָ����޸��ļ�");
        if (filepath != null)
        {
            try
            {
                string jsonStr = System.IO.File.ReadAllText(filepath);
                List<SegCommentData> segCommentList = JsonConvert.DeserializeObject<List<SegCommentData>>(jsonStr);
                dataManager.SegCommentList = segCommentList;
                dataManager.SegCommentListSavedFlag = true;
                SetSegCommentSavedFlag(true);
                selectedTimeIndex = -1;
                startTimeIndex = -1;
                UpdateSegCommentsWindow();
                UpdateSegCommentListLength();
                ScrollView_SegComments.verticalNormalizedPosition = 1f;
            }
            catch (Exception e)
            {
                MessageBox.DisplayMessageBox("����", "�ļ���ȡ����", true, null);
                Debug.LogError(e);
            }
        }
    }

    public void OnClick_Button_LocateSegCommentOnChart()
    {
        if (selectedTimeIndex != -1)
        {
            int newStTime = (int)(selectedTimeIndex / dataManager.AnnotationFs - 0.5 * DataManager.k_MainChartPeriod_s);
            newStTime = Math.Clamp(newStTime, 0, (int)(dataManager.ECGData.Count / dataManager.ECGDataFs) - DataManager.k_MainChartPeriod_s);
            dataManager.SetMainChartFocusTimeIndex(selectedTimeIndex, newStTime);
        }
    }

    public void OnClick_Button_LocateSegCommentsByChart()
    {
        int segCommentIndex = dataManager.GetLowerBoundIndexOfSegCommentList((int)Math.Round(dataManager.MainChartStTime_s * dataManager.AnnotationFs));
        if (segCommentIndex < 0)
        {
            segCommentIndex = ~segCommentIndex;
        }
        if (segCommentIndex < dataManager.SegCommentList.Count && dataManager.IsInMainChartScope((float)(dataManager.SegCommentList[segCommentIndex].timeIndex / dataManager.AnnotationFs)))
        {
            selectedTimeIndex = dataManager.SegCommentList[segCommentIndex].timeIndex;
            startTimeIndex = selectedTimeIndex;
            startTimeIndex = ClampStartTimeIndex();
            //float newPosition = 1f;
            //if (dataManager.SegCommentList.Count > k_MaxVirualListLength && segCommentIndex >= dataManager.SegCommentList.Count - 2 * k_VirtualListLoad)
            //{
            //    newPosition = 0f;
            //}
            //ScrollView_SegComments.verticalNormalizedPosition = newPosition;
            ScrollView_SegComments.verticalNormalizedPosition = GetScrollPositionFocusSelected();
            UpdateSegCommentsWindow();
            dataManager.SetMainChartFocusTimeIndex(selectedTimeIndex);
        }
        else
        {
            MessageBox.DisplayMessageBox("�޷���ͼ�ж�λ�ָ��޸���", "��ǰ�ĵ�ͼ��ʾ�������޷ָ��޸���", dismissable: true);
        }
    }

    public void OnClick_Button_SelectSegCommentIndex()
    {
        int segCommentIndex = -1;
        try
        {
            segCommentIndex = int.Parse(InputField_SelectSegCommentIndex.text) - 1;
            if (segCommentIndex >= dataManager.SegCommentList.Count || segCommentIndex < 0)
            {
                throw new Exception("�ָ��޸����޷���ת��������Χ");
            }
            selectedTimeIndex = dataManager.SegCommentList[segCommentIndex].timeIndex;
            startTimeIndex = selectedTimeIndex;
            startTimeIndex = ClampStartTimeIndex();
            ScrollView_SegComments.verticalNormalizedPosition = GetScrollPositionFocusSelected();
            UpdateSegCommentsWindow();
            OnClick_Button_LocateSegCommentOnChart();
            //float newPosition = 1f;
            //if (dataManager.SegCommentList.Count > k_MaxVirualListLength && segCommentIndex >= dataManager.SegCommentList.Count - 2 * k_VirtualListLoad)
            //{
            //    newPosition = 0f;
            //}
            //ScrollView_SegComments.verticalNormalizedPosition = newPosition;
            
        }
        catch (Exception e)
        {
            MessageBox.DisplayMessageBox("�ָ��޸ı����תʧ��", "Ŀ�����Ϊ�ջ��ڷ�Χ��");
            Debug.LogError(e);
        }
    }

    public void OnValueChanged_ScrollView_SegComments(Vector2 position)
    {
        if (dataManager.SegCommentList.Count == 0)
        {
            return;
        }
        int curStartTimeIndex = startTimeIndex;
        int curStartIndex = dataManager.GetLowerBoundIndexOfSegCommentList(startTimeIndex);
        int startIndex = curStartIndex;
        //if (position.y < k_VirtualEdgeRatio)
        if (position.y > 1 - k_VirtualEdgeRatio)
        {
            startIndex = Math.Max(0, startIndex - k_VirtualListLoad);
        }
        //else if (position.y > 1 - k_VirtualEdgeRatio)
        else if (position.y < k_VirtualEdgeRatio)
        {
            startIndex = Math.Min(startIndex + k_MaxVirualListLength + k_VirtualListLoad, dataManager.SegCommentList.Count) - k_MaxVirualListLength;
            startIndex = Math.Max(0, startIndex);
        }
        startTimeIndex = dataManager.SegCommentList[startIndex].timeIndex;
        if (curStartTimeIndex != startTimeIndex)
        {
            float y = position.y + (float)(startIndex - curStartIndex) / k_MaxVirualListLength;
            y = Math.Clamp(y, 0f, 1f);
            ScrollView_SegComments.verticalNormalizedPosition = y;
            UpdateSegCommentsWindow();
        }
    }
    
    public void UpdateSegCommentsWindow()
     {
        int curSelectedTimeIndex = selectedTimeIndex;
        bool setSelectedTimeIndex = false;
        Queue<GameObject> itemQueue = new Queue<GameObject>();
        while (Content_SegComments.transform.childCount > 0)
        {
            Transform childTransform = Content_SegComments.transform.GetChild(0);
            childTransform.SetParent(null);
            childTransform.gameObject.GetComponent<Toggle>().isOn = false;
            itemQueue.Enqueue(childTransform.gameObject);
        }
        if (dataManager.SegCommentList.Count > 0)
        {
            int startIndex = 0;
            if (startTimeIndex == -1)
            {
                startTimeIndex = dataManager.SegCommentList[0].timeIndex;
            }
            else
            {
                startIndex = dataManager.GetLowerBoundIndexOfSegCommentList(startTimeIndex);
            }
            int endIndex = Math.Min(startIndex + k_MaxVirualListLength, dataManager.SegCommentList.Count);
            for (int i = startIndex; i < endIndex; i++)
            {
                GameObject item;
                if (itemQueue.Count > 0)
                {
                    item = itemQueue.Dequeue();
                    item.transform.SetParent(Content_SegComments.transform);
                }
                else
                {
                    item = GameObject.Instantiate(segCommentItemPrefab, Content_SegComments.transform);
                }
                item.GetComponent<SegCommentItem>().SetByData(i + 1, dataManager.SegCommentList[i]);
                if (dataManager.SegCommentList[i].timeIndex == curSelectedTimeIndex)
                {
                    item.GetComponent<Toggle>().isOn = true;
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
        OnValueChanged_ScrollView_SegComments(ScrollView_SegComments.normalizedPosition);
    }

    public void UpdateSegCommentListLength()
    {
        Text_SegCommentListLength.text = string.Format("����޸�������{0}", dataManager.SegCommentList.Count);
    }

    public void SetSegCommentSavedFlag(bool flag)
    {
        dataManager.SegCommentListSavedFlag = flag;
        if (flag)
        {
            Text_SegCommentSaveStatus.text = "";
            //Text_SegCommentSaveStatus.color = Color.black;
        }
        else
        {
            Text_SegCommentSaveStatus.text = "���޸ģ��뼰ʱ����";
            Text_SegCommentSaveStatus.color = Color.red;
        }
    }

    private void UpdateContentSize()
    {
        Content_SegComments.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1, Content_SegComments.transform.childCount) * segCommentItemPrefab.GetComponent<RectTransform>().rect.height);
    }

    // ������д����е�����gameobject
    private void ClearSegCommentItem()
    {
        while (Content_SegComments.transform.childCount > 0)
        {
            var curCommentItem = Content_SegComments.transform.GetChild(0).gameObject;
            curCommentItem.transform.SetParent(null);
            GameObject.Destroy(curCommentItem);
            //GameObject.DestroyImmediate(Content_SegComments.transform.GetChild(0).gameObject);
        }
        selectedTimeIndex = -1;
        startTimeIndex = -1;
    }

    private int ClampStartTimeIndex()
    {
        if (startTimeIndex == -1)
        {
            return -1;
        }
        int startIndex = dataManager.GetLowerBoundIndexOfSegCommentList(startTimeIndex);
        startIndex = Math.Clamp(startIndex, 0, dataManager.SegCommentList.Count - k_MaxVirualListLength);
        return dataManager.SegCommentList[startIndex].timeIndex;
    }

    private float GetScrollPositionFocusSelected()
    {
        int startIndex = dataManager.GetLowerBoundIndexOfSegCommentList(startTimeIndex);
        int selectedIndex = dataManager.GetLowerBoundIndexOfSegCommentList(selectedTimeIndex);
        return 1f - (float)(selectedIndex - startIndex) / Math.Min(k_MaxVirualListLength, dataManager.SegCommentList.Count);
    }
}
