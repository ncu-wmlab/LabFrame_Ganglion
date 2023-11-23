using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;
using System.Threading;
using Newtonsoft.Json;

public class GanglionManager : LabSingleton<GanglionManager>, IManager
{
    /// <summary>
    /// 目前是否有與 Ganglion 設備連線
    /// </summary>
    public bool IsConnected {get; private set;} = false; 
    public bool IsUsingEEG {get; private set;} = false;
    public bool IsUsingImpedance {get; private set;} = false;

    protected Ganglion_EEGData _lastEegData;
    protected Ganglion_ImpedanceData _currentImpedanceData;

    GanglionConfig _config;

    protected bool _doWriteEegData = false;
    protected bool _doWriteImpedanceData = false;
    
#if UNITY_ANDROID
    private AndroidJavaObject _pluginInstance;
#endif
    Coroutine _checkConnectedCoroutine;


    #region IManager
    public void ManagerInit()
    {
#if UNITY_ANDROID
        // Init
        _config = LabTools.GetConfig<GanglionConfig>();
        _currentImpedanceData = new Ganglion_ImpedanceData(5);

        // Plugin
        _pluginInstance = new AndroidJavaObject("com.xrlab.ganglion_plugin.PluginInstance");
        if (_pluginInstance == null)
            LabTools.LogError("Error while creating Ganglion PluginInstance object");
        _pluginInstance.CallStatic("receiveUnityActivity", AndroidHelper.CurrentActivity);

        // Preferred Ganglion Name
        if(!string.IsNullOrEmpty(_config.PreferredDeviceName))
            _pluginInstance.Call("SetPreferredGanglionName", _config.PreferredDeviceName);
        
        // Do connect!
        if(_config.AutoConnectOnInit)
            Connect();

        // Check Connected Coroutine
#endif
        _checkConnectedCoroutine = StartCoroutine(CheckConnected());
    }

    public IEnumerator ManagerDispose()
    {
        StopStreamData();
        StopStreamImpedance();

        StopCoroutine(_checkConnectedCoroutine);
        _checkConnectedCoroutine = null;

        try
        {
            Disconnect();
        }
        catch
        {
            LabTools.LogError("[Ganglion] Disconnect failed");
        }

#if UNITY_ANDROID                
        _pluginInstance.Dispose();
        _pluginInstance = null;
#endif        

        _config = null;
        yield return 0;
    }
    
    #endregion


    void Connect()
    {
        if(!IsConnected)
        {
#if UNITY_ANDROID
            _pluginInstance.Call("Init");
#endif
        }
        LabTools.Log("[Ganglion] Start Connect...");
    }

    /// <summary>
    /// 持續檢查連線狀態，並於斷線時嘗試重新連線
    /// </summary>
    IEnumerator CheckConnected()
    {
        bool lastIsConnected = false;
        while(true)
        {
#if UNITY_ANDROID
            IsConnected = _pluginInstance.Get<bool>("mConnected");
            IsUsingEEG = _pluginInstance.Get<bool>("mUseEeg");
            IsUsingImpedance = _pluginInstance.Get<bool>("mUseImpedance");            
#endif
            if(!IsConnected && lastIsConnected)
            {
                LabPromptBox.Show("腦電已斷線！\nGanglion connection lost!");                
            }
            lastIsConnected = IsConnected;
            
            yield return null;
        }
    } 

    void Disconnect()
    {
        if(IsConnected)
        {
#if UNITY_ANDROID
            _pluginInstance.Call("Disconnect");
#endif
        }
        LabTools.Log("[Ganglion] Disconnected");
    }

    #region Android Plugin Callback            
    /// <summary>
    /// (DON'T CALL THIS MANULLY!!!) This will be called by the Android Plugin.
    /// </summary>
    /// <param name="json"></param>
    public void ReceiveData(string json) // called by Android plugin
    {
        string[] values = json.Split('|');
        _lastEegData = new Ganglion_EEGData(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]);
        if(_doWriteEegData)
            LabDataManager.Instance.WriteData(_lastEegData);        
    }

    /// <summary>
    /// (DON'T CALL THIS MANULLY!!!) This will be called by the Android Plugin.
    /// </summary>
    /// <param name="json"></param> 
    public void ReceiveImpedance(string json) // called by Android plugin
    {
        // "ch|imp_value"
        string[] values = json.Split('|');
        _currentImpedanceData.ImpedanceValues[int.Parse(values[0])] = int.Parse(values[1]);
        if(_doWriteEegData)
            LabDataManager.Instance.WriteData(_currentImpedanceData);   
    }
    #endregion

    #region Public Function    
    /// <summary>
    /// 開始記錄 EEG
    /// </summary>
    /// <param name="autoWriteLabData">自動把收到的數據送到 LabDataManager 儲存</param>
    public void StreamData(bool autoWriteLabData = true)
    {
        if(!IsConnected)
        {
            Debug.LogWarning("[Ganglion] Not connected!");
            return;
        }
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
        if(IsUsingEEG)
            _pluginInstance.Call("StopStreamData");
#endif
    }
    /// <summary>
    /// 開始記錄阻抗
    /// </summary>
    /// <param name="autoWriteLabData">自動把收到的數據送到 LabDataManager 儲存</param>
    public void StreamImpedance(bool autoWriteLabData = true)
    {
        if(!IsConnected)
        {
            Debug.LogWarning("[Ganglion] Not connected!");
            return;
        }
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
        if(IsUsingImpedance)
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
    /// 獲取目前的阻抗資料
    /// </summary>
    public Ganglion_ImpedanceData GetImpedanceData()
    {
        return _currentImpedanceData;
    }    

    /// <summary>
    /// 手動開始連線
    /// (目前無法在斷線後再次連線，敬請注意)
    /// </summary>
    public void ManualConnect()
    {
        Connect();
    }

    /// <summary>
    /// 手動進行斷線
    /// (目前無法在斷線後再次連線，敬請注意)
    /// </summary>
    public void ManualDisconnect()
    {
        Disconnect();
    }
    #endregion

}