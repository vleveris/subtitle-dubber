using System;

namespace Web.Data
{

    public static class State
    {
        private static int _appState;
public static string InputVideoFileName { get; set; }
        public static string OutputVideoFileName { get; set; }
        public static string InputSubtitleFileName { get; set; }
        public static string OutputSubtitleFileName { get; set; }

        public static int AppState
        {
            get { return _appState; }
            set
            {
                // test removed so we get notification every time
                _appState = value;
                OnAppStateChanged(_appState);
            }
        }

        public static event EventHandler AppStateChanged;

        private static void OnAppStateChanged(int intValue)
        {
            if (AppStateChanged != null)
            {
                AppStateChanged(intValue, EventArgs.Empty);
            }
        }
    }
}