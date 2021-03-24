using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comp
{
    class coefficient
    {
        coefficient index { get; set; }
    }

    class Formula : coefficient
    {

    }

    class Term
    {
        public List<coefficient> numerator;
        public List<coefficient> denominator
    }
    
}
