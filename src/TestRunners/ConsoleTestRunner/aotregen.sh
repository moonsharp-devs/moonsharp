# No need for trampolines anymore
# mono --debug --aot=full,soft-debug,nrgctx-trampolines=8192,nimt-trampolines=8192,ntrampolines=4096 /usr/lib/mono/2.0/mscorlib.dll
# for i in /usr/lib/mono/gac/*/*/*.dll; do mono --debug --aot=full,soft-debug,nrgctx-trampolines=8192,nimt-trampolines=8192,ntrampolines=4096 $i; done


mono --debug --aot=full /usr/lib/mono/2.0/mscorlib.dll
for i in /usr/lib/mono/gac/*/*/*.dll; do mono --aot=full $i; done

