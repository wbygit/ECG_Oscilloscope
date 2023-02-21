using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Tool
{
    // C++��񣬷�������IList���ڵ��ڵĵ�һ��λ�ã�>=0�����������������Ƿ������ֵ
    public static int GetLowerBoundIndex(IList<int> list, int target)
    {
        int l = 0, r = list.Count - 1, ans = list.Count;
        while (l <= r)
        {
            int mid = (l + r) / 2;
            if (list[mid] >= target)
            {
                ans = mid;
                r = mid - 1;
            }
            else
            {
                l = mid + 1;
            }
        }
        return ans;
    }

    public static void InitArrythmiaDropdown(TMP_Dropdown dropdown, ArrythmiaDict arrythmiaDict, string curArrythmia)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(arrythmiaDict.arrythmiaOptions);
        dropdown.SetValueWithoutNotify(arrythmiaDict.GetIndex(curArrythmia));
    }


    public static int GetSimilarValueIndexInArr(double[] arr, double value, double eps = 1e-4)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (Math.Abs(arr[i] - value) < eps)
            {
                return i;
            }
        }
        return -1;
    }
}
