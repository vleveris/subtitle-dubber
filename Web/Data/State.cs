using System;

namespace Web.Data
{
    public static class State
    {
        private static StateType _appState;
public static string InputVideoFileName { get; set; }
        public static string OutputVideoFileName { get; set; }
        public static string InputSubtitleFileName { get; set; }
        public static string OutputSubtitleFileName { get; set; }

        public static StateType AppState
        {
            get { return _appState; }
            set
            {
                _appState = value;
                OnAppStateChanged(_appState);
            }
        }

        public static event EventHandler AppStateChanged;

        private static void OnAppStateChanged(StateType stateTypeValue)
        {
            if (AppStateChanged != null)
            {
                AppStateChanged(stateTypeValue, EventArgs.Empty);
            }
        }
    }
}