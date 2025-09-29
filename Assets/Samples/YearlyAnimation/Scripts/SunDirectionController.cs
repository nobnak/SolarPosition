using UnityEngine;
using Unity.Mathematics;
using jp.nobnak.solar;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 太陽の高度角・方位角を入力として、自身のTransformのRotationを更新するコンポーネント
/// 将来的に太陽の輝度や色温度制御機能を追加するための基盤
/// </summary>
[ExecuteAlways]
public class SunDirectionController : MonoBehaviour {
    
    #region Fields & Properties
    [SerializeField] private Config config = new Config();
    Runtime rt = new();
    
    /// <summary>
    /// 設定へのアクセスを提供（クローンを返し、設定時に更新をトリガー）
    /// </summary>
    public Config Settings {
        get => config.Clone();
        set {
            if (value != null && !config.Equals(value)) {
                config = value.Clone();  // クローンを代入
                config.ValidateSettings();
                rt.valid = false;  // 設定変更により無効化
            }
        }
    }
    
    [Header("ライト制御")]
    [Tooltip("制御するDirectional Light")]
    public Light directionalLight;
    #endregion

    #region Unity Messages
    void OnEnable() {
        rt = new();
        // 設定値の検証・修正
        config.ValidateSettings();
        
        // Directional Lightが未設定の場合、自動で取得を試行
        if (directionalLight == null)
            directionalLight = GetComponent<Light>();
        
        // 初期ローテーションを適用
        UpdateSunRotation();
    }
    
    void Update() {
        // 設定が無効な場合は更新
        if (!rt.valid) {
            UpdateSunRotation();
            rt.valid = true;
        }
    }

    /// <summary>
    /// エディターで値が変更された際の検証
    /// </summary>
    void OnValidate() {
        config.ValidateSettings();
        if (rt != null)  // 初期化前の呼び出しを回避
            rt.valid = false;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 太陽の角度を設定（外部から呼び出し用）
    /// </summary>
    /// <param name="newElevation">高度角（度）</param>
    /// <param name="newAzimuth">方位角（度）</param>
    public void SetSunAngles(float newElevation, float newAzimuth) {
        var newConfig = Settings;  // クローンを取得
        newConfig.elevation = newElevation;
        newConfig.azimuth = newAzimuth;
        Settings = newConfig;  // Settings プロパティ経由で設定（自動的に rt.valid = false される）
        UpdateSunRotation();
    }
    
    /// <summary>
    /// 太陽の高度角・方位角からローテーションを計算して適用
    /// </summary>
    public void UpdateSunRotation() {
        var rotation = SolarRotationConverter.CalculateSunRotation(config.elevation, config.azimuth);
        transform.rotation = rotation; // 暗示的変換でUnityEngine.Quaternionに変換される
        
        // ライトプロパティを更新
        UpdateSunProperties();
    }
    
    /// <summary>
    /// 現在の太陽方向ベクトルを取得
    /// Unity.Mathematics版、Vector3への暗示的変換対応
    /// </summary>
    public float3 GetSunDirection() {
        var rotation = (quaternion)transform.rotation;
        return math.rotate(rotation, new float3(0, 0, 1)); // forward vector
    }
    
    /// <summary>
    /// 太陽が地平線より上にあるかチェック
    /// </summary>
    public bool IsSunAboveHorizon() => config.IsSunAboveHorizon();
    
    /// <summary>
    /// 太陽の状態文字列を取得
    /// </summary>
    public string GetSunStateString() => config.GetSunStateString();
    
    /// <summary>
    /// 現在の色温度を取得
    /// </summary>
    public float GetCurrentColorTemperature() => config.GetCurrentColorTemperature();
    #endregion

    #region Private Methods
    /// <summary>
    /// 太陽の輝度・色温度などの属性を更新
    /// </summary>
    void UpdateSunProperties() {
        if (directionalLight == null) return;
        
        // 高度角による輝度変化
        // elevation = -18度（天文薄明）でintensity = 0, elevation = 0度で最大値
        var intensityFactor = math.smoothstep(-18f, 0f, config.elevation);
        directionalLight.intensity = intensityFactor * config.maxIntensity;
        
        // 色温度を設定
        directionalLight.colorTemperature = config.GetCurrentColorTemperature();
        
        // 将来実装予定：
        // - Material プロパティの更新
    }
    #endregion



    #region Definitions
    /// <summary>
    /// 太陽制御設定を管理するConfig
    /// </summary>
    [System.Serializable]
    public class Config {
        [Header("太陽角度")]
        [Tooltip("太陽の高度角（地平線からの角度、度）")]
        [Range(-90f, 90f)]
        public float elevation = 0f;
        
        [Tooltip("太陽の方位角（北を0度とした時計回り、度）")]
        [Range(0f, 360f)]
        public float azimuth = 0f;
        
        [Header("ライト制御")]
        [Tooltip("太陽の最大強度")]
        [Range(0f, 5f)]
        public float maxIntensity = 1f;
        
        [Tooltip("頂点時の色温度（90度、ケルビン）")]
        [Range(2000f, 20000f)]
        public float zenithColorTemperature = 5778f; // 真昼の太陽色温度
        
        [Tooltip("水平線時の色温度（0度、ケルビン）")]
        [Range(1000f, 10000f)]
        public float horizonColorTemperature = 2000f; // 夕焼け色温度
        
        /// <summary>
        /// 設定値を検証・修正する
        /// </summary>
        public void ValidateSettings() {
            elevation = math.clamp(elevation, -90f, 90f);
            azimuth = math.clamp(azimuth, 0f, 360f);
            maxIntensity = math.max(0f, maxIntensity);
            zenithColorTemperature = math.clamp(zenithColorTemperature, 2000f, 20000f);
            horizonColorTemperature = math.clamp(horizonColorTemperature, 1000f, 10000f);
        }
        
        /// <summary>
        /// 太陽が地平線より上にあるかチェック
        /// </summary>
        public bool IsSunAboveHorizon() => elevation > 0f;
        
        /// <summary>
        /// 太陽の状態文字列を取得
        /// </summary>
        public string GetSunStateString() {
            if (elevation < -18) return "天文薄明前/後";
            if (elevation < -12) return "天文薄明";
            if (elevation < -6) return "航海薄明";
            if (elevation < 0) return "市民薄明";
            return "日中";
        }
        
        /// <summary>
        /// 現在の色温度を取得
        /// </summary>
        public float GetCurrentColorTemperature() {
            var elevationNormalized = math.clamp(elevation / 90f, 0f, 1f);
            return math.lerp(horizonColorTemperature, zenithColorTemperature, elevationNormalized);
        }
        
        /// <summary>
        /// この設定のクローンを作成
        /// </summary>
        public Config Clone() => new Config {
            elevation = this.elevation,
            azimuth = this.azimuth,
            maxIntensity = this.maxIntensity,
            zenithColorTemperature = this.zenithColorTemperature,
            horizonColorTemperature = this.horizonColorTemperature
        };
        
        /// <summary>
        /// 他の設定との等価性をチェック
        /// </summary>
        public bool Equals(Config other) {
            if (other == null) return false;
            return elevation == other.elevation &&
                   azimuth == other.azimuth &&
                   maxIntensity == other.maxIntensity &&
                   zenithColorTemperature == other.zenithColorTemperature &&
                   horizonColorTemperature == other.horizonColorTemperature;
        }
    }
    public class Runtime {
        public bool valid = false;
    }
    #endregion

    #region Editor
#if UNITY_EDITOR
    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(SunDirectionController))]
    public class SunDirectionControllerEditor : Editor {
        
        public override void OnInspectorGUI() {
            var sunController = (SunDirectionController)target;
            
            // デフォルトのインスペクターを表示
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== 太陽方向情報 ===", EditorStyles.boldLabel);
            
            // 現在の状態表示
            EditorGUILayout.LabelField("太陽の状態", sunController.GetSunStateString());
            EditorGUILayout.LabelField("地平線より上", sunController.IsSunAboveHorizon() ? "Yes" : "No");
            
            var sunDir = sunController.GetSunDirection();
            EditorGUILayout.LabelField("太陽方向", $"({sunDir.x:F2}, {sunDir.y:F2}, {sunDir.z:F2})");
            
            // ライト情報表示
            if (sunController.directionalLight != null) {
                EditorGUILayout.Space(5);
                var intensityFactor = math.smoothstep(-18f, 0f, sunController.Settings.elevation);
                var currentIntensity = intensityFactor * sunController.Settings.maxIntensity;
                EditorGUILayout.LabelField("現在の輝度", $"{currentIntensity:F3} ({intensityFactor * 100:F1}%)");
                
                var currentColorTemp = sunController.GetCurrentColorTemperature();
                EditorGUILayout.LabelField("現在の色温度", $"{currentColorTemp:F0}K");
                
                EditorGUILayout.LabelField("ライト色温度", $"{sunController.directionalLight.colorTemperature:F0}K");
                
                EditorGUILayout.LabelField("ライト名", sunController.directionalLight.name);
            } else {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Directional Lightが設定されていません", MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            
            // クイック設定ボタン
            EditorGUILayout.LabelField("クイック設定:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("正午")) {
                sunController.SetSunAngles(60f, 180f); // 南向き高度60度
                EditorUtility.SetDirty(sunController);
            }
            
            if (GUILayout.Button("夕方")) {
                sunController.SetSunAngles(10f, 270f); // 西向き低高度
                EditorUtility.SetDirty(sunController);
            }
            
            if (GUILayout.Button("夜")) {
                sunController.SetSunAngles(-30f, 0f); // 北向き地平線下
                EditorUtility.SetDirty(sunController);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("高度角: 地平線からの角度\n" +
                                    "方位角: 北=0°, 東=90°, 南=180°, 西=270°\n" +
                                    "輝度制御: -18°（天文薄明）で0、0°で最大値（smoothstep）\n" +
                                    "色温度制御: 0°で水平線色温度、90°で頂点色温度を線形補間\n" +
                                    "ライトはFilter and Temperatureモードで使用", MessageType.Info);
            
            // リアルタイム更新
            if (Application.isPlaying)
                Repaint();
        }
    }
#endif
    #endregion
}
