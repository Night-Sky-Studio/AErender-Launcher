namespace AErenderLauncher.Interfaces;

public interface ICloneable<out T> {
    public T Clone();
}