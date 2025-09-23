using UnityEngine;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 太陽の高度角・方位角を入力として、自身のTransformのRotationを更新するコンポーネント
/// 将来的に太陽の輝度や色温度制御機能を追加するための基盤
/// </summary>
[ExecuteAlways]
public class SunDirectionController : MonoBehaviour {
    
    [Header("太陽角度")]
    [Tooltip("太陽の高度角（地平線からの角度、度）")]
    [Range(-90f, 90f)]
    public float elevation = 0f;
    
    [Tooltip("太陽の方位角（北を0度とした時計回り、度）")]
    [Range(0f, 360f)]
    public float azimuth = 0f;
    
    
    [Header("ライト制御")]
    [Tooltip("制御するDirectional Light")]
    public Light directionalLight;
    
    [Tooltip("太陽の最大強度")]
    [Range(0f, 5f)]
    public float maxIntensity = 1f;
    
    [Tooltip("頂点時の色温度（90度、ケルビン）")]
    [Range(2000f, 20000f)]
    public float zenithColorTemperature = 5778f; // 真昼の太陽色温度
    
    [Tooltip("水平線時の色温度（0度、ケルビン）")]
    [Range(1000f, 10000f)]
    public float horizonColorTemperature = 2000f; // 夕焼け色温度
    
    // 角度変更検知用
    private float lastElevation = float.NaN;
    private float lastAzimuth = float.NaN;
    
    void OnEnable() {
        // 初期角度を記録
        StoreLastAngles();
        
        // Directional Lightが未設定の場合、自動で取得を試行
        if (directionalLight == null)
            directionalLight = GetComponent<Light>();
        
        // 初期ローテーションを適用
        UpdateSunRotation();
    }
    
    void Update() {
        // 角度変更チェック
        if (HasAnglesChanged()) {
            UpdateSunRotation();
            StoreLastAngles();
        }
    }
    
    /// <summary>
    /// 角度が変更されたかチェック
    /// </summary>
    bool HasAnglesChanged() => 
        elevation != lastElevation || azimuth != lastAzimuth;
    
    /// <summary>
    /// 現在の角度を記録
    /// </summary>
    void StoreLastAngles() {
        lastElevation = elevation;
        lastAzimuth = azimuth;
    }
    
    /// <summary>
    /// 太陽の角度を設定（外部から呼び出し用）
    /// </summary>
    /// <param name="newElevation">高度角（度）</param>
    /// <param name="newAzimuth">方位角（度）</param>
    public void SetSunAngles(float newElevation, float newAzimuth) {
        elevation = newElevation;
        azimuth = newAzimuth;
        UpdateSunRotation();
    }
    
    /// <summary>
    /// 太陽の高度角・方位角からローテーションを計算して適用
    /// </summary>
    public void UpdateSunRotation() {
        var rotation = CalculateSunRotation(elevation, azimuth);
        transform.rotation = rotation;
        
        // ライトプロパティを更新
        UpdateSunProperties();
    }
    
    /// <summary>
    /// 太陽の高度角・方位角からローテーションを計算
    /// </summary>
    /// <param name="elevationDeg">高度角（度）</param>
    /// <param name="azimuthDeg">方位角（度、北=0°）</param>
    /// <returns>太陽方向を向くローテーション</returns>
    public static Quaternion CalculateSunRotation(float elevationDeg, float azimuthDeg) {
        // Unity.Mathematics使用
        var elevationRad = math.radians(elevationDeg);
        var azimuthRad = math.radians(azimuthDeg);
        
        // 太陽の方向ベクトルを計算（Unityの座標系：Y=上、Z=北）
        var sunDirection = new float3(
            math.sin(azimuthRad) * math.cos(elevationRad), // X: 東西
            math.sin(elevationRad),                        // Y: 上下
            math.cos(azimuthRad) * math.cos(elevationRad)  // Z: 南北
        );
        
        // 太陽方向を向くローテーションを計算
        return Quaternion.LookRotation(sunDirection, Vector3.up);
    }
    
    /// <summary>
    /// 現在の太陽方向ベクトルを取得
    /// </summary>
    public Vector3 GetSunDirection() {
        return transform.rotation * Vector3.forward;
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
    /// 太陽の輝度・色温度などの属性を更新
    /// </summary>
    void UpdateSunProperties() {
        if (directionalLight == null) return;
        
        // 高度角による輝度変化
        // elevation = -18度（天文薄明）でintensity = 0, elevation = 0度で最大値
        var intensityFactor = math.smoothstep(-18f, 0f, elevation);
        directionalLight.intensity = intensityFactor * maxIntensity;
        
        // 高度角による色温度変化
        // elevation = 0度で horizonColorTemperature, elevation = 90度で zenithColorTemperature
        var elevationNormalized = math.clamp(elevation / 90f, 0f, 1f);
        var currentColorTemperature = math.lerp(horizonColorTemperature, zenithColorTemperature, elevationNormalized);
        
        // Directional LightのTemperatureプロパティを設定
        directionalLight.colorTemperature = currentColorTemperature;
        
        // 将来実装予定：
        // - Material プロパティの更新
    }
    
    
    /// <summary>
    /// 現在の色温度を取得
    /// </summary>
    public float GetCurrentColorTemperature() {
        var elevationNormalized = math.clamp(elevation / 90f, 0f, 1f);
        return math.lerp(horizonColorTemperature, zenithColorTemperature, elevationNormalized);
    }

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
                var intensityFactor = math.smoothstep(-18f, 0f, sunController.elevation);
                var currentIntensity = intensityFactor * sunController.maxIntensity;
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
}
