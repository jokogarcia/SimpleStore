using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStore.Exceptions
{
    public class StoreDisposedException:Exception
    {
        public StoreDisposedException() : base("Item has been disposed. Cannot be used.") { }

    }
}
