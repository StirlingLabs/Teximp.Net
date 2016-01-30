using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeximpNet.Sample
{
    public partial class MipViewerForm : Form
    {
        public MipViewerForm(List<Bitmap> mipChain)
        {
            InitializeComponent();

            SuspendLayout();

            Size = Screen.FromControl(this).WorkingArea.Size;
            CenterToScreen();

            BackColor = Color.FromArgb(255, 80, 80, 80);
            ScrollableControl panel = new ScrollableControl();
            panel.Dock = DockStyle.Fill;
            Controls.Add(panel);

            int offset = 0;
            foreach(Bitmap image in mipChain)
            {
                PictureBox box = new PictureBox();
                box.Image = image;
                box.Width = image.Width;
                box.Height = image.Height;

                box.Left = offset;
                offset += image.Width;

                panel.Controls.Add(box);
            }

            panel.AutoScroll = true;

            panel.ResumeLayout();

            ResumeLayout();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
