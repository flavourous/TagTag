using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Interop;

namespace TagTag.Backend
{
    public interface IPlatform
    {
        void WriteLine(String s);
        ISQLitePlatform sqlite { get; }
        String AppData { get; }
        void DeleteFile(String path);
        Stream ReadFile(String path);
        int AppVersion { get; }
    }
}
