using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class OnSnapshotSaveArgs<T> : EventArgs
    {
        public T Data { get; set; }
        public string FileName { get; set; }
        public Action<string> OnSnapshotSaveComplete = (s) => { ;};
    }

    public class OnSnapshotLoadedArgs<T> : EventArgs
    {
        public string ID { get; set; }
        public string FileName { get; set; }
        public Action<T, string> OnSnapshotLoadedComplete = (ps, maml) => { ;};
    }
}
