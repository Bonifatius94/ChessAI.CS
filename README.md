# About

This project offers a cross-plattform chess library for C# .NET Core, implementing all the basic chess draw functionality and also a bit of AI for 'best' draws computation. Those libraries are provided by a game server, mainly for AI vs. AI competition but also for interactive matchmaking with human players via website. (But the game server is only the icing on the cake, the main focus is clearly on the chess libs and chess AI) 

Moreover, the main purpose of this project is learning and trying out new programming techniques. (I've chosen a chess game because everyone knows at least a bit about chess, and games are always fun, right?)

# How To Build

You just need to have Visual Studio with .NET Core tools installed for building the basic source code (current framework is .NET Core SDK 3.1, backward compatibility probably at least for .NET Core SDK 3.0, but that's not really tested). For building the XML documentation you need to additionally install sandcastle (see: https://github.com/EWSoftware/SHFB), but this is currently disabled. When using Visual Studio for development, everything can be done using the standard workflows, no further knowledge required.

You can also build / run unit tests / package using the dotnet command line tools for Linux
```sh
dotnet restore
dotnet build
dotnet test
dotnet package
```

# Roadmap

## Game Server:

- make chess game server work
- add website

## AI Stuff:

- train a neuronal network by applying deep learing to some extracted pgn data from pro chess games (-> this may fix the performance issues of minimax algorithm)
- improve AI performance -> increase search depth of minimax algorithm (e.g. by using a little chess draw database of common draws / positions)

## Other:
- add server-side build automation using a CI pipeline
- add some more unit tests
- add packaging for the game server website component (docker image)
