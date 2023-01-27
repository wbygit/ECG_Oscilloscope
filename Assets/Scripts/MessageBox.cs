﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    Text[] textComponents;
    delegate void DelegateMessageBoxButtonAction();
    DelegateMessageBoxButtonAction m_DelegateDismissMessageBoxAction;
    DelegateMessageBoxButtonAction m_DelegateConfirmMessageBoxAction;

    #endregion // PRIVATE_MEMBERS


    #region PUBLIC_METHODS

    /// <summary>
    /// 弹窗接口
    /// </summary>
    /// <param name="title">弹窗的标题</param>
    /// <param name="body">弹窗的具体信息</param>
    /// <param name="dismissable">是否需要关闭按钮</param>
    /// <param name="dismissAction">关闭按钮后续的操作</param>
    public static void DisplayMessageBox(string title, string body, bool dismissable = true, Action closeButton = null, string dissmissText = "关闭", bool confirm = false, Action confirmButton = null, string confirmText = "确认")
    {
        GameObject prefab = (GameObject)Resources.Load("MessageBox");
        if (prefab)
        {
            MessageBox messageBox = Instantiate(prefab.GetComponent<MessageBox>());
            messageBox.Setup(title, body, dismissable, closeButton, dissmissText, confirm, confirmButton, confirmText);
        }
    }

    public void MessageBoxDismissButton()
    {
        // This method called by the UI Canvas Button

        // If there's a custom method, run it first
        if (m_DelegateDismissMessageBoxAction != null)
        {
            m_DelegateDismissMessageBoxAction();
        }

        // Destroy MessageBox
        Destroy(gameObject);
    }

    public void MessageBoxConfirmButton()
    {
        // This method called by the UI Canvas Button

        // If there's a custom method, run it first
        if (m_DelegateConfirmMessageBoxAction != null)
        {
            m_DelegateConfirmMessageBoxAction();
        }

        // Destroy MessageBox
        Destroy(gameObject);
    }

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS

    void Setup(string title, string body, bool dismissable = true, Action closeButton = null, string dissmissText = "关闭", bool confirm = false, Action confirmButton = null, string confirmText = "确认")
    {
        textComponents = GetComponentsInChildren<Text>();

        if (textComponents.Length >= 2)
        {
            textComponents[0].text = title;
            textComponents[1].text = body;
            textComponents[2].text = dissmissText;
            textComponents[3].text = confirmText;
        }

        if (closeButton != null)
            m_DelegateDismissMessageBoxAction = new DelegateMessageBoxButtonAction(closeButton);
        if (confirmButton != null)
            m_DelegateConfirmMessageBoxAction = new DelegateMessageBoxButtonAction(confirmButton);

        Button []button = GetComponentsInChildren<Button>();
        if (button.Length > 0)
        {
            button[0].gameObject.SetActive(dismissable);
            button[1].gameObject.SetActive(confirm);
        }
    }

    #endregion // PRIVATE_METHODS
}
