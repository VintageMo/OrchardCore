using System;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Core.Services
{
    public class TemporaryFileBuilder : IFileBuilder, IDisposable
    {
        private readonly bool _deleteOnDispose;

        public TemporaryFileBuilder(bool deleteOnDispose = true)
        {
            Folder = PathExtensions.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _deleteOnDispose = deleteOnDispose;
        }

        public string Folder { get; }

        public void Dispose()
        {
            if (_deleteOnDispose)
            {
                if (Directory.Exists(Folder))
                {
                     SafeDeleteDirectory(Folder);
                }
            }
        }

        private static void SafeDeleteDirectory(string targetDirectory)
        {
            foreach (string file in Directory.GetFiles(targetDirectory))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in Directory.GetDirectories(targetDirectory))
            {
                SafeDeleteDirectory(dir);
            }

            Directory.Delete(targetDirectory, false);
        }

        public async Task SetFileAsync(string subpath, Stream stream)
        {
            var fullname = PathExtensions.Combine(Folder, subpath);

            var directory = new FileInfo(fullname).Directory;

            if (!directory.Exists)
            {
                directory.Create();
            }

            using (var fs = File.Create(fullname, 4 * 1024, FileOptions.None))
            {
                await stream.CopyToAsync(fs);
            }
        }

        public override string ToString()
        {
            return Folder;
        }
    }
}
