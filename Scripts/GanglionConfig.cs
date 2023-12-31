using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GanglionConfig
{
    /// <summary>
    /// 指定連線的 Ganglion 設備名稱，例如 Ganglion-5599 (可以只填 5599)。
    /// 若為空字串則自動連線第一個找到的 Ganglion 設備
    /// </summary>
    public string PreferredDeviceName = "";

    /// <summary>
    /// 啟動時自動開啟連線
    /// </summary>
    public bool AutoConnectOnInit = false;

    /// <summary>
    /// 是否顯示斷線通知
    /// </summary>
    public bool DisconnectNotification = true;
}