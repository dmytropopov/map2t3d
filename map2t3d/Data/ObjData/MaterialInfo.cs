using System;
using System.Collections.Generic;
using System.Text;

namespace map2t3d.Data.ObjData
{
    public class MaterialInfo
    {
        public string MaterialName { get; set; }
        public int TextureSizeU { get; set; }
        public int TextureSizeV { get; set; }

        public double UTiling { get; set; } = 1;
        public double VTiling { get; set; } = 1;
    }
}
