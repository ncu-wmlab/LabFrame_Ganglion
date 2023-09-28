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

    protected bool _doWriteEegData = false;
    protected bool _doWriteImpedanceData = false;
    
#if UNITY_ANDROID
    private AndroidJavaObject _pluginInstance;
    Coroutine _checkConnectedCoroutine;
#endif

    #region IManager
    public void ManagerInit()
    {
#if UNITY_ANDROID
        _pluginInstance = new AndroidJavaObject("com.xrlab.ganglion_plugin.PluginInstance");
        if (_pluginInstance == null)
            LabTools.LogError("Error while creating Ganglion PluginInstance object");
        _pluginInstance.CallStatic("receiveUnityActivity", AndroidHelper.CurrentActivity);

        _checkConnectedCoroutine = StartCoroutine(CheckConnected());
#endif
    }

    public IEnumerator ManagerDispose()
    {
        StopStreamData();
#if UNITY_ANDROID
        StopCoroutine(_checkConnectedCoroutine);
        
        try
        {
            _pluginInstance.Call("Disconnect");
            // _pluginInstance.Call("Close");
            LabTools.Log("[Ganglion] Disconnected");
        }
        catch
        {
            LabTools.LogError("[Ganglion] Disconnect failed");
        }

        _pluginInstance.Dispose();
        _pluginInstance = null;
#endif
        yield return 0;
    }
    
    #endregion


#if UNITY_ANDROID
    /// <summary>
    /// 持續檢查連線狀態，並於斷線時嘗試重新連線
    /// </summary>
    IEnumerator CheckConnected()
    {
        while(true)
        {
            IsConnected = _pluginInstance.Get<bool>("mConnected");
            if(!IsConnected)
            {
                _pluginInstance.Call("Init");
                LabTools.Log("[Ganglion] Connecting... ");
            }
            yield return new WaitForSecondsRealtime(0.8763f);
        }
    } 
#endif

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
        if(_doWriteEegData)
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
        if(_doWriteEegData)
            LabDataManager.Instance.WriteData(_lastImpedanceData);   
    }
    #endregion

    #region Public Function    
    /// <summary>
    /// 開始記錄 EEG
    /// </summary>
    /// <param name="autoWriteLabData">自動把收到的數據送到 LabDataManager 儲存</param>
    public void StreamData(bool autoWriteLabData = true)
    {
        _doWriteEegData = autoWriteLabData;
#if UNITY_ANDROID
        _pluginInstance.Call("StreamData");
#endif
    }
    /// <summary>
    /// 停止記錄 EEG
    /// </summary>
    public void StopStreamData()
    {
        _doWriteEegData = false;
#if UNITY_ANDROID
        _pluginInstance.Call("StopStreamData");
#endif
    }
    /// <summary>
    /// 開始記錄阻抗
    /// </summary>
    /// <param name="autoWriteLabData">自動把收到的數據送到 LabDataManager 儲存</param>
    public void StreamImpedance(bool autoWriteLabData = true)
    {
        _doWriteImpedanceData = autoWriteLabData;
#if UNITY_ANDROID
        _pluginInstance.Call("StreamImpedance");
#endif
    }
    /// <summary>
    /// 停止記錄阻抗
    /// </summary>
    public void StopStreamImpedance()
    {
        _doWriteImpedanceData = false;
#if UNITY_ANDROID
        _pluginInstance.Call("StopStreamImpedance");
#endif
    }

    /// <summary>
    /// 獲取最近一次的腦電資料，可能為 null
    /// </summary>
    public Ganglion_EEGData GetEegData()
    {
        return _lastEegData;
    }
    /// <summary>
    /// 獲取最近一次的阻抗資料，可能為 null
    /// </summary>
    public Ganglion_ImpedanceData GetImpedanceData()
    {
        return _lastImpedanceData;
    }    
    #endregion

}