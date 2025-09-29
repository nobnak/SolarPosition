using System;
using UnityEngine;
using jp.nobnak.solar;

/// <summary>
/// 太陽位置計算のデモスクリプト
/// Unity Inspector上で日時と位置を設定して太陽位置を計算・表示する
/// ExecuteAlways により、エディタモードでも動作する
/// </summary>
[ExecuteAlways]
public class SolarPositionDemo : MonoBehaviour {
    
    [Header("設定")]
    [SerializeField]
    [Tooltip("太陽位置計算の設定")]
    private Config config = new Config();
    
    [Header("出力・プリセット設定")]
    [SerializeField]
    [Tooltip("出力設定とプリセット")]
    private Preset preset = new Preset();
    
    [Header("計算結果")]
    [SerializeField]
    [Tooltip("計算結果")]
    private SolarPositionCalculator.SolarPosition solarPosition;
    
    private float lastUpdateTime;
    
    
    #region Unity Lifecycle
    
    void OnEnable() {
        // オブジェクトがアクティブになった時の初期化
        InitializeDemo();
    }
    
    void OnDisable() {
        // オブジェクトが非アクティブになった時のクリーンアップ
        CleanupDemo();
    }
    
    void Update() {
        // 自動更新はPlayモードでのみ動作
        if (Application.isPlaying && config.autoUpdate && Time.time - lastUpdateTime >= config.updateInterval) {
            CalculateSolarPosition();
            lastUpdateTime = Time.time;
        }
    }
    
    void OnValidate() {
        // Inspector上で値が変更されたときに自動計算
        // ExecuteAlwaysによりエディタモードでも動作
        if (config != null && preset != null) {
            CalculateSolarPosition();
        }
    }

    #endregion

    #region Private Implementation

    /// <summary>
    /// デモの初期化処理
    /// </summary>
    private void InitializeDemo() {
        // 設定の検証
        if (!ValidateConfig()) {
            Debug.LogWarning("設定に問題があります。デフォルト値を適用します。");
            config.ResetToTokyo();
        }
        
        // 最終更新時間をリセット
        lastUpdateTime = Time.time;
        
        // 初回計算
        CalculateSolarPosition();
        
        string mode = Application.isPlaying ? "Playモード" : "エディタモード";
        Debug.Log($"SolarPositionDemo が有効になりました ({mode})");
    }
    
    /// <summary>
    /// 設定の妥当性を検証
    /// </summary>
    /// <returns>設定が有効かどうか</returns>
    private bool ValidateConfig() {
        if (config == null) {
            Debug.LogError("Config が null です");
            return false;
        }
        
        // 日付の検証
        try {
            var testDate = new DateTime(config.year, config.month, config.day);
        } catch (ArgumentOutOfRangeException) {
            Debug.LogError($"無効な日付です: {config.year}/{config.month}/{config.day}");
            return false;
        }
        
        // 緯度経度の検証
        if (config.latitude < -90f || config.latitude > 90f) {
            Debug.LogError($"緯度が範囲外です: {config.latitude}");
            return false;
        }
        
        if (config.longitude < -180f || config.longitude > 180f) {
            Debug.LogError($"経度が範囲外です: {config.longitude}");
            return false;
        }
        
        // 更新間隔の検証
        if (config.updateInterval <= 0f) {
            Debug.LogError($"更新間隔が無効です: {config.updateInterval}");
            return false;
        }
        
        // Transform出力の検証
        if (!preset.ValidateTransformOutput()) {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// デモのクリーンアップ処理
    /// </summary>
    private void CleanupDemo() {
        string mode = Application.isPlaying ? "Playモード" : "エディタモード";
        Debug.Log($"SolarPositionDemo が無効になりました ({mode})");
    }
    
    /// <summary>
    /// 太陽位置をTransformのローテーションに変換して適用
    /// </summary>
    /// <param name="solarPos">太陽位置データ</param>
    private void ApplyRotationToTransforms(SolarPositionCalculator.SolarPosition solarPos) {
        if (!preset.enableTransformOutput || preset.targetTransforms == null)
            return;
            
        // オフセットを適用した角度を計算
        float adjustedAzimuth = solarPos.azimuth + preset.azimuthOffset;
        float adjustedElevation = solarPos.elevation + preset.elevationOffset;
        
        // 方位角を-180~180度の範囲に正規化
        while (adjustedAzimuth > 180f) adjustedAzimuth -= 360f;
        while (adjustedAzimuth <= -180f) adjustedAzimuth += 360f;
        
        // 高度角を-90~90度の範囲にクランプ
        adjustedElevation = Mathf.Clamp(adjustedElevation, -90f, 90f);
        
        // Quaternion回転を作成（Unity.Mathematics最適化、暗示的変換でTransform.rotationに対応）
        var rotation = SolarRotationConverter.CalculateSunRotation(adjustedElevation, adjustedAzimuth);
        
        // 対象のTransformsに適用
        int successCount = 0;
        foreach (var transform in preset.targetTransforms) {
            if (transform != null) {
                transform.rotation = rotation;
                successCount++;
            }
        }
        
        if (successCount > 0) {
            Debug.Log($"太陽の回転を{successCount}個のTransformに適用しました " +
                     $"(方位角: {adjustedAzimuth:F1}°, 高度: {adjustedElevation:F1}°)");
        }
    }
    
    #endregion

    #region Public Interface

    /// <summary>
    /// 太陽位置を計算して結果を更新
    /// </summary>
    public void CalculateSolarPosition() {
        // オブジェクトがアクティブでない場合は計算しない
        if (!gameObject.activeInHierarchy)
            return;
            
        // エディタモードでのnullチェックを強化
        if (config == null || preset == null) {
            Debug.LogWarning("設定が初期化されていません");
            return;
        }
            
        try {
            DateTimeOffset dateTime = config.GetDateTimeOffset();
            
            solarPosition = SolarPositionCalculator.Calculate(dateTime, config.latitude, config.longitude);
            
            // Transformに回転を適用
            ApplyRotationToTransforms(solarPosition);
            
            string mode = Application.isPlaying ? "[Runtime]" : "[Editor]";
            Debug.Log($"{mode} 太陽位置計算完了: {solarPosition}");
            Debug.Log($"{mode} 太陽の状態: {solarPosition.GetSunState()}");
        } catch (ArgumentOutOfRangeException e) {
            Debug.LogError($"日時設定エラー: {e.Message}");
        } catch (ArgumentException e) {
            Debug.LogError($"座標設定エラー: {e.Message}");
        } catch (Exception e) {
            Debug.LogError($"太陽位置計算エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// 強制的に計算を実行（デバッグ用）
    /// </summary>
    [ContextMenu("強制計算実行")]
    public void ForceCalculate() {
        if (ValidateConfig()) {
            CalculateSolarPosition();
        }
        else
        {
            Debug.LogError("設定が無効なため計算できません");
        }
    }
    
    
    /// <summary>
    /// Transform出力のテスト用メソッド
    /// </summary>
    [ContextMenu("Transform出力テスト")]
    public void TestTransformOutput() {
        if (preset.targetTransforms == null || preset.targetTransforms.Length == 0) {
            Debug.LogWarning("Transform出力のテスト: 対象Transformが設定されていません");
            return;
        }
        
        // テスト用の太陽位置を作成
        var testPosition = new SolarPositionCalculator.SolarPosition
        {
            azimuth = 180f, // 南向き
            elevation = 45f, // 45度の高度
            latitude = config.latitude,
            longitude = config.longitude,
            dateTime = DateTimeOffset.Now
        };
        
        ApplyRotationToTransforms(testPosition);
        Debug.Log("Transform出力テスト実行: 南向き45度の太陽位置を適用しました");
    }
    
    /// <summary>
    /// プリセット位置を適用
    /// </summary>
    /// <param name="presetIndex">プリセットのインデックス</param>
    public void ApplyLocationPreset(int presetIndex) {
        if (preset.ApplyLocationPreset(presetIndex, ref config.latitude, ref config.longitude)) {
            var locationPreset = preset.locationPresets[presetIndex];
            Debug.Log($"位置プリセット適用: {locationPreset.name} ({locationPreset.latitude}, {locationPreset.longitude})");
            CalculateSolarPosition();
        }
        else
        {
            Debug.LogError($"無効なプリセットインデックス: {presetIndex}");
        }
    }
    
    /// <summary>
    /// 現在時刻に設定
    /// </summary>
    public void SetCurrentTime() {
        config.UpdateToCurrentTime();
        
        DateTimeOffset now = DateTimeOffset.Now;
        Debug.Log($"現在時刻に設定: {now:yyyy-MM-dd HH:mm:ss zzz}");
        
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 夏至の日に設定（6月21日）
    /// </summary>
    public void SetSummerSolstice() {
        config.month = 6;
        config.day = 21;
        Debug.Log("夏至の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 冬至の日に設定（12月22日）
    /// </summary>
    public void SetWinterSolstice() {
        config.month = 12;
        config.day = 22;
        Debug.Log("冬至の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 春分の日に設定（3月21日）
    /// </summary>
    public void SetVernalEquinox() {
        config.month = 3;
        config.day = 21;
        Debug.Log("春分の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 秋分の日に設定（9月23日）
    /// </summary>
    public void SetAutumnalEquinox() {
        config.month = 9;
        config.day = 23;
        Debug.Log("秋分の日に設定");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// 結果をCSV形式で出力
    /// </summary>
    public void ExportToCSV() {
        string csv = $"日時,緯度,経度,高度角,方位角,太陽の状態\n";
        csv += $"{solarPosition.dateTime:yyyy-MM-dd HH:mm:ss},{solarPosition.latitude},{solarPosition.longitude},";
        csv += $"{solarPosition.elevation},{solarPosition.azimuth},{solarPosition.GetSunState()}\n";
        
        Debug.Log("CSV出力:");
        Debug.Log(csv);
        
        // ファイルに保存する場合はここに追加
        // System.IO.File.WriteAllText("solar_position.csv", csv);
    }
    
    /// <summary>
    /// 設定をデフォルト値にリセット
    /// </summary>
    [ContextMenu("設定をリセット")]
    public void ResetConfig() {
        config.ResetToTokyo();
        string mode = Application.isPlaying ? "[Runtime]" : "[Editor]";
        Debug.Log($"{mode} 設定を東京のデフォルト値にリセットしました");
        CalculateSolarPosition();
    }
    
    /// <summary>
    /// エディタ専用: Transform出力設定をリセット
    /// </summary>
    [ContextMenu("Transform出力設定をリセット")]
    public void ResetTransformOutputConfig() {
        preset.ResetTransformOutput();
        string mode = Application.isPlaying ? "[Runtime]" : "[Editor]";
        Debug.Log($"{mode} Transform出力設定をリセットしました");
    }
    
    /// <summary>
    /// 太陽位置から回転を作成
    /// Unity標準のYアップ座標系に基づく回転
    /// </summary>
    
    #endregion

    #region Definitions
    
    /// <summary>
    /// 設定項目を整理するConfigクラス
    /// </summary>
    [System.Serializable]
    public class Config {
        [Header("日時設定")]
        [SerializeField]
        [Tooltip("計算する年")]
        public int year = 2025;
        
        [SerializeField]
        [Tooltip("計算する月 (1-12)")]
        [Range(1, 12)]
        public int month = 9;
        
        [SerializeField]
        [Tooltip("計算する日 (1-31)")]
        [Range(1, 31)]
        public int day = 21;
        
        [SerializeField]
        [Tooltip("計算する時間 (0-23)")]
        [Range(0, 23)]
        public int hour = 12;
        
        [SerializeField]
        [Tooltip("計算する分 (0-59)")]
        [Range(0, 59)]
        public int minute = 0;
        
        [Header("位置設定")]
        [SerializeField]
        [Tooltip("緯度（度、-90～+90）")]
        [Range(-90f, 90f)]
        public float latitude = 35.6762f; // 東京の緯度
        
        [SerializeField]
        [Tooltip("経度（度、-180～+180）")]
        [Range(-180f, 180f)]
        public float longitude = 139.6503f; // 東京の経度
        
        [Header("動作設定")]
        [SerializeField]
        [Tooltip("自動更新を有効にする")]
        public bool autoUpdate = true;
        
        [SerializeField]
        [Tooltip("現在時刻を使用する")]
        public bool useCurrentTime = false;
        
        [SerializeField]
        [Tooltip("更新間隔（秒）")]
        [Range(0.1f, 10f)]
        public float updateInterval = 1.0f;
        
        /// <summary>
        /// 設定をリセットして東京の初期値に戻す
        /// </summary>
        public void ResetToTokyo() {
            latitude = 35.6762f;
            longitude = 139.6503f;
            year = System.DateTime.Now.Year;
            month = System.DateTime.Now.Month;
            day = System.DateTime.Now.Day;
            hour = 12;
            minute = 0;
        }
        
        /// <summary>
        /// 現在時刻で設定を更新
        /// </summary>
        public void UpdateToCurrentTime() {
            var now = System.DateTimeOffset.Now;
            year = now.Year;
            month = now.Month;
            day = now.Day;
            hour = now.Hour;
            minute = now.Minute;
        }
        
        /// <summary>
        /// 設定値を検証・修正する
        /// </summary>
        public void ValidateSettings() {
            // 基本的な範囲チェック
            year = Mathf.Max(1, year);
            month = Mathf.Clamp(month, 1, 12);
            hour = Mathf.Clamp(hour, 0, 23);
            minute = Mathf.Clamp(minute, 0, 59);
            
            // 月に応じて日数を制限
            int maxDays = System.DateTime.DaysInMonth(year, month);
            day = Mathf.Clamp(day, 1, maxDays);
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディターで値が変更された際の検証
        /// </summary>
        void OnValidate() => ValidateSettings();
#endif
        
        /// <summary>
        /// DateTimeOffsetを取得
        /// </summary>
        /// <returns>設定に基づくDateTimeOffset</returns>
        public DateTimeOffset GetDateTimeOffset() {
            if (useCurrentTime) {
                return DateTimeOffset.Now;
            } else {
                ValidateSettings(); // 設定値を検証・修正
                DateTime localDateTime = new DateTime(year, month, day, hour, minute, 0);
                return new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
            }
        }
    }

    /// <summary>
    /// 出力・プリセット設定を管理するクラス
    /// </summary>
    [System.Serializable]
    public class Preset {
        [Header("Transform出力")]
        [SerializeField]
        [Tooltip("太陽位置をローテーションに反映するTransforms")]
        public Transform[] targetTransforms = new Transform[0];
        
        [SerializeField]
        [Tooltip("Transform出力を有効にする")]
        public bool enableTransformOutput = true;
        
        [SerializeField]
        [Tooltip("方位角のオフセット（度）")]
        [Range(-180f, 180f)]
        public float azimuthOffset = 0f;
        
        [SerializeField]
        [Tooltip("高度角のオフセット（度）")]
        [Range(-90f, 90f)]
        public float elevationOffset = 0f;
        
        
        [Header("ロケーションプリセット")]
        [SerializeField]
        [Tooltip("有名な場所のプリセット")]
        public LocationPreset[] locationPresets = new LocationPreset[]
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
        
        /// <summary>
        /// プリセット位置を適用
        /// </summary>
        /// <param name="presetIndex">プリセットのインデックス</param>
        /// <param name="latitude">緯度（参照渡し）</param>
        /// <param name="longitude">経度（参照渡し）</param>
        /// <returns>適用に成功したかどうか</returns>
        public bool ApplyLocationPreset(int presetIndex, ref float latitude, ref float longitude) {
            if (presetIndex >= 0 && presetIndex < locationPresets.Length) {
                var locationPreset = locationPresets[presetIndex];
                latitude = locationPreset.latitude;
                longitude = locationPreset.longitude;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Transform出力設定をリセット
        /// </summary>
        public void ResetTransformOutput() {
            targetTransforms = new Transform[0];
            enableTransformOutput = true;
            azimuthOffset = 0f;
            elevationOffset = 0f;
        }
        
        /// <summary>
        /// Transform出力の妥当性を検証
        /// </summary>
        /// <returns>検証結果</returns>
        public bool ValidateTransformOutput() {
            if (!enableTransformOutput) return true;
            
            if (targetTransforms == null) {
                Debug.LogWarning("targetTransforms が null です");
                return false;
            }
            
            int nullCount = 0;
            foreach (var transform in targetTransforms) {
                if (transform == null) nullCount++;
            }
            
            if (nullCount > 0) {
                Debug.LogWarning($"Transform出力: {nullCount}個のnullTransformがあります");
            }
            
            return true;
        }
    }

    /// <summary>
    /// ロケーションプリセット用のデータクラス
    /// </summary>
    [System.Serializable]
    public class LocationPreset {
        [Tooltip("場所の名前")]
        public string name;
        
        [Tooltip("緯度")]
        public float latitude;
        
        [Tooltip("経度")]
        public float longitude;
        
        public LocationPreset(string name, float latitude, float longitude) {
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
    
    #endregion
}
