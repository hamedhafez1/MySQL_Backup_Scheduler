using G9ConfigManagement.Abstract;
using G9ConfigManagement.DataType;

namespace G9MySqlBackup.Config;

public class G9MySqlBackupConfig : G9AConfigStructure<G9MySqlBackupConfig>
{
    public string MySqlUser { set; get; } = "root";
    public string MySqlPass { set; get; } = "";
    public TimeSpan BackupHourTime { set; get; } = new(0, 0, 0);
    public string[] DataBaseName { set; get; } = { "#AllDatabases#" };
    public bool EnableCompress { set; get; } = true;
    public string BackupDirectoryPath { set; get; } = "./DatabaseBackup/";
    public sbyte KeepBackupDays { set; get; } = 7;
    public override G9DtConfigVersion ConfigVersion { get; set; } = new(1, 0, 0, 0);
}