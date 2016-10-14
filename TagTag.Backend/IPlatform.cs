using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Interop;
using System.IO;

namespace TagTag.Backend
{
    public interface IPlatform
    {
        int AppVersion { get; }

        void WriteLine(String s);
        ISQLitePlatform sqlite { get; }
        String AppData { get; }
        void DeleteFile(String path);
        Stream ReadFile(String path);
    }
}
