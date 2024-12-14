using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessUtilities
{
    public static class ThreadManager
    {
        [Flags]
        public enum ThreadAccessRights : int
        {
            Terminate = 0x0001,
            SuspendResume = 0x0002,
            GetContext = 0x0008,
            SetContext = 0x0010,
            SetInformation = 0x0020,
            QueryInformation = 0x0040,
            SetThreadToken = 0x0080,
            Impersonate = 0x0100,
            DirectImpersonation = 0x0200
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccessRights desiredAccess, bool inheritHandle, uint threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SuspendThread(IntPtr threadHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ResumeThread(IntPtr threadHandle);

        // Pauses the entire process by suspending all threads in that process
        public static void PauseThread(Process targetProcess)
        {
            SuspendProcess(targetProcess);
        }

        // Suspends all threads in a target process
        public static void SuspendProcess(Process targetProcess)
        {
            foreach (ProcessThread thread in targetProcess.Threads)
            {
                IntPtr threadHandle = OpenThread(ThreadAccessRights.SuspendResume, false, (uint)thread.Id);
                if (threadHandle != IntPtr.Zero)
                {
                    SuspendThread(threadHandle);
                }
            }
        }

        // Resumes all threads in a target process
        public static void ResumeProcess(Process targetProcess)
        {
            foreach (ProcessThread thread in targetProcess.Threads)
            {
                IntPtr threadHandle = OpenThread(ThreadAccessRights.SuspendResume, false, (uint)thread.Id);
                if (threadHandle != IntPtr.Zero)
                {
                    ResumeThread(threadHandle);
                }
            }
        }
    }
}
