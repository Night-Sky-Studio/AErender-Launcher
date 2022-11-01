namespace AErenderLauncher.Classes.Project; 

public class ProjectItem {
    public int ID { get; set; }
    public string Name { get; set; }
    public string ItemType { get; set; }
    public object? FolderContents { get; set; }
    public int[] FootageDimensions { get; set; }
    public double FootageFramerate { get; set; }
    public int[] Frames { get; set; }
    public int FootageType { get; set; }
    public int[] BackgroundColor { get; set; }
    public object? CompositionLayers { get; set; }
}