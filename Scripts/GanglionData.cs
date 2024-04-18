using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;
using System;

/// <summary>
/// 腦電資料
/// </summary>
[Serializable]
public class Ganglion_EEGData : LabDataBase
{
    public double ch1_1;
    public double ch1_2;
    public double ch2_1;
    public double ch2_2;
    public double ch3_1;
    public double ch3_2;
    public double ch4_1;
    public double ch4_2;

    [Obsolete("此為舊版 ch, 只能取得 4 個 channel 的第一個值，實際上我們能拿到 4 ch 各 2 個。請直接取用 chX_X 或改用 EEGValuesFull")]
    public List<double> EEGValues => new List<double>(){ch1_1, ch2_1, ch3_1, ch4_1};
    public List<double> EEGValuesFull => new List<double>(){ch1_1, ch1_2, ch2_1, ch2_2, ch3_1, ch3_2, ch4_1, ch4_2};
        
    public Ganglion_EEGData(params string[] ch) : base()
    {
        ch1_1 = double.Parse(ch[0]);
        ch1_2 = double.Parse(ch[1]);
        ch2_1 = double.Parse(ch[2]);
        ch2_2 = double.Parse(ch[3]);
        ch3_1 = double.Parse(ch[4]);
        ch3_2 = double.Parse(ch[5]);
        ch4_1 = double.Parse(ch[6]);
        ch4_2 = double.Parse(ch[7]);        
    }

    public override string ToString()
    {
        return $"{ch1_1:N2} | {ch2_1:N2} | {ch3_1:N2} | {ch4_1:N2}\n{ch1_2:N2} | {ch2_2:N2} | {ch3_2:N2} | {ch4_2:N2}";
    }
}

/// <summary>
/// 腦電阻抗資料
/// </summary>
public class Ganglion_ImpedanceData : LabDataBase
{
    public List<int> ImpedanceValues;

    public Ganglion_ImpedanceData(int channels)
    {
        ImpedanceValues = new List<int>(channels);
        for (int i = 0; i < channels; i++)
        {
            ImpedanceValues.Add(0);
        }
    }

    public override string ToString()
    {
        if(ImpedanceValues == null || ImpedanceValues.Count == 0)
            return "";

        string s = "";
        ImpedanceValues.ForEach(v => s += v.ToString("N2") + " | ");
        s.Remove(s.Length - 3);
        return s;
    }
}
