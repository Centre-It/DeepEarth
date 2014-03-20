#pragma warning disable 1591 

namespace DeepEarth.Client.MapControl.Controls.GeometryTools
{
    public class ScaleControl : MapControl
    {
        #region Nested type: DrawLayersDisplay

        public class DrawLayersDisplay : MapControl
        {
            public DrawLayersDisplay()
                : this(Map.DefaultInstance)
            {
            }

            public DrawLayersDisplay(Map map)
            {
                _Map = map;

                DefaultStyleKey = typeof (DrawLayersDisplay);
            }
        }

        #endregion
    }
}

#pragma warning restore 1591