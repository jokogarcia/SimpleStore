using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStore.Exceptions
{
    public class ItemNotFoundException:Exception
    {
        public ItemNotFoundException(UInt32 key) : base($"Item with key {key} not found.") { }
    }
}
