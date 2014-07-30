Moon# 
=========
http://www.moonsharp.org


A Lua interpreter written entirely in C# for the .NET, Mono, Xamarin and Unity3D platforms.
It aims at language compatibility with Lua 5.2 and, in the long run, some nifty additions.


**Project Status**

* All Lua 5.2 language constructs are implemented, except goto/labels. See [the wiki](https://github.com/xanathar/moonsharp/wiki/Differences-between-Moon%23-and-Lua) for known differences.
* Development of the standard library is ongoing. Updated situation on [googledocs](https://docs.google.com/spreadsheets/d/1Iw8YMSY8N0tGEyaD-vmmJnlaQ5te4P4CqTXYpEiSEL8/edit#gid=0)
* Moon#/.NET integration is at quite a crude level. Will be started soon.
* The debugger "almost" works on the bytecode level. Source level is not yet supported. Currently as a Windows.Forms application, a better solution might be planned sooner or later - TBC
* Coroutines and state-save support has not been started yet.


**Roadmap**

* support for all core language structures (see documentation for differences between Moon# and Lua)
* compatibility with all applyable tests in Lua Test Suite (http://www.lua.org/tests/5.2/) and Lua Test More (http://fperrad.github.io/lua-TestMore/). Most tests of the suite are passing.
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
