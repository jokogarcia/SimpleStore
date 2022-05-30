using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStore.Exceptions
{
    public class FileformatException:Exception
    {
        public FileformatException():base("File could not be procesed as SimpleStore of the defined Type."){}
    }
}
