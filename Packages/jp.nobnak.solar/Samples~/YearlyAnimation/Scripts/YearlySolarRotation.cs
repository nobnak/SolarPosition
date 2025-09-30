using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using jp.nobnak.solar;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 毎日特定の時刻を一年分、ローテーションとして出力するコルーチンとデモ機能を持つスクリプト
/// </summary>
public class YearlySolarRotation : MonoBehaviour {
    
    [Header("設定")]
    [Tooltip("観測地点の緯度（度）")]
    public float latitude = 35.6762f; // 東京の緯度
    
    [Tooltip("観測地点の経度（度）")]
    public float longitude = 139.6503f; // 東京の経度
    
    [Tooltip("計算する時刻（0-23）")]
    [Range(0, 23)]
    public int targetHour = 12; // 正午
    
    [Tooltip("計算する分（0-59）")]
    [Range(0, 59)]
    public int targetMinute = 0;
    
    [Tooltip("計算開始年")]
    public int year = DateTime.Now.Year;
    
    
    [Header("表示オブジェクト")]
    [Tooltip("太陽方向を制御するSunDirectionController")]
    public SunDirectionController sunDirectionController;
    
    [Tooltip("日時を表示するTextMeshProコンポーネント")]
    public TextMeshProUGUI dateTimeText;
    
    [Header("アニメーション")]
    [Tooltip("自動でアニメーションを再生")]
    public bool autoPlayAnimation = true;
    
    [Tooltip("アニメーション速度（日/秒）")]
    public float animationSpeed = 10f;
    
    [Header("UI設定")]
    [Tooltip("日時表示を有効にする")]
    public bool showDateTime = true;
    
    [Tooltip("アニメーション中の日時表示フォーマット")]
    public string dateTimeFormat = "yyyy年MM月dd日\nHH時mm分";
    
    [Header("リアルタイム結果")]
    [Tooltip("現在計算されている太陽位置")]
    [System.NonSerialized] public SolarPositionCalculator.SolarPosition currentSolarPosition;
    
    [Tooltip("現在のローテーション")]
    [System.NonSerialized] public quaternion currentRotation = quaternion.identity;
    
    [System.NonSerialized] public bool isAnimating = false;
    [System.NonSerialized] public float animationTime = 0f;
    [System.NonSerialized] public string debugInfo = "";
    
    // パラメータ変更検知用
    private float lastLatitude;
    private float lastLongitude;
    private int lastTargetHour;
    private int lastTargetMinute;
    private int lastYear;
    
    void Start() {
        // 初期パラメータを記録
        StoreLastParameters();
        
        // 初期計算
        UpdateSolarPosition();
        
        // アニメーション自動開始
        if (autoPlayAnimation)
            isAnimating = true;
    }
    
    void Update() {
        // パラメータ変更チェック
        if (HasParametersChanged()) {
            UpdateSolarPosition();
            StoreLastParameters();
        }
        
        // 手動でアニメーション制御
        if (Input.GetKeyDown(KeyCode.Space))
            ToggleAnimation();
        
        // アニメーション更新
        if (isAnimating)
            UpdateAnimation();
        else
            UpdateCurrentDayDisplay();
    }
    
    /// <summary>
    /// パラメータが変更されたかチェック
    /// </summary>
    bool HasParametersChanged() => 
        latitude != lastLatitude || longitude != lastLongitude || 
        targetHour != lastTargetHour || targetMinute != lastTargetMinute || 
        year != lastYear;
    
    /// <summary>
    /// 現在のパラメータを記録
    /// </summary>
    void StoreLastParameters() {
        lastLatitude = latitude;
        lastLongitude = longitude;
        lastTargetHour = targetHour;
        lastTargetMinute = targetMinute;
        lastYear = year;
    }
    
    /// <summary>
    /// 現在の設定に基づいて太陽位置を更新
    /// </summary>
    public void UpdateSolarPosition() {
        var today = DateTime.Today;
        var dayOfYear = isAnimating ? Mathf.FloorToInt(animationTime) % GetDaysInYear() + 1 : today.DayOfYear;
        CalculateAndApplySolarPosition(dayOfYear);
    }
    
    /// <summary>
    /// 指定した日の太陽位置を計算してオブジェクトに適用
    /// </summary>
    void CalculateAndApplySolarPosition(int dayOfYear) {
        var solarPosition = CalculateSolarPositionForDay(dayOfYear);
        var rotation = SolarRotationConverter.CalculateSunRotation(solarPosition.elevation, solarPosition.azimuth);
        
        currentSolarPosition = solarPosition;
        currentRotation = rotation;
        
        ApplySolarPosition(solarPosition, dayOfYear);
    }
    
    /// <summary>
    /// 指定した日の太陽位置をリアルタイム計算
    /// </summary>
    public SolarPositionCalculator.SolarPosition CalculateSolarPositionForDay(int dayOfYear) {
        var startDate = new DateTime(year, 1, 1);
        var targetDate = startDate.AddDays(dayOfYear - 1);
        var targetDateTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, targetHour, targetMinute, 0);
        
        // 経度ベースのタイムゾーンオフセット計算（15度で1時間、15分単位で丸め）
        // 東経が正、西経が負。例：東京(139.65°) = UTC+9:15, ロンドン(0°) = UTC+0:00, ニューヨーク(-74°) = UTC-5:00
        var roundedOffsetHours = GetRoundedTimezoneOffset();
        var dateTimeOffset = new DateTimeOffset(targetDateTime, TimeSpan.FromHours(roundedOffsetHours));
        
        return SolarPositionCalculator.Calculate(dateTimeOffset, latitude, longitude);
    }
    
    /// <summary>
    /// 現在の年の日数を取得
    /// </summary>
    public int GetDaysInYear() => DateTime.IsLeapYear(year) ? 366 : 365;
    
    /// <summary>
    /// 経度から丸められたタイムゾーンオフセット（時間）を計算
    /// </summary>
    /// <returns>15分単位で丸められたオフセット時間</returns>
    private double GetRoundedTimezoneOffset() {
        var longitudeOffsetHours = longitude / 15.0;
        // 15分単位で丸める（0.25時間 = 15分）
        // 例：9.31 → 9.25, 9.38 → 9.5, 9.13 → 9.0
        return Math.Round(longitudeOffsetHours * 4) / 4.0;
    }
    
    /// <summary>
    /// タイムゾーンオフセットの表示文字列を生成
    /// </summary>
    /// <returns>UTC±HH:MM形式の文字列</returns>
    private string GetTimezoneOffsetString() {
        var longitudeOffsetHours = longitude / 15.0;
        var roundedOffsetHours = GetRoundedTimezoneOffset();
        
        var offsetTimeSpan = TimeSpan.FromHours(roundedOffsetHours);
        var offsetSign = roundedOffsetHours >= 0 ? "+" : "";
        var offsetString = $"UTC{offsetSign}{(int)offsetTimeSpan.TotalHours:00}:{Math.Abs(offsetTimeSpan.Minutes):00}";
        
        return $"{offsetString} (生値: {longitudeOffsetHours:F2})";
    }
    
    
    
    /// <summary>
    /// 指定した日のローテーションをリアルタイム計算で取得
    /// </summary>
    /// <param name="dayOfYear">年の日数（1-365/366）</param>
    /// <returns>その日のローテーション</returns>
    public quaternion GetRotationForDay(int dayOfYear) {
        var daysInYear = GetDaysInYear();
        if (dayOfYear <= 0 || dayOfYear > daysInYear) {
            Debug.LogWarning($"無効な日数です: {dayOfYear} (有効範囲: 1-{daysInYear})");
            return quaternion.identity;
        }
        
        var solarPosition = CalculateSolarPositionForDay(dayOfYear);
        return SolarRotationConverter.CalculateSunRotation(solarPosition.elevation, solarPosition.azimuth);
    }
    
    /// <summary>
    /// 現在の日付に基づいてローテーションをリアルタイム計算で取得
    /// </summary>
    /// <returns>今日のローテーション</returns>
    public quaternion GetTodayRotation() {
        var today = DateTime.Today;
        var dayOfYear = today.DayOfYear;
        return GetRotationForDay(dayOfYear);
    }

    
    
    /// <summary>
    /// アニメーションのオン/オフ切り替え
    /// </summary>
    [ContextMenu("アニメーション切り替え")]
    public void ToggleAnimation() {
        isAnimating = !isAnimating;
        Debug.Log($"アニメーション: {(isAnimating ? "開始" : "停止")}");
    }
    
    /// <summary>
    /// アニメーションの更新（リアルタイム計算版）
    /// </summary>
    void UpdateAnimation() {
        animationTime += Time.deltaTime * animationSpeed;
        
        int totalDays = GetDaysInYear();
        
        // 年間をループ
        int dayIndex = Mathf.FloorToInt(animationTime) % totalDays;
        
        // 補間用の次の日
        int nextDayIndex = (dayIndex + 1) % totalDays;
        float t = animationTime - Mathf.Floor(animationTime);
        
        // 現在の日と次の日の太陽位置を計算
        var currentSolar = CalculateSolarPositionForDay(dayIndex + 1);
        var nextSolar = CalculateSolarPositionForDay(nextDayIndex + 1);
        
        var currentRot = SolarRotationConverter.CalculateSunRotation(currentSolar.elevation, currentSolar.azimuth);
        var nextRot = SolarRotationConverter.CalculateSunRotation(nextSolar.elevation, nextSolar.azimuth);
        
        // ローテーションを補間
        var interpolatedRotation = math.slerp(currentRot, nextRot, t);
        
        // 現在のデータを更新（補間された値として）
        currentSolarPosition = currentSolar; // 基準として現在の日を使用
        currentRotation = interpolatedRotation;
        
        // SunDirectionControllerに補間された角度を設定
        ApplySolarPositionInterpolated(currentSolar, nextSolar, t, dayIndex + 1);
    }
    
    /// <summary>
    /// 現在の日付のローテーションを表示
    /// </summary>
    void UpdateCurrentDayDisplay() {
        var today = DateTime.Today;
        CalculateAndApplySolarPosition(today.DayOfYear);
    }
    
    /// <summary>
    /// 太陽位置をSunDirectionControllerに適用
    /// </summary>
    void ApplySolarPosition(SolarPositionCalculator.SolarPosition solarPosition, int dayOfYear) {
        // SunDirectionControllerに角度を設定
        if (sunDirectionController != null)
            sunDirectionController.SetSunAngles(solarPosition.elevation, solarPosition.azimuth);
        
        // 日時UIを更新
        UpdateDateTimeUI(dayOfYear);
        
        // デバッグ情報（エディター表示用）
        var startDate = new DateTime(year, 1, 1);
        var targetDate = startDate.AddDays(dayOfYear - 1);
        
        debugInfo = $"日: {dayOfYear}/{GetDaysInYear()}\n" +
                   $"日付: {targetDate:yyyy-MM-dd}\n" +
                   $"高度: {solarPosition.elevation:F2}°\n" +
                   $"方位: {solarPosition.azimuth:F2}°\n" +
                   $"経度: {longitude:F4}°\n" +
                   $"オフセット: {GetTimezoneOffsetString()}";
    }
    
    /// <summary>
    /// 日時UIを更新
    /// </summary>
    void UpdateDateTimeUI(int dayOfYear) {
        if (!showDateTime || dateTimeText == null) return;
        
        var startDate = new DateTime(year, 1, 1);
        var targetDate = startDate.AddDays(dayOfYear - 1);
        var targetDateTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, targetHour, targetMinute, 0);
        
        dateTimeText.text = targetDateTime.ToString(dateTimeFormat);
    }
    
    /// <summary>
    /// 補間された太陽位置をSunDirectionControllerに適用
    /// </summary>
    void ApplySolarPositionInterpolated(SolarPositionCalculator.SolarPosition currentSolar, 
                                       SolarPositionCalculator.SolarPosition nextSolar, 
                                       float t, int dayOfYear) {
        // 高度角と方位角を補間
        var interpolatedElevation = Mathf.Lerp(currentSolar.elevation, nextSolar.elevation, t);
        var interpolatedAzimuth = Mathf.LerpAngle(currentSolar.azimuth, nextSolar.azimuth, t);
        
        // SunDirectionControllerに補間された角度を設定
        if (sunDirectionController != null)
            sunDirectionController.SetSunAngles(interpolatedElevation, interpolatedAzimuth);
        
        // 日時UIを更新
        UpdateDateTimeUI(dayOfYear);
        
        // デバッグ情報（エディター表示用）
        var startDate = new DateTime(year, 1, 1);
        var targetDate = startDate.AddDays(dayOfYear - 1);
        
        debugInfo = $"日: {dayOfYear}/{GetDaysInYear()}\n" +
                   $"日付: {targetDate:yyyy-MM-dd}\n" +
                   $"高度: {interpolatedElevation:F2}°\n" +
                   $"方位: {interpolatedAzimuth:F2}°\n" +
                   $"経度: {longitude:F4}°\n" +
                   $"オフセット: {GetTimezoneOffsetString()}";
    }
    

#if UNITY_EDITOR
    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(YearlySolarRotation))]
    public class YearlySolarRotationEditor : Editor {
        
        public override void OnInspectorGUI() {
            var solarRotation = (YearlySolarRotation)target;
            
            // デフォルトのインスペクターを表示
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== 制御パネル ===", EditorStyles.boldLabel);
            
            // リアルタイム計算状態の表示
            EditorGUILayout.LabelField("状態", "リアルタイム計算", EditorStyles.helpBox);
            
            // デバッグ情報表示
            if (!string.IsNullOrEmpty(solarRotation.debugInfo)) {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("現在の情報:", EditorStyles.boldLabel);
                var lines = solarRotation.debugInfo.Split('\n');
                foreach (var line in lines)
                    if (!string.IsNullOrEmpty(line))
                        EditorGUILayout.LabelField("  " + line);
            }
            
            EditorGUILayout.Space(10);
            
            // アニメーション制御
            EditorGUILayout.LabelField("アニメーション制御:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(solarRotation.isAnimating ? "停止" : "開始"))
                solarRotation.ToggleAnimation();
            
            if (GUILayout.Button("今日の位置"))
                solarRotation.animationTime = DateTime.Today.DayOfYear - 1;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            solarRotation.animationSpeed = EditorGUILayout.Slider("速度 (日/秒)", solarRotation.animationSpeed, 0.1f, 100f);
            
            EditorGUILayout.Space(10);
            
            // 手動制御ボタン
            EditorGUILayout.LabelField("手動制御:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("現在位置更新")) {
                solarRotation.UpdateSolarPosition();
                EditorUtility.SetDirty(solarRotation);
            }
            
            if (GUILayout.Button("統計計算"))
                Debug.Log(GetYearlyStatisticsString(solarRotation));
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("スペースキー: アニメーション切り替え\n" +
                                    "Sceneビューでギズモ表示", MessageType.Info);
            
            // リアルタイム更新
            if (Application.isPlaying)
                Repaint();
        }
        
        /// <summary>
        /// 一年分の統計をリアルタイム計算で取得
        /// </summary>
        string GetYearlyStatisticsString(YearlySolarRotation solarRotation) {
            float maxElevation = float.MinValue;
            float minElevation = float.MaxValue;
            DateTime maxElevationDate = DateTime.MinValue;
            DateTime minElevationDate = DateTime.MinValue;
            
            var daysInYear = solarRotation.GetDaysInYear();
            
            // 一年分を計算（サンプリング：10日ごと）
            for (int dayOfYear = 1; dayOfYear <= daysInYear; dayOfYear += 10) {
                var pos = solarRotation.CalculateSolarPositionForDay(dayOfYear);
                
                if (pos.elevation > maxElevation) {
                    maxElevation = pos.elevation;
                    maxElevationDate = pos.dateTime.DateTime;
                }
                if (pos.elevation < minElevation) {
                    minElevation = pos.elevation;
                    minElevationDate = pos.dateTime.DateTime;
                }
            }
            
            return $"=== 太陽位置統計（サンプリング） ===\n" +
                   $"最高高度: {maxElevation:F2}° ({maxElevationDate:yyyy-MM-dd})\n" +
                   $"最低高度: {minElevation:F2}° ({minElevationDate:yyyy-MM-dd})\n" +
                   $"計算年: {solarRotation.year} ({daysInYear}日)";
        }
    }
#endif
}
