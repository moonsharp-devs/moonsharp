mono --debug --aot=full ConsoleTestRunner.exe
mono --debug --aot=full *.dll
mono --full-aot ConsoleTestRunner.exe
exit $?
