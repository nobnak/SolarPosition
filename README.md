# Solar Position Calculator

A Unity package for calculating solar position (elevation and azimuth angles) based on date, time, latitude, and longitude.

[![Example](http://img.youtube.com/vi/yEXLL2l7BfA/hqdefault.jpg)](https://youtu.be/yEXLL2l7BfA)

## Features

- **Accurate Solar Position**: Calculate solar elevation and azimuth angles using astronomical algorithms
- **Timezone Support**: Proper timezone handling with `DateTimeOffset` 
- **Unity Integration**: Ready-to-use components and demo scripts
- **Real-time Calculation**: Current time and scheduled updates support

## Installation

### Via OpenUPM (Recommended)

1. Open **Edit** → **Project Settings**
2. Select **Package Manager** in the left panel
3. Click **+** to add a new scoped registry:
   - **Name**: `OpenUPM`
   - **URL**: `https://package.openupm.com`
   - **Scope**: `jp.nobnak`
4. Click **Save**
5. Open **Window** → **Package Manager**
6. Change dropdown from "Unity Registry" to "My Registries"
7. Search for `jp.nobnak.solar` and click **Install**

## Quick Start

```csharp
using jp.nobnak.solar;
using System;

// Calculate solar position
DateTimeOffset dateTime = new DateTimeOffset(2025, 6, 21, 12, 0, 0, TimeSpan.FromHours(9)); // JST
var result = SolarPositionCalculator.Calculate(dateTime, 35.6762f, 139.6503f); // Tokyo

Debug.Log($"Elevation: {result.elevation:F2}°, Azimuth: {result.azimuth:F2}°");
Debug.Log($"Sun State: {result.GetSunState()}");

// For current time
var now = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f);
```

## API Reference

### SolarPositionCalculator (Static Class)

| Method | Description |
|--------|-------------|
| `Calculate(DateTimeOffset, float, float)` | Calculate for specific date/time with timezone |
| `CalculateNow(float, float)` | Calculate for current local time |
| `CalculateNow(float, float, TimeZoneInfo)` | Calculate for current time in specific timezone |

### SolarPosition Struct

| Property | Type | Description |
|----------|------|-------------|
| `elevation` | float | Solar elevation angle (degrees above horizon) |
| `azimuth` | float | Solar azimuth angle (degrees from north, clockwise) |
| `dateTime` | DateTimeOffset | Date/time used for calculation |
| `latitude` | float | Latitude coordinate |
| `longitude` | float | Longitude coordinate |

| Method | Description |
|--------|-------------|
| `ToString()` | Formatted string representation |
| `GetSunState()` | Sun state description (daylight, twilight, etc.) |

### Transform Rotation Utility

```csharp
using UnityEngine;
using Unity.Mathematics;
using jp.nobnak.solar;

// Convert solar position to Unity rotation (Unity.Mathematics optimized)
quaternion rotation = SolarRotationConverter.CalculateSunRotation(elevation, azimuth);
transform.rotation = rotation; // Implicit conversion to UnityEngine.Quaternion

// Direction vector (also supports implicit conversion to Vector3)
float3 sunDirection = SolarRotationConverter.CalculateSunDirection(elevation, azimuth);
```

## Unity Demo Component

The `SolarPositionDemo` component provides:

- **Inspector Configuration**: Date, time, location settings
- **Location Presets**: Major cities (Tokyo, New York, London, etc.)
- **Seasonal Presets**: Solstices and equinoxes  
- **Auto Update**: Real-time recalculation
- **Transform Output**: Apply solar rotation to game objects
- **ExecuteAlways**: Works in editor mode

## Coordinate System

- **Elevation**: 0° = horizon, 90° = zenith, negative = below horizon
- **Azimuth**: 0° = north, 90° = east, 180° = south, 270° = west
- **Latitude**: -90° (south) to +90° (north)
- **Longitude**: -180° (west) to +180° (east)

## Timezone Examples

```csharp
// Same moment, different timezone representations
var jstTime = new DateTimeOffset(2025, 6, 21, 12, 0, 0, TimeSpan.FromHours(9));  // JST
var utcTime = new DateTimeOffset(2025, 6, 21, 3, 0, 0, TimeSpan.Zero);           // UTC
// Both give identical results for the same location

// Timezone-specific calculation
var tokyoTime = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f, 
    TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
```

## Requirements

- Unity 2019.4 or later
- .NET Standard 2.0 compatible

## Accuracy

- **Elevation**: ±0.5° precision
- **Azimuth**: ±1.0° precision  
- Suitable for most applications; for higher precision, consider VSOP87 or JPL ephemeris

## License

MIT License

## References

- Astronomical Algorithms by Jean Meeus
- NOAA Solar Position Calculator
