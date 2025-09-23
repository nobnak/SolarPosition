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

// Calculate solar position for Tokyo on September 21, 2025 at 12:00:00 JST
DateTime localDateTime = new DateTime(2025, 9, 21, 12, 0, 0);
DateTimeOffset dateTime = new DateTimeOffset(localDateTime, TimeSpan.FromHours(9)); // JST
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

// Calculate for current time in specific timezone
var jstResult = SolarPositionCalculator.CalculateNow(35.6762f, 139.6503f, 
    TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
Debug.Log(jstResult.ToString());
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

- **`Calculate(DateTimeOffset dateTime, float latitude, float longitude)`**
  - Calculates solar position for specified date/time and location (with timezone)
  - Returns: `SolarPosition` struct

- **`CalculateNow(float latitude, float longitude)`**
  - Calculates solar position for current time
  - Returns: `SolarPosition` struct

- **`CalculateNow(float latitude, float longitude, TimeZoneInfo timeZone)`**
  - Calculates solar position for current time in specific timezone
  - Returns: `SolarPosition` struct

### SolarPosition Struct

#### Properties

- **`elevation`** (float): Solar elevation angle (degrees)
- **`azimuth`** (float): Solar azimuth angle (degrees)
- **`dateTime`** (DateTimeOffset): Date/time with timezone used for calculation
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
- **Date/Time**: .NET DateTimeOffset supported range (includes timezone information)

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
// Summer Solstice (around June 21) - JST
var summerSolstice = SolarPositionCalculator.Calculate(
    new DateTimeOffset(2025, 6, 21, 12, 0, 0, TimeSpan.FromHours(9)), 35.6762f, 139.6503f);

// Winter Solstice (around December 22) - JST
var winterSolstice = SolarPositionCalculator.Calculate(
    new DateTimeOffset(2025, 12, 22, 12, 0, 0, TimeSpan.FromHours(9)), 35.6762f, 139.6503f);

// Vernal Equinox (around March 21) - JST
var vernalEquinox = SolarPositionCalculator.Calculate(
    new DateTimeOffset(2025, 3, 21, 12, 0, 0, TimeSpan.FromHours(9)), 35.6762f, 139.6503f);

// Autumnal Equinox (around September 23) - JST
var autumnalEquinox = SolarPositionCalculator.Calculate(
    new DateTimeOffset(2025, 9, 23, 12, 0, 0, TimeSpan.FromHours(9)), 35.6762f, 139.6503f);

// UTC calculation example
var utcResult = SolarPositionCalculator.Calculate(
    new DateTimeOffset(2025, 6, 21, 3, 0, 0, TimeSpan.Zero), 35.6762f, 139.6503f); // 3:00 UTC = 12:00 JST
```

## License

This script is released into the public domain. Feel free to use it for any purpose.

## References

- Astronomical Algorithms by Jean Meeus
- NOAA Solar Position Calculator
- Celestial Mechanics and Dynamical Astronomy

## Important Notes

### Timezone Handling

This calculator uses `DateTimeOffset` for precise timezone-aware calculations. The solar position depends on the exact time including timezone information:

- **Local Time**: Use `new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime))`
- **Specific Timezone**: Use `new DateTimeOffset(dateTime, timeZoneOffset)` 
- **UTC Time**: Use `new DateTimeOffset(dateTime, TimeSpan.Zero)`

Example:
```csharp
// Same moment in time, different representations
var jstTime = new DateTimeOffset(2025, 6, 21, 12, 0, 0, TimeSpan.FromHours(9));  // 12:00 JST
var utcTime = new DateTimeOffset(2025, 6, 21, 3, 0, 0, TimeSpan.Zero);          // 03:00 UTC
// Both represent the same moment and will give identical solar positions for the same location
```

## Changelog

- 2025-09-22: DateTimeOffset Update
  - Changed from `DateTime` to `DateTimeOffset` for timezone-aware calculations
  - Added timezone-specific `CalculateNow` method
  - Improved calculation accuracy for different timezones
  
- 2025-09-21: Initial Release
  - Basic solar position calculation functionality
  - Unity demo script
  - Test validation script
