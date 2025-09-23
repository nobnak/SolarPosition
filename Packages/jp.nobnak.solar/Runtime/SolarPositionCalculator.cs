using System;
using UnityEngine;

namespace jp.nobnak.solar {

    /// <summary>
    /// 太陽位置計算クラス
    /// 日付、緯度経度から地平座標系における太陽の角度（高度・方位角）を計算する
    /// </summary>
    public static class SolarPositionCalculator {

    #region Definitions
    
    /// <summary>
    /// 太陽位置の計算結果を格納する構造体
    /// </summary>
    [System.Serializable]
    public struct SolarPosition {
        [Header("太陽の角度")]
        [Tooltip("太陽の高度角（地平線からの角度、度）")]
        public float elevation;
        
        [Tooltip("太陽の方位角（北を0度とした時計回り、度）")]
        public float azimuth;
        
        [Header("計算パラメータ")]
        [Tooltip("計算に使用した日時")]
        public DateTimeOffset dateTime;
        
        [Tooltip("緯度（度）")]
        public float latitude;
        
        [Tooltip("経度（度）")]
        public float longitude;
        
        /// <summary>
        /// 結果を文字列として取得
        /// </summary>
        public override string ToString() {
            return $"太陽位置 - 高度: {elevation:F2}°, 方位: {azimuth:F2}° " +
                   $"(日時: {dateTime:yyyy-MM-dd HH:mm:ss zzz}, 位置: {latitude:F4}°, {longitude:F4}°)";
        }
        
        /// <summary>
        /// 太陽の状態を文字列で取得
        /// </summary>
        public string GetSunState() {
            if (elevation < -18) return "天文薄明前/後 (太陽は地平線下18°以下)";
            if (elevation < -12) return "天文薄明 (太陽は地平線下12°～18°)";
            if (elevation < -6) return "航海薄明 (太陽は地平線下6°～12°)";
            if (elevation < 0) return "市民薄明 (太陽は地平線下0°～6°)";
            if (elevation >= 0) return "日中 (太陽は地平線上)";
            return "不明";
        }
    }
    
    #endregion

    #region Public Interface

    /// <summary>
    /// 指定された日時と位置での太陽位置を計算
    /// </summary>
    /// <param name="dateTime">計算する日時（タイムゾーン情報を含む）</param>
    /// <param name="latitude">緯度（度、-90～+90）</param>
    /// <param name="longitude">経度（度、-180～+180）</param>
    /// <returns>太陽位置の計算結果</returns>
    public static SolarPosition Calculate(DateTimeOffset dateTime, float latitude, float longitude)
    {
        // 入力値の検証
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException($"緯度は-90から90の範囲で指定してください: {latitude}");
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException($"経度は-180から180の範囲で指定してください: {longitude}");
        
        // ユリウス日の計算（UTCで）
        double julianDay = GetJulianDay(dateTime);
        
        // 太陽の赤道座標を計算
        var (rightAscension, declination) = GetSolarEquatorialCoordinates(julianDay);
        
        // 地方時角を計算
        double localHourAngle = GetLocalHourAngle(julianDay, longitude, rightAscension);
        
        // 地平座標に変換
        var (elevation, azimuth) = EquatorialToHorizontal(
            rightAscension, declination, localHourAngle, latitude);
        
        return new SolarPosition
        {
            elevation = (float)elevation,
            azimuth = (float)azimuth,
            dateTime = dateTime,
            latitude = latitude,
            longitude = longitude
        };
    }
    
    /// <summary>
    /// 現在時刻での太陽位置を計算
    /// </summary>
    /// <param name="latitude">緯度（度）</param>
    /// <param name="longitude">経度（度）</param>
    /// <returns>太陽位置の計算結果</returns>
    public static SolarPosition CalculateNow(float latitude, float longitude)
    {
        return Calculate(DateTimeOffset.Now, latitude, longitude);
    }
    
    /// <summary>
    /// 指定されたタイムゾーンでの現在時刻での太陽位置を計算
    /// </summary>
    /// <param name="latitude">緯度（度）</param>
    /// <param name="longitude">経度（度）</param>
    /// <param name="timeZone">タイムゾーン</param>
    /// <returns>太陽位置の計算結果</returns>
    public static SolarPosition CalculateNow(float latitude, float longitude, TimeZoneInfo timeZone)
    {
        DateTimeOffset now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        return Calculate(now, latitude, longitude);
    }
    
    #endregion

    #region Private Implementation

    /// <summary>
    /// ユリウス日を計算（UTCベース）
    /// </summary>
    private static double GetJulianDay(DateTimeOffset dateTimeOffset) {
        // UTC時刻で計算
        DateTime utcDateTime = dateTimeOffset.UtcDateTime;
        
        int year = utcDateTime.Year;
        int month = utcDateTime.Month;
        int day = utcDateTime.Day;
        double hour = utcDateTime.Hour + utcDateTime.Minute / 60.0 + utcDateTime.Second / 3600.0 + utcDateTime.Millisecond / 3600000.0;
        
        if (month <= 2) {
            year -= 1;
            month += 12;
        }
        
        int a = year / 100;
        int b = 2 - a + a / 4;
        
        double jd = Math.Floor(365.25 * (year + 4716)) + 
                   Math.Floor(30.6001 * (month + 1)) + 
                   day + hour / 24.0 + b - 1524.5;
        
        return jd;
    }
    
    /// <summary>
    /// 太陽の赤道座標を計算
    /// </summary>
    private static (double rightAscension, double declination) GetSolarEquatorialCoordinates(double julianDay) {
        // J2000.0からの経過日数
        double n = julianDay - 2451545.0;
        
        // 太陽の平均黄経
        double L = (280.460 + 0.9856474 * n) % 360;
        if (L < 0) L += 360;
        
        // 太陽の平均近点角
        double g = Deg2Rad((357.528 + 0.9856003 * n) % 360);
        
        // 真黄経
        double lambda = Deg2Rad(L + 1.915 * Math.Sin(g) + 0.020 * Math.Sin(2 * g));
        
        // 黄道傾斜角
        double epsilon = Deg2Rad(23.439 - 0.0000004 * n);
        
        // 赤道座標に変換
        double rightAscension = Math.Atan2(Math.Cos(epsilon) * Math.Sin(lambda), Math.Cos(lambda));
        double declination = Math.Asin(Math.Sin(epsilon) * Math.Sin(lambda));
        
        // 角度を0-2πの範囲に正規化
        if (rightAscension < 0) rightAscension += 2 * Math.PI;
        
        return (rightAscension, declination);
    }
    
    /// <summary>
    /// 地方時角を計算
    /// </summary>
    private static double GetLocalHourAngle(double julianDay, double longitude, double rightAscension) {
        // グリニッジ恒星時
        double T = (julianDay - 2451545.0) / 36525.0;
        double gst = (280.46061837 + 360.98564736629 * (julianDay - 2451545.0) + 
                     0.000387933 * T * T - T * T * T / 38710000.0) % 360;
        
        if (gst < 0) gst += 360;
        
        // 地方恒星時
        double lst = (gst + longitude) % 360;
        if (lst < 0) lst += 360;
        
        // 地方時角
        double hourAngle = Deg2Rad(lst) - rightAscension;
        
        return hourAngle;
    }
    
    /// <summary>
    /// 赤道座標から地平座標に変換
    /// </summary>
    private static (double elevation, double azimuth) EquatorialToHorizontal(
        double rightAscension, double declination, double hourAngle, double latitude) {
        double latRad = Deg2Rad(latitude);
        
        // 高度の計算
        double elevation = Math.Asin(
            Math.Sin(declination) * Math.Sin(latRad) + 
            Math.Cos(declination) * Math.Cos(latRad) * Math.Cos(hourAngle));
        
        // 方位角の計算
        double azimuth = Math.Atan2(
            -Math.Sin(hourAngle),
            Math.Tan(declination) * Math.Cos(latRad) - Math.Sin(latRad) * Math.Cos(hourAngle));
        
        // 角度を度に変換し、方位角を0-360度に正規化
        elevation = Rad2Deg(elevation);
        azimuth = Rad2Deg(azimuth);
        
        if (azimuth < 0) azimuth += 360;
        
        return (elevation, azimuth);
    }
    
    /// <summary>
    /// 度からラジアンに変換
    /// </summary>
    private static double Deg2Rad(double degrees) {
        return degrees * Math.PI / 180.0;
    }
    
    /// <summary>
    /// ラジアンから度に変換
    /// </summary>
    private static double Rad2Deg(double radians) {
        return radians * 180.0 / Math.PI;
    }

    #endregion
    }
}
