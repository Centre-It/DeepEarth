using System.Windows.Controls;
using System.Windows.Input;

namespace ExampleControl.Controls.ColourPicker
{
    public class ClickTextBox : TextBox
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = false;
            SelectAll();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            e.Handled = false;
        }
    }
}