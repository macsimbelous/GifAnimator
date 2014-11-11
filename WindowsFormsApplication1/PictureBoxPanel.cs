/*
* AntiDupl.NET Program.
*
* Copyright (c) 2002-2013 Yermalayeu Ihar.
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
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace WindowsFormsApplication1
{
    public class PictureBoxPanel : Panel
    {
        private const int MAX_PATH = 260;

        private Rectangle m_bitmapRect;
        private MemoryStream m_memoryStream;
        private Bitmap m_bitmap;
        private bool m_animationEnable = false;
        private bool m_currentlyAnimating = false;
        private int imgw;
        private int imgh;

        public PictureBoxPanel()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            Location = new System.Drawing.Point(0, 0);
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.Fixed3D;
            BackColor = Color.DarkGray;
            DoubleBuffered = true;

            DoubleClick += new EventHandler(OnImageDoubleClicked);
            SizeChanged += new EventHandler(OnSizeChanged);
        }

        public void UpdateImage(string file)
        {
            StopAnimate();
            if (LoadFileToMemoryStream(file))
            {
                m_bitmap = new Bitmap(m_memoryStream);
                imgw = m_bitmap.Width;
                imgh = m_bitmap.Height;
                m_animationEnable = ImageAnimator.CanAnimate(m_bitmap);
                if (m_animationEnable)
                    m_currentlyAnimating = false;
            }
            else
            {
                m_bitmap = null;
            }
        }

        private bool LoadFileToMemoryStream(string path)
        {
            if (m_memoryStream != null)
            {
                m_memoryStream.Close();
                m_memoryStream.Dispose();
                m_memoryStream = null;
            }
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                try
                {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    fileStream.Close();
                    m_memoryStream = new MemoryStream(buffer);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }


        private void AnimateImage()
        {
            if (!m_currentlyAnimating)
            {
                ImageAnimator.Animate(m_bitmap, new EventHandler(OnFrameChanged));
                m_currentlyAnimating = true;
            }
        }

        private void StopAnimate()
        {
            m_animationEnable = false;
            ImageAnimator.StopAnimate(m_bitmap, new EventHandler(OnFrameChanged));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (m_animationEnable)
            {
                try
                {
                    AnimateImage();
                    ImageAnimator.UpdateFrames();
                }
                catch
                {
                    m_animationEnable = false;
                }
            }
            if (m_bitmap != null)
            {
                e.Graphics.DrawImage(m_bitmap, m_bitmapRect);
            }
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
        
        private void OnImageDoubleClicked(object sender, System.EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "test.gif";
            try
            {
                Process.Start(startInfo);
            }
            catch (System.Exception exeption)
            {
                MessageBox.Show(exeption.Message);
            }
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            UpdateImagePadding();
        }

        private void OnImageViewChange()
        {
            UpdateImagePadding();
        }

        public void UpdateImagePadding()
        {
            m_bitmapRect = ScaleImage(m_bitmap, ClientSize.Width, ClientSize.Height);
            this.Parent.Text = m_bitmapRect.ToString() + ClientSize.ToString();
            Refresh();
        }
        private Rectangle ScaleImage(Bitmap source, int width, int height)
        {
            Rectangle dest;
            float srcwidth = source.Width;
            float srcheight = source.Height;
            float dstwidth = width;
            float dstheight = height;

            if (srcwidth <= dstwidth && srcheight <= dstheight)  // Исходное изображение меньше целевого
            {
                float w = dstwidth - srcwidth;
                float h = height - source.Height;
                float left;
                float top;
                if (w <= h)
                {
                    float pw = srcwidth / (dstwidth / 100);
                    h = (srcheight / pw) * 100;
                    w = dstwidth;
                    left = 0;
                    top = (dstheight - h) / 2;
                }
                else
                {
                    float ph = srcheight / (dstheight / 100);
                    w = (srcwidth / ph) * 100;
                    h = dstheight;
                    left = (dstwidth - w) / 2;
                    top = 0;
                }
                //w = ratio * source.Width;
                //h = ratio * source.Height;
                //int left = (width - source.Width) / 2;
                //int top = (height - source.Height) / 2;
                //dest = new Rectangle(left, top, source.Width, source.Height);
                dest = new Rectangle((int)left, (int)top, (int)w, (int)h);
            }
            else if (srcwidth / srcheight > dstwidth / dstheight)  // Пропорции исходного изображения более широкие
            {
                float cy = srcheight / srcwidth * dstwidth;
                float top = ((float)dstheight - cy) / 2.0f;
                if (top < 1.0f) top = 0;
                dest = new Rectangle(0, (int)top, (int)dstwidth, (int)cy);
            }
            else  // Пропорции исходного изображения более узкие
            {
                float cx = srcwidth / srcheight * dstheight;
                float left = ((float)dstwidth - cx) / 2.0f;
                if (left < 1.0f) left = 0;
                dest = new Rectangle((int)left, 0, (int)cx, (int)dstheight);
            }
            return dest;
        }
        private Rectangle ScaleImage2(Bitmap source, int width, int height)
        {
                int hp = 0, vp = 0;
                int w = ClientSize.Width;
                int h = ClientSize.Height;
                int cw = source.Width;
                int ch = source.Height;
                if (cw > 0 && ch > 0)
                {
                    
                        int nw = w;
                        int nh = h;
                        int mw = Math.Max(cw, nw);
                        int mh = Math.Max(ch, nh);
                        if (mw >= w || mh >= h)
                        {
                            if (mw * h > mh * w)
                            {
                                vp = (h - ch * w / mw) / 2;
                                hp = (w - cw * w / mw) / 2;
                            }
                            else
                            {
                                vp = (h - ch * h / mh) / 2;
                                hp = (w - cw * h / mh) / 2;
                            }
                        }
                        else
                        {
                            vp = (h - ch) / 2;
                            hp = (w - cw) / 2;
                        }
                    }
                    else
                    {
                        if (cw >= w || ch >= h)
                        {
                            if (cw * h > ch * w)
                            {
                                vp = (h - w * ch / cw) / 2;
                            }
                            else
                            {
                                hp = (w - h * cw / ch) / 2;
                            }
                        }
                        else
                        {
                            vp = (h - ch) / 2;
                            hp = (w - cw) / 2;
                        }
                    }
                
                return new Rectangle(hp, vp, w - 2 * hp, h - 2 * vp);
            
        }
    }
}
