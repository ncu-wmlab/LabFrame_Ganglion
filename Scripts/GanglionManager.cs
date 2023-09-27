using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;
using System.Threading;
using Newtonsoft.Json;

public class GanglionManager : LabSingleton<GanglionManager>, IManager
{
    public bool IsConnected {get; private set;} = false;    
    protected Ganglion_EEGData _lastEegData;
    protected Ganglion_ImpedanceData _lastImpedanceData;
    protected bool _doWriteData = false;
    

#if UNITY_ANDROID
    private AndroidJavaObject _pluginInstance;
    Coroutine _checkConnectedCoroutine;


    #region IManager

    public void ManagerInit()
    {
        _pluginInstance = new AndroidJavaObject("com.xrlab.ganglion_plugin.PluginInstance");
        if (_pluginInstance == null)
            LabTools.LogError("Error while creating Ganglion PluginInstance object");
        _pluginInstance.CallStatic("receiveUnityActivity", AndroidHelper.CurrentActivity);
        _checkConnectedCoroutine = StartCoroutine(CheckConnected());
    }

    public IEnumerator ManagerDispose()
    {
        StopWriteLabData();
        StopCoroutine(_checkConnectedCoroutine);
        
        try
        {
            _pluginInstance.Call("Disconnect");
            // _pluginInstance.Call("Close");
        }
        catch
        {
            Debug.LogError("Disconnect failed");
        }

        _pluginInstance.Dispose();
        _pluginInstance = null;
        yield return 0;
    }
    
    #endregion


    /// <summary>
    /// 初始化套件
    /// </summary>
    public void IntializePlugin()
    {    
        StartCoroutine(GanglionConnnect());        
    }

    IEnumerator GanglionConnnect()
    {
        while (!IsConnected)
        {
            // Debug.Log("[Ganglion] Connecting... ");
            _pluginInstance.Call("Init");
            yield return new WaitForSeconds(.8763f);
        }
        LabTools.Log("[Ganglion] Connected");
        IsConnected = true;
    }
    
    /// <summary>
    /// Check Connected
    /// </summary>
    IEnumerator CheckConnected()
    {
        while(true)
        {
            IsConnected = _pluginInstance.Get<bool>("mConnected");
            yield return new WaitForSecondsRealtime(0.48763f);
        }
    } 


    #region Android Plugin Callback            
    /// <summary>
    /// (DON'T CALL THIS MANULLY!!!) This will be called by the Android Plugin.
    /// </summary>
    /// <param name="json"></param>
    public void ReceiveData(string json) // called by Android plugin
    {
        // Convert to Dictionary
        var values = JsonConvert.DeserializeObject<Dictionary<string, double>>(json);
        _lastEegData = new Ganglion_EEGData(
            values["ch1_1"], 
            values["ch2_1"], 
            values["ch3_1"], 
            values["ch4_1"]);
        if(_doWriteData)
            LabDataManager.Instance.WriteData(_lastEegData);        
    }

    /// <summary>
    /// (DON'T CALL THIS MANULLY!!!) This will be called by the Android Plugin.
    /// </summary>
    /// <param name="json"></param> 
    public void ReceiveImpedance(string json) // called by Android plugin
    {
        var values = JsonConvert.DeserializeObject<Dictionary<int, int>>(json);
        _lastImpedanceData = new Ganglion_ImpedanceData{ ImpedanceValues = new List<int>(5) };
        foreach (KeyValuePair<int, int> kvp in values)
        {
            // Debug.Log($"name = impedance {kvp.Key}, val = {kvp.Value}");
            _lastImpedanceData.ImpedanceValues[kvp.Key] = kvp.Value;
        }
        if(_doWriteData)
            LabDataManager.Instance.WriteData(_lastImpedanceData);   
    }
    #endregion
#endif

    #region Public Function
    /// <summary>
    /// 獲取最近一次的腦電資料
    /// </summary>
    public Ganglion_EEGData GetEegData()
    {
        return _lastEegData;
    }
    /// <summary>
    /// 獲取最近一次的阻抗資料
    /// </summary>
    public Ganglion_ImpedanceData GetImpedanceData()
    {
        return _lastImpedanceData;
    }
    /// <summary>
    /// 開始記錄 LabData
    /// </summary>
    public void StartWriteLabData()
    {
        _doWriteData = true;
        _pluginInstance.Call("StreamData");
        _pluginInstance.Call("StreamImpedance");
    }
    /// <summary>
    /// 停止記錄 LabData
    /// </summary>
    public void StopWriteLabData()
    {
        _doWriteData = false;
        _pluginInstance.Call("StopStreamData");
        _pluginInstance.Call("StopStreamImpedance");
    }
    #endregion

}