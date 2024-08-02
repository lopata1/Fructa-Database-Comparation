using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fructa_Database_Comparation
{
    public partial class FructaButton : Button
    {
        public FructaButton() {
            SetStyle(ControlStyles.Selectable, false);
        }
        protected override bool ShowFocusCues
        {
            get
            {
                return false;
            }
        }
    }
}
