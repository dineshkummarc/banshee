include $(top_srcdir)/build/Common.Makefile

ASSEMBLY_NAME = MusicBrainzSharp
ASSEMBLY = $(ASSEMBLY_NAME).dll

musicbrainzsharpdir = $(pkglibdir)
musicbrainzsharp_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

ASSEMBLY_SOURCES = $(srcdir)/*.cs

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(BUILD_LIB) -out:$@ -r:System.XML $(ASSEMBLY_SOURCES)

EXTRA_DIST = $(ASSEMBLY_SOURCES)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
DISTCLEANFILES = *.pidb
MAINTAINERCLEANFILES = Makefile.in