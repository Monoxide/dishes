using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OottTray
{
    public partial class MainForm : Form
    {
        NotifyIcon ni = new NotifyIcon();
        ContextMenuStrip menu = new ContextMenuStrip();

        public MainForm()
        {
            InitializeComponent();
            ni.Icon = this.Icon;
            try {
                ni.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            } catch { }
            ni.Text = this.Text;
            ni.ContextMenuStrip = menu;
            ni.Visible = true;
            RebuildMenu();
        }

        void RebuildMenu()
        {
            var exe = Assembly.GetEntryAssembly().Location;
            var dir = Path.Combine(Path.GetDirectoryName(exe), Path.GetFileNameWithoutExtension(exe));
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            menu.Items.Clear();
            var sub = new ToolStripMenuItem("&OottTray");
            menu.Items.Add(sub);
            sub.DropDownItems.Add("&Refresh", null, (s, e) => RebuildMenu());
            sub.DropDownItems.Add("&Explorer...", null, (s, e) => Process.Start("explorer.exe", dir));
            sub.DropDownItems.Add("-");
            sub.DropDownItems.Add("&Exit", null, (s, e) => Application.Exit());
            menu.Items.Add("-");
            foreach (var i in Directory.EnumerateFiles(dir).OrderBy((i) => i))
            {
                Image icon = null;
                try
                {
                    icon = Icon.ExtractAssociatedIcon(i).ToBitmap();
                } catch { }
                menu.Items.Add(Path.GetFileNameWithoutExtension(i), icon, (s, e) => {
                    Process.Start(i);
                });
            }
        }
    }
}
