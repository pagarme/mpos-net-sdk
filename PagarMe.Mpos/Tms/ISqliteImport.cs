using System;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos.Tms
{
    internal interface ISqliteImport
    {
        int Threadsafe();

        SQLite3.Result Open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db);

        SQLite3.Result Open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db, int flags, IntPtr zvfs);

        SQLite3.Result Open(byte[] filename, out IntPtr db, int flags, IntPtr zvfs);

        SQLite3.Result Open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db);

        SQLite3.Result EnableLoadExtension(IntPtr db, int onoff);

        SQLite3.Result Close(IntPtr db);

        SQLite3.Result Close2(IntPtr db);

        SQLite3.Result Initialize();

        SQLite3.Result Shutdown();

        SQLite3.Result Config(SQLite3.ConfigOption option);

        int SetDirectory(uint directoryType, string directoryPath);

        SQLite3.Result BusyTimeout(IntPtr db, int milliseconds);

        int Changes(IntPtr db);

        SQLite3.Result Prepare2(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, int numBytes, out IntPtr stmt, IntPtr pzTail);

#if NETFX_CORE
        Result Prepare2 (IntPtr db, byte[] queryBytes, int numBytes, out IntPtr stmt, IntPtr pzTail);
#endif

        SQLite3.Result Step(IntPtr stmt);

        SQLite3.Result Reset(IntPtr stmt);

        SQLite3.Result Finalize(IntPtr stmt);

        long LastInsertRowid(IntPtr db);

        IntPtr Errmsg(IntPtr db);

        int BindParameterIndex(IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string name);

        int BindNull(IntPtr stmt, int index);

        int BindInt(IntPtr stmt, int index, int val);

        int BindInt64(IntPtr stmt, int index, long val);

        int BindDouble(IntPtr stmt, int index, double val);

        int BindText(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string val, int n, IntPtr free);

        int BindBlob(IntPtr stmt, int index, byte[] val, int n, IntPtr free);

        int ColumnCount(IntPtr stmt);

        IntPtr ColumnName(IntPtr stmt, int index);

        IntPtr ColumnName16Internal(IntPtr stmt, int index);

        SQLite3.ColType ColumnType(IntPtr stmt, int index);

        int ColumnInt(IntPtr stmt, int index);

        long ColumnInt64(IntPtr stmt, int index);

        double ColumnDouble(IntPtr stmt, int index);

        IntPtr ColumnText(IntPtr stmt, int index);

        IntPtr ColumnText16(IntPtr stmt, int index);

        IntPtr ColumnBlob(IntPtr stmt, int index);

        int ColumnBytes(IntPtr stmt, int index);

        SQLite3.ExtendedResult ExtendedErrCode(IntPtr db);

        int LibVersionNumber();

    }
}
