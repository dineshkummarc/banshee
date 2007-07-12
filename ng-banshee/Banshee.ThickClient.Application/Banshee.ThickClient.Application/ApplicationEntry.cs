using System;
using System.Collections.Generic;
using Gtk;

using Banshee.ServiceStack;
using Banshee.Sources;
using Banshee.Database;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Collection.Gui;
using Banshee.Sources.Gui;

namespace Banshee.ThickClient.Application
{
    public static class ApplicationEntry
    {
        public static void Main()
        {
            Gtk.Application.Init();
            Banshee.ServiceStack.Application.Run();
                    
            Window win = new Window("Banshee Demo");
            win.DeleteEvent += delegate { Gtk.Application.Quit(); };
            win.WindowPosition = WindowPosition.Center;
            win.BorderWidth = 10;
            win.SetSizeRequest(1150, 750);
            
            SourceView source_view = new SourceView();
            
            VBox box = new VBox();
            HBox header = new HBox();
            
            box.Spacing = 10;
            header.Spacing = 5;
            
            Label filter_label = new Label("Filter:");
            Entry filter_entry = new Entry();
            Button clear_reload_button = new Button("Clear Filter & Reload");
            Button reload_button = new Button("Reload");
            Button clear_button = new Button("Clear Model");
            Label track_count_label = new Label();
            
            header.PackStart(filter_label, false, false, 0);
            header.PackStart(filter_entry, false, false, 0);
            /*header.PackStart(clear_reload_button, false, false, 0);
            header.PackStart(reload_button, false, false, 0);
            header.PackStart(clear_button, false, false, 0);
            header.PackStart(track_count_label, false, false, 0);*/
            
            CompositeTrackListView view = new CompositeTrackListView();
            view.TrackView.HeaderVisible = false;
            
            ServiceManager.SourceManager.ActiveSourceChanged += delegate {
                view.TrackModel = null;
                view.ArtistModel = null;
                view.AlbumModel = null;
                view.TrackView.HeaderVisible = false;
                
                if(ServiceManager.SourceManager.ActiveSource is ITrackModelSource) {
                    ITrackModelSource track_source = (ITrackModelSource)ServiceManager.SourceManager.ActiveSource;
                    view.TrackModel = track_source.TrackModel;
                    view.ArtistModel = track_source.ArtistModel;
                    view.AlbumModel = track_source.AlbumModel;
                    view.TrackView.HeaderVisible = true;
                }
            };
                    
            box.PackStart(header, false, false, 0);
            
            HPaned view_pane = new HPaned();
            ScrolledWindow source_scroll = new ScrolledWindow();
            source_scroll.ShadowType = ShadowType.In;
            source_scroll.Add(source_view);
            view_pane.Add1(source_scroll);
            view_pane.Add2(view);
            view_pane.Position = 200;
            box.PackStart(view_pane, true, true, 0);
            
            win.Add(box);
            
            win.ShowAll();
            
            uint filter_timeout = 0;
            
            filter_entry.Changed += delegate { 
                if(filter_timeout == 0) {
                    filter_timeout = GLib.Timeout.Add(25, delegate {
                        Source source = ServiceManager.SourceManager.ActiveSource;
                        if(!(source is ITrackModelSource)) {
                            filter_timeout = 0;
                            return false;
                        }
                        
                        TrackListModel track_model = ((ITrackModelSource)source).TrackModel;
                        
                        if(!(track_model is Hyena.Data.IFilterable)) {
                            filter_timeout = 0;
                            return false;
                        }
                        
                        Hyena.Data.IFilterable filterable = (Hyena.Data.IFilterable)track_model;
                        
                        filterable.Filter = filter_entry.Text; 
                        filterable.Refilter();
                        track_model.Reload();
                        filter_timeout = 0;
                        return false;
                    });
                }
            };
            
            /*
            HBox query_parser_box = new HBox();
            Entry qpe = new Entry();
            Button runqpe = new Button("Parse Query");
            runqpe.Clicked += delegate {
                
                view.TrackView.HeaderVisible = !view.TrackView.HeaderVisible;
                
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("artist", "Tracks.Artist");
                map.Add("album", "Tracks.AlbumTitle");
                map.Add("title", "Tracks.Title");
                map.Add("genre", "Tracks.Genre");
                QueryParser parser = new QueryParser(qpe.Text);
                SqlQueryGenerator generator = new SqlQueryGenerator(map, parser.BuildTree());
                Console.WriteLine(generator.GenerateQuery());
            };
            query_parser_box.PackStart(qpe, true, true, 0);
            query_parser_box.PackStart(runqpe, false, false, 0);
            box.PackStart(query_parser_box, false, false, 0);
            query_parser_box.ShowAll();*/
           
            Gtk.Application.Run();
        }
    }
}
