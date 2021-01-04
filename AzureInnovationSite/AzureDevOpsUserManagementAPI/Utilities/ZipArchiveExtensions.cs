using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace AzureDevOpsUserManagementAPI.Utilities
{
    public static class ZipArchiveExtensions
    {
        public static string ReadTextFile(this ZipArchive zip, string relativePath)
        {
            relativePath = (relativePath ?? string.Empty).Replace("\\", "/").Trim('/');

            using (var zs = zip.GetEntry(relativePath).Open())
            using (var sr = new StreamReader(zs))
            {
                return sr.ReadToEnd();
            }
        }

    }
}