using System;
using System.Runtime.InteropServices;

namespace Assets.GEL
{
    public class IntVector : IIntVector
    {
        
        public IntPtr[] _intVector;

        public IntVector(int size)
        {
            _intVector = IntVector_new(size);
        }

        public IntVector(IntPtr[] intVector)
        {
            _intVector = intVector;
        }

        ~IntVector()
        {
            IntVector_delete(_intVector);
        }
        
        public int Get(int index)
        {
            return IntVector_get(_intVector, index);
        }

        public int Size()
        {
            return IntVector_size(_intVector);
        }

        public IntPtr[] GetVector()
        {
            return _intVector;
        }
    }
}