using System.Diagnostics;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private void OpenResultsFolder()
        {
            Process.Start("explorer.exe", _resultsDir);
        }
    }
}
