using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace comicDownLoad
{
    class DoublebufferListview: ListView
    {
         public DoublebufferListview()
         {
              SetStyle(ControlStyles.DoubleBuffer |
              ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint, true);
              UpdateStyles();
         }

        
    }
}
