# NEXUS Dashboard

> A sleek, glassmorphism-styled Windows 11 system dashboard built with WinUI 3 and C# .NET 8.

![Platform](https://img.shields.io/badge/Platform-Windows%2011-0078D4?logo=windows)
![Framework](https://img.shields.io/badge/Framework-.NET%208-512BD4?logo=dotnet)
![UI](https://img.shields.io/badge/UI-WinUI%203-0078D4)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

---

## Overview

NEXUS is a high-performance, real-time system monitoring dashboard for Windows 11. It combines hardware telemetry, RGB lighting control, live weather, and Bluetooth device tracking into a single unified glassmorphism UI — built entirely with WinUI 3 and the Windows App SDK.

## Features

- **System Monitoring** — Real-time CPU, GPU, RAM, and temperature metrics via LibreHardwareMonitor
- **RGB Control** — Unified RGB lighting management for compatible devices via OpenRGB
- **Live Weather** — Current conditions and forecasts using the Open-Meteo API (no API key required)
- **Bluetooth Battery** — Live battery levels for paired Bluetooth devices via WinRT APIs
- **Glassmorphism UI** — Frosted glass, blur, and acrylic effects native to Windows 11
- **MVVM Architecture** — Clean separation of concerns using CommunityToolkit.Mvvm and CommunityToolkit.WinUI

## Tech Stack

| Layer | Technology |
|---|---|
| UI Framework | WinUI 3 / Windows App SDK |
| Language | C# 12, .NET 8 |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Hardware Metrics | LibreHardwareMonitorLib |
| RGB Control | OpenRGB.NET |
| Weather | Open-Meteo REST API |
| Bluetooth | Windows.Devices.Bluetooth (WinRT) |
| Bindings | CommunityToolkit.WinUI |

## Project Structure

```
NexusDashboard/
├── Services/        # Hardware, weather, RGB, and BT data services
├── ViewModels/      # MVVM view models (CommunityToolkit.Mvvm)
├── Views/
│   └── Widgets/     # Modular dashboard widget views
├── Themes/          # ResourceDictionaries, color tokens, acrylic brushes
└── Assets/          # Icons, images, and static resources
```

## Prerequisites

- Windows 11 (Build 22000 or later)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **Windows App SDK** workload installed
- [OpenRGB](https://openrgb.org/) running as a server (for RGB control features)

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/nexus-dashboard.git
   cd nexus-dashboard
   ```

2. **Open in Visual Studio 2022**
   Open `NexusDashboard.sln` in Visual Studio 2022.

3. **Restore NuGet packages**
   Visual Studio will restore packages automatically, or run:
   ```bash
   dotnet restore
   ```

4. **Build and run**
   Set the startup project to `NexusDashboard` and press `F5`.

> **Note:** Some hardware monitoring features require the application to be run with elevated (administrator) privileges.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file included in this repository.
