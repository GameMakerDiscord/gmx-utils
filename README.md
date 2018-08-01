# GMXU

This repository contains the source code for a Command-Line Interface (CLI) that can be used along side GameMaker Studio 2 to automatically truncate project scripts, manage, and manipulate extension data. This is useful for large packages where it would be a waste of time to manually update extension information for every script after every unique build.

## Converting Project Scripts Into Standalone *.gml Files
Perhaps you want to store all your useful scripts in an extension to keep your project nice and tidy, but you have too many to count and don't want to manually compile a *.gml file to import into an extension, this command has you covered.
```
Syntax:
compile <directory> [destination]

 <directory>
  The directory the scripts you want to compile are located in.
  
 [destination]
  An optional argument you can supply if you want to define an output path, defaults to one directory above the supplied directory.
```
The following example command will compile all scripts located at the path "C:\Users\User\Projects\Platformer\scripts" and store the final file in the tmp directory with a file name of "compiledProjectScripts.gml"
```
Example:
compile C:\Users\User\Projects\Platformer\scripts C:\tmp\compiledProjectScripts.gml
```

## Updating Extension Help Information
We all know how tedious it is to add help information manually when creating *.gml extensions, especially if you have to keep looking back at the parameter names in the source file. This command hopes to alleviate that burden.
```
Syntax:
amend <filepath>

 <filepath>
  The file path that of your *.yy extension file you want to update the help information for.
```
The following example command will update all help information of the extension file located at the path "C:\Users\User\Projects\ZombieGame\extensions\ShadowEngine\ShadowEngine.yy" per script, by referencing the source files for JSDoc argument information.
```
Example:
amend C:\Users\User\Projects\ZombieGame\extensions\ShadowEngine\ShadowEngine.yy
```

## Automatically Adding Macros
External *.gml source files don't allow for #macro tokens to be used, so having a way to automate the task of adding constants is useful for large packages
```
Syntax:
exmacros <filepath>

 <filepath>
  The file path that of your *.yy extension file you want to update the macros of.
```
The following example command will extract and add all macro information from the linked source files.
```
Example:
compile C:\Users\User\Projects\ZombieGame\extensions\ShadowEngine\ShadowEngine.yy
```