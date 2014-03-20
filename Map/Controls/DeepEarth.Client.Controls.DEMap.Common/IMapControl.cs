using System;
using System.Windows.Controls;

namespace DeepEarth.Client.Controls.DEMap
{
    public interface IMapControl<T> : IDisposable where T : Control
    {
        T MapInstance { get; set; }
        string MapName { get; set; }
    }
}