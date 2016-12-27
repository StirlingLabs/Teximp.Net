/*
* Copyright (c) 2016-2017 TeximpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

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
