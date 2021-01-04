﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsRestAPI.Extractor
{
    public class ProjectProperties
    {
        public class Value
        {
            public string Name { get; set; }
            public string RefValue { get; set; }
        }

        public class Properties
        {
            public int Count { get; set; }
            public IList<Value> Value { get; set; }

            public string TypeClass { get; set; }
        }


    }
}
