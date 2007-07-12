namespace Banshee.Data
{
    public interface IFilterable
    {
        void Refilter();
        string Filter { get; set; }
    }
}
