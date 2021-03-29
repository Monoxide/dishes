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

namespace OottTray
{
    public partial class MainForm : Form
    {
        NotifyIcon ni = new NotifyIcon();
        ContextMenuStrip menu = new ContextMenuStrip();
        string exePath;
        string title;
        string dir;

        public MainForm()
        {
            exePath = Assembly.GetEntryAssembly().Location;
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

        void RebuildMenu()
        {
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            menu.Items.Clear();
            var sub = new ToolStripMenuItem("&OottTray");
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
