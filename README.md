# GameMaker Extension Utilities (gmx-utils)

This repository contains the source code and binaries for gmx-utils; a command-line tool which can be used along side [GameMaker Studio 2](https://www.yoyogames.com/gamemaker) to automatically manage large extensions.

## Features

The following commands can be used by typing `gmx-utils <command>` into the interface:

 - `compile` for converting many individual `*.gml` scripts into a single source file.
 - `amend` for updating the JSDoc help information of your extension.
 - `exmacros` for extracting macros from the source files and inserting them into the IDE. (see [Macros (Constants)](https://docs2.yoyogames.com/source/_build/3_scripting/3_gml_overview/6_scope.html))

## Getting Started

### Downloads

You can download pre-built executables from the available [releases](https://github.com/GameMakerDiscord/gmx-utils/releases). (`gmx-utils.zip`)

### Installing

The zip file contains everything you should need, so there is no installation needed.

You can simply call the tool using `gmx-utils` in the windows command line. Add the exe to your environment `PATH` for easy use.

### Requirements
 - Visual Studio
 - Visual Basic .NET
 - Newtonsoft JSON v12.0.2