using System;

namespace AErenderLauncher.Classes.Rendering;

public class RenderingException(string message) : Exception(message);

public class MissingAeException() :
    RenderingException("After Effects is not installed on this system or it's path wasn't specified.");

public class EmptyOutputException(string projectName) : 
    RenderingException($"{projectName}: Output path is empty.");