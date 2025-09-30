# Solar Position Calculator

A Unity package for calculating solar position (elevation and azimuth angles) in the horizontal coordinate system based on date, time, latitude, and longitude.

[![Example](http://img.youtube.com/vi/yEXLL2l7BfA/hqdefault.jpg)](https://youtu.be/yEXLL2l7BfA)

## Features

- **Accurate Solar Position**: Calculate solar elevation and azimuth angles using astronomical algorithms
- **Timezone Support**: Proper timezone handling with `DateTimeOffset` 
- **Unity Integration**: Ready-to-use components and demo scripts with `ExecuteAlways` support
- **Real-time Calculation**: Current time and scheduled updates support
- **Unity.Mathematics Optimization**: High-performance calculations with implicit conversion to Unity APIs
- **Twilight Classification**: Detailed sun state detection (astronomical, nautical, civil twilight)
- **Light Control**: Automatic directional light intensity and color temperature control
- **Transform Integration**: Direct rotation and direction vector conversion for game objects

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

### SolarRotationConverter (Static Class)

High-performance utility for converting solar positions to Unity transforms using Unity.Mathematics optimization.

| Method | Description |
|--------|-------------|
| `CalculateSunRotation(float, float)` | Convert elevation/azimuth to Unity rotation |
| `CalculateSunRotation(SolarPosition)` | Convert SolarPosition struct to Unity rotation |
| `CalculateSunDirection(float, float)` | Convert elevation/azimuth to direction vector |
| `CalculateSunDirection(SolarPosition)` | Convert SolarPosition struct to direction vector |

```csharp
using UnityEngine;
using Unity.Mathematics;
using jp.nobnak.solar;

// Convert solar position to Unity rotation (Unity.Mathematics optimized)
quaternion rotation = SolarRotationConverter.CalculateSunRotation(elevation, azimuth);
transform.rotation = rotation; // Implicit conversion to UnityEngine.Quaternion

// Direction vector (also supports implicit conversion to Vector3)
float3 sunDirection = SolarRotationConverter.CalculateSunDirection(elevation, azimuth);

// Direct conversion from SolarPosition struct
var solarPos = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f);
quaternion sunRotation = SolarRotationConverter.CalculateSunRotation(solarPos);
float3 sunDir = SolarRotationConverter.CalculateSunDirection(solarPos);
```

## Unity Components

### SolarPositionDemo Component

The `SolarPositionDemo` component provides:

- **Inspector Configuration**: Date, time, location settings
- **Location Presets**: Major cities (Tokyo, New York, London, etc.)
- **Seasonal Presets**: Solstices and equinoxes  
- **Auto Update**: Real-time recalculation
- **Transform Output**: Apply solar rotation to game objects
- **ExecuteAlways**: Works in editor mode

### SunDirectionController Component

Advanced component for realistic solar simulation with automatic light control:

- **Real-time Light Control**: Automatic directional light intensity and color temperature
- **Twilight Detection**: Detailed sun state classification (astronomical, nautical, civil twilight)
- **Smooth Transitions**: Smoothstep-based intensity control from -18° to 0° elevation
- **Color Temperature**: Linear interpolation between horizon and zenith temperatures
- **Inspector Integration**: Custom editor with real-time status display
- **ExecuteAlways**: Works in both editor and runtime modes

```csharp
// SunDirectionController usage
var sunController = GetComponent<SunDirectionController>();

// Set sun angles directly
sunController.SetSunAngles(45f, 180f); // 45° elevation, south (180°)

// Get current sun state
string sunState = sunController.GetSunStateString(); // "日中", "市民薄明", etc.
bool isAboveHorizon = sunController.IsSunAboveHorizon();

// Get current color temperature
float colorTemp = sunController.GetCurrentColorTemperature(); // Kelvin
```

## Sun State Classification

The package provides detailed twilight classification based on solar elevation:

| Elevation Range | State | Description |
|----------------|-------|-------------|
| `elevation >= 0°` | **日中** | Sun is above horizon |
| `-6° < elevation < 0°` | **市民薄明** | Civil twilight - bright enough for outdoor activities |
| `-12° < elevation ≤ -6°` | **航海薄明** | Nautical twilight - horizon visible at sea |
| `-18° < elevation ≤ -12°` | **天文薄明** | Astronomical twilight - sky not completely dark |
| `elevation ≤ -18°` | **天文薄明前/後** | Night - sky is completely dark |

```csharp
var solarPos = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f);
string sunState = solarPos.GetSunState(); // Returns Japanese state description
```

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

## Sample Projects

The package includes two comprehensive sample projects:

### DateTime Demo
- **Basic Usage**: Simple solar position calculation with date/time inputs
- **Inspector Controls**: Easy-to-use UI for setting date, time, and location
- **Transform Output**: Apply solar rotation to multiple game objects
- **Location Presets**: Quick access to major cities worldwide
- **Seasonal Presets**: Solstices and equinoxes for testing

### Yearly Solar Animation
- **Advanced Simulation**: Year-round solar movement animation
- **SunDirectionController**: Realistic sun direction control with light management
- **Building Shadows**: Dynamic shadow casting based on solar position
- **Material System**: Ground and skybox materials for realistic solar simulation
- **Stonehenge Model**: Historical monument for solar alignment demonstration

## Performance & Optimization

- **Unity.Mathematics**: High-performance calculations using Burst-compatible math functions
- **Implicit Conversions**: Seamless integration with Unity's Transform and Vector3 APIs
- **ExecuteAlways**: Components work in both editor and runtime modes
- **Memory Efficient**: Struct-based design minimizes garbage collection

## Requirements

- Unity 2019.4 or later
- .NET Standard 2.0 compatible
- Unity.Mathematics (included with Unity 2019.4+)

## Accuracy

- **Elevation**: ±0.5° precision
- **Azimuth**: ±1.0° precision  
- Suitable for most applications; for higher precision, consider VSOP87 or JPL ephemeris

## Version History

- **v1.3.0**: Added SunDirectionController, Unity.Mathematics optimization, twilight classification
- **v1.2.0**: Added SolarRotationConverter, enhanced sample projects
- **v1.1.0**: Added timezone support, ExecuteAlways components
- **v1.0.0**: Initial release with basic solar position calculation

## License

MIT License

## References

- Astronomical Algorithms by Jean Meeus
- NOAA Solar Position Calculator
