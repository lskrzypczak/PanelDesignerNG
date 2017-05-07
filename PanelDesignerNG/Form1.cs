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
    public partial class Form1 : Form
    {
        public List<Window> PanelWindows = new List<Window>();  //ta klasa trzyma okienka panelu. Publiczna bo potrtzebny jest do niej dostep z innej formy
        private Boolean bHaveMouse;
        private Point ptOriginal = new Point();
        private Point ptLast = new Point();     //powyższe 3 zmiene sa do zaznaczania kawałk aobrazka
        private Boolean bEnableCut;
        private Point IconLastPoint = new Point();
        private WindowObject IconSelectedObject;
        private Boolean IconUnderCursor;
        private Point IconLastLocation = new Point();
        private TreeNode LastSelectedTreeNode;
        private Window ActiveWindow = new Window();

        private void RedrawCurrentPanel()
        {
            pBPanel.Refresh();
            WindowObject[] two = ActiveWindow.bitmaps.ToArray();
            foreach (WindowObject ttw in two)
            {
                Bitmap moved_bmp = new Bitmap(ttw.bitmap[0].bitmap);
                Point moved_p = new Point(ttw.x, ttw.y);
                Graphics moved_g = pBPanel.CreateGraphics();
                moved_g.DrawImage(moved_bmp, moved_p);
            }//foreach
        }//RedrawCurrentPanel
                        
        public Form1()
        {
            InitializeComponent();
            bHaveMouse = false;
            bEnableCut = false;
            IconUnderCursor = false;
            
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer, true);
            
            
        }

        private void wyjścieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Convert and normalize the points and draw the reversible frame.
        private void MyDrawReversibleRectangle(Point p1, Point p2)
        {
            Rectangle rc = new Rectangle();

            // Convert the points to screen coordinates.
            p1 = PointToScreen(p1);
            p2 = PointToScreen(p2);
            // Normalize the rectangle.
            if (p1.X < p2.X)
            {
                rc.X = p1.X;
                rc.Width = p2.X - p1.X;
            }
            else
            {
                rc.X = p2.X;
                rc.Width = p1.X - p2.X;
            }
            if (p1.Y < p2.Y)
            {
                rc.Y = p1.Y;
                rc.Height = p2.Y - p1.Y;
            }
            else
            {
                rc.Y = p2.Y;
                rc.Height = p1.Y - p2.Y;
            }
            // Draw the reversible frame.
            ControlPaint.DrawReversibleFrame(rc, Color.Red, FrameStyle.Dashed);
        }

        private void tłoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oFDBackground.ShowDialog();     //Pokaż dialog otwierania tła do okienka
        }

        private void oFDBackground_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.Stream myStream = null;

            try
            {
                if ((myStream = oFDBackground.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        Window myWindow = new Window();     //nowe okienko (klasa Window)
                        myWindow.background = new Bitmap(myStream);     //bitmapka tego okienka z pliku
                        myWindow.name = "Okno " + PanelWindows.Count;   //Nazwa z kolejnym numerem
                        myWindow.index = PanelWindows.Count;
                        PanelWindows.Add(myWindow);     //dodaj do listy

                        treeView1.Nodes.Clear();        //czyścimy widok drzewka żeby dodać elementy listy w kolejności
                        foreach (Window i in PanelWindows)  //...każde okienko
                        {
                            WindowObject[] two = i.bitmaps.ToArray();   //ponieważ aby do treeview dodac listy hierarchiczne musimy je utabelić
                            List<TreeNode> tn = new List<TreeNode>();
                            foreach (WindowObject ttw in two)           //dodajemy każdy obiekt w okienku
                            {
                                TreeNode ttn = new TreeNode(ttw.name);
                                tn.Add(ttn);
                            }//foreach
                            TreeNode[] array = tn.ToArray();
                            TreeNode tttn = new TreeNode(i.name, array);
                            treeView1.Nodes.Add(tttn);
                        }//foreach
                        // treeView1.SelectedNode = listBox1.Items.Count - 1;
                        //treeView1.SelectedNode = treeView1.Nodes.Count - 1;
                        //pBPanel.Image = myWindow.background;                //ustawiamy obrazek z ostatnio wczytanego
                        pBPanel.BackgroundImageLayout = ImageLayout.Stretch;
                        pBPanel.BackgroundImage = myWindow.background;
                        
                        ActiveWindow = PanelWindows[PanelWindows.Count - 1];
                    }//using
                }//if
            }//try
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: Nie mogę przeczytać pliku ! Oryginał: " + ex.Message);

            }//catch
            finally
            {
                myStream.Close();
            }//finally
        }
        
        private void pBPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // Make a note that we "have the mouse".
            bHaveMouse = true;
            if (bEnableCut)
            {
                // Store the "starting point" for this rubber-band rectangle.
                ptOriginal.X = e.X + pBPanel.Location.X;
                ptOriginal.Y = e.Y + pBPanel.Location.Y;
                // Special value lets us know that no previous
                // rectangle needs to be erased.
                ptLast.X = -1;
                ptLast.Y = -1;
            }//if
            IconLastPoint = e.Location;
        }

        private void pBPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (bHaveMouse && bEnableCut)
            {
                WindowObject wo = new WindowObject();           //tworzymy nowy obiekt z zaznaczonego kawałka
                wo.x = ptOriginal.X - pBPanel.Location.X;       //liczymy relatywne położenie ponieważ współrz. myszki mamy względem ekranu a w bitmapce mamy wsp. lokalne
                wo.y = ptOriginal.Y - pBPanel.Location.Y;
                if (ptLast.X > ptOriginal.X) wo.size_x = ptLast.X - ptOriginal.X;
                else wo.size_x = ptOriginal.X - ptLast.X;
                if (ptLast.Y > ptOriginal.Y) wo.size_y = ptLast.Y - ptOriginal.Y;
                else wo.size_y = ptOriginal.Y - ptLast.Y;

                int temp = PanelWindows.FindIndex(
                    delegate(Window w)
                    {
                        return w.name == treeView1.SelectedNode.Text;
                    }//delegate
                );      //to wyszukuje na liście element o nazwie jak wybrana w treeview

                //Teraz trzeba policzyć rozmiar okienka do wycięcia. Współrzędne z myszki są w rozdzielczości rozciągniętej bitmapki a chccemy wydobyć odpowiadający fragment tej bitmapki
                float factor = (float)PanelWindows[temp].background.Width / (float)pBPanel.Width;
                wo.size_x = (int)((float)wo.size_x * factor);
                wo.x = (int)((float)wo.x * factor);
                factor = (float)PanelWindows[temp].background.Height / (float)pBPanel.Height;
                wo.size_y = (int)((float)wo.size_y * factor);
                wo.y = (int)((float)wo.y * factor);

                //wycinamy za pomoca prostokącika
                Rectangle rect = new Rectangle(wo.x, wo.y, wo.size_x, wo.size_y);
                //Bitmap tb = (Bitmap)pBPanel.Image.Clone();
                Bitmap tb = (Bitmap)pBPanel.BackgroundImage.Clone();
                Bitmap tb2 = (Bitmap)tb.Clone(rect, tb.PixelFormat);

                wo.bitmap = new List<BitmapObject>();
                BitmapObject bo = new BitmapObject();
                bo.bitmap = (Bitmap)tb2.Clone();
                
                bo.state = 0;
                bo.name = "Stan" + bo.state.ToString();     //dodajemy bitmapkę z odpowiednią nazwą
                wo.bitmap.Add(bo);
                if (PanelWindows[temp].bitmaps == null)
                    wo.name = PanelWindows[temp].name + "_0";
                else
                    wo.name = PanelWindows[temp].name + "_" + PanelWindows[temp].bitmaps.Count.ToString();
                PanelWindows[temp].bitmaps.Add(wo);

                //a teraz wyczyscimy wyciety fragment tla

                Bitmap bmp = new Bitmap(pBPanel.BackgroundImage.Width, pBPanel.BackgroundImage.Height);     //tworzymy nowa bitmapke o odpowiednich rozmiarach. Nie mozna od razu zrobic bitmapy jako referencji do tla bo bedzie wyjatek
                Graphics rem_frame_g = Graphics.FromImage(bmp);                         //teraz grafika po ktorej bedziemy mazac
                rem_frame_g.DrawImage(pBPanel.BackgroundImage, new Point(0, 0));                  //uzupelniamy bitmapke o wlasciwy obrazek
                SolidBrush rem_frame_sb = new SolidBrush(pBPanel.BackColor);
                rem_frame_g.FillRectangle(rem_frame_sb, rect);                          //rysujemy prostokacik w kolorze tla
                pBPanel.BackgroundImage = bmp;                                                    //i calosc dopiero umieszczamy na ekranie
                ActiveWindow.background = bmp;

                //Na koniec trzeba przerysować wszystkie ikonki bo mazanie po tle usuwa je. Zrobimy to przy okazji odswiezania drzewka
                //Poniższe odświeża widok drzewka
                String temp_node = treeView1.SelectedNode.Text;
                LastSelectedTreeNode = null;

                treeView1.Nodes.Clear();
                foreach (Window i in PanelWindows)
                {
                    WindowObject[] two = i.bitmaps.ToArray();
                    List<TreeNode> tn = new List<TreeNode>();
                    foreach (WindowObject ttw in two)
                    {
                        TreeNode ttn = new TreeNode(ttw.name);
                        tn.Add(ttn);
                        if (ttn.Text == temp_node) LastSelectedTreeNode = ttn;
                    }//foreach
                    TreeNode[] array = tn.ToArray();
                    TreeNode tttn = new TreeNode(i.name, array);
                    treeView1.Nodes.Add(tttn);
                    if ((LastSelectedTreeNode == null) && (tttn.Text == temp_node)) LastSelectedTreeNode = tttn;
                }//foreach

                //nabazgralismy tlo to teraz ikonki
                WindowObject[] two2 = ActiveWindow.bitmaps.ToArray();
                foreach (WindowObject ttw in two2)
                {
                    Bitmap moved_bmp = new Bitmap(ttw.bitmap[0].bitmap);
                    Point moved_p = new Point(ttw.x, ttw.y);
                    Graphics moved_g = pBPanel.CreateGraphics();
                    moved_g.DrawImage(moved_bmp, moved_p);
                }//foreach
                pBPanel.Refresh();
            }//if
            
            if (LastSelectedTreeNode.Parent != null)
                treeView1.SelectedNode = LastSelectedTreeNode.Parent;
            else
                treeView1.SelectedNode = LastSelectedTreeNode;
            
            // Set internal flag to know we no longer "have the mouse".
            bHaveMouse = false;
            // If we have drawn previously, draw again in that spot
            // to remove the lines.

            if (ptLast.X != -1)
            {
                Point ptCurrent = new Point(e.X + pBPanel.Location.X, e.Y + pBPanel.Location.Y);
                MyDrawReversibleRectangle(ptOriginal, ptLast);
            }
            // Set flags to know that there is no "previous" line to reverse.
            ptLast.X = -1;
            ptLast.Y = -1;
            ptOriginal.X = -1;
            ptOriginal.Y = -1;

            bEnableCut = false;
        }

        private void pBPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Point ptCurrent = new Point(e.X + pBPanel.Location.X, e.Y + pBPanel.Location.Y);

            // If we "have the mouse", then we draw our lines.
            if (bHaveMouse && bEnableCut)
            {
                // If we have drawn previously, draw again in
                // that spot to remove the lines.
                if (ptLast.X != -1)
                {
                    MyDrawReversibleRectangle(ptOriginal, ptLast);
                }
                // Update last point.
                ptLast = ptCurrent;
                // Draw new lines.
                MyDrawReversibleRectangle(ptOriginal, ptCurrent);
            }

            //rozdzielimy rysowanie ramki i przenoszenie obiektu
            //najpierw rysowanie ramki
            int SelectedWindow = ActiveWindow.index;      //indeks aktualnie wybranego okienka
            label1.Text = SelectedWindow.ToString();
            if ((PanelWindows.Count > 0) && (PanelWindows[SelectedWindow].bitmaps.Count > 0) && (!IconUnderCursor))
            {
                int i = 0;
                
                while ((i < PanelWindows[SelectedWindow].bitmaps.Count) && (!IconUnderCursor))
                {
                    IconSelectedObject = PanelWindows[SelectedWindow].bitmaps[i];
                    if ((e.X > IconSelectedObject.x) && (e.X < (IconSelectedObject.x + IconSelectedObject.size_x)) &&
                       (e.Y > IconSelectedObject.y) && (e.Y < (IconSelectedObject.y + IconSelectedObject.size_y)))
                        IconUnderCursor = true;
                    else
                        i++;
                    if (i == PanelWindows[SelectedWindow].bitmaps.Count)
                        IconSelectedObject = null;
                    
                }//while
            }//if
            if (IconUnderCursor)    //rysujemy ramke
            {
                Rectangle frame_r = new Rectangle(IconSelectedObject.x, IconSelectedObject.y, IconSelectedObject.size_x, IconSelectedObject.size_y);
                Graphics frame_g = pBPanel.CreateGraphics();
                Pen frame_p = new Pen(Color.Red, 1);
                RedrawCurrentPanel();
                frame_g.DrawRectangle(frame_p, frame_r);
                IconLastLocation.X = IconSelectedObject.x;
                IconLastLocation.Y = IconSelectedObject.y;
            }//if
            if ((IconUnderCursor) &&
                !((e.X > IconSelectedObject.x) && (e.X < (IconSelectedObject.x + IconSelectedObject.size_x)) &&
                  (e.Y > IconSelectedObject.y) && (e.Y < (IconSelectedObject.y + IconSelectedObject.size_y))))      //usuwamy ramke bo wyjechalismy poza ikonke
            {
                Bitmap bmp_orig = (Bitmap)pBPanel.BackgroundImage;
                Rectangle rem_frame_r = new Rectangle(IconSelectedObject.x, IconSelectedObject.y, IconSelectedObject.size_x, IconSelectedObject.size_y);
                Point rem_frame_p = new Point(IconSelectedObject.x, IconSelectedObject.y);
                Graphics rem_frame_g = pBPanel.CreateGraphics();
                //SolidBrush rem_frame_sb = new SolidBrush(pBPanel.BackColor);
                //rem_frame_g.FillRectangle(rem_frame_sb, rem_frame_r);
                /*
                float x_factor = (float)(IconSelectedObject.size_x) / (float)pBPanel.Width;
                float y_factor = (float)(IconSelectedObject.size_y) / (float)pBPanel.Height;
                Rectangle rem_frame_r2 = new Rectangle(IconSelectedObject.x, IconSelectedObject.y, (int)Math.Ceiling((float)bmp_orig.Width * x_factor), (int)Math.Ceiling((float)bmp_orig.Height * y_factor));
                Bitmap bmp_copy = (Bitmap)bmp_orig.Clone(rem_frame_r2, bmp_orig.PixelFormat);
                Bitmap bmp = new Bitmap(bmp_copy, IconSelectedObject.size_x + 1, IconSelectedObject.size_y + 1);
                rem_frame_g.DrawImage(bmp, rem_frame_p);
                */
                //pBPanel.Refresh();
                RedrawCurrentPanel();
                Bitmap bmp2 = new Bitmap(IconSelectedObject.bitmap[0].bitmap);
                rem_frame_g.DrawImage(bmp2, rem_frame_p);
                
                IconUnderCursor = false;
                IconSelectedObject = null;
            }//if

            //Teraz kod odpowiedzialny za przesuniecie ikonki
            if (bHaveMouse && IconUnderCursor)
            {
                IconSelectedObject.x += e.X - IconLastPoint.X;
                IconSelectedObject.y += e.Y - IconLastPoint.Y;
                Bitmap moved_bmp = new Bitmap(IconSelectedObject.bitmap[0].bitmap);
                Point moved_p = new Point(IconSelectedObject.x, IconSelectedObject.y);
                Graphics moved_g = pBPanel.CreateGraphics();
                //moved_g.DrawImage(moved_bmp, moved_p);
                //Bitmap moved_bmp_orig = (Bitmap)pBPanel.BackgroundImage;
                //float x_factor = (float)(IconSelectedObject.size_x) / (float)pBPanel.Width;
                //float y_factor = (float)(IconSelectedObject.size_y) / (float)pBPanel.Height;
                //Rectangle moved_r2 = new Rectangle(IconLastLocation.X, IconLastLocation.Y, (int)Math.Ceiling((float)moved_bmp_orig.Width * x_factor), (int)Math.Ceiling((float)moved_bmp_orig.Height * y_factor));
                //Bitmap moved_bmp_copy = (Bitmap)moved_bmp_orig.Clone(moved_r2, moved_bmp_orig.PixelFormat);
                //Bitmap moved_bmpx = new Bitmap(moved_bmp_copy, IconSelectedObject.size_x + 1, IconSelectedObject.size_y + 1);
                //moved_g.DrawImage(moved_bmpx, IconLastLocation);
                //pBPanel.Refresh();
                RedrawCurrentPanel();
                moved_g.DrawImage(moved_bmp, moved_p);
                IconLastPoint = e.Location;
                //IconLastLocation.X = IconSelectedObject.x;
                //IconLastLocation.Y = IconSelectedObject.y;
            }//if
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                //Sprawdzamy czy wybraliśmy obiekt czy okienko
                if (treeView1.SelectedNode.Level == 1)  //obiekt
                {
                    //znajdujemy indeksy obiektu (temp1) i okienka (temp)
                    int temp1 = PanelWindows.FindIndex(
                        delegate(Window wo1)
                        {
                            return wo1.name == treeView1.SelectedNode.Parent.Text;
                        }//delegate
                    );
                    int temp = PanelWindows[temp1].bitmaps.FindIndex(
                        delegate(WindowObject wo)
                        {
                            return wo.name == treeView1.SelectedNode.Text;
                        }//delegate
                    );
                    //Teraz tworzymy nową formę przekazując odpowiednie dane
                    WindowObjectBrowser wob = new WindowObjectBrowser(this, temp1, temp);
                    
                    wob.pictureBox1.Image = PanelWindows[temp1].bitmaps[temp].bitmap[0].bitmap;
                    wob.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    //Wypełniamy listę
                   
                    ImageList imageListSmall = new ImageList();
                    wob.listView1.Clear();
                    
                    foreach (BitmapObject bo in PanelWindows[temp1].bitmaps[temp].bitmap)
                    {
                        ListViewItem item = wob.listView1.Items.Add(bo.name);
                        imageListSmall.Images.Add(bo.bitmap);
                        item.ImageIndex = bo.state;
                        item.Tag = bo;
                    }//foreach
                    wob.listView1.SmallImageList = imageListSmall;
                    
                    wob.Show();
                    wob.MdiParent = this;
                    
                }//if
                if (treeView1.SelectedNode.Level == 0)  //okienko
                {
                    int temp = PanelWindows.FindIndex(
                        delegate(Window wo)
                        {
                            return wo.name == treeView1.SelectedNode.Text;
                        }//delegate
                    );
                    pBPanel.BackgroundImageLayout = ImageLayout.Stretch;
                    pBPanel.BackgroundImage = PanelWindows[temp].background;
                    
                    ActiveWindow = PanelWindows[temp];
/*
                    //nabazgralismy tlo to teraz ikonki
                    WindowObject[] two = ActiveWindow.bitmaps.ToArray();
                    foreach (WindowObject ttw in two)
                    {
                        Bitmap moved_bmp = new Bitmap(ttw.bitmap[0].bitmap);
                        Point moved_p = new Point(ttw.x, ttw.y);
                        Graphics moved_g = pBPanel.CreateGraphics();
                        moved_g.DrawImage(moved_bmp, moved_p);
                    }//foreach
*/
                }//if
            }//try
            catch
            {

            }//catch
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            oFDObjectBitmap.ShowDialog();
        }

        private void oFDObjectBitmap_FileOk(object sender, CancelEventArgs e)
        {
             System.IO.Stream myStream = null;

             try
             {
                 if ((myStream = oFDObjectBitmap.OpenFile()) != null)
                 {
                     using (myStream)
                     {
                         //myWindow.background = new Bitmap(myStream);     //bitmapka tego okienka z pliku
                         WindowObject wo = new WindowObject();           //tworzymy nowy obiekt
                         wo.x = 0;
                         wo.y = 0;

                         wo.bitmap = new List<BitmapObject>();
                         BitmapObject bo = new BitmapObject();
                         bo.bitmap = new Bitmap(myStream);

                         wo.size_x = bo.bitmap.Width;
                         wo.size_y = bo.bitmap.Height;

                         bo.state = 0;
                         bo.name = "Stan" + bo.state.ToString();     //dodajemy bitmapkę z odpowiednią nazwą
                         wo.bitmap.Add(bo);

                         int temp = PanelWindows.FindIndex(
                            delegate(Window w)
                            {
                                return w.name == treeView1.SelectedNode.Text;
                            }//delegate
                         );      //to wyszukuje na liście element o nazwie jak wybrana w treeview

                         if (PanelWindows[temp].bitmaps == null)
                             wo.name = PanelWindows[temp].name + "_0";
                         else
                             wo.name = PanelWindows[temp].name + "_" + PanelWindows[temp].bitmaps.Count.ToString();
                         PanelWindows[temp].bitmaps.Add(wo);

                         

                     }//using
                 }//if
             }//try
             catch (Exception ex)
             {
                 MessageBox.Show("Błąd: Nie mogę przeczytać pliku ! Oryginał: " + ex.Message);

             }//catch
             finally
             {
                 //Poniższe odświeża widok rzewka i w zamierzeniu miało pozostawiac zaznaczone to co ostatnio. Nie działe niestety.
                 String temp_node = treeView1.SelectedNode.Text;

                 treeView1.Nodes.Clear();
                 foreach (Window i in PanelWindows)
                 {
                     WindowObject[] two = i.bitmaps.ToArray();
                     List<TreeNode> tn = new List<TreeNode>();
                     foreach (WindowObject ttw in two)
                     {
                         TreeNode ttn = new TreeNode(ttw.name);
                         tn.Add(ttn);
                     }//foreach
                     
                     TreeNode[] array = tn.ToArray();
                     TreeNode tttn = new TreeNode(i.name, array);
                     treeView1.Nodes.Add(tttn);
                 }//foreach

                 treeView1.SelectedNode = treeView1.Nodes[temp_node];
                 /*
                 int temp = PanelWindows.FindIndex(
                    delegate(Window w)
                    {
                        return w.name == treeView1.SelectedNode.Text;
                    }//delegate
                  );      //to wyszukuje na liście element o nazwie jak wybrana w treeview
                 */
             
                 WindowObject[] two1 = PanelWindows[0].bitmaps.ToArray();
                 foreach (WindowObject ttw in two1)
                 {
                     Bitmap bmp = new Bitmap(ttw.bitmap[0].bitmap);
                     Point p = new Point(ttw.x, ttw.y);
                     
                     Graphics g = pBPanel.CreateGraphics();
                     g.DrawImage(bmp, p);
                     
                 }//foreach
                 
                 myStream.Close();
            
             }//finally
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            bEnableCut = true;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
                LastSelectedTreeNode = e.Node;
        }

        private void pBPanel_BackgroundImageChanged(object sender, EventArgs e)
        {
            RedrawCurrentPanel();
        }
    }
}
