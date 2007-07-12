all:
	mdtool build

run:
	mono --debug Banshee.ThickClient.Application/Banshee.ThickClient.Application.exe

run-hb:
	mono --profile=heap-buddy Banshee.ThickClient.Application/Banshee.ThickClient.Application.exe; \
	heap-buddy

clean:
	find . -iregex "^.*\.\(exe\|dll\|mdb\)$$" | grep -v "^.\/NDesk" | xargs -r rm -v

count-lines:
	wc -l `find . -iregex "^.*\.cs$$" | xargs`

