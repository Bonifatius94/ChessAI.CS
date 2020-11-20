# About

This project offers a cross-plattform chess library for C# .NET Core, implementing all the basic chess draw functionality and also a bit of AI for 'best' draws computation. Those libraries are provided by several applications such as a WPF desktop app and for future releases also a "game server" for hosting AI vs. AI competitions.

Moreover, the main purpose of this project is learning and trying out new programming techniques. (I've chosen a chess game because everyone knows at least a bit about chess, and games are always fun, right?)

# How To Build

You just need to have Visual Studio with .NET Core tools installed for building the basic source code (current framework is .NET Core SDK 3.1, backward compatibility probably at least for .NET Core SDK 3.0, but that's not really tested). For building the XML documentation you need to additionally install sandcastle (see: https://github.com/EWSoftware/SHFB), but this is currently disabled. When using Visual Studio for development, everything can be done using the standard workflows, no further knowledge required.

You can also build / run unit tests / package using the *dotnet* command line tools for Linux.
```sh
dotnet restore
dotnet build
dotnet test
dotnet package
```

# Roadmap

## Chess Lib

- make the draw computation a lot faster by adding a (64-bit) bitboard implementation. This is supposed to increase minimax search depth to about 10 steps, instead of just 5 (change is already implemented, was about 30 times faster like before but actually not fast enough to get the desired effect)
- remove slow Linq statements and replace them with optimized loops, cached arrays, array concat, sub-array, etc. (mostly done, is rather complicated)
- also add tests for measuring the performance (used the VS profiler tool)

## AI:

- further improve the minimax implementation
- add a deep learning approach using the PGN data already used for the "good draws" cache database

## Game Server:

- make chess game server work
- add website

## Other:
- add server-side build automation using a CI pipeline
- add some more unit tests
- add packaging for the game server website component (docker image)
