using Microsoft.Extensions.Options;
using MyLab.FileStorage.Tools;
using MyLab.Log.Dsl;
using MyLab.TaskApp;
using ExceptionDto = MyLab.Log.ExceptionDto;

namespace MyLab.FileStorage.Cleaner
{
    class CleanerTaskLogic : ITaskLogic
    {
        private readonly ICleanerStrategy _strategy;
        private readonly TimeSpan _lostFileTtl;
        private readonly TimeSpan? _storedTtl;
        private readonly IDslLogger? _log;
        private readonly FileIdToNameConverter _fidConverter;

        public CleanerTaskLogic(ICleanerStrategy strategy, IOptions<CleanerOptions> opts, ILogger<CleanerTaskLogic>? logger = null)
            :this(strategy, opts.Value, logger)
        {
        }

        public CleanerTaskLogic(ICleanerStrategy strategy, CleanerOptions opts, ILogger<CleanerTaskLogic>? logger = null)
        {
            _log = logger?.Dsl();

            _strategy = strategy;

            _fidConverter = new FileIdToNameConverter(opts.Directory);
            _lostFileTtl = TimeSpan.FromHours(opts.LostFileTtlHours);
            _storedTtl = opts.StoredFileTtlHours.HasValue
                ? TimeSpan.FromHours(opts.StoredFileTtlHours.Value)
                : null;
        }

        public Task Perform(CancellationToken cancellationToken)
        {
            foreach (var fsFile in _strategy.GetFileDirectories(cancellationToken))
            {
                var liveTime = DateTime.Now - fsFile.CreateDt;

                if (fsFile.Confirmed)
                {
                    if (_storedTtl.HasValue && liveTime > _storedTtl.Value)
                    {
                        _strategy.DeleteDirectory(fsFile.Directory);

                        LogDeletion(fsFile.Directory, "The stored file has been deleted", liveTime);
                    }
                }
                else
                {
                    if (liveTime > _lostFileTtl)
                    {
                        _strategy.DeleteDirectory(fsFile.Directory);

                        LogDeletion(fsFile.Directory, "The lost file has been deleted", liveTime);
                    }
                }
            }

            return Task.CompletedTask;
        }

        void LogDeletion(string fileDirectory, string message, TimeSpan liveTime)
        {
            if(_log == null) return;
            
            object fid;

            try
            {
                fid = _fidConverter
                    .GetIdFromDirectory(fileDirectory)
                    .ToString("N");
            }
            catch (Exception e)
            {
                fid = ExceptionDto.Create(e);
            }

            _log?.Action(message)
                .AndFactIs("file-id", fid)
                .AndFactIs("live-time", liveTime)
                .Write();
        }
    }
}
