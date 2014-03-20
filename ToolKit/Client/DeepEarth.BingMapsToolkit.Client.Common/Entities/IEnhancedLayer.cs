// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;

namespace DeepEarth.BingMapsToolkit.Client.Common.Entities
{
    public interface IEnhancedLayer : IDisposable
    {
        string ID { get; set; }
        int ZIndex { get; set; }
    }
}
