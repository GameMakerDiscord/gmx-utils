# GameMaker Extension Utilities (gmx-utils)

This repository contains the source code and binaries for gmx-utils; a command-line tool which can be used along side [GameMaker Studio 2](https://www.yoyogames.com/gamemaker) to automatically manage large extensions.

## Features

The following commands can be used by typing `gmx-utils <command>` into the interface:

 - `compile` for converting many individual `*.gml` scripts into a single source file.
 - `amend` for updating the JSDoc help information of your extension.
 - `exmacros` for extracting macros from the source files and inserting them into the IDE. (see [Macros (Constants)](https://docs2.yoyogames.com/source/_build/3_scripting/3_gml_overview/6_scope.html))

## Downloads

You can download the pre-built executable from https://github.com/GameMakerDiscord/gmx-utils/releases. (`gmx-utils.zip`)

### Getting Started

Once you have a binary, you can simply call it using `gmx-utils` in windows cmd, or `./gmx-utils` in Bash.

Add the exe to your PATH for easy use.

## Requirements
 - Visual Studio
 - Visual Basic .NET
 - Newtonsoft JSON v12.0.2