namespace Banshee.Data
{
    public interface ISortable
    {
        void Sort(ISortableColumn column);
        ISortableColumn SortColumn { get; }
    }
}
