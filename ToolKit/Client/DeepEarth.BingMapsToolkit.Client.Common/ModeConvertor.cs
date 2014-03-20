using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Common
{
    public static class ModeConvertor
    {
        public static string Convert(MapMode mode)
        {
            if (mode is RoadMode)
            {
                return "Microsoft.Maps.MapControl.RoadMode";
            }
            if (mode is AerialMode)
            {
                if (((AerialMode)mode).Labels)
                {
                    return "Microsoft.Maps.MapControl.AerialWithLabelsMode";
                }
                return "Microsoft.Maps.MapControl.AerialMode";
            }
            return "";
        }

        public static MapMode ConvertBack(string location)
        {
            if (location == "Microsoft.Maps.MapControl.AerialWithLabelsMode")
            {
                return new AerialMode(true);
            }
            if (location == "Microsoft.Maps.MapControl.AerialMode")
            {
                return new AerialMode(false);
            }
            if (location == "Microsoft.Maps.MapControl.RoadMode")
            {
                return new RoadMode();
            }
            return null;
        }
    }
}