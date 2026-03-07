# Program Specifications

## Overview

User should be able to...

- play the game outlined in GameDesign.md
- import their own `Puzzle` Json files and play them
- convert an instance of Subset Sum into the equivalent instance of this game (barring memory constraints)

### Main Menu

Buttons for

- quitting the game
- playing the game (goes to 'level select')
- NP-Completeness reduction
- Either a separate tutorial button which also tracks completion, or we combine the tutorial with the level select
- Settings? controls?
- Credits and possibly other informational buttons for explaining NP-completeness

### Level Select

- should indicate if a level has been completed before or not
- basic 2 x 4 grid of clickable panels (adjust dimensions as necessary)
  - each panel has the name of the JSON file
  - possibly a thumbnail of the puzzle
- buttons for navigating pages, maybe a slider as well for quick navigation (or some other widget that allows random access)
- clicking the panel loads the puzzle and starts the level

### NP-Completeness reduction

- take input: a target integer and an array of integers
- all integers must be positive
- generate a `Puzzle` json according to a reduction
- involves chaining gadgets together, like adding circuits to a circuit board
- reduction details TBA, for now we'll just need a way to define gadgets (length & width) and paste them onto a 2D array of tiles

### Gameplay

- Top-down view of grid of tiles
- Able to undo
- Pressing arrow keys, WASD, (controller support?) will move blocks
- Camera can pan and zoom
- Restart button and menu button

## Not in scope

- Ability to save your current puzzle state. Once you exit, you lose all progress!
- "Campaign" mode or any sort of level-progression
- No in-game puzzle editor/creator
