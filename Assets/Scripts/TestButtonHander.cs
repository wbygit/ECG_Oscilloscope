using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButtonHander : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMessageBox1()
    {
        MessageBox.DisplayMessageBox("TestBox1", "show message box1", true, null);
    }

    public void ShowMessageBox2()
    {
        MessageBox.DisplayMessageBox("TestBox2", "show message box2", true, OnErrorDialogClose);
    }

    public void OnErrorDialogClose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
