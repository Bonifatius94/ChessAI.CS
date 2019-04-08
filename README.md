# Abstract

This project offers a cross-plattform chess library for C# .NET Core, implementing all the basic functionality and also a bit of AI for 'best' draws computation. Moreover the project also contains a RESTful Web API for hosting a chess game server and a small website for interactive matchmaking is also planned.

# How To Build

You just need to have Visual Studio 2017 with cross-plattform .NET Core tools installed for building the basic source code (Community Edition should be enough). For building the documentation project you need to additionally install sandcastle (see: https://github.com/EWSoftware/SHFB). Open the solution with a suitable Visual Studio version, wait until loading finished and hit the build button (either Release or Debug mode as usual). There were no special build settings made, so just visit the Microsoft documentation of Visual Studio for more details.

# Roadmap

- improve the scoring system (better AI decision making)
- improve AI performance -> increase search depth
- build a little database with 'good' chess draws (especially for the first draws of a game)
- extract chess draws from games of professional chess players (PGN files) and do some deep learning stuff with it (-> fix minimax performance issues)