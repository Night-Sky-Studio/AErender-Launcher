namespace AErenderLauncher.Classes.Project;

public class ProjectItem {
    public string Name { get; set; } = "";
    public int[] FootageDimensions { get; set; } = new int[2];
    public double FootageFramerate { get; set; }
    public uint[] Frames { get; set; } = new uint[2];
    public byte[] BackgroundColor { get; set; } = new byte[3];

    public override string ToString() {
        return $"{Name, -36} | [{FootageDimensions[0], 6},{FootageDimensions[1], 6}] | {FootageFramerate, -8} | [{Frames[0], 8},{Frames[1], 8}] | [{BackgroundColor[0], 3},{BackgroundColor[1], 3},{BackgroundColor[2], 3}]";
    }
}