/*
 *  Copyright (c) 2007 Scott Peterson <lunchtimemama@gmail.com> 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using Gtk;
using Mono.Unix;

namespace Banshee.PlayerMigration
{
    public class ItunesImportDialog : Dialog
    {
        private const string library_filename = "iTunes Music Library.xml";
        private string library_uri;
        private bool local_library;
        private readonly Button import_button;
        private readonly CheckButton ratings;
        private readonly CheckButton stats;
        private readonly CheckButton playlists;
        private readonly CheckButton smart_playlists;
        
        public bool Ratings {
            get { return ratings.Active; }
        }
        public bool Stats {
            get { return stats.Active; }
        }
        public bool Playliststs {
            get { return playlists.Active; }
        }
        public bool SmartPlaylists {
            get { return smart_playlists.Active; }
        }
        public string LibraryUri {
            get { return library_uri; }
        }
        public bool LocalLibrary {
            get { return local_library; }
        }
        
        public ItunesImportDialog() : base ()
        {
            Title = Catalog.GetString("iTunes Importer");
            Resizable = false;

            Button cancel_button = new Button(Stock.Cancel);
            cancel_button.Clicked += delegate { Respond(ResponseType.Cancel); };
            cancel_button.ShowAll();
            AddActionWidget(cancel_button, ResponseType.Cancel);
            cancel_button.CanDefault = true;
            cancel_button.GrabFocus();

            import_button = new Button();
            import_button.Label = Catalog.GetString("_Import");
            import_button.UseUnderline = true;
            import_button.Image = Image.NewFromIconName(Stock.Open, IconSize.Button);
            import_button.Clicked += delegate { Respond(ResponseType.Ok); };
            import_button.ShowAll();
            AddActionWidget(import_button, ResponseType.Ok);
            
            VBox vbox1 = new VBox();
            vbox1.BorderWidth = 8;
            vbox1.Spacing = 8;
            
            VBox vbox2 = new VBox();
            ratings = new CheckButton(Catalog.GetString("Import song ratings"));
            ratings.Active = true;
            vbox2.PackStart(ratings);
            stats = new CheckButton(Catalog.GetString("Import play statistics (playcount, etc.)"));
            stats.Active = true;
            vbox2.PackStart(stats);
            playlists = new CheckButton(Catalog.GetString("Import playlists"));
            playlists.Active = true;
            vbox2.PackStart(playlists);
            smart_playlists = new CheckButton(Catalog.GetString("Import smart playlists"));
            smart_playlists.Active = true;
            vbox2.PackStart(smart_playlists);

            string possible_location = System.IO.Path.Combine(System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "iTunes"),
                library_filename);

            if (File.Exists(possible_location)) {
                local_library = true;
                library_uri = possible_location;
            }
            else {
                HBox hbox = new HBox();
                hbox.Spacing = 8;
                Image image = new Gtk.Image();
                image.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-open", 18, 0);
                hbox.PackStart(image);
                Label label1 = new Label();
                label1.Markup = String.Format("<b>{0}</b>", GLib.Markup.EscapeText(
                    Catalog.GetString("Locate your \"" + library_filename + "\" file...")));
                label1.SetAlignment(0.0f, 0.5f);
                hbox.PackStart(label1);
                Button browse_button = new Button(hbox);
                browse_button.Clicked += OnBrowseButtonClicked;
                vbox1.PackStart(browse_button);

                ratings.Sensitive = stats.Sensitive = playlists.Sensitive =
                    smart_playlists.Sensitive = import_button.Sensitive = false;
            }
            
            vbox1.PackStart(vbox2);
            VBox.PackStart(vbox1);

            DefaultResponse = ResponseType.Cancel;
            
            VBox.ShowAll();
        }
        
        private void OnBrowseButtonClicked(object o, EventArgs args)
        {
            Button browse_button = o as Button;
            FileChooserDialog file_chooser = new FileChooserDialog(
                Catalog.GetString("Locate \"" + library_filename + "\""),
                this, FileChooserAction.Open,
                Stock.Cancel, ResponseType.Cancel,
                Stock.Open, ResponseType.Ok);
            FileFilter filter = new FileFilter();
            filter.AddPattern("*" + library_filename);
            filter.Name = library_filename;
            file_chooser.AddFilter(filter);
            if(file_chooser.Run() == (int)ResponseType.Ok) {
                browse_button.Sensitive = false;
                ratings.Sensitive = stats.Sensitive = playlists.Sensitive =
                    smart_playlists.Sensitive = import_button.Sensitive = true;
                library_uri = file_chooser.Filename;
            }
            file_chooser.Destroy();
        }
    }
}