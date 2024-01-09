using System;
using Kuroko.Shared;

namespace Kuroko.Modules.Audio
{
    internal class TempFileInstance : IDisposable
    {
        private bool disposedValue;
        public string FilePath { get; private set; }

        public TempFileInstance()
        {
            FilePath = Path.Combine(DataDirectories.TEMPFILES, $"probe_{Path.GetRandomFileName()}.tmp");
            File.Create(FilePath).Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) { }

                File.Delete(FilePath);
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
