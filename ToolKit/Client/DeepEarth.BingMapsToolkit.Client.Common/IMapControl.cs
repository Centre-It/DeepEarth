using System;
using System.Windows.Controls;

namespace DeepEarth.BingMapsToolkit.Client.Common
{
    public interface IMapControl<T> : IDisposable where T : Control
    {
        T MapInstance { get; set; }
        string MapName { get; set; }
    }
}
