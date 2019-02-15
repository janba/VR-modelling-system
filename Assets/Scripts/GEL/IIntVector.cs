using System;
using System.Runtime.InteropServices;

namespace Assets.GEL
{
    public class IIntVector
    {
        [DllImport("GELExt")]
        protected static extern IntPtr[] IntVector_new(int size);
        
        [DllImport("GELExt")]
        protected static extern int IntVector_get(IntPtr[] intVector, int index);

        [DllImport("GELExt")]
        protected static extern int IntVector_size(IntPtr[] intVector);

        [DllImport("GELExt")]
        protected static extern void IntVector_delete(IntPtr[] intVector);
    }
}