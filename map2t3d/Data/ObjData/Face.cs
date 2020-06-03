using System;
using System.Collections.Generic;
using System.Text;

namespace map2t3d.Data.ObjData
{
    public class Face
    {
        public List<FaceVertex> FaceVertices { get; set; }
        public MaterialInfo Material { get; set; }
    }
}
