// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Point=System.Windows.Point;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_xCoordTextBlock, Type = typeof (TextBlock))]
    [TemplatePart(Name = PART_yCoordTextBlock, Type = typeof (TextBlock))]
    [TemplatePart(Name = PART_zoomTextBlock, Type = typeof (TextBlock))]
    [TemplatePart(Name = PART_gotoLatitudeText, Type = typeof (TextBox))]
    [TemplatePart(Name = PART_gotoLongitudeText, Type = typeof (TextBox))]
    [TemplatePart(Name = PART_gotoButton, Type = typeof (Button))]
    [TemplatePart(Name = PART_toolPanel, Type = typeof (ToolPanel))]
    [TemplatePart(Name = PART_displayModeDecimal, Type = typeof (RadioButton))]
    [TemplatePart(Name = PART_displayModeDDMMSS, Type = typeof (RadioButton))]
    [TemplatePart(Name = PART_displayModeUTM, Type = typeof (RadioButton))]
    public class CoordinatePanel : Control, IMapControl<MapCore>
    {
        private const string PART_displayModeDDMMSS = "PART_displayModeDDMMSS";
        private const string PART_displayModeDecimal = "PART_displayModeDecimal";
        private const string PART_displayModeUTM = "PART_displayModeUTM";
        private const string PART_gotoButton = "PART_gotoButton";
        private const string PART_gotoLatitudeText = "PART_gotoLatitudeText";
        private const string PART_gotoLongitudeText = "PART_gotoLongitudeText";
        private const string PART_toolPanel = "PART_toolPanel";
        private const string PART_xCoordTextBlock = "PART_xCoordTextBlock";
        private const string PART_yCoordTextBlock = "PART_yCoordTextBlock";
        private const string PART_zoomTextBlock = "PART_zoomTextBlock";
        private const string ISOKey = "CoordinatePanel.DisplayMode";

        private static Point lastMouseLocation;
        private RadioButton displayModeDDMMSS;
        private RadioButton displayModeDecimal;
        private RadioButton displayModeUTM;
        private Button gotoButton;
        private TextBox gotoLatitudeText;
        private TextBox gotoLongitudeText;
        private ToolPanel toolPanel;
        private TextBlock xCoordTextBlock;
        private TextBlock yCoordTextBlock;
        private TextBlock zoomTextBlock;
        private string persistedSetting;

        private MapCore mapInstance;
        private string mapName;

        public CoordinatePanel()
        {
            IsEnabled = false;
            lastMouseLocation = new Point();
            DefaultStyleKey = typeof (CoordinatePanel);
            Loaded += CoordinatePanel_Loaded;
            PersistedCommands.PersistedStateLoadedCommand.Executed += PersistedStateLoadedCommand_Executed;
        }

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        public MapCore MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    mapInstance.MouseMove -= map_MouseMove;
                    mapInstance.ViewChangeEnd -= map_ViewChangeEnd;
                    mapInstance.ViewChangeOnFrame -= map_ViewChangeOnFrame;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    mapInstance.MouseMove += map_MouseMove;
                    mapInstance.ViewChangeEnd += map_ViewChangeEnd;
                    mapInstance.ViewChangeOnFrame += map_ViewChangeOnFrame;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            MapInstance = null;
            if (gotoButton != null)
            {
                gotoButton.Click -= gotoButton_Click;
            }
        }

        #endregion

        private void CoordinatePanel_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
            PersistedCommands.PersistedStateSaveCommand.Executed += PersistedStateSaveCommand_Executed;
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }

        private void PersistedStateSaveCommand_Executed(object sender, ExecutedEventArgs e)
        {
            if (displayModeDecimal != null)
            {
                string displaymode = "";
                if (displayModeDecimal.IsChecked.GetValueOrDefault())
                {
                    displaymode = "decimal";
                }
                if (displayModeDDMMSS.IsChecked.GetValueOrDefault())
                {
                    displaymode = "ddmmss";
                }
                if (displayModeUTM.IsChecked.GetValueOrDefault())
                {
                    displaymode = "utm";
                }
                ((PersistedSettings) e.Parameter).CustomSettings.Add(ISOKey, displaymode);
            }
        }

        private void PersistedStateLoadedCommand_Executed(object sender, ExecutedEventArgs e)
        {
            persistedSetting = ((PersistedSettings)e.Parameter).CustomSettings[ISOKey];
            applyPersistedState();
        }

        private void applyPersistedState()
        {
            if (persistedSetting != null)
            {
                switch (persistedSetting)
                {
                    case "decimal":
                        if (displayModeDecimal != null) displayModeDecimal.IsChecked = true;
                        break;
                    case "ddmmss":
                        if (displayModeDDMMSS != null) displayModeDDMMSS.IsChecked = true;
                        break;
                    case "utm":
                        if (displayModeUTM != null) displayModeUTM.IsChecked = true;
                        break;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            xCoordTextBlock = (TextBlock) GetTemplateChild(PART_xCoordTextBlock);
            yCoordTextBlock = (TextBlock) GetTemplateChild(PART_yCoordTextBlock);
            zoomTextBlock = (TextBlock) GetTemplateChild(PART_zoomTextBlock);
            gotoButton = (Button) GetTemplateChild(PART_gotoButton);
            gotoLatitudeText = (TextBox) GetTemplateChild(PART_gotoLatitudeText);
            gotoLongitudeText = (TextBox) GetTemplateChild(PART_gotoLongitudeText);
            displayModeDecimal = (RadioButton) GetTemplateChild(PART_displayModeDecimal);
            displayModeDDMMSS = (RadioButton) GetTemplateChild(PART_displayModeDDMMSS);
            displayModeUTM = (RadioButton) GetTemplateChild(PART_displayModeUTM);
            toolPanel = (ToolPanel) GetTemplateChild(PART_toolPanel);
            IsEnabled = true;

            if (gotoButton != null)
            {
                gotoButton.Click += gotoButton_Click;
            }

            if (displayModeDecimal != null)
            {
                displayModeDecimal.Click += displayMode_Click;
                displayModeDecimal.IsChecked = true;
            }

            if (displayModeDDMMSS != null)
            {
                displayModeDDMMSS.Click += displayMode_Click;
            }

            if (displayModeUTM != null)
            {
                displayModeUTM.Click += displayMode_Click;
            }

            if (gotoLatitudeText != null)
            {
                gotoLatitudeText.GotFocus += (o, e) => gotoLatitudeText.SelectAll();
            }

            if (gotoLongitudeText != null)
            {
                gotoLongitudeText.GotFocus += (o, e) => gotoLongitudeText.SelectAll();
            }

            applyPersistedState();

            //set inital values
            setXYCoordinates(lastMouseLocation);
            setZoomLevel();
        }

        private void displayMode_Click(object sender, RoutedEventArgs e)
        {
            setXYCoordinates(lastMouseLocation);
        }

        private void gotoButton_Click(object sender, RoutedEventArgs e)
        {
            //validate input, set map centre.
            double lat;
            double lon;
            if (double.TryParse(gotoLatitudeText.Text, out lat) && double.TryParse(gotoLongitudeText.Text, out lon))
            {
                if (lat <= 90 && lat >= -90 && lon <= 180 && lon >= -180)
                {
                    MapInstance.Center = new Location(lat,lon);
                }
            }
            if (toolPanel != null)
            {
                toolPanel.CloseOptions();
            }
        }

        private void map_ViewChangeOnFrame(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                setZoomLevel();
            }
        }

        private void map_ViewChangeEnd(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                setXYCoordinates(lastMouseLocation);
                setZoomLevel();
            }
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsEnabled)
            {
                lastMouseLocation = e.GetPosition(MapInstance);
                setXYCoordinates(lastMouseLocation);
            }
        }

        private void setZoomLevel()
        {
            if (MapInstance != null)
            {
                zoomTextBlock.Text = string.Format("{0:N2}", MapInstance.ZoomLevel);
            }
        }

        private void setXYCoordinates(Point point)
        {
            if (MapInstance != null)
            {
                var mapCoordinate = MapInstance.ViewportPointToLocation(point);
                if (displayModeDecimal.IsChecked.GetValueOrDefault())
                {
                    xCoordTextBlock.Text = string.Format("{0:N4}", mapCoordinate.Longitude);
                    yCoordTextBlock.Text = string.Format("{0:N4},", mapCoordinate.Latitude);
                    return;
                }
                if (displayModeDDMMSS.IsChecked.GetValueOrDefault())
                {
                    xCoordTextBlock.Text = Utilities.DDMMSSFromDecimal(mapCoordinate.Longitude) +
                                           ((mapCoordinate.Longitude > 0) ? "E " : "W ");
                    yCoordTextBlock.Text = Utilities.DDMMSSFromDecimal(mapCoordinate.Latitude) +
                                           ((mapCoordinate.Latitude > 0) ? "N " : "S ");
                    return;
                }
                if (displayModeUTM.IsChecked.GetValueOrDefault())
                {
                    //Transform to UTM
                    var ctfac = new CoordinateTransformationFactory();
                    GeographicCoordinateSystem wgs84geo = GeographicCoordinateSystem.WGS84;
                    var zone = (int) Math.Ceiling((mapCoordinate.Longitude + 180)/6);
                    ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, mapCoordinate.Latitude > 0);
                    ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84geo, utm);
                    Point pUtm =
                        trans.MathTransform.Transform(new Point(mapCoordinate.Longitude, mapCoordinate.Latitude));
                    xCoordTextBlock.Text = string.Format("N{0}", Math.Round(pUtm.Y, 0)) +
                                           string.Format(" ({0}{1})", zone, mapCoordinate.Latitude > 0 ? 'N' : 'S');
                    yCoordTextBlock.Text = string.Format("E{0} ", Math.Round(pUtm.X, 0));
                    return;
                }
            }
        }
    }
}