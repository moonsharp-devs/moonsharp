Moon# 
=========
http://www.moonsharp.org


An interpreter for a very close cousin of the Lua language, written entirely in C# for the .NET, Mono, Xamarin and Unity3D platforms.
It targets Lua 5.1, with some features of Lua 5.2 backported.


**Project Status**

The project has just been started,  yet it is able to parse most Lua structures and execute them correctly. The code is,  however,  heavily unoptimized. 


**Roadmap**

* support for all core language structures (see documentation for differences between Moon# and Lua)
* compatibility with all applyable tests in Lua Test Suite (http://www.lua.org/tests/5.2/) and Lua Test More (http://fperrad.github.io/lua-TestMore/) 
* optimizations 
* better integration between Lua/Moon# tables and CLR objects
* debugger embeddable in applications using Moon# 
* REPL interpreter
* standard library 

**Associated Resources**

* You can see the future backlog and status on [https://www.pivotaltracker.com/n/projects/1082626](https://www.pivotaltracker.com/n/projects/1082626 "Moon# Backlog on Pivotal Tracker")
* You can read some posts about the design choices of Moon# on my blog: http://www.mastropaolo.com/category/programming/moonsharp/


**License**

The program and libraries are released under a 3-clause BSD license - see the license section.

This work is based on the ANTLR4 Lua grammar Copyright (c) 2013, Kazunori Sakamoto.
