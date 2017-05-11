using System;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos.Tms
{
    class SqliteImport : ISqliteImport
    {
        const string LibraryPath = "sqlite3";

        public static ISqliteImport Dll = new SqliteImport();
        private SqliteImport() { }

        [DllImport(LibraryPath, EntryPoint = "sqlite3_threadsafe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreadsafeInternal();
        public int Threadsafe()
        {
            return ThreadsafeInternal();
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_open", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result OpenInternal([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db);
        public SQLite3.Result Open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db)
        {
            return OpenInternal(filename, out db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result OpenInternal([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db, int flags, IntPtr zvfs);
        public SQLite3.Result Open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db, int flags, IntPtr zvfs)
        {
            return OpenInternal(filename, out db, flags, zvfs);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result OpenInternal(byte[] filename, out IntPtr db, int flags, IntPtr zvfs);
        public SQLite3.Result Open(byte[] filename, out IntPtr db, int flags, IntPtr zvfs)
        {
            return OpenInternal(filename, out db, flags, zvfs);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_open16", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result Open16Internal([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db);
        public SQLite3.Result Open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db)
        {
            return Open16Internal(filename, out db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_enable_load_extension", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result EnableLoadExtensionInternal(IntPtr db, int onoff);
        public SQLite3.Result EnableLoadExtension(IntPtr db, int onoff)
        {
            return EnableLoadExtensionInternal(db, onoff);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result CloseInternal(IntPtr db);
        public SQLite3.Result Close(IntPtr db)
        {
            return CloseInternal(db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_close_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result Close2Internal(IntPtr db);
        public SQLite3.Result Close2(IntPtr db)
        {
            return Close2Internal(db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result InitializeInternal();
        public SQLite3.Result Initialize()
        {
            return InitializeInternal();
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_shutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result ShutdownInternal();
        public SQLite3.Result Shutdown()
        {
            return ShutdownInternal();
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_config", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result ConfigInternal(SQLite3.ConfigOption option);
        public SQLite3.Result Config(SQLite3.ConfigOption option)
        {
            return ConfigInternal(option);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_win32_set_directory", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int SetDirectoryInternal(uint directoryType, string directoryPath);
        public int SetDirectory(uint directoryType, string directoryPath)
        {
            return SetDirectoryInternal(directoryType, directoryPath);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_busy_timeout", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result BusyTimeoutInternal(IntPtr db, int milliseconds);
        public SQLite3.Result BusyTimeout(IntPtr db, int milliseconds)
        {
            return BusyTimeoutInternal(db, milliseconds);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_changes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ChangesInternal(IntPtr db);
        public int Changes(IntPtr db)
        {
            return ChangesInternal(db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result Prepare2Internal(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, int numBytes, out IntPtr stmt, IntPtr pzTail);
        public SQLite3.Result Prepare2(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, int numBytes, out IntPtr stmt, IntPtr pzTail)
        {
            return Prepare2Internal(db, sql, numBytes, out stmt, pzTail);
        }


#if NETFX_CORE
        [DllImport (LibraryPath, EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Prepare2InternalInternal(IntPtr db, byte[] queryBytes, int numBytes, out IntPtr stmt, IntPtr pzTail);
        public Result Prepare2Internal(IntPtr db, byte[] queryBytes, int numBytes, out IntPtr stmt, IntPtr pzTail)
        {
            return Prepare2InternalInternal(db, queryBytes, numBytes, out stmt, pzTail);
        }
#endif

        [DllImport(LibraryPath, EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result StepInternal(IntPtr stmt);
        public SQLite3.Result Step(IntPtr stmt)
        {
            return StepInternal(stmt);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_reset", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result ResetInternal(IntPtr stmt);
        public SQLite3.Result Reset(IntPtr stmt)
        {
            return ResetInternal(stmt);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.Result FinalizeInternal(IntPtr stmt);
        public SQLite3.Result Finalize(IntPtr stmt)
        {
            return FinalizeInternal(stmt);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_last_insert_rowid", CallingConvention = CallingConvention.Cdecl)]
        public static extern long LastInsertRowidInternal(IntPtr db);
        public long LastInsertRowid(IntPtr db)
        {
            return LastInsertRowidInternal(db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_errmsg16", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ErrmsgInternal(IntPtr db);
        public IntPtr Errmsg(IntPtr db)
        {
            return ErrmsgInternal(db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_parameter_index", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BindParameterIndexInternal(IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string name);
        public int BindParameterIndex(IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string name)
        {
            return BindParameterIndexInternal(stmt, name);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_null", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BindNullInternal(IntPtr stmt, int index);
        public int BindNull(IntPtr stmt, int index)
        {
            return BindNullInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BindIntInternal(IntPtr stmt, int index, int val);
        public int BindInt(IntPtr stmt, int index, int val)
        {
            return BindIntInternal(stmt, index, val);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BindInt64Internal(IntPtr stmt, int index, long val);
        public int BindInt64(IntPtr stmt, int index, long val)
        {
            return BindInt64Internal(stmt, index, val);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_double", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BindDoubleInternal(IntPtr stmt, int index, double val);
        public int BindDouble(IntPtr stmt, int index, double val)
        {
            return BindDoubleInternal(stmt, index, val);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_text16", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int BindTextInternal(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string val, int n, IntPtr free);
        public int BindText(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string val, int n, IntPtr free)
        {
            return BindTextInternal(stmt, index, val, n, free);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_bind_blob", CallingConvention = CallingConvention.Cdecl)]
        public static extern int BindBlobInternal(IntPtr stmt, int index, byte[] val, int n, IntPtr free);
        public int BindBlob(IntPtr stmt, int index, byte[] val, int n, IntPtr free)
        {
            return BindBlobInternal(stmt, index, val, n, free);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumnCountInternal(IntPtr stmt);
        public int ColumnCount(IntPtr stmt)
        {
            return ColumnCountInternal(stmt);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_name", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnNameInternal(IntPtr stmt, int index);
        public IntPtr ColumnName(IntPtr stmt, int index)
        {
            return ColumnNameInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_name16", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnName16InternalInternal(IntPtr stmt, int index);
        public IntPtr ColumnName16Internal(IntPtr stmt, int index)
        {
            return ColumnName16InternalInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.ColType ColumnTypeInternal(IntPtr stmt, int index);
        public SQLite3.ColType ColumnType(IntPtr stmt, int index)
        {
            return ColumnTypeInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_int", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumnIntInternal(IntPtr stmt, int index);
        public int ColumnInt(IntPtr stmt, int index)
        {
            return ColumnIntInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
        public static extern long ColumnInt64Internal(IntPtr stmt, int index);
        public long ColumnInt64(IntPtr stmt, int index)
        {
            return ColumnInt64Internal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
        public static extern double ColumnDoubleInternal(IntPtr stmt, int index);
        public double ColumnDouble(IntPtr stmt, int index)
        {
            return ColumnDoubleInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnTextInternal(IntPtr stmt, int index);
        public IntPtr ColumnText(IntPtr stmt, int index)
        {
            return ColumnTextInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnText16Internal(IntPtr stmt, int index);
        public IntPtr ColumnText16(IntPtr stmt, int index)
        {
            return ColumnText16Internal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnBlobInternal(IntPtr stmt, int index);
        public IntPtr ColumnBlob(IntPtr stmt, int index)
        {
            return ColumnBlobInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumnBytesInternal(IntPtr stmt, int index);
        public int ColumnBytes(IntPtr stmt, int index)
        {
            return ColumnBytesInternal(stmt, index);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_extended_errcode", CallingConvention = CallingConvention.Cdecl)]
        public static extern SQLite3.ExtendedResult ExtendedErrCodeInternal(IntPtr db);
        public SQLite3.ExtendedResult ExtendedErrCode(IntPtr db)
        {
            return ExtendedErrCodeInternal(db);
        }


        [DllImport(LibraryPath, EntryPoint = "sqlite3_libversion_number", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LibVersionNumberInternal();
        public int LibVersionNumber()
        {
            return LibVersionNumberInternal();
        }

    }

}
