using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;
using DevExpress.Data.Linq;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.Utils.About;
using DevExpress.Xpo.DB;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors.Filtering;
using DevExpress.XtraTab;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using GuitarPu.Common;

namespace GuitarPu
{
    public partial class frmMain : DevExpress.XtraEditors.XtraForm
    {
        public frmMain()
        {
            InitializeComponent();
            InitSkinGallery();
            UserLookAndFeel.Default.StyleChanged += OnLookAndFeelStyleChanged;

            DataTable dt = initTreeTable();
            try
            {
                dt.ReadXml(Application.StartupPath + "\\Data\\Data.xml");
                treeList1.DataSource = dt;
                treeList1.ExpandAll();
            }
            catch
            {
                RefreshTree();
            }
        }
        void OnLookAndFeelStyleChanged(object sender, EventArgs e)
        {
            UpdateSchemeCombo();
            ConfigAppSettings.SetValue("theme",UserLookAndFeel.Default.ActiveSkinName);
        }
        #region SkinGallery
        void InitSkinGallery()
        {
            DevExpress.XtraBars.Helpers.SkinHelper.InitSkinGallery(ribbonGalleryBarItem1, true);
        }
        #endregion
        void UpdateSchemeCombo()
        {
            if (ribbonControl1.RibbonStyle == RibbonControlStyle.MacOffice ||
                ribbonControl1.RibbonStyle == RibbonControlStyle.Office2010 || ribbonControl1.RibbonStyle == RibbonControlStyle.Office2013)
            {
                //beScheme.Visibility = UserLookAndFeel.Default.ActiveSkinName.Contains("Office 2010") ? BarItemVisibility.Always : BarItemVisibility.Never;
            }
            else
            {
                //beScheme.Visibility = BarItemVisibility.Never;
            }
        }

        private int index = 0;
        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.openFileDialog1.Filter = @"图片文件|*.jpg;*.png;*.gif;*.bmp;*.jpeg";
            int i = 0;
            if (this.openFileDialog1.ShowDialog() != DialogResult.OK) return;

            this.barStaticItem1.Caption = string.Format("正在添加...");
            for (i = 0; i < openFileDialog1.FileNames.Count(); i++)
            {
                string fn = this.openFileDialog1.FileNames[i];
                string sfn = this.openFileDialog1.SafeFileNames[i];

                File.Copy(fn, Setting.ImgDir + sfn);
            }
            this.barStaticItem1.Caption = string.Format("添加了{0}个曲谱", i + 1);
            RefreshTree();
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;

            this.barStaticItem1.Caption = string.Format("正在添加...");
            var dir = this.folderBrowserDialog1.SelectedPath;
            var files = Directory.GetFiles(dir);
            int i = 0;
            foreach (var file in files)
            {
                var fn = Path.GetFileName(file);
                File.Copy(file, Setting.ImgDir + fn);
                i++;
            }
            this.barStaticItem1.Caption = string.Format("添加了{0}个曲谱", i);
            RefreshTree();
        }

        private void RefreshTree()
        {
            //List<TreeModel> tree = new List<TreeModel>();
            index = 0;

            //GetFiles(Setting.ImgDir, tree, 0);
            //treeList1.DataSource = tree;

            DataTable dt = initTreeTable();
            GetFiles(Setting.ImgDir, dt, 0);
            treeList1.DataSource = dt;
            treeList1.ExpandAll();
            dt.WriteXml(Application.StartupPath + "\\Data\\Data.xml");
        }
        public DataTable initTreeTable()
        {
            var dt = new DataTable("GuitarPu");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("ParentID", typeof(int));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("Icon", typeof(int));
            return dt;
        }
        private void GetFiles(string dir, DataTable dt, int pid)
        {
            var tempdir = dir.TrimEnd(Convert.ToChar("\\"));
            var dirName = tempdir.Substring(tempdir.LastIndexOf("\\") + 1);

            DataRow dr = dt.NewRow();
            dr["ID"] = index;
            dr["Title"] = dirName;
            dr["ParentID"] = pid;
            dr["Icon"] = 0;
            dt.Rows.Add(dr);

            //var files = Directory.GetFiles(dir,"*.jpg,*.gif,*.png,*.jpeg,*.bmp");
            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                string fn = Path.GetFileName(file);
                string ext = Path.GetExtension(fn);
                if (!"*.jpg,*.gif,*.png,*.jpeg,*.bmp".Contains(ext)) continue;
                
                index++;
                DataRow dr2 = dt.NewRow();
                dr2["ID"] = index;
                dr2["Title"] = fn;
                dr2["ParentID"] = dr["ID"];
                dr2["FileName"] = file;
                dr2["Icon"] = 1;
                dt.Rows.Add(dr2);
            }
            var dirs = Directory.GetDirectories(dir);
            foreach (var subdir in dirs)
            {
                index++;
                GetFiles(subdir, dt, Convert.ToInt32(dr["ID"]));
            }
        }
        private void GetFiles(string dir, List<TreeModel> models, int pid)
        {
            var tempdir = dir.TrimEnd(Convert.ToChar("\\"));
            var dirName = tempdir.Substring(tempdir.LastIndexOf("\\") + 1);

            TreeModel model = new TreeModel();
            model.ID = index;
            model.Title = dirName;
            model.ParentID = pid;
            models.Add(model);
            var files = Directory.GetFiles(dir, "*.jpg;*.gif;*.png;*.jpeg;*.bmp");
            foreach (var file in files)
            {
                index++;
                var fn = Path.GetFileName(file);
                TreeModel fmodel = new TreeModel();
                fmodel.ID = index;
                fmodel.Title = fn;
                fmodel.ParentID = model.ID;
                fmodel.FileName = file;
                models.Add(fmodel);
            }
            var dirs = Directory.GetDirectories(dir);
            foreach (var subdir in dirs)
            {
                index++;
                GetFiles(subdir, models, model.ID);
            }
        }

        private void treeList1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 2) return;
            TreeListHitInfo hi = treeList1.CalcHitInfo(new Point(e.X, e.Y));
            TreeListNode node = hi.Node;
            if (node != null && !node.HasChildren)
            {
                ShowTab(node);
            }
        }

        private void ShowTab(TreeListNode node)
        {
            string title = node.GetValue("Title").ToString();
            string file = node.GetValue("FileName").ToString();
            if (string.IsNullOrEmpty(file)) return;

            XtraTabPage t = new XtraTabPage();
            bool old = false;
            foreach (XtraTabPage tab in xtraTabControl1.TabPages)
            {
                if (tab.Text == title)
                {
                    old = true;
                    t = tab;
                }
            }
            if (old)
            {
                t.Show();
            }
            else
            {
                var tab = new XtraTabPage();

                tab.Text = title;
                tab.ShowCloseButton = DefaultBoolean.True;

                var picBox = new PictureBox();
                picBox.Left = 0;
                picBox.Top = 0;
                tab.Controls.Add(picBox);
                picBox.Click += picBox_Click;
                picBox.DoubleClick += picBox_DoubleClick;
                picBox.MouseWheel += picBox_MouseWheel;
                picBox.BackColor = System.Drawing.Color.Gray;
                picBox.SizeMode = PictureBoxSizeMode.Zoom;

                var rPicBox = new PictureBox();
                rPicBox.Image = GetImage(node);
                rPicBox.Visible = false;
                tab.Controls.Add(rPicBox);

                Bitmap oldImg = (Bitmap)rPicBox.Image;

                ImgInfo info = new ImgInfo { file = file, width = oldImg.Width, height = oldImg.Height };
                tab.Tag = info;

                xtraTabControl1.TabPages.Add(tab);
                tab.Show();
                tab.AutoScroll = true;
                //tab.FireScrollEventOnMouseWheel=false;
                //tab.HorizontalScroll.Visible = true;
                tab.MouseWheel += tab_MouseWheel;
                ShowImage(tab, oldImg);
            }
        }

        private void tab_MouseWheel(object sender, MouseEventArgs e)
        {
            XtraTabPage tab = xtraTabControl1.SelectedTabPage;
            if (barCheckItem1.Checked)
            {
                tab.AutoScroll = false;
            }
            else
            {
                if (tab.AutoScroll == false)
                {
                    tab.AutoScroll = true;
                    ShowImage(tab);
                }
            }
        }


        void picBox_DoubleClick(object sender, EventArgs e)
        {
            splitContainerControl1.Collapsed = !splitContainerControl1.Collapsed;
            ribbonControl1.Minimized = splitContainerControl1.Collapsed;

        }

        Image GetImage(TreeListNode node)
        {
            Image oldImg = new Bitmap(1, 1);
            Graphics g;
            string title = node.GetValue("Title").ToString();
            string file = node.GetValue("FileName").ToString();
            string fileName = file.Substring(file.LastIndexOf('\\'));
            fileName = fileName.Remove(fileName.LastIndexOf('.') - 1);

            if (fileName.Length < 3)
            {
                oldImg = Image.FromFile(file);
                return oldImg;
            }
            while (node.PrevNode != null && !node.PrevNode.HasChildren && node.PrevNode.GetValue("FileName").ToString().Contains(fileName))
            {
                node = node.PrevNode;
            }
            do
            {
                file = node.GetValue("FileName").ToString();
                Image img = Image.FromFile(file);
                Image oimg = oldImg;
                oldImg = new Bitmap(img.Width, img.Height + oldImg.Height);
                g = Graphics.FromImage(oldImg);

                g.DrawImage(oimg, 0, 0);
                g.DrawImage(img, 0, oimg.Height);

                node = node.NextNode;
            } while (node != null && !node.HasChildren && node.GetValue("FileName").ToString().Contains(fileName));
            return oldImg;
        }

        Bitmap newImg;
        void ShowImage(XtraTabPage tab, Bitmap oldImg)
        {

            Graphics graphic;
            newImg = new Bitmap(oldImg.Width, tab.ClientSize.Height);
            graphic = Graphics.FromImage(newImg);

            var picBox = tab.Controls[0] as PictureBox;

            ImageToGraphic(graphic, newImg, oldImg);
            picBox.Image = newImg;
            //picBox.SizeMode = PictureBoxSizeMode.CenterImage;
            //picBox.Dock = DockStyle.Fill;
            picBox.Size = picBox.Image.Size;
            //picBox.SizeChanged += picBox_SizeChanged;
            //tab.Show();
            GC.Collect();
        }

        void ShowImage(XtraTabPage tab)
        {
            tab = xtraTabControl1.SelectedTabPage;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;
            Rectangle newRect;
            newRect = new Rectangle(0, 0, info.width, info.height);

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }
        void ImageToGraphic(Graphics graphics, Bitmap img, Bitmap oimg)
        {
            //graphics.DrawImage(oimg, 0, 0, RectangleF.FromLTRB(0, 0, oimg.Width, oimg.Height), GraphicsUnit.Display);
            //graphics.DrawImage(oimg, new RectangleF(img.Width - oimg.Width, 0, oimg.Width, img.Height));
            //graphics.DrawImage(oimg, new Rectangle(img.Width - oimg.Width, 0, oimg.Width, img.Height), 0, 0, oimg.Width, img.Height, GraphicsUnit.Pixel);
            graphics.DrawImage(oimg, img.Width - oimg.Width, 0, new Rectangle(0, 0, oimg.Width, oimg.Height), GraphicsUnit.Pixel);
            if (oimg.Height > img.Height)
            {
                newImg = null;
                newImg = new Bitmap(img.Width + oimg.Width, img.Height);
                graphics = Graphics.FromImage(newImg);
                graphics.DrawImage(img, 0, 0);

                //将原图片裁去已经绘制的部分形成新图片
                Rectangle newRect = new Rectangle(0, img.Height, oimg.Width, oimg.Height - img.Height);
                oimg = oimg.Clone(newRect, oimg.PixelFormat);
                ImageToGraphic(graphics, newImg, oimg);
            }
        }
        //void picBox_SizeChanged(object sender, EventArgs e)
        //{
        //    var graph = new Graphics();
        //    var picBox = sender as PictureBox;
        //    var img = picBox.Image;
        //    picBox.Left = 0;
        //    picBox.Top = 0;
        //    XtraTabPage tab = picBox.Parent as XtraTabPage;


        //}

        void picBox_Click(object sender, EventArgs e)
        {
            var picBox = sender as PictureBox;
            picBox.Focus();
            ;
        }
        private void picBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!barCheckItem1.Checked) return;
            var tab = xtraTabControl1.SelectedTabPage;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;
            Rectangle newRect = new Rectangle();
            if (e.Delta > 2)
            {
                newRect = new Rectangle(0, 0, Convert.ToInt32(info.width * 1.01), Convert.ToInt32(info.height * 1.01));
            }
            else if (e.Delta < -2)
            {
                newRect = new Rectangle(0, 0, Convert.ToInt32(info.width * 0.99), Convert.ToInt32(info.height * 0.99));
            }
            info.width = newRect.Width;
            info.height = newRect.Height; tab.Tag = info;
            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);

            ShowImage(tab, oldImg);
        }

        private void xtraTabControl1_CloseButtonClick(object sender, EventArgs e)
        {
            xtraTabControl1.TabPages.Remove(xtraTabControl1.SelectedTabPage);
        }
        private struct ImgInfo
        {

            public string file { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int leftPad { get; set; }

            //int _leftPad;
            //public int leftPad { get { return _leftPad; }  set { _leftPad = leftPad; } }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (xtraTabControl1.SelectedTabPage != null)
                ShowImage(xtraTabControl1.SelectedTabPage);
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width - 10), Convert.ToInt32(info.height));
            info.width = newRect.Width;
            info.height = newRect.Height;
            info.leftPad += 5;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width + 10), Convert.ToInt32(info.height));
            info.width = newRect.Width;
            info.height = newRect.Height;
            info.leftPad -= 5;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width - 10), Convert.ToInt32(info.height));
            info.width = newRect.Width;
            info.height = newRect.Height;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);

        }

        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width), Convert.ToInt32(info.height - 10));
            info.width = newRect.Width;
            info.height = newRect.Height;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);

        }

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width + 10), Convert.ToInt32(info.height));
            info.width = newRect.Width;
            info.height = newRect.Height;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }

        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width), Convert.ToInt32(info.height + 10));
            info.width = newRect.Width;
            info.height = newRect.Height;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }

        private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;
            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width * 1.01), Convert.ToInt32(info.height * 1.01));
            info.width = newRect.Width;
            info.height = newRect.Height;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var tab = xtraTabControl1.SelectedTabPage;
            if (tab == null) return;

            var info = (ImgInfo)tab.Tag;
            var picBox = tab.Controls[1] as PictureBox;

            //Bitmap oldImg=new Bitmap(info.file);
            var img = picBox.Image;

            Rectangle newRect;
            newRect = new Rectangle(0, 0, Convert.ToInt32(info.width * 0.99), Convert.ToInt32(info.height * 0.99));
            info.width = newRect.Width;
            info.height = newRect.Height;
            tab.Tag = info;

            Bitmap oldImg = new Bitmap(newRect.Width, newRect.Height);
            var g = Graphics.FromImage(oldImg);
            g.DrawImage(img, newRect, info.leftPad, 0, img.Width - 2 * info.leftPad, img.Height, GraphicsUnit.Pixel);
            ShowImage(tab, oldImg);
        }

        private void xtraTabControl1_SizeChanged(object sender, EventArgs e)
        {
            if (xtraTabControl1.SelectedTabPage != null)
                ShowImage(xtraTabControl1.SelectedTabPage);
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            RefreshTree();
        }

        private void barButtonItem12_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + Common.Setting.ImgDir;
            System.Diagnostics.Process.Start(psi);
        }
    }
}
