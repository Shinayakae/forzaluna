using System.Diagnostics;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private void OpenResultsFolder()
        {
            try
            {
                Process.Start("explorer.exe", _resultsDir);
            }
            catch (System.Exception ex)
            {
                Log("Could not open results folder: " + ex.Message);
            }
        }
    }
}
