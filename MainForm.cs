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
            ni.ContextMenuStrip = menu;
            ni.Visible = true;
            RebuildMenu();
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

        void RebuildMenu()
        {
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            menu.Items.Clear();
            var sub = new ToolStripMenuItem("&Dishes");
            sub.RightToLeft = RightToLeft.Yes;
            menu.Items.Add(sub);
            sub.DropDown.RightToLeft = RightToLeft.No;
            ((ToolStripDropDownMenu)sub.DropDown).ShowImageMargin = false;
            sub.DropDownItems.Add("&Refresh", null, (s, e) => RebuildMenu());
            sub.DropDownItems.Add("&Explorer...", null, (s, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = dir,
                UseShellExecute = true
            }));
            sub.DropDownItems.Add("-");
            sub.DropDownItems.Add("&About...", null, (s, e) => ShowAboutBox());
            sub.DropDownItems.Add("-");
            sub.DropDownItems.Add("&Exit", null, (s, e) => Application.Exit());
            menu.Items.Add("-");
            var regex = new Regex(@"^\d+\.");
            foreach (var i in Directory.EnumerateFiles(dir).OrderBy((i) => i))
            {
                var attr = File.GetAttributes(i);
                if (attr.HasFlag(FileAttributes.Hidden) || attr.HasFlag(FileAttributes.System)) continue;

                var name = Path.GetFileNameWithoutExtension(i);
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
        }
    }
}
