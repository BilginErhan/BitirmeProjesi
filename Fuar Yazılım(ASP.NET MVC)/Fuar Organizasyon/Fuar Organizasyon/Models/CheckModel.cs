using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fuar_Organizasyon.Models
{
    public class CheckModel
    {
        public int Id
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool Checked
        {
            get;
            set;
        }
    }
}