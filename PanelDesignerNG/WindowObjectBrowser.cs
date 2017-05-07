using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PanelDesignerNG
{
    public partial class WindowObjectBrowser : Form
    {
        private Form1 m_parent;
        private int m_WObject, m_OBitmap;

        public WindowObjectBrowser(Form1 frm1, int WObject, int OBitmap)
        {
            InitializeComponent();
            m_parent = frm1;
            m_WObject = WObject;
            m_OBitmap = OBitmap;
        }

        private void toolStripButton3_Click(object sender, EventArgs e) //kopiujemy bitmapkę tworząc nowy "stan"
        {
            BitmapObject selected_bo = m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap[listView1.Items.IndexOf(listView1.SelectedItems[0])]; //Okienko zawierające wybrany obiekt
            BitmapObject new_bo = BitmapObject.Copy(selected_bo);
            new_bo.name = selected_bo.name + " kopia";
            new_bo.state = selected_bo.state + 1;
            m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap.Add(new_bo);

            ImageList imageListSmall = new ImageList();
            listView1.Clear();
            foreach (BitmapObject bo in m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap)
            {
                ListViewItem item = listView1.Items.Add(bo.name);
                imageListSmall.Images.Add(bo.bitmap);
                item.ImageIndex = bo.state;
                item.Tag = bo;
                                
                //listView1.Items.Add(bo.name);
            }//foreach
            listView1.SmallImageList = imageListSmall;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap[listView1.Items.IndexOf(listView1.SelectedItems[0])]; //Okienko zawierające wybrany obiekt
            m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap.Remove(m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap[listView1.Items.IndexOf(listView1.SelectedItems[0])]);


            ImageList imageListSmall = new ImageList();
            listView1.Clear();
            foreach (BitmapObject bo in m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap)
            {
                ListViewItem item = listView1.Items.Add(bo.name);
                imageListSmall.Images.Add(bo.bitmap);
                item.ImageIndex = bo.state;
                item.Tag = bo;

                //listView1.Items.Add(bo.name);
            }//foreach
            listView1.SmallImageList = imageListSmall;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.Stream myStream = null;

            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        //myWindow.background = new Bitmap(myStream);     //bitmapka tego okienka z pliku
                        BitmapObject new_bo = new BitmapObject();
                        new_bo.bitmap = new Bitmap(myStream);
                        new_bo.name = "Stan " + m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap.Count.ToString();
                        new_bo.state = m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap.Count;
                        m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap.Add(new_bo);
                    }//using
                }//if
            }//try
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: Nie mogę przeczytać pliku ! Oryginał: " + ex.Message);

            }//catch
            finally
            {
                ImageList imageListSmall = new ImageList();
                listView1.Clear();
                listView1.SmallImageList = null;
                foreach (BitmapObject bo in m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap)
                {
                    ListViewItem item = listView1.Items.Add(bo.name);
                    imageListSmall.Images.Add(bo.bitmap);
                    item.ImageIndex = bo.state;
                    item.Tag = bo;

                    //listView1.Items.Add(bo.name);
                }//foreach
                listView1.SmallImageList = imageListSmall;
            }//finally
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = m_parent.PanelWindows[m_WObject].bitmaps[m_OBitmap].bitmap[listView1.Items.IndexOf(listView1.SelectedItems[0])].bitmap;
        }
    }
}
