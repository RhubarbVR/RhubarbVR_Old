﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine.Components.Audio;
using RhubarbEngine.Components.Rendering;

namespace RhubarbEngine.World
{
    public class UpdateLists
    {
        public List<AudioOutput> audioOutputs = new List<AudioOutput>();
        public List<IRenderObject> trenderObject = new List<IRenderObject>();
        public List<IRenderObject> renderObject = new List<IRenderObject>();



    }
}
