using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gridifier
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        // Functions

        void UpdateImage()
        {
            if (m_sourceImage == null)
                return;

            var graphics = Graphics.FromImage(m_editImage);
            graphics.DrawImageUnscaled(m_sourceImage, Point.Empty);

            Point start = new Point(m_start.X, m_start.Y);
            Point end = new Point(m_start.X, m_end.Y);
            int width = m_end.X - m_start.X;
            int height = m_end.Y - m_start.Y;

            for (int i = 0; i <= m_gridXCount; ++i)
            {
                start.X = end.X = m_start.X + (i * width / m_gridXCount);
                graphics.DrawLine(Pens.Yellow, start, end);
            }

            start.X = m_start.X;
            end.X = m_end.X;

            for (int i = 0; i <= m_gridYCount; ++i)
            {
                start.Y = end.Y = m_start.Y + (i * height / m_gridYCount);
                graphics.DrawLine(Pens.Yellow, start, end);
            }

            imageBox.Image = m_editImage;

            Text = String.Format("Gridifier ({0}x{1}) - {2} pixels",
                m_gridXCount, m_gridYCount, (m_end.X - m_start.X) / m_gridXCount);
        }

        private void Save(String filename)
        {
            var width = m_end.X - m_start.X + 1;
            var height = m_end.Y - m_start.Y + 1;
            Rectangle dstRect = new Rectangle(0, 0, width, height);
            Rectangle srcRect = new Rectangle(m_start.X, m_start.Y, width, height);

            var newImage = new Bitmap(width, height);
            var graphics = Graphics.FromImage(newImage);
            graphics.DrawImage(m_sourceImage, dstRect, srcRect, GraphicsUnit.Pixel);
            graphics.Save();

            newImage.Save(filename);

            m_sourceImage = newImage;
            m_editImage = m_sourceImage.Clone() as Image;
            m_filename = filename;
            m_end.X -= m_start.X;
            m_end.Y -= m_start.Y;
            m_start = Point.Empty;
            UpdateImage();
        }

        // Data
        private String m_filename;
        private Image m_sourceImage;
        private Image m_editImage;
        private Point m_start;
        private Point m_end;
        private int m_gridXCount = 1;
        private int m_gridYCount = 1;

        // Events
        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void fileOpen_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Open Image";
            ofd.Multiselect = false;
            ofd.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*jpeg;*.png;*.bmp|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            m_filename = ofd.FileName;
            m_sourceImage = Image.FromFile(m_filename);
            m_editImage = m_sourceImage.Clone() as Image;
            m_start.X = 0;
            m_start.Y = 0;
            m_end.X = m_sourceImage.Width - 1;
            m_end.Y = m_sourceImage.Height - 1;
            m_gridXCount = m_gridYCount = 1;
            UpdateImage();
        }

        private void fileSave_Click(object sender, EventArgs e)
        {
            Save(m_filename);
        }

        private void fileSaveAs_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.FileName = m_filename;
            sfd.Title = "Save Image As";
            sfd.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*jpeg;*.png;*.bmp|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            Save(sfd.FileName);
        }

        private void imageBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (m_sourceImage == null || e.Location.X >= m_editImage.Size.Width || e.Location.Y >= m_editImage.Size.Height)
                return;
            if (e.Button == MouseButtons.Left)
            {
                if (e.Location.X >= m_end.X || e.Location.Y >= m_end.Y)
                    return;
                m_start = e.Location;
                UpdateImage();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (e.Location.X <= m_start.X || e.Location.Y <= m_start.Y)
                    return;
                m_end = e.Location;
                UpdateImage();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_sourceImage == null)
                return;
            if (e.KeyCode == Keys.Add)
            {
                if (e.Shift)
                    m_gridYCount += 1;
                if (!e.Shift)
                    m_gridXCount += 1;

                UpdateImage();
            }
            if (e.KeyCode == Keys.Subtract)
            {
                if (e.Shift && m_gridYCount > 1)
                    m_gridYCount -= 1;
                if (!e.Shift && m_gridXCount > 1)
                    m_gridXCount -= 1;
                UpdateImage();
            }
            if (e.KeyCode == Keys.Up)
            {
                if (e.Alt)
                {
                    int gridY = (m_end.Y - m_start.Y) / m_gridYCount;
                    if (e.Shift && m_gridYCount > 1)
                    {
                        m_end.Y -= gridY;
                        --m_gridYCount;
                    }
                    if (!e.Shift && m_start.Y >= gridY)
                    {
                        m_start.Y -= gridY;
                        ++m_gridYCount;
                    }
                }
                else
                {
                    if (e.Shift && m_end.Y > m_start.Y + 1)
                        m_end.Y -= 1;
                    if (!e.Shift && m_start.Y > 0)
                        m_start.Y -= 1;
                }
                UpdateImage();
            }
            if (e.KeyCode == Keys.Down)
            {
                if (e.Alt)
                {
                    int gridY = (m_end.Y - m_start.Y) / m_gridYCount;
                    if (e.Shift && m_end.Y < m_editImage.Height - gridY)
                    {
                        m_end.Y += gridY;
                        ++m_gridYCount;
                    }
                    if (!e.Shift && m_gridYCount > 1)
                    {
                        m_start.Y += gridY;
                        --m_gridYCount;
                    }
                }
                else
                {
                    if (e.Shift && m_end.Y < m_editImage.Height - 1)
                        m_end.Y += 1;
                    if (!e.Shift && m_start.Y < m_end.Y - 1)
                        m_start.Y += 1;
                }
                UpdateImage();
            }
            if (e.KeyCode == Keys.Left)
            {
                if (e.Alt)
                {
                    int gridX = (m_end.X - m_start.X) / m_gridXCount;
                    if (e.Shift && m_gridXCount > 1)
                    {
                        m_end.X -= gridX;
                        --m_gridXCount;
                    }
                    if (!e.Shift && m_start.X >= gridX)
                    {
                        m_start.X -= gridX;
                        ++m_gridXCount;
                    }
                }
                else
                {
                    if (e.Shift && m_end.X > m_start.X + 1)
                        m_end.X -= 1;
                    if (!e.Shift && m_start.X > 0)
                        m_start.X -= 1;
                }
                UpdateImage();
            }
            if (e.KeyCode == Keys.Right)
            {
                if (e.Alt)
                {
                    int gridX = (m_end.X - m_start.X) / m_gridXCount;
                    if (e.Shift && m_end.X < m_editImage.Width - gridX)
                    {
                        m_end.X += gridX;
                        ++m_gridXCount;
                    }
                    if (!e.Shift && m_gridXCount > 1)
                    {
                        m_start.X += gridX;
                        --m_gridXCount;
                    }
                }
                else
                {
                    if (e.Shift && m_end.X < m_editImage.Width - 1)
                        m_end.X += 1;
                    if (!e.Shift && m_start.X < m_end.X - 1)
                        m_start.X += 1;
                }
                UpdateImage();
            }
        }
    }
}
