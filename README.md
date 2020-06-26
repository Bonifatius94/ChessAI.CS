# About

This project offers a cross-plattform chess library for C# .NET Core, implementing all the basic functionality and also a bit of AI for 'best' draws computation. Moreover the project also contains a RESTful Web API for hosting a chess game server (implemented but not tested) and a small website for interactive matchmaking is also planned.

The main purpose of the project is learning and trying out new programming techniques. (I've chosen a chess game because everyone knows at least a bit about chess, and games are always fun, right?)

# How To Build

You just need to have Visual Studio 2017 / 2019 with .NET Core tools installed for building the basic source code (Visual Studio Community Edition should be enough). For building the documentation project you need to additionally install sandcastle (see: https://github.com/EWSoftware/SHFB), but this is currently disabled.

Open the solution with a suitable Visual Studio version, wait until loading finished and hit the build button (either Release or Debug mode as usual). There were no special build settings made, everything is pretty standard. The unit tests can be executed using the standard Visual Studio Test Explorer. (there is currently no CI pipeline and server-side build automation)

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
