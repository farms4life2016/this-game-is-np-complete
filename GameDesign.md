# This Game is NP-Complete

This document aims to outline how the puzzle game should work.
~~It is also useful to give to an LLM to generate Unity code.~~

## Setup

This is a puzzle game. Each `Puzzle` will have a `Target` integer.
Additionally, each `Puzzle` consists of a rectangular grid of `Tile`s.
Each `Tile` has a `Modifier` (an integer) and is exactly one of these types:

- `Blank`
- `Multiply`
- `Add`
- `Wall`

On top of the grid of `Tile` are a number of `Block`s. Each `Block` has a `Value` (any int).
`Block` are not allowed to be placed on a `Tile` that is `Wall`.

The `Puzzle` is assumed to be surrounded by `Wall` indefinitely in all directions.

The initial `Puzzle` layout -- grid dimensions, `Tile` types and placement, `Block` positions, integer values, etc. – is determined based on user-defined input (e.g. a JSON file defining the `Puzzle`).

## Game Loop

The game is played in `Turn`s.
The user keeps playing `Turn`s until there is only one `Block` left in the `Puzzle`.

### `Turn`s

1. the user chooses their input as one of the four cardinal `Direction`s
2. all `Block`s move as far as they can in the chosen `Direction`
   - a `Block` stops on the `Tile` right before it hits a `Wall`, since `Block`s cannot occupy `Wall`s.
   - if a `Block` moves over `Multiply`, then its `Value` gets multiplied by `Modifier`
   - if a `Block` moves over `Add`, then its `Value` gets added by `Modifier`
3. any `Block`s that share the same `Tile` with each other get merged into a single `Block`
   whose `Value` is the sum of all original `Value`s prior to the merge

**Technical note:** the effects of `Multiply` and `Add` should only take place when a `Block` first "enters" the `Tile`, or when the `Block` remains on the same `Tile` after the input `Direction`. In other words, moving off a `Tile` does not trigger `Multiply` or `Add` effects, but moving onto a `Tile` or staying on a `Tile` does.

## Win/Loss Condition

The game ends when there is only one `Block` left in the `Puzzle`.

- Win: `Block` has `Value` equal to `Target`
- Loss: `Block` has `Value` not equal to `Target`

The goal is therefore to find some order of `Direction`s that allow you to win from the initial `Puzzle` setup.

## NP-Completeness

Each game is essentially a fancy instance of the Subset Sum Problem (SSP).
Given an instance $I$ of SSP, we should be able to generate a `Puzzle` that encodes $I$.
