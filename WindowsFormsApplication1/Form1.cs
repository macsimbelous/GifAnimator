using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Bitmap animatedImage = new Bitmap("test.gif");
        bool currentlyAnimating = false;
        PictureBoxPanel panel;
        //This method begins the animation.
        /*public void AnimateImage()
        {
            if (!currentlyAnimating)
            {

                //Begin the animation only once.
                ImageAnimator.Animate(animatedImage, new EventHandler(this.OnFrameChanged));
                currentlyAnimating = true;
            }
        }

        private void OnFrameChanged(object o, EventArgs e)
        {

            //Force a call to the Paint event handler.
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            //Begin the animation.
            AnimateImage();

            //Get the next frame ready for rendering.
            ImageAnimator.UpdateFrames();

            //Draw the next frame in the animation.
            //e.Graphics.DrawImage(this.animatedImage, new Point(0, 0));
        }*/
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.panel = new PictureBoxPanel();
            this.panel.UpdateImage("test.gif");
            this.Dock = DockStyle.Fill;
            
            this.Controls.Add(this.panel);
            
        }
    }
}
