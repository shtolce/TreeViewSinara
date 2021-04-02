using PreactorRepositoryService.DAL.Services;
using PreactorRepositoryService.DAL.StructModels;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.ShrMemRepository
{
    public class ShrMemOrderRepo
    {
        public const long offset = 0x10000000; // 256 megabytes
        public const long length = 0x20000000; // 512 megabytes

        public ShrMemOrderRepo()
        {
            ShrMemManager.RunShrMemProcess();
        }


        public void writeOrders()
        {
            int size = 0;
            test t = new test { isChecked = 3, name = 5,s=DateTime.Now};
            test t2 = new test { isChecked = 3, name = 4, s = DateTime.Now };
            t.copyString("qwe");
            t2.copyString("zxc");

            List<test> listTest = new List<test> { t, t2 };
            size = Marshal.SizeOf(typeof(test));
            MemoryMappedFile sharedMemory = MemoryMappedFile.CreateOrOpen("MemoryFile111", length, MemoryMappedFileAccess.ReadWriteExecute);
            MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size * 6);
            int i1 = 0;
            foreach (var d in listTest)
            {
                var t1 = d;
                writer.Write(i1, ref t1);
                i1 += size;

            }
            //ShrMemManager.KillMemorySharedProcess();

        }

        public List<test> readOrders()
        {
            int size;
            size = Marshal.SizeOf(typeof(test));
            MemoryMappedFile sharedMemory = MemoryMappedFile.OpenExisting("MemoryFile111");
            test t;
            List<test> listOrder = new List<test>();
            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, length, MemoryMappedFileAccess.Read))
            {
                for (long i = 0; i < length; i += size)
                {
                    reader.Read(i, out t);
                    unsafe
                    {
                        var res = t.getUnsafeString(t.tname).ToString();
                    }

                    if (t.name > 0)
                        listOrder.Add(t);
                    else break;
                }

            }
            //переписать на редис
            return listOrder;
        }


    }
}
