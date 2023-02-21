using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FunctionTab : MonoBehaviour
{
    public DataManager dataManager;

    public Toggle Toggle_SegCommentsTab;
    public Toggle Toggle_ClsAnnotationTab;

    public GameObject SegCommentsTabBackground;
    public GameObject ClsAnnotationTabBackground;

    private void Awake()
    {
        dataManager = GameObject.Find("AppConfig").GetComponent<DataManager>();
        Toggle_SegCommentsTab = GameObject.Find("Toggle_SegCommentsTab").GetComponent<Toggle>();
        Toggle_ClsAnnotationTab = GameObject.Find("Toggle_ClsAnnotationTab").GetComponent<Toggle>();
        SegCommentsTabBackground = GameObject.Find("SegCommentsTabBackground");
        ClsAnnotationTabBackground = GameObject.Find("ClsAnnotationTabBackground");
        SegCommentsTabBackground.SetActive(false);
        ClsAnnotationTabBackground.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dataManager.IsValidPlotRPeak())
        {
            Toggle_SegCommentsTab.interactable = true;
        }
        else
        {
            Toggle_SegCommentsTab.isOn = false;
            Toggle_SegCommentsTab.interactable = false;
        }

        if (dataManager.IsValidPlotClsOutput() || dataManager.IsValidPlotClsLabel())
        {
            Toggle_ClsAnnotationTab.interactable = true;
        }
        else
        {
            Toggle_ClsAnnotationTab.isOn = false;
            Toggle_ClsAnnotationTab.interactable = false;
        }
    }

    public void OnValueChanged_Toggle_SegCommentsTab(bool value)
    {
        SegCommentsTabBackground.SetActive(value);
    }

    public void OnValueChanged_Toggle_ClsAnnotationTab(bool value)
    {
        ClsAnnotationTabBackground.SetActive(value);
    }
}
