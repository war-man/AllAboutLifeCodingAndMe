﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPaper.Models
{
    public class BasketWithName : BasketProduct
    {
        public string Name { get; set; }

        public string ProductName { get; set; }
    }
}
