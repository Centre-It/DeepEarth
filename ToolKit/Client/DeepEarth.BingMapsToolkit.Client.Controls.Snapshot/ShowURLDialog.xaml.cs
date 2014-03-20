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

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public partial class ShowURLDialog : ChildWindow
    {
        public ShowURLDialog(string message)
        {
            InitializeComponent();
            URLResultTextBox.Text = message;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}

