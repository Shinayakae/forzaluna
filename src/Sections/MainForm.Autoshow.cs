using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private void BuildMainPage()
        {
            _mainPage.AutoScroll = true;
            AddPageHeader(_mainPage, "Autoshow", "All Auto Show actions live here. Connect first, then run the action you need.");

            var actions = MakeCard(_mainPage, 0, 72, TwoColumnContentWidth, 174, "All Auto Show Actions", "Show every Autoshow car, or seed Add All Cars grant data before reopening FH6.");
            actions.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            var actionGrid = new Panel();
            actionGrid.Location = new Point(18, 58);
            actionGrid.Size = new Size(TwoColumnContentWidth - 36, 92);
            actionGrid.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            actionGrid.BackColor = Surface;
            actions.Controls.Add(actionGrid);

            _allCars = MakeLargeActionButton("Show All Cars: OFF");
            MakeLargeAutoshowButton(_allCars, AccentBlue, string.Empty);
            _allCars.Click += delegate { RunWorker("Show All Cars", ToggleAllCars); };
            actionGrid.Controls.Add(_allCars);

            _addAllCars = MakeLargeActionButton("Add All Cars: OFF");
            MakeLargeAutoshowButton(_addAllCars, AccentRed, "+");
            _addAllCars.Click += delegate
            {
                SetAddAllCarsState(1);
                RunWorker("Add All Cars", delegate
                {
                    try
                    {
                        AddAllCars();
                    }
                    catch
                    {
                        SetAddAllCarsState(0);
                        throw;
                    }
                });
            };
            actionGrid.Controls.Add(_addAllCars);
            actionGrid.Resize += delegate { LayoutAutoshowActions(actionGrid); };
            LayoutAutoshowActions(actionGrid);

            _restore = MakeActionButton("Restore Autoshow");
            _restore.Click += delegate { RestoreAutoshow(); };
            UpdateAllCarsButton();
            UpdateAddAllCarsButton();

            BuildCarTableSection(_mainPage, 270);
            ResizePageHeader(_mainPage, TwoColumnContentWidth, _mainPage.Tag as Label);
        }

        private void ShowCarPage()
        {
            ShowMainPage();
        }

        private void ShowMainPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowMainPage);
                return;
            }
            HidePages();
            _mainPage.Visible = true;
            _mainPage.BringToFront();
            SetStatus("Autoshow");
            UpdateNavigationState(_navAutoshow);
        }
    }
}
