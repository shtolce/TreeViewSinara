using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.MemorySharedRepository
{

    public struct test
    {
        public int name;
        public int isChecked;
    }

    public class OrderMSharedRepo
    {

        static void writeData()
        {
            long offset = 0x10000000;
            long length = 0x0010000;
            int size = 0;
            test t = new test { isChecked = 1, name = 1 };
            test t2 = new test { isChecked = 2, name = 2 };
            List<test> listTest = new List<test> { t, t2 };
            size = Marshal.SizeOf(typeof(test));
            MemoryMappedFile sharedMemory = MemoryMappedFile.CreateOrOpen("MemoryFile111", length, MemoryMappedFileAccess.ReadWriteExecute);
            MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, length);
            int i1 = 0;
            foreach (var d in listTest)
            {
                var t1 = d;
                i1 += size;
                writer.Write(i1, ref t1);
            }
            while (true) ;
        }


    }
}
