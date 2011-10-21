using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ants
{
    public class Debug
    {
        public static Boolean blanked = false;

        public static void Write(String text)
        {
            /*
            if (!blanked)
            {
                System.IO.File.WriteAllText(@"F:\Programming\Projects\C#\ExNihilo\tools\log.txt", "");
                blanked = true;
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"F:\Programming\Projects\C#\ExNihilo\tools\log.txt", true))
            {
                file.WriteLine(text);
            }
            */
        }
    }
}
