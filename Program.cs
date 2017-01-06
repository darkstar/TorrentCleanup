/*
TorrentCleanup
Copyright (C) 2016 Michael Drüing

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TorrentCleanup
{
    class Program
    {
        static string MakePath(TList segments)
        {
            List<string> path = new List<string>();

            foreach (TObject pe in segments)
            {
                if (pe is TString)
                {
                    path.Add((pe as TString).Value);
                }
            }

            return String.Join(@"\", path);
        }

        static List<TorrentFileInfo> GetFileList(string path)
        {
            List<TorrentFileInfo> results = new List<TorrentFileInfo>();
            char[] progress = new char[] { '|', '/', '-', '\\' };
            int progressIdx = 0;
            DateTime starttime = DateTime.Now;

            foreach (string f in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                results.Add(new TorrentFileInfo(f, new FileInfo(f).Length));
                if (DateTime.Now - starttime > TimeSpan.FromMilliseconds(250))
                {
                    starttime = DateTime.Now;
                    Console.Write("{0}\x08", progress[progressIdx]);
                    progressIdx = (progressIdx + 1) % progress.Length;
                }
            }

            return results;
        }

        struct TorrentFileInfo
        {
            private string m_path;
            private long m_size;

            public TorrentFileInfo(string path, long size)
            {
                m_path = path;
                m_size = size;
            }

            public long Size
            {
                get
                {
                    return m_size;
                }
            }

            public string Path
            {
                get
                {
                    return m_path;
                }
            }

            public override string ToString()
            {
                return m_path;
            }
        }

        static void Main(string[] args)
        {
            string basePath = null;
            bool del = false;
            List<TorrentFileInfo> localFiles;
            HashSet<string> torrentFiles = new HashSet<string>();
            List<string> torrents = new List<string>();
            //Testing.DoTests();

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: TorrentCleanup <torrentfile> [<torrentfile>...] <directory> [-d]");
                Console.WriteLine("  -d : delete local files not in torrent. USE WITH CARE!");
                return;
            }

            foreach (string s in args)
            {
                if (s == "-d")
                {
                    del = true;
                    continue;
                }
                if (File.Exists(s))
                {
                    torrents.Add(s);
                    continue;
                }
                // otherwise assume directory
                if (Directory.Exists(s))
                {
                    if (basePath == null)
                    {
                        basePath = s;
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Please specify only one directory:");
                        Console.WriteLine("  {0}", basePath);
                        Console.WriteLine("  {0}", s);
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Unknown option '{0}'", s);
                    return;
                }
            }

            if (basePath == null)
            {
                Console.WriteLine("Please specify a path to check");
                return;
            }

            if (torrents.Count == 0)
            {
                Console.WriteLine("Please specify at least one torrent file");
                return;
            }

            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            foreach (string tor in torrents)
            {
                using (StreamReader sr = new StreamReader(tor, Encoding.Default))
                {
                    TDictionary foo = new TDictionary();
                    TDictionary info;
                    TList files;
                    System.Text.Encoding encoding = System.Text.Encoding.Default;

                    foo.ParseFromStream(sr);
                    if (foo.ContainsKey("comment"))
                        Console.WriteLine("Comment: {0}", (foo["comment"] as TString).Value);
                    if (foo.ContainsKey("encoding"))
                    {
                        encoding = System.Text.Encoding.GetEncoding((foo["encoding"] as TString).Value);
                        Console.WriteLine("Encoding: {0}", encoding.EncodingName);
                    }
                    info = foo["info"] as TDictionary;
                    if (info.ContainsKey("length"))
                    {
                        Console.WriteLine("Single-file torrent: {0}", (info["name"] as TString).Value);
                    }
                    else
                    {
                        files = info["files"] as TList;
                        Console.Write("Multi-file torrent contains ");
                        foreach (TObject o in files)
                        {
                            if (o is TDictionary)
                            {
                                string localPath = MakePath((o as TDictionary)["path"] as TList);
                                string globalPath = basePath + encoding.GetString(Encoding.Default.GetBytes(localPath)); ;

                                torrentFiles.Add(globalPath.ToLower());
                            }
                        }
                        Console.WriteLine("{0} files", torrentFiles.Count);
                    }
                }
            }
            Console.Write("Local directory tree contains ");
            localFiles = GetFileList(basePath);
            Console.WriteLine("{0} files", localFiles.Count);

            int totalFiles = 0;
            ulong totalSize = 0;

            foreach (TorrentFileInfo s in localFiles)
            {
                if (!torrentFiles.Contains(s.Path.ToLower()))
                {
                    Console.WriteLine("Local file {0} not in torrent", s);
                    totalFiles++;
                    totalSize += (ulong)s.Size;

                    if (del)
                        File.Delete(s.Path);
                }
            }
            Console.WriteLine("Total: {0:0} MB and {1} of {2} files NOT in any torrent", totalSize / (1024.0 * 1024.0), totalFiles, localFiles.Count);
        }
    }
}
