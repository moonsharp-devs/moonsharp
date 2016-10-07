# MoonSharp VSCode Debugger

This is an extension to allow debugging of MoonSharp scripts running inside other applications.

It requires the application to be embedding MoonSharp 1.8.0.0 (or later) and exposing the VSCode debugger extensions.


## Features supported

* Connect to one script object embedded in an application
* Supports breakpoints, watches, local variables, `self` inspection
* Call stack, with visualization of current coroutine
* Watches can contain free formed expressions, as long they are side-effects free
* Inspection of values including internal ids and table contents


## Features not supported

* Debugging of multiple script objects from the same vscode instance
* Editing of values not supported
* No checks are made for file contents changes
* Due to how vscode works, token-based breakpoints are not supported


## Screenshot

![Screenshot](src/moonsharp-vscode-debug/images/screenshot.png)


## How to use

1) Make sure the application you want to debug supports VSCode debugging.

2) Create a launch.json with these contents:

```
{
    "version": "0.2.0",
    "debugServer" : 41912,
    "configurations": [
        {
            "name": "MoonSharp Attach",
            "type": "moonsharp-debug",
            "request": "attach",
            "HELP": "Please set 'debugServer':41912 (or whatever port you ar connecting to) right after the 'version' field in this json."
        }
    ]
}
```


