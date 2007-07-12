using System;
using System.Collections.Generic;
using Gtk;

using Banshee.Data;
using Banshee.Data.Gui;
using Banshee.Data.Query;

public static class Application
{
    public static void Main()
    {
        Gtk.Application.Init();
                
        Window win = new Window("ListView Test");
        win.DeleteEvent += delegate { Gtk.Application.Quit(); };
        win.WindowPosition = WindowPosition.Center;
        win.BorderWidth = 10;
        win.SetSizeRequest(850, 650);
        
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
        header.PackStart(clear_reload_button, false, false, 0);
        header.PackStart(reload_button, false, false, 0);
        header.PackStart(clear_button, false, false, 0);
        header.PackStart(track_count_label, false, false, 0);
        
        BansheeDatabase database = new BansheeDatabase();
        database.Open();
        
        TrackListDatabaseModel track_model;
        using(new Timer("Track Model")) {
            track_model = new TrackListDatabaseModel(database);
            track_model.Reloaded += delegate { track_count_label.Text = String.Format("Tracks: {0}", track_model.Rows); };
            track_model.Reload();
        }
        
        ArtistListDatabaseModel artist_model;
        using(new Timer("Artist Model")) {
            artist_model = new ArtistListDatabaseModel(database);
            artist_model.Reload();
        }
        
        AlbumListDatabaseModel album_model;
        using(new Timer("Album Model")) {
            album_model = new AlbumListDatabaseModel(database);
            album_model.Reload();
        }
        
        CompositeTrackListView view;
        using(new Timer("View")) {
            view = new CompositeTrackListView();
            view.TrackModel = track_model;
            view.ArtistModel = artist_model;
            view.AlbumModel = album_model;
        }
                
        box.PackStart(header, false, false, 0);
        box.PackStart(view, true, true, 0);
        
        win.Add(box);
        
        win.ShowAll();
        
        uint filter_timeout = 0;
        
        filter_entry.Changed += delegate { 
            if(filter_timeout == 0) {
                filter_timeout = GLib.Timeout.Add(25, delegate {
                    track_model.Filter = filter_entry.Text; 
                    track_model.Refilter();
                    track_model.Reload();
                    filter_timeout = 0;
                    return false;
                });
            }
        };
        
        clear_reload_button.Clicked += delegate { filter_entry.Text = String.Empty; track_model.Sort(null); track_model.Refilter(); track_model.Reload(); };
        reload_button.Clicked += delegate { track_model.Reload(); };
        clear_button.Clicked += delegate { track_count_label.Text = String.Empty; track_model.Clear(); };
        
        HBox query_parser_box = new HBox();
        Entry qpe = new Entry();
        Button runqpe = new Button("Parse Query");
        runqpe.Clicked += delegate {
            
            view.TrackView.HeaderVisible = !view.TrackView.HeaderVisible;
            
            /*Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add("artist", "Tracks.Artist");
            map.Add("album", "Tracks.AlbumTitle");
            map.Add("title", "Tracks.Title");
            map.Add("genre", "Tracks.Genre");
            QueryParser parser = new QueryParser(qpe.Text);
            SqlQueryGenerator generator = new SqlQueryGenerator(map, parser.BuildTree());
            Console.WriteLine(generator.GenerateQuery());*/
        };
        query_parser_box.PackStart(qpe, true, true, 0);
        query_parser_box.PackStart(runqpe, false, false, 0);
        box.PackStart(query_parser_box, false, false, 0);
        query_parser_box.ShowAll();
        
        Gtk.Application.Run();
    }
}
