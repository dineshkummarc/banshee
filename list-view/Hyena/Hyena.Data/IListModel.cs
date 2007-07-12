using System;

namespace Banshee.Data
{
    public interface IListModel<T>
    {
        event EventHandler Cleared;
        event EventHandler Reloaded;
        void Clear();
        void Reload();
        T GetValue(int index);
        int Rows { get; }
    }
}
