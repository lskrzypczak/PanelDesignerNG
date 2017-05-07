using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PanelDesignerNG
{
    public class Window
    {
        private Bitmap _background;
        private List<WindowObject> _bitmaps;
        private String _name;
        private int _index;

        public Window()
        {
            _bitmaps = new List<WindowObject>();
        }//Constructor

        public Bitmap background
        {
            get { return _background; }
            set { _background = value; }
        }//background

        public List<WindowObject> bitmaps
        {
            get { return _bitmaps; }
            set { _bitmaps = value; }
        }//bitmaps

        public String name
        {
            get { return _name; }
            set { _name = value; }
        }//name

        public int index
        {
            get { return _index; }
            set { _index = value; }
        }//name
    }
}
