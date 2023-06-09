mono --debug --aot=full --aot=nimt-trampolines=4096 ConsoleTestRunner.exe
mono --debug --aot=full --aot=nimt-trampolines=4096 *.dll
mono --full-aot ConsoleTestRunner.exe
exit $?
