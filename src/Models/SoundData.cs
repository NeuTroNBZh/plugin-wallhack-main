namespace WallhackPluginCS2.Models;

public struct SoundData
{
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public bool HackyReload { get; set; }
    public float RevealUntil { get; set; }

    public SoundData(float startTime = -1f, float endTime = -1f)
    {
        StartTime = startTime;
        EndTime = endTime;
        HackyReload = false;
        RevealUntil = 0f;
    }
}