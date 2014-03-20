// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DeepEarth.Client.MapControl;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace DeepEarth.Client.Controls.DEMap
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
    public class CoordinatePanel : Control, IMapControl<Map>
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

        private static Point lastMouseLocation;
        private RadioButton displayModeDDMMSS;
        private RadioButton displayModeDecimal;
        private RadioButton displayModeUTM;
        private Button gotoButton;
        private TextBox gotoLatitudeText;
        private TextBox gotoLongitudeText;
        private Map mapInstance;

        private string mapName;
        private ToolPanel toolPanel;
        private TextBlock xCoordTextBlock;
        private TextBlock yCoordTextBlock;
        private TextBlock zoomTextBlock;

        public CoordinatePanel()
        {
            IsEnabled = false;
            lastMouseLocation = new Point();
            DefaultStyleKey = typeof (CoordinatePanel);
            Loaded += CoordinatePanel_Loaded;
        }

        #region IMapControl<Map> Members

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        public Map MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    mapInstance.Events.MapMouseMove -= map_MouseMove;
                    mapInstance.Events.MapViewChanged -= map_ViewChangeOnFrame;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    mapInstance.Events.MapMouseMove += map_MouseMove;
                    mapInstance.Events.MapViewChanged += map_ViewChangeOnFrame;
                }
            }
        }

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
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
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
                    MapInstance.GeoCenter = new Point(lon, lat);
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
            zoomTextBlock.Text = string.Format("{0:N2}", MapInstance.ZoomLevel);
        }

        private void setXYCoordinates(Point point)
        {
            Point mapCoordinate = MapInstance.CoordHelper.PixelToGeo(point);
            if (displayModeDecimal.IsChecked.GetValueOrDefault())
            {
                xCoordTextBlock.Text = string.Format("{0:N4}", mapCoordinate.X);
                yCoordTextBlock.Text = string.Format("{0:N4},", mapCoordinate.Y);
                return;
            }
            if (displayModeDDMMSS.IsChecked.GetValueOrDefault())
            {
                xCoordTextBlock.Text = Utilities.DDMMSSFromDecimal(mapCoordinate.X) +
                                       ((mapCoordinate.X > 0) ? "E " : "W ");
                yCoordTextBlock.Text = Utilities.DDMMSSFromDecimal(mapCoordinate.Y) +
                                       ((mapCoordinate.Y > 0) ? "N " : "S ");
                return;
            }
            if (displayModeUTM.IsChecked.GetValueOrDefault())
            {
                //Transform to UTM
                var ctfac = new CoordinateTransformationFactory();
                GeographicCoordinateSystem wgs84geo = GeographicCoordinateSystem.WGS84;
                var zone = (int) Math.Ceiling((mapCoordinate.X + 180)/6);
                ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, mapCoordinate.Y > 0);
                ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84geo, utm);
                Point pUtm = trans.MathTransform.Transform(new Point(mapCoordinate.X, mapCoordinate.Y));
                xCoordTextBlock.Text = string.Format("N{0}", Math.Round(pUtm.Y, 0)) +
                                       string.Format(" ({0}{1})", zone, mapCoordinate.Y > 0 ? 'N' : 'S');
                yCoordTextBlock.Text = string.Format("E{0} ", Math.Round(pUtm.X, 0));
                return;
            }
        }
    }
}