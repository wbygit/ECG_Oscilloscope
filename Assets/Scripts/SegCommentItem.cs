using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegCommentItem : MonoBehaviour
{
    public TMP_Text Text_SegCommentItemOrderIndex;
    public TMP_Text Text_SegCommentItemIndex;
    public TMP_Text Text_SegCommentItemITime;
    public TMP_Text Text_SegCommentItemIType;
    public TMP_InputField InputField_SegCommentItemNote;
    

    public SegComments segComments;

    public void SetByData(int orderIndex, SegCommentData data)
    {
        Text_SegCommentItemOrderIndex.text = orderIndex.ToString();
        Text_SegCommentItemIndex.text = data.timeIndex.ToString();
        Text_SegCommentItemITime.text = DataManager.TransSecondToHMSp2(data.timeIndex / segComments.dataManager.AnnotationFs);
        Text_SegCommentItemIType.text = data.type;
        InputField_SegCommentItemNote.text = data.note;
    }

    public void OnValueChanged_Toggle_SegCommentItem(bool value)
    {
        if (value)
        {
            segComments.selectedTimeIndex = int.Parse(Text_SegCommentItemIndex.text);
        }
        else
        {
            if (segComments.selectedTimeIndex == int.Parse(Text_SegCommentItemIndex.text))
            {
                segComments.selectedTimeIndex = -1;
            }
        }
    }

    public void OnEndEdit_InputField_SegCommentItemINote()
    {
        int idx = segComments.dataManager.GetLowerBoundIndexOfSegCommentList(int.Parse(Text_SegCommentItemIndex.text));
        if (segComments.dataManager.SegCommentList[idx].note != InputField_SegCommentItemNote.text)
        {
            segComments.dataManager.SegCommentList[idx].note = InputField_SegCommentItemNote.text;
            segComments.SetSegCommentSavedFlag(false);
        }
    }

    private void Awake()
    {
        segComments = GameObject.Find("SegCommentsTabBackground").GetComponent<SegComments>();
        gameObject.GetComponent<Toggle>().group = GameObject.Find("Content_SegComments").GetComponent<ToggleGroup>();
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
