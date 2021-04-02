using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.StructModels
{
        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
    public unsafe struct test
    {
        public int name;
        public int isChecked;
        public DateTime s;
        public fixed char tname[255];

        public void  copyString(string dId)
        {
            fixed (char* distId = tname)
            {
                char[] chars = dId.ToCharArray();
                Marshal.Copy(chars, 0, new IntPtr(distId), chars.Length);
            }
        }
        
        public string getUnsafeString(char* s)
        {
            var result = new StringBuilder();
            unsafe
            {
                for (var i1 = 0; i1 < 255; i1++)
                {
                    fixed (char* distId = tname)
                    {
                        if ((char)Marshal.ReadByte(new IntPtr(distId + i1), 0) == 0)
                            continue;
                        result.Append((char)Marshal.ReadByte(new IntPtr(distId + i1), 0));
                    }
                }
            }
            var res = result.ToString();
            return res;
        }



    }


}

