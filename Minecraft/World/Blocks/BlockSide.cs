﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public enum BlockSide : sbyte
    {
        Back = 0, //Side facing negative Z
        Right = 1, //Side facing negative X
        Front = 2, //Side facing positive Z
        Left = 3, //Side facing positive X
        Top = 4, //Side facing positive Y
        Bottom = 5 //Side facing negative Y
    };
}