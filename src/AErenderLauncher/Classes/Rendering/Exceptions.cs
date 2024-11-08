using System;

namespace AErenderLauncher.Classes.Rendering;

public class MissingAeException(string message) : Exception(message) { }