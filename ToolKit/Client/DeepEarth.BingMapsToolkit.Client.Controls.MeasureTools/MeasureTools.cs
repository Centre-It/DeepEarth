// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using Point = System.Windows.Point;
using System.Collections.Generic;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void GeometryChangedEventHandler(object sender, GeometryChangedEventArgs args);

    [TemplatePart(Name = PART_LineStringToggleButton, Type = typeof(ToggleButton))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class MeasureTools : Control, IMapControl<MapCore>
    {


        private const string PART_LineStringToggleButton = "PART_LineStringToggleButton";
        private const string PART_PolygonToggleButton = "PART_PolygonToggleButton";
        private const string PART_LengthTextBlock = "PART_LengthTextBlock";
        private const string PART_AreaTextBlock = "PART_AreaTextBlock";
        private const string PART_DoneButton = "PART_DoneButton";

        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";

        private ToggleButton lineStringToggleButton;
        private ToggleButton polygonToggleButton;
        private TextBlock areaBlock, lengthBlock;
        private Button DoneButton;

        private MapCore mapInstance;
        private string mapName;
        private MeasureGeometry editGeometry;
		
        private bool hasGeometry = false;
        private bool editingPolygon = false;
        private bool validGeometry = false;

        public event GeometryChangedEventHandler MeasuredGeom = (s, a)=>{;};
        public event GeometryChangedEventHandler RemoveGeometry = (s, a) => { ;};
        
        private bool isMouseOver;
		private ICoordinate mouseOver = null;
		private ICoordinate lastMouseClick= null;

		public delegate void OnApplyTemplate_CompleteDelegate();
		public OnApplyTemplate_CompleteDelegate OnApplyTemplate_Complete;

        public MeasureTools()
        {
            IsEnabled = false;
            DefaultStyleKey = typeof(MeasureTools);
            Loaded += MeasureTools_Loaded;
        }

        private void MeasureTools_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
			
            //bind any abstract events here
            GoToState(true);
            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;         
        }


        protected bool Editing { get; set; }
        bool setGeometryChanged = false;
        protected GeometryType GeomType { get; set; }

        protected double Length
        {
            get
            {
                double len = 0.0;

                if (Editing && editGeometry != null && (GeomType == GeometryType.LineString))
                {
                    len = CalcLength();
                    validGeometry = true;
                }

                else if (Editing && editGeometry != null && editingPolygon && (GeomType == GeometryType.Polygon))
                {
                    len = CalcLength();
                    validGeometry = true;
                }

                if (editGeometry != null && !setGeometryChanged)
                {
                    editGeometry.EditChange += editGeometry_EditChange;
                    setGeometryChanged = true;
                }

                return len;
            }
        }

        double CalcLength()
        {
            LineString ls = new LineString(GetCoordinateListInUTM(Geometry.Coordinates));
            return ls.Length;
        }

        protected void ComputeArea()
        {
            if (Editing && editGeometry != null && editingPolygon && (GeomType == GeometryType.Polygon))
            {
                try
                {
                    LinearRing lineRing = new LinearRing(
                        GetCoordinateListInUTM(Geometry.Coordinates)
                        );
                    Polygon polygon = new Polygon(lineRing);
                    Area = polygon.Area;
					
                    polygon = null;
                }
                catch { Area = null; }
            }
            else
            {
                editingPolygon = true;
                Area = null;
            }
        }

        protected double? Area { get; set; }

        ICoordinate[] GetCoordinateListInUTM(ICoordinate[] coordinates)
        {
            CoordinateList coordinateList = new CoordinateList();

            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinateList.Add(CoordinateConvertor.ConvertDegreesToUTM(coordinates[i]));
            }
			

            return coordinateList.ToArray();
        }

		ICoordinate[] GetLine(ICoordinate first, ICoordinate second)
		{
			CoordinateList coordinateList = new CoordinateList();

			coordinateList.Add(CoordinateConvertor.ConvertDegreesToUTM(first));
			coordinateList.Add(CoordinateConvertor.ConvertDegreesToUTM(second));

			return coordinateList.ToArray();
		}

        void editGeometry_EditChange(object sender, EditGeometryChangedEventArgs args)
        {
            MapInstance_MouseClick(sender, null);
        }

        void editGeometry_EditCompleted(object sender, EditGeometryChangedEventArgs args)
        {
            DoneEditing();
        }

        protected void lineStringToggleButton_Checked(object sender, RoutedEventArgs args)
        {
            SetEditGeometry(GeometryType.LineString);
            ClearAreaAndLengthText();
        }

        protected void lineStringToggleButton_UnChecked(object sender, RoutedEventArgs arg)
        {
            editGeometry_EditCompleted(null, null);
        }

        protected void polygonToggleButton_Checked(object sender, RoutedEventArgs arg)
        {
            SetEditGeometry(GeometryType.Polygon);
            ClearAreaAndLengthText();
        }

        protected void polygonToggleButton_UnChecked(object sender, RoutedEventArgs arg)
        {
            editGeometry_EditCompleted(null, null);
        }

        protected void done_clicked(object sender, RoutedEventArgs arg)
        {
            DoneEditing();
        }

        void DoneEditing()
        {
            if (validGeometry)
            {
                MeasuredGeom(this, new GeometryChangedEventArgs()
                {
                    NewGeometry = Geometry
                }
                    );
            }
            DisposeEditGeometry();
            lastMouseClick = null;
            hasGeometry = true;
            Editing = false;
            editingPolygon = false;
            validGeometry = false;
            setGeometryChanged = false;
            ToggleButtonsAfterDone();
        }

        void MapInstance_MouseClick(object sender, MapMouseEventArgs arg)
        {
            if (hasGeometry)
            {
                RemoveGeometry(sender, null);
                hasGeometry = false;
            }
        }

        void SetEditGeometry(GeometryType geometryType)
        {
            if (editGeometry == null)
            {
                editGeometry = new MeasureGeometry()
                {
                    MapInstance = MapInstance
                };
            }

			editGeometry.AddNew(geometryType);
            editGeometry.EditCompleted += editGeometry_EditCompleted;
            GeomType = geometryType;
            Editing = false;
            editingPolygon = false;
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
                    MapInstance.MouseClick -= MapInstance_MouseClick;
					MapInstance.MouseClick -= MapInstace_Click;
					MapInstance.MouseMove -= map_MouseMove;
					//MapInstance.MouseMove -= (s, a) => { UpdateMeaseruPanel(); };
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    MapInstance.MouseClick += MapInstance_MouseClick;
					MapInstance.MouseClick += MapInstace_Click;
					MapInstance.MouseMove += map_MouseMove;
					//MapInstance.MouseMove += (s, a) => { UpdateMeaseruPanel(); };
                }
            }
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //wire up all your parts here

            IsEnabled = true;

            GeomType = GeometryType.None;
            lineStringToggleButton = (ToggleButton)GetTemplateChild(PART_LineStringToggleButton);
            polygonToggleButton = (ToggleButton)GetTemplateChild(PART_PolygonToggleButton);
            areaBlock = (TextBlock)GetTemplateChild(PART_AreaTextBlock);
            lengthBlock = (TextBlock)GetTemplateChild(PART_LengthTextBlock);
            DoneButton = (Button)GetTemplateChild(PART_DoneButton);
            ClearAreaAndLengthText();

            if (lineStringToggleButton != null)
            {
                lineStringToggleButton.Checked += lineStringToggleButton_Checked;
                lineStringToggleButton.Unchecked += lineStringToggleButton_UnChecked;
            }

            if (polygonToggleButton != null)
            {
                polygonToggleButton.Checked += polygonToggleButton_Checked;
                polygonToggleButton.Unchecked += polygonToggleButton_UnChecked;
            }

            if (DoneButton != null)
            {
                DoneButton.Click += done_clicked;
            }

			if (OnApplyTemplate_Complete != null)
				OnApplyTemplate_Complete();
        }

        public void ToggleButtonsAfterDone()
        {
       
                if (lineStringToggleButton.IsChecked == true)
                {
                    lineStringToggleButton.IsChecked = false;
                    return;
                }
            
                if (polygonToggleButton.IsChecked == true)
                {
                    polygonToggleButton.IsChecked = false;
                    return;
                }
        }

        public void UpdateMeaseruPanel()
        {
            if (Editing)
            {
                if (GeomType == GeometryType.Polygon)
                {
                    ComputeArea();
                    {
                        if (Area != null)
                        {
							areaBlock.Text = string.Format("Area: {0:N2}m2", Area);
                                lengthBlock.Text = string.Format("Length: {0:N2}m", Length);
                        }
                        else
                        {
                            ClearAreaAndLengthText();
                        }
                    }
                }

                else if (GeomType == GeometryType.LineString)
					lengthBlock.Text = string.Format("Length: {0:N}m", Length);
                UpdateLayout();
                return;
            }
            else
                ClearAreaAndLengthText();
            Editing = true;
        }

        protected void ClearAreaAndLengthText()
        {
            Dispatcher.BeginInvoke(delegate()
            {
                areaBlock.Text = "Area: N/A";
                lengthBlock.Text = "Length: N/A";
            }
            );
        }

        #region MouseEvents
        private void mouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }
        #endregion

        void MapInstace_Click(object sender, MapMouseEventArgs arg)
        {
            if (
				(polygonToggleButton.IsChecked != null  || lineStringToggleButton.IsChecked != null)
					&& (polygonToggleButton.IsChecked.Value || lineStringToggleButton.IsChecked.Value))
            {
			    lastMouseClick = XYCoordinates(arg.ViewportPoint);
                UpdateMeaseruPanel();
            }
        }

		private void map_MouseMove(object sender, MouseEventArgs e)
		{

			if ((IsEnabled))
			{
				if (((GeomType == GeometryType.LineString) ) 
					&& (lastMouseClick != null) 
					&& (polygonToggleButton.IsChecked != null  || lineStringToggleButton.IsChecked != null)
					&& (polygonToggleButton.IsChecked.Value || lineStringToggleButton.IsChecked.Value))
				{
					Point lastMouseLocation = e.GetPosition(MapInstance);

					mouseOver = XYCoordinates(lastMouseLocation);
					double lastLength;
					try
					{
						lastLength = Length;
					}
					catch
					{
						lastLength = 0;
					}
					lengthBlock.Text = string.Format("Length: {0:N2}m", lastLength + (new LineString(GetLine(lastMouseClick, mouseOver))).Length);
				}


				if (((GeomType == GeometryType.Polygon)) 
					&& (lastMouseClick != null) 
					&& (polygonToggleButton.IsChecked != null  || lineStringToggleButton.IsChecked != null)
					&& (polygonToggleButton.IsChecked.Value || lineStringToggleButton.IsChecked.Value))
				{
					Point lastMouseLocation = e.GetPosition(MapInstance);
					mouseOver = XYCoordinates(lastMouseLocation);
					List<ICoordinate> coord = new List<ICoordinate>(editGeometry.LastPoints);
					if (coord.Count >= 2)
					{
						coord.Add(mouseOver);
						coord.Add(coord[0]);
						LinearRing lineRing = new LinearRing(GetCoordinateListInUTM(coord.ToArray())); 
						
						Polygon polygon = new Polygon(lineRing);
						areaBlock.Text = string.Format("Area: {0:N2}m2", polygon.Area);
						lengthBlock.Text = string.Format("Length: {0:N2}m", lineRing.Length);
					}
					else
					{
						areaBlock.Text = "Area: N/A";
					}
				}
			}
		}

		private Coordinate XYCoordinates(Point point)
		{
			var mapCoordinate = MapInstance.ViewportPointToLocation(point);
			return new Coordinate(mapCoordinate.Longitude, mapCoordinate.Latitude);
		}

        private void GoToState(bool useTransitions)
        {
            if (isMouseOver)
            {
                VisualStateManager.GoToState(this, VSM_MouseOver, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_Normal, useTransitions);
            }
        }

        public void Dispose()
        {
            MouseEnter -= mouseEnter;
            MouseLeave -= mouseLeave;
            lineStringToggleButton.Checked -= lineStringToggleButton_Checked;
            lineStringToggleButton.Unchecked -= lineStringToggleButton_UnChecked;
            polygonToggleButton.Checked -= polygonToggleButton_Checked;
            polygonToggleButton.Unchecked -= polygonToggleButton_UnChecked;
        }

        void DisposeEditGeometry()
        {
            if (editGeometry != null)
            {
                editGeometry.EditCompleted -= editGeometry_EditCompleted;
                editGeometry.EditChange -= editGeometry_EditChange;
                editGeometry.Dispose();
                editGeometry = null;
            }
        }

        public IGeometry Geometry
        {
            get
            {
                if (editGeometry != null)
                {
                    return editGeometry.Geometry;
                }
                return null;
            }
            set
            {
                editGeometry = new MeasureGeometry()
                {
                    MapInstance = MapInstance
                };
            }
        }
    }
}