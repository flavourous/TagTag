using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TagTag.Backend
{
    public interface IPlatform
    {
        int AppVersion { get; }

        void WriteLine(String s);
        String AppData { get; }
        void DeleteFile(String path);
        Stream ReadFile(String path);
    }
}
