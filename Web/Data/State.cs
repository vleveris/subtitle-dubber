using System;

namespace Web.Data
{

    public static class State
    {
        private static int _appState;
public static string InputFilePath { get; set; }
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

        // language purists are going to hate what I am
        // doing here ... but it works, and it has worked
        // for many, many, years :)
        private static void OnAppStateChanged(int intValue)
        {
            if (AppStateChanged != null)
            {
                AppStateChanged(intValue, EventArgs.Empty);
            }
        }
    }
}