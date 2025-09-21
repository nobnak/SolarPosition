using System;
using UnityEngine;

/// <summary>
/// 太陽位置計算のテスト用スクリプト
/// コンソールで様々な条件での太陽位置を計算して表示
/// </summary>
public class SolarPositionTest : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField]
    [Tooltip("開始時にテストを実行する")]
    private bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            RunAllTests();
        }
    }
    
    /// <summary>
    /// すべてのテストを実行
    /// </summary>
    [ContextMenu("全テスト実行")]
    public void RunAllTests()
    {
        Debug.Log("=== 太陽位置計算テスト開始 ===");
        
        TestBasicCalculation();
        TestDifferentSeasons();
        TestDifferentLocations();
        TestDifferentTimes();
        TestEdgeCases();
        
        Debug.Log("=== 太陽位置計算テスト完了 ===");
    }
    
    /// <summary>
    /// 基本的な計算テスト
    /// </summary>
    private void TestBasicCalculation()
    {
        Debug.Log("--- 基本計算テスト ---");
        
        try
        {
            // 東京、2025年春分の日正午での計算
            DateTime springEquinox = new DateTime(2025, 3, 21, 12, 0, 0);
            var result = SolarPositionCalculator.Calculate(springEquinox, 35.6762f, 139.6503f);
            
            Debug.Log($"東京、春分の日正午: {result}");
            Debug.Log($"太陽の状態: {result.GetSunState()}");
            
            // 期待値の確認（春分の日の正午なので、ある程度高い位置にあるはず）
            if (result.elevation > 30)
            {
                Debug.Log("✓ 基本計算テスト: 合格");
            }
            else
            {
                Debug.LogWarning("⚠ 基本計算テスト: 期待される高度と異なる可能性があります");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ 基本計算テスト: エラー - {e.Message}");
        }
    }
    
    /// <summary>
    /// 異なる季節でのテスト
    /// </summary>
    private void TestDifferentSeasons()
    {
        Debug.Log("--- 季節別テスト ---");
        
        float lat = 35.6762f; // 東京
        float lon = 139.6503f;
        int year = 2025;
        
        var seasons = new[]
        {
            (name: "春分", date: new DateTime(year, 3, 21, 12, 0, 0)),
            (name: "夏至", date: new DateTime(year, 6, 21, 12, 0, 0)),
            (name: "秋分", date: new DateTime(year, 9, 23, 12, 0, 0)),
            (name: "冬至", date: new DateTime(year, 12, 22, 12, 0, 0))
        };
        
        foreach (var season in seasons)
        {
            try
            {
                var result = SolarPositionCalculator.Calculate(season.date, lat, lon);
                Debug.Log($"{season.name}: 高度={result.elevation:F2}°, 方位={result.azimuth:F2}°");
            }
            catch (Exception e)
            {
                Debug.LogError($"✗ {season.name}テスト: エラー - {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 異なる場所でのテスト
    /// </summary>
    private void TestDifferentLocations()
    {
        Debug.Log("--- 地域別テスト ---");
        
        DateTime testDate = new DateTime(2025, 6, 21, 12, 0, 0); // 夏至
        
        var locations = new[]
        {
            (name: "東京", lat: 35.6762f, lon: 139.6503f),
            (name: "札幌", lat: 43.0642f, lon: 141.3469f),
            (name: "那覇", lat: 26.2125f, lon: 127.6792f),
            (name: "赤道", lat: 0.0f, lon: 0.0f),
            (name: "北極圏", lat: 70.0f, lon: 0.0f),
            (name: "南半球（シドニー）", lat: -33.8688f, lon: 151.2093f)
        };
        
        foreach (var location in locations)
        {
            try
            {
                var result = SolarPositionCalculator.Calculate(testDate, location.lat, location.lon);
                Debug.Log($"{location.name}: 高度={result.elevation:F2}°, 方位={result.azimuth:F2}° ({result.GetSunState()})");
            }
            catch (Exception e)
            {
                Debug.LogError($"✗ {location.name}テスト: エラー - {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 異なる時刻でのテスト
    /// </summary>
    private void TestDifferentTimes()
    {
        Debug.Log("--- 時刻別テスト ---");
        
        float lat = 35.6762f; // 東京
        float lon = 139.6503f;
        DateTime baseDate = new DateTime(2025, 6, 21); // 夏至
        
        for (int hour = 0; hour <= 23; hour += 3)
        {
            try
            {
                DateTime testTime = baseDate.AddHours(hour);
                var result = SolarPositionCalculator.Calculate(testTime, lat, lon);
                
                string sunState = result.GetSunState();
                Debug.Log($"{hour:D2}:00 - 高度: {result.elevation:F2}°, 方位: {result.azimuth:F2}° ({sunState})");
            }
            catch (Exception e)
            {
                Debug.LogError($"✗ {hour}時テスト: エラー - {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// エッジケースのテスト
    /// </summary>
    private void TestEdgeCases()
    {
        Debug.Log("--- エッジケーステスト ---");
        
        DateTime testDate = new DateTime(2025, 6, 21, 12, 0, 0);
        
        var edgeCases = new[]
        {
            (name: "北極", lat: 90.0f, lon: 0.0f),
            (name: "南極", lat: -90.0f, lon: 0.0f),
            (name: "東経180度", lat: 0.0f, lon: 180.0f),
            (name: "西経180度", lat: 0.0f, lon: -180.0f),
            (name: "北回帰線", lat: 23.5f, lon: 0.0f),
            (name: "南回帰線", lat: -23.5f, lon: 0.0f)
        };
        
        foreach (var testCase in edgeCases)
        {
            try
            {
                var result = SolarPositionCalculator.Calculate(testDate, testCase.lat, testCase.lon);
                Debug.Log($"{testCase.name}: 高度={result.elevation:F2}°, 方位={result.azimuth:F2}°");
            }
            catch (Exception e)
            {
                Debug.LogError($"✗ {testCase.name}テスト: エラー - {e.Message}");
            }
        }
        
        // 不正な値でのテスト
        Debug.Log("--- 入力値検証テスト ---");
        
        try
        {
            SolarPositionCalculator.Calculate(testDate, 91.0f, 0.0f); // 不正な緯度
            Debug.LogError("✗ 緯度範囲外テスト: エラーが発生しませんでした");
        }
        catch (ArgumentException)
        {
            Debug.Log("✓ 緯度範囲外テスト: 正常にエラーが発生");
        }
        
        try
        {
            SolarPositionCalculator.Calculate(testDate, 0.0f, 181.0f); // 不正な経度
            Debug.LogError("✗ 経度範囲外テスト: エラーが発生しませんでした");
        }
        catch (ArgumentException)
        {
            Debug.Log("✓ 経度範囲外テスト: 正常にエラーが発生");
        }
    }
    
    /// <summary>
    /// 現在時刻での計算テスト
    /// </summary>
    [ContextMenu("現在時刻テスト")]
    public void TestCurrentTime()
    {
        Debug.Log("--- 現在時刻テスト ---");
        
        try
        {
            var result = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f);
            Debug.Log($"現在の太陽位置（東京）: {result}");
            Debug.Log($"太陽の状態: {result.GetSunState()}");
        }
        catch (Exception e)
        {
            Debug.LogError($"✗ 現在時刻テスト: エラー - {e.Message}");
        }
    }
}
