# AltNetworkUtility
A clone of Apple's now-discontinued Network Utility

# Status

It's in alpha stage. That means most of the architecture is there, but many features are still missing.

# Build

ANU is written in C# against Xamarin Forms / MAUI, although it currently only targets macOS.

To build, your easiest bet is to install [Visual Studio for Mac](https://visualstudio.microsoft.com) (not Visual Studio Code!) and [Xcode](https://developer.apple.com/xcode/). It is currently developed against Visual Studio 8.10.0.1759 and Xcode 12.5.

# Design goals and non-goals

ANU is like Apple's tool (NU) in that it is a thin GUI on top of CLI Unix commands.

Unlike NU:

* it is still maintained 😛
* it's OSS
* where invoking CLI tools (e.g., Netstat, Ping) is involved, it attempts to show how to accomplish the same yourself, a bit like [Commando in A/UX](http://toastytech.com/guis/aux3.html) and MPW

## Possible later goals

* it **may** come to other platforms

## Non-goals

* like NU, it is mostly about status and diagnostics, **not** configuration
