using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PanelDesignerNG
{
    public class BitmapObject
    {
        private Bitmap _bitmap;
        private String _name;
        private int _state;

        public static BitmapObject Copy(BitmapObject bo)
        {
            return (BitmapObject)bo.MemberwiseClone();
        }//Copy

        public Bitmap bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }//bitmap

        public String name
        {
            get { return _name; }
            set { _name = value; }
        }//name

        public int state
        {
            get { return _state; }
            set { _state = value; }
        }//state

    }
}
