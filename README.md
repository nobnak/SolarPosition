# 太陽位置計算スクリプト (C# for Unity)

日付、緯度経度を入力して、地平座標系における太陽の角度（高度・方位角）を返すC#スクリプトです。

## 概要

このスクリプトは以下の機能を提供します：

- **太陽高度角**: 地平線からの角度（0-90度）
- **太陽方位角**: 北を0度とした時計回りの角度（0-360度）
- **太陽の状態**: 日中、薄明、夜間の判定

## ファイル構成

### メインファイル

- **`SolarPositionCalculator.cs`** - 太陽位置計算の核となるクラス
- **`SolarPositionDemo.cs`** - Unity Inspector上で使用できる実用的なデモ
- **`SolarPositionTest.cs`** - 動作確認用のテストスクリプト

## 使用方法

### 1. 基本的な使用方法

```csharp
using System;

// 2025年9月21日 12:00:00、東京での太陽位置を計算
DateTime dateTime = new DateTime(2025, 9, 21, 12, 0, 0);
float latitude = 35.6762f;   // 東京の緯度
float longitude = 139.6503f; // 東京の経度

var result = SolarPositionCalculator.Calculate(dateTime, latitude, longitude);

Debug.Log($"太陽高度角: {result.elevation}°");
Debug.Log($"太陽方位角: {result.azimuth}°");
Debug.Log($"太陽の状態: {result.GetSunState()}");
```

### 2. 現在時刻での計算

```csharp
// 現在時刻での太陽位置を計算
var currentResult = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f);
Debug.Log(currentResult.ToString());
```

## Unity での使用方法

### SolarPositionDemo の使用

1. `SolarPositionDemo.cs` をGameObjectにアタッチ
2. Inspectorで以下のパラメータを設定：
   - **日時設定**: 年、月、日、時、分
   - **位置設定**: 緯度、経度
   - **オプション**: 自動更新、現在時刻使用など

3. Play モードで実行すると自動的に太陽位置が計算されます

### 便利な機能

- **位置プリセット**: 主要都市の座標がプリセットされています
- **季節設定**: 夏至、冬至、春分、秋分の日付を簡単に設定
- **自動更新**: 指定間隔で自動的に再計算
- **現在時刻**: 現在時刻を使用した計算

### SolarPositionTest での動作確認

1. `SolarPositionTest.cs` をGameObjectにアタッチ
2. Play モードで実行すると、様々な条件でのテストが自動実行されます
3. Consoleで結果を確認できます

## API リファレンス

### SolarPositionCalculator クラス

#### メソッド

- **`Calculate(DateTime dateTime, float latitude, float longitude)`**
  - 指定された日時と位置での太陽位置を計算
  - 戻り値: `SolarPosition` 構造体

- **`CalculateNow(float latitude, float longitude)`**
  - 現在時刻での太陽位置を計算
  - 戻り値: `SolarPosition` 構造体

### SolarPosition 構造体

#### プロパティ

- **`elevation`** (float): 太陽高度角（度）
- **`azimuth`** (float): 太陽方位角（度）
- **`dateTime`** (DateTime): 計算に使用した日時
- **`latitude`** (float): 緯度
- **`longitude`** (float): 経度

#### メソッド

- **`ToString()`**: 結果を文字列で取得
- **`GetSunState()`**: 太陽の状態を文字列で取得

## 計算精度

このスクリプトは簡略化された計算式を使用しており、以下の精度を持ちます：

- **高度角**: ±0.5度程度の精度
- **方位角**: ±1.0度程度の精度

より高精度が必要な場合は、VSOP87理論やJPL ephemerisを使用した計算をお勧めします。

## パラメータ範囲

- **緯度**: -90° ～ +90°
- **経度**: -180° ～ +180°
- **日時**: .NET DateTime の対応範囲

## 使用例

### 主要都市の座標

```csharp
// 日本の主要都市
var tokyo = (lat: 35.6762f, lon: 139.6503f);      // 東京
var osaka = (lat: 34.6937f, lon: 135.5023f);      // 大阪
var sapporo = (lat: 43.0642f, lon: 141.3469f);    // 札幌
var naha = (lat: 26.2125f, lon: 127.6792f);       // 那覇

// 海外の主要都市
var newYork = (lat: 40.7128f, lon: -74.0060f);    // ニューヨーク
var london = (lat: 51.5074f, lon: -0.1278f);      // ロンドン
var paris = (lat: 48.8566f, lon: 2.3522f);       // パリ
var sydney = (lat: -33.8688f, lon: 151.2093f);   // シドニー
```

### 特別な日の計算

```csharp
// 夏至（6月21日頃）
var summerSolstice = SolarPositionCalculator.Calculate(
    new DateTime(2025, 6, 21, 12, 0, 0), 35.6762f, 139.6503f);

// 冬至（12月22日頃）
var winterSolstice = SolarPositionCalculator.Calculate(
    new DateTime(2025, 12, 22, 12, 0, 0), 35.6762f, 139.6503f);

// 春分（3月21日頃）
var vernalEquinox = SolarPositionCalculator.Calculate(
    new DateTime(2025, 3, 21, 12, 0, 0), 35.6762f, 139.6503f);

// 秋分（9月23日頃）
var autumnalEquinox = SolarPositionCalculator.Calculate(
    new DateTime(2025, 9, 23, 12, 0, 0), 35.6762f, 139.6503f);
```

## ライセンス

このスクリプトはパブリックドメインです。自由にご利用ください。

## 参考文献

- Astronomical Algorithms by Jean Meeus
- NOAA Solar Position Calculator
- Celestial Mechanics and Dynamical Astronomy

## 更新履歴

- 2025-09-21: 初回リリース
  - 基本的な太陽位置計算機能
  - Unity用デモスクリプト
  - テスト用スクリプト
