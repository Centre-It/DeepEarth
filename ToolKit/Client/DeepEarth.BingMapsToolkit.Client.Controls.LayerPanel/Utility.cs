// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public static class Utility
    {
        public static string GetSelectedLayerIDs(IEnumerable<LayerDefinition> layerDefinitions)
        {
            var result = new StringBuilder();
            if (layerDefinitions != null)
            {
                foreach (var layerDefinition in layerDefinitions)
                {

                    if (layerDefinition.Selected)
                    {
                        result.Append(layerDefinition.LayerID);
                        if (layerDefinition.LabelOn)
                        {
                            result.Append("L");
                        }
                        result.Append(",");
                    }
                }
            }
            return result.ToString();
        }

        public static UIElement CombineTemplate(Dictionary<string, string> data, string template)
        {
            if (!string.IsNullOrEmpty(template))
            {
                foreach (var item in data)
                {
                    //TODO: encode value.
                    template = template.Replace("[" + item.Key.Trim() + "]", item.Value);
                }
                try
                {
                    return (UIElement)System.Windows.Markup.XamlReader.Load(template);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            //if fails to read xaml just return an empty canvas
            return new Canvas();
        }
    }
}