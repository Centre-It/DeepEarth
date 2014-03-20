// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using DeepEarth.Client.Common.Commanding;

namespace DeepEarth.Client.Controls.LayerPanel
{
    public static class Commands
    {
        static Commands()
        {
            EditVector = new Command("EditVector");
            LoadBalloonData = new Command("LoadBalloonData");
        }

        public static Command EditVector { get; private set; }
        public static Command LoadBalloonData { get; private set; }
    }
}