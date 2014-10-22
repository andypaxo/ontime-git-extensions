using System;
using System.Linq;
using System.Windows.Forms;

namespace OnTimeTicket
{
    public class WinFormUtils
    {
        public static Control FindControl(Func<Control, bool> predicate, Control parent)
        {
            foreach (var control in parent.Controls.Cast<Control>())
            {
                if (predicate(control))
                    return control;

                var foundChildControl = FindControl(predicate, control);
                if (foundChildControl != null)
                    return foundChildControl;
            }

            return null;
        } 
    }
}