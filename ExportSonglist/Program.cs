using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

using Nanook.QueenBee.Parser;

namespace ExportSonglist
{
    class Program
    {
        public static Dictionary<string, string> fileNames = new Dictionary<string, string>
        {
            { "en", "qb" },
            { "fr", "qb_f" },
            { "de", "qb_g" },
            { "es", "qb_s" },
            { "it", "qb_i" },
            { "ko", "qb_k" }
        };

        static void  Main(string[] args)
        {
            string gh3path = null;
            string gh3lang = null;
            try
            {
                gh3path = (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Aspyr\Guitar Hero III", @"Path", null);
                gh3lang = (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Aspyr\Guitar Hero III", @"Language", null);
            }
            catch(SecurityException ex) {
                Console.Error.WriteLine("Permissions error: {0}", ex.Message);
            }
            if (gh3path == null || gh3lang == null)
            {
                Console.Error.WriteLine("Could not locate your Guitar Hero 3 installation.");
                return;
            }

            var gameDataDir = Path.Combine(gh3path, "DATA", "PAK");
            exportSongList(gameDataDir, fileNames[gh3lang]);
        }

        static void exportSongList(string gameDataDir, string gameFileName)
        {
            var qb = Path.Combine(gameDataDir, gameFileName);
            var pf = new PakFormat(qb + ".pak.xen", qb + ".pab.xen", Path.Combine(gameDataDir, "dbg.pak.xen"), PakFormatType.PC);
            var editor = new PakEditor(pf);
            var qbFile = editor.ReadQbFile(@"scripts\guitar\songlist.qb");
            var songlist = (QbItemStruct) qbFile.FindItem(QbKey.Create("permanent_songlist_props"), false);

            using (StreamWriter sw = new StreamWriter("songs.csv"))
            {
                sw.WriteLine("checksum,name,title,artist");

                foreach (var i in songlist.Items) {
                    var entry = (QbItemStruct)i;
                    var checksum = (QbItemQbKey)entry.FindItem(QbKey.Create("checksum"), false);
                    var internalName = (QbItemString)entry.FindItem(QbKey.Create("name"), false);
                    var title = (QbItemString)entry.FindItem(QbKey.Create("title"), false);
                    var artist = (QbItemString)entry.FindItem(QbKey.Create("artist"), false);

                    sw.WriteLine("{0:X8},{1},{2},{3}", checksum.Values[0], internalName.Strings[0], title.Strings[0], artist.Strings[0]);
                }
            }

        }
    }
}
