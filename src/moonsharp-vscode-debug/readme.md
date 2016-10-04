# MoonSharp VSCode Debugger

This is an extension to allow debugging of MoonSharp scripts running inside other applications.

It requires the application to be embedding MoonSharp 1.8.0.0 (or later) and exposing the VSCode debugger extensions.

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


