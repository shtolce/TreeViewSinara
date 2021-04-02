using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PreactorRepositoryService.DAL.Services
{
    public static class ShrMemManager
    {
        public static void RunShrMemProcess()
        {
            var cProc = Process.GetProcessesByName("ShrMemProcProject").Count();
            if (cProc!=0)
                return;
            var s = Assembly.GetExecutingAssembly().Location;
            AppDomain otherDomain = AppDomain.CreateDomain("shtolceDomain");
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = s.Replace("PreactorRepositoryService.DAL.dll", "ShrMemProcProject.exe")  ;
            procInfo.UseShellExecute = false;
            procInfo.CreateNoWindow = true;
            procInfo.Domain = "shtolceDomain";
            Process.Start(procInfo);
        }

        public static void KillMemorySharedProcess()
        {
            try
            {
                Process.GetProcessesByName("ShrMemProcProject").First().Kill();
            }
            catch { };
        }



    }
}
