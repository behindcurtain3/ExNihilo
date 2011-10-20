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
            if (!blanked)
            {
                System.IO.File.WriteAllText(@"C:\Users\Justin.User\Projects\Ants\tools\log.txt", "");
                blanked = true;
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Justin.User\Projects\Ants\tools\log.txt", true))
            {
                file.WriteLine(text);
            }
         
        }
    }
}
