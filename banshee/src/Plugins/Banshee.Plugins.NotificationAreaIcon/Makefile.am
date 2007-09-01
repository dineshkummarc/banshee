include $(top_srcdir)/build/Common.Makefile

ASSEMBLY_NAME = Banshee.Plugins.NotificationAreaIcon
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_GCONF_SCHEMA = banshee-plugin-notificationarea.schemas.in

notificationareaicondir = $(pkglibdir)/Banshee.Plugins
notificationareaicon_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb $(ASSEMBLY).config

MENU_RESOURCE = NotificationAreaIconMenu.xml
ICON_RESOURCE = banshee_icon.ico

ASSEMBLY_SOURCES = \
	$(top_srcdir)/src/AssemblyInfo.cs \
	$(srcdir)/NotificationAreaIconPlugin.cs \
	$(srcdir)/NotificationAreaIconConfigPage.cs \
	$(srcdir)/NotificationArea.cs \ 
	$(srcdir)/SystemTray.cs

$(ASSEMBLY): $(ASSEMBLY_SOURCES) $(MENU_RESOURCE) $(ICON_RESOURCE)
	$(BUILD_LIB_BANSHEE_CORE) -out:$@ $(LINK_GTK) $(LINK_MONO_UNIX) -r:System.Drawing -r:System.Windows.Forms -resource:$(srcdir)/$(MENU_RESOURCE),$(MENU_RESOURCE) -resource:$(top_srcdir)/data/images/$(ICON_RESOURCE),$(ICON_RESOURCE) $(ASSEMBLY_SOURCES)

include $(top_srcdir)/build/gconf-schema-rules

EXTRA_DIST = \
	$(ASSEMBLY_SOURCES) \
	$(ASSEMBLY).config.in \
	$(MENU_RESOURCE) \
	$(ASSEMBLY_GCONF_SCHEMA)

CLEANFILES = $(ASSEMBLY) *.dll *.exe
DISTCLEANFILES = *.mdb $(schema_DATA)
MAINTAINERCLEANFILES = Makefile.in

