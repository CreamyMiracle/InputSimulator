﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Inputs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}
