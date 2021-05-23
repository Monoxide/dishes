using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monoxide.Dishes
{
    internal static class DirectionAwareToolStripRenderer {
        public static void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item is ToolStripDropDownItem ddi)
            {
                if (e.Direction == ArrowDirection.Right && ddi.DropDownDirection == ToolStripDropDownDirection.Left)
                {
                    e.Direction = ArrowDirection.Left;
                }
            }
        }

        public class ToolStripProfessionalRenderer : System.Windows.Forms.ToolStripProfessionalRenderer
        {
            public ToolStripProfessionalRenderer() : base() { }
            public ToolStripProfessionalRenderer(ProfessionalColorTable professionalColorTable) : base(professionalColorTable) { }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                if (this.ColorTable is ImmersiveColorTable ict)
                {
                    e.ArrowColor = ict.ItemTextColor;
                }
                DirectionAwareToolStripRenderer.OnRenderArrow(e);
                base.OnRenderArrow(e);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (this.ColorTable is ImmersiveColorTable ict)
                {
                    e.TextColor = ict.ItemTextColor;
                }
                base.OnRenderItemText(e);
            }
        }

        public class NativeToolStripRenderer : UnFound.NativeToolStripRenderer
        {
            public NativeToolStripRenderer(UnFound.ToolbarTheme theme) : base(theme) { }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                DirectionAwareToolStripRenderer.OnRenderArrow(e);
                base.OnRenderArrow(e);
            }
        }

        public class ToolStripSystemRenderer : System.Windows.Forms.ToolStripSystemRenderer
        {
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                DirectionAwareToolStripRenderer.OnRenderArrow(e);
                base.OnRenderArrow(e);
            }
        }
    }

    public class ImmersiveColorTable: ProfessionalColorTable
    {
        public virtual Color ItemTextColor => SystemColors.ControlText;
    }

    public class ImmersiveLightColorTable: ImmersiveColorTable
    {
        public override Color ItemTextColor => Color.FromArgb(0, 0, 0);
        public override Color MenuBorder => Color.FromArgb(160, 160, 160);
        public override Color SeparatorDark => Color.FromArgb(145, 145, 145);
        public override Color SeparatorLight => Color.Transparent;
        public override Color MenuItemSelected => Color.FromArgb(255, 255, 255);
        public override Color MenuItemBorder => MenuItemSelected;
        public override Color ToolStripDropDownBackground => Color.FromArgb(238, 238, 238);
        public override Color ImageMarginGradientBegin => ToolStripDropDownBackground;
        public override Color ImageMarginGradientMiddle => ToolStripDropDownBackground;
        public override Color ImageMarginGradientEnd => ToolStripDropDownBackground;
    }

    public class ImmersiveDarkColorTable: ImmersiveColorTable
    {
        public override Color ItemTextColor => Color.FromArgb(255, 255, 255);
        public override Color MenuBorder => Color.FromArgb(160, 160, 160);
        public override Color SeparatorDark => Color.FromArgb(128, 128, 128);
        public override Color SeparatorLight => Color.Transparent;
        public override Color MenuItemSelected => Color.FromArgb(65, 65, 65);
        public override Color MenuItemBorder => MenuItemSelected;
        public override Color ToolStripDropDownBackground => Color.FromArgb(43, 43, 43);
        public override Color ImageMarginGradientBegin => ToolStripDropDownBackground;
        public override Color ImageMarginGradientMiddle => ToolStripDropDownBackground;
        public override Color ImageMarginGradientEnd => ToolStripDropDownBackground;
    }
}
