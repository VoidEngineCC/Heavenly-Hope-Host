using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessUtilities
{
    internal class MemoryOperations
    {
        public static void InjectLibrary(int targetProcessId, string libraryPath)
        {
            IntPtr processHandle = MemoryOperations.OpenTargetProcess(1082, false, targetProcessId);
            IntPtr loadLibraryAddress = MemoryOperations.GetFunctionAddress(MemoryOperations.GetLibraryHandle("kernel32.dll"), "LoadLibraryA");
            uint bufferSize = (uint)((libraryPath.Length + 1) * Marshal.SizeOf(typeof(char)));
            IntPtr allocatedMemory = MemoryOperations.AllocateMemory(processHandle, IntPtr.Zero, bufferSize, 12288U, 4U);
            UIntPtr bytesWritten;
            MemoryOperations.WriteToMemory(processHandle, allocatedMemory, Encoding.Default.GetBytes(libraryPath), bufferSize, out bytesWritten);
            MemoryOperations.StartRemoteThread(processHandle, IntPtr.Zero, 0U, loadLibraryAddress, allocatedMemory, 0U, IntPtr.Zero);
        }

        [DllImport("kernel32.dll")]
        public static extern int PauseThread(IntPtr threadHandle);

        [DllImport("kernel32.dll")]
        public static extern int ContinueThread(IntPtr threadHandle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThreadHandle(int accessRights, bool inheritHandle, int threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReleaseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenTargetProcess(int accessRights, bool inheritHandle, int processId);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetFunctionAddress(IntPtr moduleHandle, string functionName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetLibraryHandle(string moduleName);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr AllocateMemory(IntPtr processHandle, IntPtr address, uint size, uint allocationType, uint protectionFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteToMemory(IntPtr processHandle, IntPtr baseAddress, byte[] buffer, uint size, out UIntPtr bytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr StartRemoteThread(IntPtr processHandle, IntPtr threadAttributes, uint stackSize, IntPtr startAddress, IntPtr parameter, uint creationFlags, IntPtr threadId);
    }
}
