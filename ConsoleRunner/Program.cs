using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTag.Backend;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Win32;

namespace ConsoleRunner
{
    class tplat : IPlatform
    {
        public string AppData
        {
            get
            {
                return "./";
            }
        }

        SQLitePlatformWin32 sq = new SQLitePlatformWin32();
        public ISQLitePlatform sqlite
        {
            get
            {
                return sq;
            }
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }


        public Stream ReadFile(string path)
        {
            throw new NotImplementedException();
        }
    }
    class Program 
    {
        static void Main(string[] args)
        {
            File.Delete("./data.db");
            Presenter.RunTests(new tplat());
        }
    }
}
