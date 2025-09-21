# Solar Position Calculator (C# for Unity)

A C# script that calculates the solar angles (elevation and azimuth) in horizontal coordinate system based on date, time, latitude, and longitude inputs.

## Overview

This script provides the following functionality:

- **Solar Elevation**: Angle from horizon (0-90 degrees)
- **Solar Azimuth**: Clockwise angle from north (0-360 degrees)  
- **Sun State**: Classification of daylight, twilight, and nighttime conditions

## File Structure

### Main Files

- **`Assets/Scripts/SolarPositionCalculator.cs`** - Core solar position calculation class
- **`Assets/Scripts/SolarPositionDemo.cs`** - Practical demo for Unity Inspector
- **`Assets/Scripts/SolarPositionTest.cs`** - Test script for validation

## Usage

### 1. Basic Usage

```csharp
using System;

// Calculate solar position for Tokyo on September 21, 2025 at 12:00:00
DateTime dateTime = new DateTime(2025, 9, 21, 12, 0, 0);
float latitude = 35.6762f;   // Tokyo latitude
float longitude = 139.6503f; // Tokyo longitude

var result = SolarPositionCalculator.Calculate(dateTime, latitude, longitude);

Debug.Log($"Solar Elevation: {result.elevation}°");
Debug.Log($"Solar Azimuth: {result.azimuth}°");
Debug.Log($"Sun State: {result.GetSunState()}");
```

### 2. Current Time Calculation

```csharp
// Calculate solar position for current time
var currentResult = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f);
Debug.Log(currentResult.ToString());
```

## Unity Usage

### Using SolarPositionDemo

1. Attach `SolarPositionDemo.cs` to a GameObject
2. Configure parameters in Inspector:
   - **Date/Time Settings**: Year, month, day, hour, minute
   - **Location Settings**: Latitude, longitude
   - **Options**: Auto-update, use current time, etc.

3. Run in Play mode to automatically calculate solar position

### Convenient Features

- **Location Presets**: Pre-configured coordinates for major cities
- **Seasonal Settings**: Easy setup for solstices and equinoxes
- **Auto Update**: Automatic recalculation at specified intervals
- **Current Time**: Real-time calculation using system clock

### Validation with SolarPositionTest

1. Attach `SolarPositionTest.cs` to a GameObject
2. Run in Play mode to execute comprehensive tests automatically
3. Check results in Console window

## API Reference

### SolarPositionCalculator Class

#### Methods

- **`Calculate(DateTime dateTime, float latitude, float longitude)`**
  - Calculates solar position for specified date/time and location
  - Returns: `SolarPosition` struct

- **`CalculateNow(float latitude, float longitude)`**
  - Calculates solar position for current time
  - Returns: `SolarPosition` struct

### SolarPosition Struct

#### Properties

- **`elevation`** (float): Solar elevation angle (degrees)
- **`azimuth`** (float): Solar azimuth angle (degrees)
- **`dateTime`** (DateTime): Date/time used for calculation
- **`latitude`** (float): Latitude
- **`longitude`** (float): Longitude

#### Methods

- **`ToString()`**: Get results as formatted string
- **`GetSunState()`**: Get sun state description as string

## Calculation Accuracy

This script uses simplified calculation formulas with the following accuracy:

- **Elevation Angle**: Approximately ±0.5° precision
- **Azimuth Angle**: Approximately ±1.0° precision

For higher precision requirements, consider using VSOP87 theory or JPL ephemeris calculations.

## Parameter Ranges

- **Latitude**: -90° to +90°
- **Longitude**: -180° to +180°
- **Date/Time**: .NET DateTime supported range

## Examples

### Major City Coordinates

```csharp
// Major Japanese cities
var tokyo = (lat: 35.6762f, lon: 139.6503f);      // Tokyo
var osaka = (lat: 34.6937f, lon: 135.5023f);      // Osaka
var sapporo = (lat: 43.0642f, lon: 141.3469f);    // Sapporo
var naha = (lat: 26.2125f, lon: 127.6792f);       // Naha

// International cities
var newYork = (lat: 40.7128f, lon: -74.0060f);    // New York
var london = (lat: 51.5074f, lon: -0.1278f);      // London
var paris = (lat: 48.8566f, lon: 2.3522f);       // Paris
var sydney = (lat: -33.8688f, lon: 151.2093f);   // Sydney
```

### Special Day Calculations

```csharp
// Summer Solstice (around June 21)
var summerSolstice = SolarPositionCalculator.Calculate(
    new DateTime(2025, 6, 21, 12, 0, 0), 35.6762f, 139.6503f);

// Winter Solstice (around December 22)
var winterSolstice = SolarPositionCalculator.Calculate(
    new DateTime(2025, 12, 22, 12, 0, 0), 35.6762f, 139.6503f);

// Vernal Equinox (around March 21)
var vernalEquinox = SolarPositionCalculator.Calculate(
    new DateTime(2025, 3, 21, 12, 0, 0), 35.6762f, 139.6503f);

// Autumnal Equinox (around September 23)
var autumnalEquinox = SolarPositionCalculator.Calculate(
    new DateTime(2025, 9, 23, 12, 0, 0), 35.6762f, 139.6503f);
```

## License

This script is released into the public domain. Feel free to use it for any purpose.

## References

- Astronomical Algorithms by Jean Meeus
- NOAA Solar Position Calculator
- Celestial Mechanics and Dynamical Astronomy

## Changelog

- 2025-09-21: Initial Release
  - Basic solar position calculation functionality
  - Unity demo script
  - Test validation script
