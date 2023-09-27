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
    public List<double> EEGValues;
    
    public Ganglion_EEGData(params double[] ch) : base()
    {
        EEGValues = new List<double>();
        foreach (var value in ch)
        {
            EEGValues.Add(value);
        }
    }
}

/// <summary>
/// 腦電阻抗資料
/// </summary>
public class Ganglion_ImpedanceData : LabDataBase
{
    public List<int> ImpedanceValues;
}
