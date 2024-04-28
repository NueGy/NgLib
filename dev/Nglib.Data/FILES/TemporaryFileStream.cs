using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nglib.FILES
{

    public class TemporaryFileStream : System.IO.FileStream
    {
        public TemporaryFileStream()
                : base(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose) { }
        public TemporaryFileStream(FileAccess access)
                : base(Path.GetTempFileName(), FileMode.Create, access, FileShare.Read, 4096, FileOptions.DeleteOnClose) { }
        public TemporaryFileStream(FileAccess access, FileShare share)
                : base(Path.GetTempFileName(), FileMode.Create, access, share, 4096, FileOptions.DeleteOnClose) { }
        public TemporaryFileStream(FileAccess access, FileShare share, int bufferSize)
                : base(Path.GetTempFileName(), FileMode.Create, access, share, bufferSize, FileOptions.DeleteOnClose) { }


 

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    try
        //    {
        //        if(!string.IsNullOrWhiteSpace(this.Name))
        //            System.IO.File.Delete(this.Name);   
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

    }


}
