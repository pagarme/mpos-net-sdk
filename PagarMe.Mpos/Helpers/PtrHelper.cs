using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos.Helpers
{
    class PtrHelper
    {
        public static T DerefOrDefault<T>(IntPtr pointer)
        {
            return pointer == IntPtr.Zero ? default(T) : Deref<T>(pointer);
        }

        public static T Deref<T>(IntPtr pointer)
        {
            return (T)Marshal.PtrToStructure(pointer, typeof(T));
        }

        public static T Deref<T>(IntPtr initialPointer, int position)
        {
            var offset = position * Marshal.SizeOf(typeof(T));
            var pointer = IntPtr.Add(initialPointer, offset);
            return Deref<T>(pointer);
        }

        public static List<T> DerefList<T>(IntPtr initialPointer, int length)
        {
            var list = new List<T>();

            for (var i = 0; i < length; i++)
            {
                var item = Deref<T>(initialPointer, i);
                list.Add(item);
            }

            return list;
        }

        public static T DoubleDeref<T>(IntPtr initialPointer, int position)
        {
            var secondPointer = Deref<IntPtr>(initialPointer, position);
            return Deref<T>(secondPointer);
        }

        public static List<T> DoubleDerefList<T>(IntPtr initialPointer, int length)
        {
            var list = new List<T>();

            for (var i = 0; i < length; i++)
            {
                var item = DoubleDeref<T>(initialPointer, i);
                list.Add(item);
            }

            return list;
        }



        public static IntPtr Ref<T>(T obj)
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }

        public static void Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }



        public static IntPtr RefLists(params IList[] listList)
        {
            var count = listList.Sum(l => l.Count);
            var tablePointer = Marshal.AllocHGlobal(IntPtr.Size * count);

            var offset = 0;

            foreach (var list in listList)
            {
                foreach (var item in list)
                {
                    var size = Marshal.SizeOf(item.GetType());
                    var ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(item, ptr, false);

                    Marshal.StructureToPtr(ptr, IntPtr.Add(tablePointer, offset * Marshal.SizeOf(typeof(IntPtr))), false);
                    offset++;
                }
            }

            return tablePointer;
        }

        public static void FreeLists(IntPtr tablePointer, params IList[] listList)
        {
            var count = listList.Sum(l => l.Count);

            for (var i = 0; i < count; i++)
            {
                var deref = Deref<IntPtr>(tablePointer, i);
                Marshal.FreeHGlobal(deref);
            }

            Marshal.FreeHGlobal(tablePointer);
        }

    }
}
