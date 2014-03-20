using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_SnapshotButton, Type = typeof(Button))]
    public class Snapshot : PersistedState
    {
        public static string DEurl = "DEurl";

        private const string PART_SnapshotButton = "PART_SnapshotButton";

        private Button SnapshotButton;

        /// <summary>
        /// Fired when the settings have been loaded from Custom Storage
        /// </summary>
        public event EventHandler<OnSnapshotSaveArgs<PersistedSettings>> SnapshotSave = (s, e) => { };

        /// <summary>
        /// Fired when setting need to be saved to Custom Storage
        /// </summary>
        public event EventHandler<OnSnapshotLoadedArgs<PersistedSettings>> SnapshotLoaded = (s, e) => { };

        public Snapshot()
        {
            this.DefaultStyleKey = typeof(Snapshot);
            ApplyTemplate();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SnapshotButton = (Button)GetTemplateChild(PART_SnapshotButton);

            if (SnapshotButton != null)
            {
                SnapshotButton.Click += new RoutedEventHandler(SnapshotButton_Click);
            }
        }

        void SnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            OnSnapshotSaveArgs<PersistedSettings> args = new OnSnapshotSaveArgs<PersistedSettings>();

            args.FileName = PersistedState.PersistedStateISOKey;
            args.Data = GetCurrentPersistedSettings();
            args.OnSnapshotSaveComplete = PocessSaveSnapshot;

            SnapshotSave(this, args);
        }

        private void PocessSaveSnapshot(string resultURL)
        {

            var dialog = new ShowURLDialog(resultURL);
            dialog.Show();
        }

        protected override void Load()
        {
            if (!HtmlPage.Document.QueryString.ContainsKey(DEurl))
            {
                base.Load();
            }
            else
            {
                LoadFromCustomStorage();
            }
        }

        private void LoadFromCustomStorage()
        {
            OnSnapshotLoadedArgs<PersistedSettings> args = new OnSnapshotLoadedArgs<PersistedSettings>();

            args.FileName = PersistedStateISOKey;
            args.ID = HtmlPage.Document.QueryString[DEurl];
            args.OnSnapshotLoadedComplete = ProcessLoadedSnapshot;

            SnapshotLoaded(this, args);
        }

        protected void ProcessLoadedSnapshot(PersistedSettings persistedSettings, string maml)
        {
            ProcessLoadedPersistedSettings(persistedSettings);
            if (!String.IsNullOrEmpty(maml))
            {
                Automation auto = new Automation();
                auto.MapInstance = MapInstance;
                auto.Process(maml);
            }
        }
    }
}
