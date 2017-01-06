# TorrentCleanup

This tool can be used to quickly clean up unused files from your local directory tree that are not in the corresponding .torrent file.

You can use it for example after "upgrading" a torrent to a newer version. If there are files that were renamed/moved, those would usually be lingering around your harddisk until you manually delete them.

Or, you can use this tool to check (and optionally delete) those unneeded files

The syntax is:

    torrentcleanup d:\foo\mytorrent.torrent d:\downloads\mydata

this will check d:\downloads\mydata for any files that are not in the mytorrent.torrent. If you add a `-d` anywhere on the command line, those extra files will be deleted.

**NOTE:** Always use scanning-only mode (without `-d`) first to see what files would be deleted!

## Advanced usage

You can optionally specify more than one torrent file on the command line. This is useful if you have merged the download locations of multiple torrents into a single directory. For example this was used on some sites with the so-called TOSEC ROM files. The tool simply joins all files in all torrents you specify into a single set of files.

Simple example:

Torrent A contains the directories `foo_a/` and `bar_a/`, while torrent B contains `foo_b/` and `bar_b/`. Your destination directory contains all four subdirectories:

    - D:\MyDownloads
      +- foo_a
      +- foo_b
      +- bar_a
      +- bar_b

Then you *must* call TorrentCleanup with both torrents, A and B, otherwise it would mark (and possibly delete) all files that are in the other torrent:

    torrentcleanup a.torrent b.torrent d:\mydownloads


