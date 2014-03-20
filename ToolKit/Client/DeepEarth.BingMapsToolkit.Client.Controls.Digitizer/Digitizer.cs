// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;
using GeoAPI.Geometries;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void GeometryChangedEventHandler(object sender, GeometryChangedEventArgs args);

    [TemplatePart(Name = PART_Layers, Type = typeof(ComboBox))]
    [TemplatePart(Name = PART_Styles, Type = typeof(ComboBox))]
    [TemplatePart(Name = PART_FreeDrawToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_LineStringToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_PolygonToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_PointPinToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_EraserToggleButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_NewStyle, Type = typeof(Button))]
    [TemplatePart(Name = PART_GeometryStylePicker, Type = typeof(GeometryStylePicker))]
    [TemplatePart(Name = PART_New, Type = typeof(Button))]
    [TemplatePart(Name = PART_Save, Type = typeof(Button))]
    [TemplatePart(Name = PART_Cancel, Type = typeof(Button))]
    [TemplatePart(Name = PART_MetaData, Type = typeof(Button))]
    [TemplatePart(Name = PART_MetaDataBack, Type = typeof(Button))]
    [TemplatePart(Name = PART_MetaDataSave, Type = typeof(Button))]
    [TemplatePart(Name = PART_AdditionalContent, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PART_ScrollViewer, Type = typeof(ScrollViewer))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_Draw, GroupName = VSM_OperationStates)]
    [TemplateVisualState(Name = VSM_Style, GroupName = VSM_OperationStates)]
    [TemplateVisualState(Name = VSM_MetaData, GroupName = VSM_OperationStates)]
    public class Digitizer : ContentControl, IMapControl<MapCore>
    {
        private const string PART_AdditionalContent = "PART_AdditionalContent";
        private const string PART_Cancel = "PART_Cancel";
        private const string PART_EraserToggleButton = "PART_EraserToggleButton";
        private const string PART_FreeDrawToggleButton = "PART_FreeDrawToggleButton";
        private const string PART_GeometryStylePicker = "PART_GeometryStylePicker";
        private const string PART_Layers = "PART_Layers";
        private const string PART_LineStringToggleButton = "PART_LineStringToggleButton";
        private const string PART_MetaData = "PART_MetaData";
        private const string PART_New = "PART_New";
        private const string PART_NewStyle = "PART_NewStyle";
        private const string PART_PointPinToggleButton = "PART_PointPinToggleButton";
        private const string PART_PolygonToggleButton = "PART_PolygonToggleButton";
        private const string PART_Save = "PART_Save";
        private const string PART_Styles = "PART_Styles";
        private const string PART_MetaDataBack = "PART_MetaDataBack";
        private const string PART_MetaDataSave = "PART_MetaDataSave";
        private const string PART_ScrollViewer = "PART_ScrollViewer";

        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_Draw = "Draw";
        private const string VSM_MetaData = "MetaData";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";
        private const string VSM_OperationStates = "OperationStates";
        private const string VSM_Style = "Style";
        private Button cancelButton;
        private state currentState;

        private StyleSpecification currentStyle;
        private EditGeometry editGeometry;

        private ToggleButton eraserToggleButton;
        private ToggleButton freeDrawToggleButton;
        private GeometryStylePicker geometryStylePicker;
        private bool isMouseOver;
        private ComboBox layerComboBox;
        private IEnumerable<LayerDefinition> layers;
        private ToggleButton lineStringToggleButton;
        private string mapName;
        private Button metaDataButton;
        private Button saveMetaDataButton;
        private Button backMetaDataButton;
        private Button newButton;
        private Button newStyle;
        private IGeometry originalGeometry;
        private ToggleButton pointPinToggleButton;
        private ToggleButton polygonToggleButton;
        private Button saveButton;
        private Dictionary<string, StyleSpecification> styles;
        private ComboBox stylesComboBox;
        private ScrollViewer scrollViewer;

        public Digitizer()
        {
            IsEnabled = false;
            DefaultStyleKey = typeof(Digitizer);
            Loaded += Digitizer_Loaded;
        }

        public IGeometry Geometry
        {
            get
            {
                //get latest version
                if (editGeometry != null)
                {
                    return editGeometry.Geometry;
                }
                return null;
            }
            set
            {
                if (editGeometry != null)
                {
                    OnCancel();
                }
                if (value != null)
                {
                    originalGeometry = (IGeometry)value.Clone();
                    editGeometry = new EditGeometry
                    {
                        MapInstance = MapInstance,
                        Geometry = (IGeometry)value.Clone(),
                    };
                }
                else
                {
                    originalGeometry = null;
                    editGeometry = new EditGeometry
                    {
                        MapInstance = MapInstance,
                    };
                }
                if (currentStyle != null)
                {
                    editGeometry.StyleSpecification = currentStyle;
                }
                updateUIAvailability(true);
            }
        }

        public string LayerID
        {
            get
            {
                if (layerComboBox != null)
                {
                    return ((LayerDefinition)layerComboBox.SelectedItem).LayerID;
                }
                return "";
            }
            set
            {
                if (layerComboBox != null)
                {
                    foreach (object item in layerComboBox.Items)
                    {
                        if (((LayerDefinition)item).LayerID == value)
                        {
                            layerComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public string StyleID
        {
            get
            {
                if (stylesComboBox != null && stylesComboBox.SelectedItem != null)
                {
                    return ((KeyValuePair<string, StyleSpecification>)stylesComboBox.SelectedItem).Key;
                }
                return "";
            }
            set
            {
                if (stylesComboBox != null)
                {
                    foreach (object item in stylesComboBox.Items)
                    {
                        if (((KeyValuePair<string, StyleSpecification>)item).Key == value)
                        {
                            stylesComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public string ItemID { get; set; }

        public IEnumerable<LayerDefinition> Layers
        {
            get { return layers; }
            set
            {
                layers = value;
                if (layerComboBox != null)
                {
                    bindLayers();
                }
            }
        }

        public Dictionary<string, StyleSpecification> Styles
        {
            get { return styles; }
            set
            {
                styles = value;
                if (stylesComboBox != null)
                {
                    bindStyles();
                }
            }
        }

        private IEnumerable icons;
        public IEnumerable Icons
        {
            get
            {
                if (geometryStylePicker != null)
                {
                    return geometryStylePicker.Icons;
                }
                return icons;
            }
            set
            {
                icons = value;
                if (geometryStylePicker != null)
                {
                    geometryStylePicker.Icons = icons;
                }
            }
        }

        public bool DisablePoint { get; set; }
        public bool DisableLine { get; set; }
        public bool DisablePolygon { get; set; }
        public bool MetadataOnEdit { get; set; }

        #region IMapControl<MapCore> Members

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        public MapCore MapInstance { get; set; }

        public void Dispose()
        {
            if (stylesComboBox != null)
            {
                stylesComboBox.SelectionChanged -= stylesComboBox_SelectionChanged;
            }

            if (layerComboBox != null)
            {
                layerComboBox.SelectionChanged -= layerComboBox_SelectionChanged;
            }

            if (!HideStyles)
            {
                if (newStyle != null)
                {
                    newStyle.Click -= newStyle_Click;
                }

                if (geometryStylePicker != null)
                {
                    geometryStylePicker.Save -= geometryStylePicker_Save;
                    geometryStylePicker.Cancel -= geometryStylePicker_Cancel;
                }

            }

            if (freeDrawToggleButton != null)
            {
                freeDrawToggleButton.Checked -= freeDrawToggleButton_Checked;
                freeDrawToggleButton.Unchecked -= freeDrawToggleButton_Unchecked;
            }

            if (lineStringToggleButton != null)
            {
                lineStringToggleButton.Checked -= lineStringToggleButton_Checked;
                lineStringToggleButton.Unchecked -= lineStringToggleButton_Unchecked;
            }

            if (polygonToggleButton != null)
            {
                polygonToggleButton.Checked -= polygonToggleButton_Checked;
                polygonToggleButton.Unchecked -= polygonToggleButton_Unchecked;
            }

            if (pointPinToggleButton != null)
            {
                pointPinToggleButton.Checked -= pointPinToggleButton_Checked;
                pointPinToggleButton.Unchecked -= pointPinToggleButton_Unchecked;
            }

            if (eraserToggleButton != null)
            {
                eraserToggleButton.Checked -= eraserToggleButton_Checked;
                eraserToggleButton.Unchecked -= eraserToggleButton_Unchecked;
            }

            if (newButton != null)
            {
                newButton.Click -= newButton_Click;
            }
            if (saveButton != null)
            {
                saveButton.Click -= saveButton_Click;
            }

            if (cancelButton != null)
            {
                cancelButton.Click -= cancelButton_Click;
            }

            if (metaDataButton != null)
            {
                metaDataButton.Click -= metaDataButton_Click;
            }

            if (backMetaDataButton != null)
            {
                backMetaDataButton.Click -= backMetaDataButton_Click;
            }

            if (saveMetaDataButton != null)
            {
                saveMetaDataButton.Click -= saveMetaDataButton_Click;
            }
        }

        #endregion

        public event GeometryChangedEventHandler Save;
        public event GeometryChangedEventHandler Cancel;
        public event EventHandler New;

        protected void OnSave()
        {
            //support cancel property to stop the save action.
            bool cancel = false;
            //save if its a new geometery or there was an old one (delete).
            if (Geometry != null || originalGeometry != null)
            {
                var eventArgs = new GeometryChangedEventArgs
                {
                    OldGeometry = originalGeometry,
                    NewGeometry = Geometry,
                    LayerID = LayerID,
                    StyleID = StyleID,
                    StyleSpecification = currentStyle,
                    Cancel = false,
                };
                if (Save != null)
                {
                    Save(this, eventArgs);
                    cancel = eventArgs.Cancel;
                }
            }
            if (cancel)
            {
                //redirect to the metadata state.
                currentState = state.metadata;
                GoToState(true);
            }
            else
            {
                disposeEditGeometry();
                updateUIAvailability(false);
            }
        }

        public void CancelDrawing()
        {
            OnCancel();
        }

        protected void OnCancel()
        {
            if (Cancel != null)
                Cancel(this, new GeometryChangedEventArgs
                {
                    OldGeometry = originalGeometry,
                    NewGeometry = Geometry,
                    LayerID = LayerID,
                    StyleID = StyleID,
                    StyleSpecification = currentStyle
                });
            disposeEditGeometry();
            updateUIAvailability(false);
        }

        protected void OnNew()
        {
            if (New != null)
                New(this, new EventArgs());
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
        }

        private void Digitizer_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
            MouseEnter += Digitizer_MouseEnter;
            MouseLeave += Digitizer_MouseLeave;
            isMouseOver = false;
            currentState = state.draw;

            GoToState(true);
        }

        private void Digitizer_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            GoToState(true);
        }

        private void Digitizer_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            GoToState(true);
        }

        public bool HideStyles { get; set; }

        public ScrollViewer ScrollViewer
        {
            get { return scrollViewer; }
            set { scrollViewer = value; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            layerComboBox = (ComboBox)GetTemplateChild(PART_Layers);
            stylesComboBox = (ComboBox)GetTemplateChild(PART_Styles);
            freeDrawToggleButton = (ToggleButton)GetTemplateChild(PART_FreeDrawToggleButton);
            lineStringToggleButton = (ToggleButton)GetTemplateChild(PART_LineStringToggleButton);
            polygonToggleButton = (ToggleButton)GetTemplateChild(PART_PolygonToggleButton);
            pointPinToggleButton = (ToggleButton)GetTemplateChild(PART_PointPinToggleButton);
            eraserToggleButton = (ToggleButton)GetTemplateChild(PART_EraserToggleButton);
            geometryStylePicker = (GeometryStylePicker)GetTemplateChild(PART_GeometryStylePicker);
            newButton = (Button)GetTemplateChild(PART_New);
            saveButton = (Button)GetTemplateChild(PART_Save);
            cancelButton = (Button)GetTemplateChild(PART_Cancel);
            metaDataButton = (Button)GetTemplateChild(PART_MetaData);
            saveMetaDataButton = (Button)GetTemplateChild(PART_MetaDataSave);
            backMetaDataButton = (Button)GetTemplateChild(PART_MetaDataBack);
            newStyle = (Button)GetTemplateChild(PART_NewStyle);
            scrollViewer = (ScrollViewer)GetTemplateChild(PART_ScrollViewer);


            if (stylesComboBox != null)
            {
                stylesComboBox.SelectionChanged += stylesComboBox_SelectionChanged;
            }

            if (stylesComboBox != null && Styles != null)
            {
                bindStyles();
            }

            if (layerComboBox != null && Layers != null)
            {
                bindLayers();
            }

            if (layerComboBox != null)
            {
                layerComboBox.SelectionChanged += layerComboBox_SelectionChanged;
            }

            if (!HideStyles)
            {

                if (newStyle != null)
                {
                    newStyle.Click += newStyle_Click;
                }

                if (geometryStylePicker != null)
                {
                    geometryStylePicker.Save += geometryStylePicker_Save;
                    geometryStylePicker.Cancel += geometryStylePicker_Cancel;
                    if (icons != null)
                    {
                        geometryStylePicker.Icons = icons;
                    }
                }
                else
                {
                    //hide the new button.
                    if (newStyle != null)
                    {
                        newStyle.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                //hide the stylesComboBox.
                if (stylesComboBox != null)
                {
                    stylesComboBox.IsEnabled = false;
                }
                //hide the new button.
                if (newStyle != null)
                {
                    newStyle.Visibility = Visibility.Collapsed;
                }
            }

            if (freeDrawToggleButton != null)
            {
                freeDrawToggleButton.Checked += freeDrawToggleButton_Checked;
                freeDrawToggleButton.Unchecked += freeDrawToggleButton_Unchecked;
            }

            if (lineStringToggleButton != null)
            {
                lineStringToggleButton.Visibility = DisableLine ? Visibility.Collapsed : Visibility.Visible;
                lineStringToggleButton.Checked += lineStringToggleButton_Checked;
                lineStringToggleButton.Unchecked += lineStringToggleButton_Unchecked;
            }

            if (polygonToggleButton != null)
            {
                polygonToggleButton.Visibility = DisablePolygon ? Visibility.Collapsed : Visibility.Visible;
                polygonToggleButton.Checked += polygonToggleButton_Checked;
                polygonToggleButton.Unchecked += polygonToggleButton_Unchecked;
            }

            if (pointPinToggleButton != null)
            {
                pointPinToggleButton.Visibility = DisablePoint ? Visibility.Collapsed : Visibility.Visible;
                pointPinToggleButton.Checked += pointPinToggleButton_Checked;
                pointPinToggleButton.Unchecked += pointPinToggleButton_Unchecked;
            }

            if (eraserToggleButton != null)
            {
                eraserToggleButton.Checked += eraserToggleButton_Checked;
                eraserToggleButton.Unchecked += eraserToggleButton_Unchecked;
            }

            if (newButton != null)
            {
                newButton.Click += newButton_Click;
            }
            if (saveButton != null)
            {
                saveButton.Click += saveButton_Click;
            }

            if (cancelButton != null)
            {
                cancelButton.Click += cancelButton_Click;
            }

            if (metaDataButton != null)
            {
                metaDataButton.Click += metaDataButton_Click;
            }

            if (backMetaDataButton != null)
            {
                backMetaDataButton.Click += backMetaDataButton_Click;
            }

            if (saveMetaDataButton != null)
            {
                saveMetaDataButton.Click += saveMetaDataButton_Click;
            }

            updateUIAvailability(false);
        }

        void layerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the default style for the layer
            if (e.AddedItems.Count > 0)
            {
                var definition = ((LayerDefinition)e.AddedItems[0]);
                if (!string.IsNullOrEmpty(definition.LayerStyleName))
                {
                    StyleID = definition.LayerStyleName;
                }
            }
        }

        void saveMetaDataButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave();
        }

        void backMetaDataButton_Click(object sender, RoutedEventArgs e)
        {
            currentState = state.draw;
            GoToState(true);
        }

        void metaDataButton_Click(object sender, RoutedEventArgs e)
        {
            currentState = state.metadata;
            GoToState(true);
        }

        private void stylesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setStyleOnGeography();
        }

        private void setStyleOnGeography()
        {
            if (stylesComboBox.SelectedItem != null)
            {
                currentStyle = ((KeyValuePair<string, StyleSpecification>)stylesComboBox.SelectedItem).Value;
                if (editGeometry != null)
                {
                    editGeometry.StyleSpecification = currentStyle;
                }
            }
        }

        void newStyle_Click(object sender, RoutedEventArgs e)
        {
            currentState = state.style;
            GoToState(true);
            stylesComboBox.IsEnabled = false;
            currentStyle = new StyleSpecification();
            geometryStylePicker.SelectedStyle = currentStyle;
            //wire up new style spec to editing object if applicable.
            if (editGeometry != null)
            {
                editGeometry.StyleSpecification = geometryStylePicker.SelectedStyle;
            }
        }

        void geometryStylePicker_Save(object sender, EventArgs e)
        {
            currentState = state.draw;
            GoToState(true);
        }

        void geometryStylePicker_Cancel(object sender, EventArgs e)
        {
            currentState = state.draw;
            GoToState(true);
            stylesComboBox.IsEnabled = !HideStyles;
            setStyleOnGeography();
        }

        private void bindLayers()
        {
            layerComboBox.ItemsSource = Layers;
            if (layerComboBox.Items.Count > 0)
            {
                layerComboBox.SelectedIndex = 0;
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void bindStyles()
        {
            stylesComboBox.ItemsSource = Styles;
            if (stylesComboBox.Items.Count > 0)
            {
                stylesComboBox.SelectedIndex = 0;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave();
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            Geometry = null;
            OnNew();
        }

        private void eraserToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(null);
        }

        private void eraserToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            editGeometry.EraseMode = true;
            editGeometry.EditCompleted += editGeometry_EditCompleted;
            updateToggleStates(sender);
        }

        public void TriggerPin()
        {
            pointPinToggleButton.IsChecked = true;
        }

        private void pointPinToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(null);
        }

        private void pointPinToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(sender);
            editGeometry.AddNew(GeometryType.Point);
            editGeometry.EditCompleted += editGeometry_EditCompleted;
        }

        public void TriggerPolygon()
        {
            polygonToggleButton.IsChecked = true;
        }

        private void polygonToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(null);
        }

        private void polygonToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(sender);
            editGeometry.AddNew(GeometryType.Polygon);
            editGeometry.EditCompleted += editGeometry_EditCompleted;
        }

        public void TriggerLineString()
        {
            lineStringToggleButton.IsChecked = true;
        }

        private void lineStringToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(null);
        }

        private void lineStringToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(sender);
            editGeometry.AddNew(GeometryType.LineString);
            editGeometry.EditCompleted += editGeometry_EditCompleted;
        }

        private void editGeometry_EditCompleted(object sender, EditGeometryChangedEventArgs args)
        {
            editGeometry.EraseMode = false;
            editGeometry.EditCompleted -= editGeometry_EditCompleted;
            updateToggleStates(null);
        }

        private void freeDrawToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            updateToggleStates(null);
        }

        private void freeDrawToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            editGeometry.AddNew(GeometryType.None);
        }

        private void disposeEditGeometry()
        {
            if (editGeometry != null)
            {
                //TODO: prompt save warning?
                editGeometry.Dispose();
                editGeometry = null;
            }
        }

        private void updateToggleStates(object sender)
        {
            Commands.StopDrawingCommand.Execute();
            if (freeDrawToggleButton != null && freeDrawToggleButton != sender &&
                freeDrawToggleButton.IsChecked.GetValueOrDefault(false))
            {
                freeDrawToggleButton.IsChecked = false;
            }
            if (lineStringToggleButton != null && lineStringToggleButton != sender &&
                lineStringToggleButton.IsChecked.GetValueOrDefault(false))
            {
                lineStringToggleButton.IsChecked = false;
            }
            if (polygonToggleButton != null && polygonToggleButton != sender &&
                polygonToggleButton.IsChecked.GetValueOrDefault(false))
            {
                polygonToggleButton.IsChecked = false;
            }
            if (pointPinToggleButton != null && pointPinToggleButton != sender &&
                pointPinToggleButton.IsChecked.GetValueOrDefault(false))
            {
                pointPinToggleButton.IsChecked = false;
            }
            if (eraserToggleButton != null && eraserToggleButton != sender &&
                eraserToggleButton.IsChecked.GetValueOrDefault(false))
            {
                eraserToggleButton.IsChecked = false;
            }
        }

        private void updateUIAvailability(bool drawEnabled)
        {
            if (freeDrawToggleButton != null)
            {
                freeDrawToggleButton.IsEnabled = drawEnabled;
            }
            if (lineStringToggleButton != null)
            {
                lineStringToggleButton.IsEnabled = drawEnabled;
            }
            if (polygonToggleButton != null)
            {
                polygonToggleButton.IsEnabled = drawEnabled;
            }
            if (pointPinToggleButton != null)
            {
                pointPinToggleButton.IsEnabled = drawEnabled;
            }
            if (eraserToggleButton != null)
            {
                eraserToggleButton.IsEnabled = drawEnabled;
            }
            if (saveButton != null)
            {
                saveButton.IsEnabled = drawEnabled;
            }
            if (cancelButton != null)
            {
                cancelButton.IsEnabled = drawEnabled;
            }
            if (stylesComboBox != null)
            {
                stylesComboBox.IsEnabled = drawEnabled && !HideStyles;
            }
            if (newStyle != null)
            {
                newStyle.IsEnabled = drawEnabled;
            }
            if (metaDataButton != null)
            {
                metaDataButton.IsEnabled = drawEnabled;
            }
            if (!drawEnabled)
            {
                currentState = MetadataOnEdit ? state.metadata : state.draw;
                GoToState(true);
            }
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
            switch (currentState)
            {
                case state.draw:
                    VisualStateManager.GoToState(this, VSM_Draw, useTransitions);
                    break;
                case state.style:
                    VisualStateManager.GoToState(this, VSM_Style, useTransitions);
                    break;
                case state.metadata:
                    VisualStateManager.GoToState(this, VSM_MetaData, useTransitions);
                    break;
            }
        }

        #region Nested type: state

        private enum state
        {
            draw,
            style,
            metadata
        }

        #endregion
    }
}