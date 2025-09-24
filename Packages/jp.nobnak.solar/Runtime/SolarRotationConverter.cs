using UnityEngine;
using Unity.Mathematics;

namespace jp.nobnak.solar {

    /// <summary>
    /// 太陽位置データをUnityのTransform回転に変換するユーティリティクラス
    /// </summary>
    public static class SolarRotationConverter {

        /// <summary>
        /// 太陽の高度角・方位角からローテーションを計算
        /// </summary>
        /// <param name="elevationDeg">高度角（度）</param>
        /// <param name="azimuthDeg">方位角（度、北=0°）</param>
        /// <returns>太陽方向を向くローテーション</returns>
        public static Quaternion CalculateSunRotation(float elevationDeg, float azimuthDeg) {
            // Unity.Mathematics使用でパフォーマンス最適化
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
        /// SolarPositionから直接ローテーションを計算
        /// </summary>
        /// <param name="solarPosition">太陽位置データ</param>
        /// <returns>太陽方向を向くローテーション</returns>
        public static Quaternion CalculateSunRotation(SolarPositionCalculator.SolarPosition solarPosition) =>
            CalculateSunRotation(solarPosition.elevation, solarPosition.azimuth);

        /// <summary>
        /// 高度角・方位角から太陽方向ベクトルを計算
        /// </summary>
        /// <param name="elevationDeg">高度角（度）</param>
        /// <param name="azimuthDeg">方位角（度、北=0°）</param>
        /// <returns>正規化された太陽方向ベクトル</returns>
        public static Vector3 CalculateSunDirection(float elevationDeg, float azimuthDeg) {
            var elevationRad = math.radians(elevationDeg);
            var azimuthRad = math.radians(azimuthDeg);
            
            return new Vector3(
                math.sin(azimuthRad) * math.cos(elevationRad), // X: 東西
                math.sin(elevationRad),                        // Y: 上下
                math.cos(azimuthRad) * math.cos(elevationRad)  // Z: 南北
            );
        }

        /// <summary>
        /// SolarPositionから直接太陽方向ベクトルを計算
        /// </summary>
        /// <param name="solarPosition">太陽位置データ</param>
        /// <returns>正規化された太陽方向ベクトル</returns>
        public static Vector3 CalculateSunDirection(SolarPositionCalculator.SolarPosition solarPosition) =>
            CalculateSunDirection(solarPosition.elevation, solarPosition.azimuth);
    }
}
