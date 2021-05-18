# AltNetworkUtility
A clone of Apple's now-discontinued Network Utility

# Status

It's in pre-alpha stage. That means some conceptual decisions still need to be made, and many features are missing.

# Build

ANU is written in C# against Xamarin Forms / MAUI, although it currently only targets macOS.

To build, your easiest bet is to install [Visual Studio for Mac](https://visualstudio.microsoft.com) (not Visual Studio Code!) and [Xcode](https://developer.apple.com/xcode/). It is currently developed against Visual Studio 8.10.0.1457 and Xcode 12.5.

# Design goals and non-goals

ANU is like Apple's tool in that:

* it is a thin GUI on top of CLI Unix commands
* it is mostly about status and diagnostics, not configuration

It is _unlike_ Apple's tool in that: it is still maintained 😛 and is OSS

## Later

* it may attempt to show how to accomplish the same in the CLI, a bit like [Commando in A/UX](http://toastytech.com/guis/aux3.html) and MPW
* it may come to other platforms
