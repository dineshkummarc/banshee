namespace Banshee.Data
{
    public interface ISortableColumn
    {
        string SortKey { get; }
        Gtk.SortType SortType { get; set; }
    }
}
