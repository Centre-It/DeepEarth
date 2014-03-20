// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;

namespace DeepEarth.BingMapsToolkit.Client.Common.Commanding
{
    /// <summary>
    /// Provides data for the System.Windows.Input.CommandManager.Executed and System.Windows.Input.CommandManager.PreviewExecuted routed events.
    /// </summary>
    public class ExecutedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedEventArgs"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameter">The parameter.</param>
        internal ExecutedEventArgs(Command command, object parameter)
            : this(command, parameter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedEventArgs"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="source">The event source.</param>
        internal ExecutedEventArgs(Command command, object parameter, object source)
        {
            Command = command;
            Parameter = parameter;
            Source = source;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the command that was invoked.
        /// </summary>
        /// <value>The command associated with this event.</value>
        public Command Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets data parameter of the command.
        /// </summary>
        /// <value>The command data. The default value is null.</value>
        public object Parameter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the element that raises the event.
        /// </summary>
        /// <value>The event source.</value>
        public object Source
        {
            get;
            private set;
        }

        #endregion Properties
    }
}