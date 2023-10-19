using MyLab.FileStorage.Tools;

namespace IntegrationTest;

public static class TestStuff
{
    public const string DataDir = "test-data";

    public static void DeleteFileDataDir(Guid fid)
    {
        var converter = new FileIdToNameConverter(DataDir);
        var firstDir = converter.ToFirstDirectory(fid);

        if(Directory.Exists(firstDir))
        {
            Directory.Delete(firstDir, true);
        }
    }

    public static void TouchDataDir()
    {
        if (!Directory.Exists(DataDir))
            Directory.CreateDirectory(DataDir);
    }
}
