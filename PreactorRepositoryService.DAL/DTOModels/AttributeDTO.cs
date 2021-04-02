using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{


    public class AttributeDTO
    {
        public struct AttributeStruct
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Color { get; set; }
            public List<string> SecondaryConstraints { get; set; }
            public int OrderNumber { get; set; }

        }
        public List<AttributeStruct> Attribute1 { get; set;}
        public List<AttributeStruct> Attribute2 { get; set; }
        public List<AttributeStruct> Attribute3 { get; set; }
        public List<AttributeStruct> Attribute4 { get; set; }
        public List<AttributeStruct> Attribute5 { get; set; }
    }
}
