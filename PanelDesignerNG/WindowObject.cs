using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PanelDesignerNG
{
    public class WindowObject
    {
        private List<BitmapObject> _bitmap;
        private int _x;
        private int _y;
        private int _size_x;
        private int _size_y;
        private String _name;

        public List<BitmapObject> bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }//bitmaps

        public int x
        {
            get { return _x; }
            set { _x = value; }

        }//x

        public int y
        {
            get { return _y; }
            set { _y = value; }

        }//y

        public int size_x
        {
            get { return _size_x; }
            set { _size_x = value; }

        }//size_x

        public int size_y
        {
            get { return _size_y; }
            set { _size_y = value; }

        }//size_y

        public String name
        {
            get { return _name; }
            set { _name = value; }
        }//name
    }
}
