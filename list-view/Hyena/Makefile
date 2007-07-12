PACKNAME = list-view
WEBROOT = /home/aaron/web/abock.org/projects/$(PACKNAME)
SOURCES = $(wildcard Banshee/*.cs Banshee.Data/*.cs Banshee.Data.Gui/*.cs Banshee.Data.Query/*.cs *.cs)
ASSEMBLY = ListView.exe
REFERENCES = -pkg:gtk-sharp-2.0 -r:Mono.Data.SqliteClient -r:System.Data -r:Mono.Cairo
RESOURCES = -resource:Banshee/Resources/browser-album-cover.png,browser-album-cover.png
MCS = gmcs -debug
MONO = mono

$(ASSEMBLY): $(SOURCES)
	$(MCS) -out:$@ $(REFERENCES) $(RESOURCES) $(SOURCES)

run: $(ASSEMBLY)
	$(MONO) --debug $<

run-hb: $(ASSEMBLY)
	$(MONO) --profile=heap-buddy:out.hb $<
	heap-buddy out.hb

clean:
	rm -f $(ASSEMBLY){,.mdb} *.pidb

upload: $(ASSEMBLY)
	scp  $(ASSEMBLY) aaron@abock.org:$(WEBROOT)

