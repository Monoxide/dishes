using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using KPreisser.UI;

namespace Monoxide.Dishes
{
    public partial class MainForm : Form
    {
        NotifyIcon ni = new NotifyIcon();
        ContextMenuStrip menu = new ContextMenuStrip();
        List<ToolStripDropDownItem> dropDownItems = new List<ToolStripDropDownItem>();
        string exePath;
        string title;
        string dir;
        string version;

        public MainForm()
        {
            var assembly = Assembly.GetEntryAssembly();
            version = assembly.GetName().Version.ToString();
            exePath = assembly.Location;
            title = Path.GetFileNameWithoutExtension(exePath);
            dir = Path.Combine(Path.GetDirectoryName(exePath), title);

            InitializeComponent();
            ni.Icon = this.Icon;
            try {
                ni.Icon = Icon.ExtractAssociatedIcon(exePath);
            } catch { }
            ni.Text = title;
            ni.Visible = true;
            RebuildMenu();

            menu.Renderer = new DirectionAwareToolStripProfessionalRenderer();
            ni.MouseMove += Ni_MouseMove;
            SetupMenuEvent();
        }

        private void Ni_MouseMove(object sender, MouseEventArgs e)
        {
            CheckTaskbarPositionThrottle();
        }

        void SetupMenuEvent()
        {
            var windowField = ni.GetType().GetField("window", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ni);
            if (windowField != null && windowField is NativeWindow notifyIconWindow)
            {
                var workingAreaConstrainedProperty = typeof(ToolStripDropDown).GetProperty("WorkingAreaConstrained", BindingFlags.Instance | BindingFlags.NonPublic);
                var modalMenuFilterType = typeof(ToolStripManager).Assembly.GetType("System.Windows.Forms.ToolStripManager+ModalMenuFilter", false);
                var modalMenuFilterInstance = modalMenuFilterType?.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                var showUnderlinesProperty = modalMenuFilterType?.GetProperty("ShowUnderlines", BindingFlags.Public | BindingFlags.Instance);

                ni.MouseUp += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        CheckTaskbarPosition();
                        GetCursorPos(out var point);
                        SetForegroundWindow(notifyIconWindow.Handle);

                        if (menu is ToolStripDropDown)
                        {
                            workingAreaConstrainedProperty?.SetValue(menu, false);
                        }

                        if (showUnderlinesProperty != null) {
                            showUnderlinesProperty.SetValue(modalMenuFilterInstance, true);
                        }

                        var direction = ToolStripDropDownDirection.AboveLeft;
                        switch(taskbarPosition)
                        {
                            case TaskbarPosition.Bottom: direction = ToolStripDropDownDirection.AboveLeft; break;
                            case TaskbarPosition.Left: direction = ToolStripDropDownDirection.AboveRight; break;
                            case TaskbarPosition.Top: direction = ToolStripDropDownDirection.BelowLeft; break;
                            case TaskbarPosition.Right: direction = ToolStripDropDownDirection.AboveLeft; break;
                        }
                        menu.Show(new Point(point.X, point.Y), direction);
                    }
                };
            }
            else
            {
                ni.ContextMenuStrip = menu;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        class DirectionAwareToolStripProfessionalRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                if (e.Item is ToolStripDropDownItem ddi)
                {
                    if (e.Direction == ArrowDirection.Right && ddi.DropDownDirection == ToolStripDropDownDirection.Left)
                    {
                        e.Direction = ArrowDirection.Left;
                    }
                }
                base.OnRenderArrow(e);
            }
        }

        TaskDialog aboutDialog;

        void ShowAboutBox()
        {
            if (this.aboutDialog != null) return;
            TaskDialogPage page = new TaskDialogPage()
            {
                Title = "About",
                Instruction = "Dishes " + this.version,
                Text = "By <A HREF=\"https://redirect.orztech.com/dishes/author\">Monoxide Apps</A>\n" +
                    "\n" +
                    "<A HREF=\"https://redirect.orztech.com/dishes\">Web page</A>\n" +
                    "<A HREF=\"https://redirect.orztech.com/dishes/bugtracker\">Bug tracker</A>\n" +
                    "<A HREF=\"https://redirect.orztech.com/dishes/donate\">Donate</A>\n" +
                    "",
                Expander = new TaskDialogExpander()
                {
                     ExpandedButtonText = "Hide &License",
                     CollapsedButtonText = "Show &License",
                     ExpandFooterArea = true,
                     Text = "Copyright (C) 2021 Monoxide Apps\n" +
                        "\n" +
                        "This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.\n\n" +
                        "This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.\n\n" +
                        "You should have received a copy of the GNU General Public License along with this program.  If not, see <A HREF=\"https://www.gnu.org/licenses/\">https://www.gnu.org/licenses/</A>.\n" +
                        "\n" +
                        "<A HREF=\"https://redirect.orztech.com/dishes/thirdpartynotices\">Third Party Notices</A>"
                },
                SizeToContent = true,
                EnableHyperlinks = true
            };
            page.HyperlinkClicked += (s, e) =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = e.Hyperlink + "?av=" + System.Uri.EscapeDataString(version),
                    UseShellExecute = true
                });
            };
            try
            {
                this.aboutDialog = new TaskDialog(page);
                this.aboutDialog.Show();
            } finally
            {
                this.aboutDialog = null;
            }
        }

        TaskbarPosition taskbarPosition = TaskbarPosition.Unknown;
        void CheckTaskbarPosition()
        {
            var position = Taskbar.Position;
            if (taskbarPosition != position)
            {
                taskbarPosition = position;
                RefreshDirection();
            }
        }

        Throttler positionThrottler = new Throttler();
        void CheckTaskbarPositionThrottle()
        {
            if (positionThrottler.Throttle(TimeSpan.FromSeconds(5)))
            {
                CheckTaskbarPosition();
            }
        }

        void RefreshDirection()
        {
            var direction = taskbarPosition == TaskbarPosition.Left ? ToolStripDropDownDirection.Default : ToolStripDropDownDirection.Left;
            foreach(var ddi in dropDownItems)
            {
                ddi.DropDownDirection = direction;
            }
        }

        void RebuildMenu()
        {
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            menu.Items.Clear();
            dropDownItems.Clear();
            var sub = new ToolStripMenuItem(title);
            menu.Items.Add(sub);
            dropDownItems.Add(sub);
            ((ToolStripDropDownMenu)sub.DropDown).ShowImageMargin = false;

            sub.DropDownItems.Add("&Refresh", null, (s, e) => RebuildMenu());
            sub.DropDownItems.Add("&Explorer...", null, (s, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = dir,
                UseShellExecute = true
            }));
            sub.DropDownItems.Add("-");
            sub.DropDownItems.Add("&About" + (title == "Dishes" ? "" : " Dishes") + "...", null, (s, e) => ShowAboutBox());
            sub.DropDownItems.Add("-");
            sub.DropDownItems.Add("&Exit", null, (s, e) => Application.Exit());
            menu.Items.Add("-");
            var regex = new Regex(@"^\d+\.");
            foreach (var i in Directory.EnumerateFiles(dir).OrderBy((i) => i))
            {
                var attr = File.GetAttributes(i);
                if (attr.HasFlag(FileAttributes.Hidden) || attr.HasFlag(FileAttributes.System)) continue;

                var name = Path.GetFileNameWithoutExtension(i);
                if (name.StartsWith(".")) continue;

                var match = regex.Match(name);
                if (match != null) {
                    name = name.Substring(match.Value.Length);
                }
                if (name == "-")
                {
                    menu.Items.Add("-");
                    continue;
                }
                Image icon = null;
                try
                {
                    icon = Icon.ExtractAssociatedIcon(i).ToBitmap();
                } catch { }
                menu.Items.Add(name, icon, (s, e) => {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = i,
                        UseShellExecute = true,
                    });
                });
            }

            RefreshDirection();
        }
    }
}
