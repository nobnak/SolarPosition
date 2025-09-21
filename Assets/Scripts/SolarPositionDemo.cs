using System;
using UnityEngine;

/// <summary>
/// 太陽位置計算のデモスクリプト
/// Unity Inspector上で日時と位置を設定して太陽位置を計算・表示する
/// </summary>
public class SolarPositionDemo : MonoBehaviour
{
    [Header("計算パラメータ")]
    [SerializeField]
    [Tooltip("計算する年")]
    private int year = 2025;
    
    [SerializeField]
    [Tooltip("計算する月 (1-12)")]
    [Range(1, 12)]
    private int month = 9;
    
    [SerializeField]
    [Tooltip("計算する日 (1-31)")]
    [Range(1, 31)]
    private int day = 21;
    
    [SerializeField]
    [Tooltip("計算する時間 (0-23)")]
    [Range(0, 23)]
    private int hour = 12;
    
    [SerializeField]
    [Tooltip("計算する分 (0-59)")]
    [Range(0, 59)]
    private int minute = 0;
    
    [SerializeField]
    [Tooltip("緯度（度、-90～+90）")]
    [Range(-90f, 90f)]
    private float latitude = 35.6762f; // 東京の緯度
    
    [SerializeField]
    [Tooltip("経度（度、-180～+180）")]
    [Range(-180f, 180f)]
    private float longitude = 139.6503f; // 東京の経度
    
    [Header("計算結果")]
    [SerializeField]
    [Tooltip("計算結果")]
    private SolarPositionCalculator.SolarPosition solarPosition;
    
    [Header("設定")]
    [SerializeField]
    [Tooltip("自動更新を有効にする")]
    private bool autoUpdate = true;
    
    [SerializeField]
    [Tooltip("現在時刻を使用する")]
    private bool useCurrentTime = false;
    
    [SerializeField]
    [Tooltip("更新間隔（秒）")]
    [Range(0.1f, 10f)]
    private float updateInterval = 1.0f;
    
    private float lastUpdateTime;
    
    [Header("有名な場所のプリセット")]
    [SerializeField]
    private LocationPreset[] locationPresets = new LocationPreset[]
    {
        new LocationPreset("東京", 35.6762f, 139.6503f),
        new LocationPreset("大阪", 34.6937f, 135.5023f),
        new LocationPreset("札幌", 43.0642f, 141.3469f),
        new LocationPreset("那覇", 26.2125f, 127.6792f),
        new LocationPreset("ニューヨーク", 40.7128f, -74.0060f),
        new LocationPreset("ロンドン", 51.5074f, -0.1278f),
        new LocationPreset("パリ", 48.8566f, 2.3522f),
        new LocationPreset("シドニー", -33.8688f, 151.2093f)
    };
    
    [System.Serializable]
    public class LocationPreset
    {
        public string name;
        public float latitude;
        public float longitude;
        
        public LocationPreset(string name, float latitude, float longitude)
        {
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
    
    void Start()
    {
        // 初回計算
        CalculateSolarPosition();
    }
    
    void Update()
    {
        if (autoUpdate && Time.time - lastUpdateTime >= updateInterval)
        {
            CalculateSolarPosition();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// 太陽位置を計算して結果を更新
    /// </summary>
    public void CalculateSolarPosition()
    {
        try
        {
            DateTime dateTime;
            
            if (useCurrentTime)
            {
                dateTime = DateTime.Now;
            }
            else
            {
                dateTime = new DateTime(year, month, day, hour, minute, 0);
            }
            
            solarPosition = SolarPositionCalculator.Calculate(dateTime, latitude, longitude);
            
            Debug.Log($"太陽位置計算完了: {solarPosition}");
            Debug.Log($"太陽の状態: {solarPosition.GetSunState()}");
        }
        catch (Exception e)
        {
            Debug.LogError($"太陽位置計算エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// プリセット位置を適用
    /// </summary>
    /// <param name="presetIndex">プリセットのインデックス</param>
    public void ApplyLocationPreset(int presetIndex)
    {
        if (presetIndex >= 0 && presetIndex < locationPresets.Length)
        {
            var preset = locationPresets[presetIndex];
            latitude = preset.latitude;
            longitude = preset.longitude;
            
            Debug.Log($"位置プリセット適用: {preset.name} ({preset.latitude}, {preset.longitude})");
            
            CalculateSolarPosition();
        }
    }
    
    /// <summary>
    /// 現在時刻に設定
    /// </summary>
    public void SetCurrentTime()
    {
        DateTime now = DateTime.Now;
        year = now.Year;
        month = now.Month;
        day = now.Day;
        hour = now.Hour;
        minute = now.Minute;
        
        Debug.Log($"現在時刻に設定: {now:yyyy-MM-dd HH:mm:ss}");
        
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 夏至の日に設定（6月21日）
    /// </summary>
    public void SetSummerSolstice()
    {
        month = 6;
        day = 21;
        Debug.Log("夏至の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 冬至の日に設定（12月22日）
    /// </summary>
    public void SetWinterSolstice()
    {
        month = 12;
        day = 22;
        Debug.Log("冬至の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 春分の日に設定（3月21日）
    /// </summary>
    public void SetVernalEquinox()
    {
        month = 3;
        day = 21;
        Debug.Log("春分の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 秋分の日に設定（9月23日）
    /// </summary>
    public void SetAutumnalEquinox()
    {
        month = 9;
        day = 23;
        Debug.Log("秋分の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 結果をCSV形式で出力
    /// </summary>
    public void ExportToCSV()
    {
        string csv = $"日時,緯度,経度,高度角,方位角,太陽の状態\n";
        csv += $"{solarPosition.dateTime:yyyy-MM-dd HH:mm:ss},{solarPosition.latitude},{solarPosition.longitude},";
        csv += $"{solarPosition.elevation},{solarPosition.azimuth},{solarPosition.GetSunState()}\n";
        
        Debug.Log("CSV出力:");
        Debug.Log(csv);
        
        // ファイルに保存する場合はここに追加
        // System.IO.File.WriteAllText("solar_position.csv", csv);
    }
    
    void OnValidate()
    {
        // Inspector上で値が変更されたときに自動計算
        if (Application.isPlaying)
        {
            CalculateSolarPosition();
        }
    }
}
