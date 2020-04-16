using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpdownload
{
    class DMZJ:PublicThing
    {
        public string GetElement(int c,int a)
        {
            return (c < a ? "" : GetElement(c / a, a)) + ((c = c % a) > 35 ? Convert.ToChar(c + 29).ToString() : AnalyseTool.DecTo36(c));
        }

        public void DecodeExercies(string p, int a,int c,string []k,int e,int d)
        {
            while (c-- > 0)
            { 
               
            }
        }
    }
}
