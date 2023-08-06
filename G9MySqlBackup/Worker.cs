using System.Diagnostics;
using G9MySqlBackup.Config;

namespace G9MySqlBackup;

public class Worker : BackgroundService
{
    private const string _mySqlDumpPath = ".\\Resources\\mysqldump.exe";
    private const string _7zipPath = ".\\Resources\\7z\\7za.exe";
    private readonly G9MySqlBackupConfig _config;
    private readonly ILogger<Worker> _logger;
    private DateTime _lastBackupDateTime = DateTime.Now.AddDays(-1);
    private DateTime _lastRemoveDateTime = DateTime.Now.AddDays(-1);

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _config = G9MySqlBackupConfig.GetConfig();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(500, stoppingToken);
            if (DateTime.Now.Date > _lastBackupDateTime.Date && DateTime.Now.TimeOfDay > _config.BackupHourTime &&
                _config.DataBaseName != null && _config.DataBaseName.Any())
            {
                foreach (var dbName in _config.DataBaseName)
                {
                    var isBackupAllDatabase = dbName == "#AllDatabases#";


                    var fileName = dbName + DateTime.Now.ToString("-yyyy-MM-dd-HH-mm");
                    var paterNameForCheckExist = dbName + DateTime.Now.ToString("-yyyy-MM-dd");
                    Console.WriteLine($"Backup File Name Is: {fileName}");

                    var path = Path.Combine(_config.BackupDirectoryPath, dbName);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    var dir = new DirectoryInfo(path);
                    if (dir.EnumerateFiles($"{paterNameForCheckExist}*").Any())
                        continue;

                    PrepareBackup(path, fileName, dbName, isBackupAllDatabase);
                }

                _lastBackupDateTime = DateTime.Now;
            }

            if (DateTime.Now > _lastRemoveDateTime.Date && _config.DataBaseName != null && _config.DataBaseName.Any() &&
                _config.KeepBackupDays != 0)
            {
                var datePattern = DateTime.Now.AddDays(-_config.KeepBackupDays);
                foreach (var dbName in _config.DataBaseName)
                {
                    var fileName = dbName + datePattern.ToString("-yyyy-MM-dd");
                    var path = Path.Combine(_config.BackupDirectoryPath, dbName);

                    var dir = new DirectoryInfo(path);
                    foreach (var file in dir.EnumerateFiles($"{fileName}*")) file.Delete();
                }

                _lastRemoveDateTime = DateTime.Now;
            }
        }
    }


    private void PrepareBackup(string path, string fileName, string databaseName, bool isBackupAllDatabase)
    {
        var fileFullPath = Path.Combine(path, fileName);
        RunCommand(
            $"{_mySqlDumpPath} --user={_config.MySqlUser} --password={_config.MySqlPass} {(isBackupAllDatabase ? "--all-databases" : databaseName)} >> \"{fileFullPath}.sql\"");

        if (!_config.EnableCompress) return;
        RunCommand($"{_7zipPath} a \"{fileFullPath}.zip\" \"{fileFullPath}.sql\"");
        File.Delete($"{fileFullPath}.sql");
    }

    private void RunCommand(string command)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C {command}";
        process.StartInfo = startInfo;

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                Console.WriteLine(args.Data);
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                Console.WriteLine(args.Data);
        };

        process.Start();

        process.BeginOutputReadLine();

        process.BeginErrorReadLine();

        process.WaitForExit();
    }
}