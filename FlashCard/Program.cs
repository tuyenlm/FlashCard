using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace FlashCard
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
	        if (RunningInstance() != null) return;
	        Application.EnableVisualStyles();
	        Application.SetCompatibleTextRenderingDefault(false);
	        Application.Run(new Form1());
        }

	    private static Process RunningInstance()
        {
            var current = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(current.ProcessName);
            //Loop through the running processes in with the same name 
            foreach (Process process in processes)
            {
	            //Ignore the current process 
	            if (process.Id == current.Id) continue;
	            //Make sure that the process is running from the exe file. 
	            if (Assembly.GetExecutingAssembly().Location.
		                Replace("/", "\\") == current.MainModule.FileName)
	            {
		            return process;
	            }
            }
            //No other instance was found, return null.  
            return null;
        }
    }
}