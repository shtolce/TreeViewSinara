using Preactor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.DTOModels
{
    public struct MatrixStringDimensions
    {
        public MatrixStringDimensions(string x, string y)
        {
            X = x;
            Y = y;
        }
        public string X { get; }
        public string Y { get; }

        public override string ToString()
        {
            return $"Matrix x:{X} y:{Y}";
        }
    }



    public class ChangeoverGroupDTO
    {
        public int Id { get; set;}
        public string Name { get; set; }
        public TimeSpan Attribute1ChangeoverTime { get; set; }
        public TimeSpan Attribute2ChangeoverTime { get; set; }
        public TimeSpan Attribute3ChangeoverTime { get; set; }
        public TimeSpan Attribute4ChangeoverTime { get; set; }
        public TimeSpan Attribute5ChangeoverTime { get; set; }
        public Dictionary<MatrixDimensions,TimeSpan> Attribute1ChangeoverMatrix { get; set; }
        public Dictionary<MatrixDimensions, TimeSpan> Attribute2ChangeoverMatrix { get; set; }
        public Dictionary<MatrixDimensions, TimeSpan> Attribute3ChangeoverMatrix { get; set; }
        public Dictionary<MatrixDimensions, TimeSpan> Attribute4ChangeoverMatrix { get; set; }
        public Dictionary<MatrixDimensions, TimeSpan> Attribute5ChangeoverMatrix { get; set; }

        public Dictionary<MatrixStringDimensions, TimeSpan> Attribute1ChangeoverMatrixStr { get; set; }
        public Dictionary<MatrixStringDimensions, TimeSpan> Attribute2ChangeoverMatrixStr { get; set; }
        public Dictionary<MatrixStringDimensions, TimeSpan> Attribute3ChangeoverMatrixStr { get; set; }
        public Dictionary<MatrixStringDimensions, TimeSpan> Attribute4ChangeoverMatrixStr { get; set; }
        public Dictionary<MatrixStringDimensions, TimeSpan> Attribute5ChangeoverMatrixStr { get; set; }

    }
}
