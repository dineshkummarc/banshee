using System;
using System.IO;
using System.Data;
using Mono.Data.SqliteClient;

namespace Banshee.Data
{
    public class BansheeDatabase : IDisposable
    {
        private IDbConnection connection;

        public BansheeDatabase() : this(true)
        {
        }

        public BansheeDatabase(bool connect)
        {
            if(connect) {
                Open();
                DatabaseFormatMigrator migrator = new DatabaseFormatMigrator(connection);
                migrator.SlowStarted += OnMigrationSlowStarted;
                migrator.SlowPulse += OnMigrationSlowPulse;
                migrator.SlowFinished += OnMigrationSlowFinished;
                migrator.Migrate();
            }
        }
        
        private Gtk.Window slow_window;
        private Gtk.ProgressBar slow_progress;
        
        private void IterateSlow()
        {
            while(Gtk.Application.EventsPending()) {
                Gtk.Application.RunIteration();
            }
        }
        
        private void OnMigrationSlowStarted(string title, string message)
        {
            lock(this) {
                if(slow_window != null) {
                    slow_window.Destroy();
                }
                
                Gtk.Application.Init();
                
                slow_window = new Gtk.Window(String.Empty);
                slow_window.BorderWidth = 10;
                slow_window.WindowPosition = Gtk.WindowPosition.Center;
                slow_window.DeleteEvent += delegate(object o, Gtk.DeleteEventArgs args) {
                    args.RetVal = true;
                };
                
                Gtk.VBox box = new Gtk.VBox();
                box.Spacing = 5;
                
                Gtk.Label title_label = new Gtk.Label();
                title_label.Xalign = 0.0f;
                title_label.Markup = String.Format("<b><big>{0}</big></b>",
                    GLib.Markup.EscapeText(title));
                
                Gtk.Label message_label = new Gtk.Label();
                message_label.Xalign = 0.0f;
                message_label.Text = message;
                message_label.Wrap = true;
                
                slow_progress = new Gtk.ProgressBar();
                
                box.PackStart(title_label, false, false, 0);
                box.PackStart(message_label, false, false, 0);
                box.PackStart(slow_progress, false, false, 0);
                
                slow_window.Add(box);
                slow_window.ShowAll();
                
                IterateSlow();
            }
        }
        
        private void OnMigrationSlowPulse(object o, EventArgs args)
        {
            lock(this) {
                slow_progress.Pulse();
                IterateSlow();
            }
        }
        
        private void OnMigrationSlowFinished(object o, EventArgs args)
        {
            lock(this) {
                slow_window.Destroy();
                IterateSlow();
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Open()
        {
            lock(this) {
                if(connection != null) {
                    return;
                }

                string dbfile = DatabaseFile;
                Console.WriteLine("Opening connection to Banshee Database: {0}", dbfile);
                connection = new SqliteConnection(String.Format("Version=3,URI=file:{0}", dbfile));
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = @"
                    PRAGMA synchronous = OFF;
                    PRAGMA cache_size = 32768;
                ";
                command.ExecuteNonQuery();
            }
        }

        public void Close()
        {
            lock(this) {
                if(connection != null) {
                    connection.Close();
                    connection = null;
                }
            }
        }

        public IDbCommand CreateCommand()
        {
            lock(this) {
                if(connection == null) {
                    throw new ApplicationException("Not connected to database");
                }

                return connection.CreateCommand();
            }
        }

        public string DatabaseFile {
            get { 
                string [] args = Environment.GetCommandLineArgs();
                if(args.Length > 1 && File.Exists(args[1])) {
                    return args[1];
                }
            
                string dbfile = Path.Combine(Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), 
                    "banshee"), 
                    "banshee.db"); 

                if(!File.Exists(dbfile)) {
                    string tdbfile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.Personal),
                        ".gnome2"),
                        "banshee"),
                        "banshee.db");

                    if(File.Exists(tdbfile)) {
                        dbfile = tdbfile;
                    }
                }

                return dbfile;
            }
        }

        public IDbConnection Connection {
            get { return connection; }
        }
    }
}
