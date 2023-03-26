# UOC - B2.503 - PEC1

![Logo](logo.png?raw=true)

## Tanks! Tanks! Tanks! Tanks!

Tanks! Tanks! Tanks! Tanks! (or Tanks!<sup>4</sup>) is the name of the prototype created for the first continuous assessment activity (PEC1) of the subject Multiplayer Games (B2.503). This subject is part of the Master's Degree in Video Game Design and Development at the Open University of Catalonia (UOC).

The goal of the activity was to extend the multiplayer capabilities of Unity's Tanks! tutorial. The original tutorial can be found [here](https://learn.unity.com/project/tanks-tutorial).

## Features and improvements

The following features and improvements were added to the original tutorial:

- Added support for up to 4 players in split-screen mode using the same keyboard.
- Allowed players to join the game at any time.
- Added main menu with player selection, controls and credits.
- Added pause menu.
- Added the wins counter for each player to the UI.
- Added a fourth screen with a minimap when three players are playing.
- Added alternative fire mode with faster shells.
- Implemented Unity's new Input System to handle player input.
- Implemented Cinemachine to handle cameras and split screen management.
- Refactored and commented the whole code base and reorganized the project structure.

## Unity version

The project was developed using Unity 2021.3.18f1.

## Builds

There are no publicly available builds for this project. However, a build for Windows x64 was successfully compiled and tested on Windows 11 using the current source code.

## How to play

The game is a 3D top-down shooter. The player controls a tank and must destroy the enemy tanks. The player can move the tank around and fire shells, which can reach further the longer the player holds the fire button. The player can also use an alternative fire mode with faster shells. The player wins the round when all the enemy tanks are destroyed. The game ends when a player wins five rounds.

## Controls

| Action | Player1 | Player2 | Player3 | Player4 |
| --- | --- | --- | --- | --- |
| Move | WASD | IJKL | Arrow Keys | Numpad 8456 |
| Fire | Q | U | Right Ctrl | Numpad 7 |
| AltFire | E | O | Right Shift | Numpad 9 |
| Join | - | - | Enter | Numpad Enter |
| Pause | Escape | Escape | Escape | Escape |
