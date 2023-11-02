﻿using System;
using Avalonia;
using Friflo.Fliox.Editor.OpenGL;
using Friflo.Fliox.Editor.UI;

namespace Friflo.Fliox.Editor;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var window = OpenGLTest.Init(args);
        OpenGLTest.RunEventLoop(window);
        window.Dispose();
        
        var editor = new Editor();
        editor.Init(args).Wait();
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        AppBuilder builder = BuildAvaloniaApp();
        builder.StartWithClassicDesktopLifetime(args);
        
        // editor.Run();
        editor.Shutdown();
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}