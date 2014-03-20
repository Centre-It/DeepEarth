using DeepEarth.BingMapsToolkit.Client.Common.Commanding;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public static class PersistedCommands
    {
        static PersistedCommands()
        {
            PersistedStateLoadedCommand = new Command("PersistedStateLoaded");
            PersistedStateSaveCommand = new Command("PersistedStateSave");
            PersistedStateForceSaveCommand = new Command("PersistedStateForceSave");
        }

        /// <summary>
        /// Fired when the settings have been loaded from ISO
        /// </summary>
        public static Command PersistedStateLoadedCommand { get; private set; }

        /// <summary>
        /// Fired when setting need to be saved to ISO
        /// </summary>
        public static Command PersistedStateSaveCommand { get; private set; }

        /// <summary>
        /// Trigger control to do a save
        /// </summary>
        public static Command PersistedStateForceSaveCommand { get; private set; }
    }
}
