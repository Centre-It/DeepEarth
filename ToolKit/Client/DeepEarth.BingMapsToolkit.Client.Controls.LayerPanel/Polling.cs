// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void PollingRefreshEventHandler(object sender, PollingEventArgs args);

    public class Polling
    {
        private BackgroundWorker backgroundWorker;
        private bool backgroundWorkerInRestart;
        public ObservableCollection<LayerTreeViewItem> ItemsSource { get; set; }
        public event PollingRefreshEventHandler RefreshNeeded;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnRefreshNeeded(PollingEventArgs e)
        {
            if (RefreshNeeded != null)
                RefreshNeeded(this, e);
        }


        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Refresh Layer
            OnRefreshNeeded(new PollingEventArgs {LayerDefinition = (LayerDefinition) e.UserState});
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var layerDefinitions = (ObservableCollection<LayerTreeViewItem>) e.Argument;
            int incrementtime = 0;
            //This loop keeps spining forever by design to process layer refreshes
            while (true)
            {
                if (backgroundWorker.CancellationPending) break;
                incrementtime += 1;
                //need to reset at limit
                if (incrementtime == int.MaxValue) incrementtime = 1;
                foreach (LayerTreeViewItem treeViewItem in layerDefinitions)
                {
                    foreach (LayerDefinition layerDefinition in treeViewItem.Layers)
                    {
                        if (layerDefinition.Selected && layerDefinition.LayerTimeout > 0 &&
                            (incrementtime % layerDefinition.LayerTimeout == 0))
                        {
                            backgroundWorker.ReportProgress(0, layerDefinition);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void resetPollingWorker(ObservableCollection<LayerTreeViewItem> itemsSource)
        {
            ItemsSource = itemsSource;
            backgroundWorkerInRestart = true;
            if (backgroundWorker != null)
            {
                backgroundWorker.CancelAsync();
            }
            else
            {
                startPollingWorker();
            }
        }

        private void startPollingWorker()
        {
            backgroundWorkerInRestart = false;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync(ItemsSource);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (backgroundWorkerInRestart)
            {
                startPollingWorker();
            }
        }
    }

    public class PollingEventArgs : EventArgs
    {
        public LayerDefinition LayerDefinition { get; set; }
    }
}