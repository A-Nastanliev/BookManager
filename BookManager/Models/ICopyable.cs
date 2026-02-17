using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.Models
{
    interface ICopyable <T>
    {
        void CopyFrom(T original);
    }
}
