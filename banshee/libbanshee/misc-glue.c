#include <gtk/gtk.h>
#ifndef WIN32 // FIXME this is kinda hacky
#include <dbus/dbus-glib.h>
#endif

typedef gboolean (* activate_handler)(GtkCellRenderer *cell,
	GdkEvent *event, 
	GtkWidget *widget,
	const gchar *path,
	GdkRectangle *background_area,
	GdkRectangle *cell_area,
	GtkCellRendererState flags);

void gtksharp_cell_renderer_activatable_configure(GtkCellRenderer *renderer, activate_handler *handler)
{
	GTK_CELL_RENDERER_GET_CLASS(renderer)->activate = handler;
	renderer->mode = GTK_CELL_RENDERER_MODE_ACTIVATABLE;
}

void banshee_dbus_compat_thread_init()
{
#ifndef WIN32
	dbus_g_thread_init();
#endif
}

