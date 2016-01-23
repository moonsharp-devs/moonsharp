mono --debug --aot=full ConsoleTestRunner.exe
mono --debug --aot=full *.dll
mono --full-aot --aot=nimt-trampolines=4096 ConsoleTestRunner.exe
exit $?
