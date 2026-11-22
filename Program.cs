using MiniHospital.Rendering;
using MiniHospital.Simulation;
using MiniHospital.Utils;
using Raylib_cs;

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
Raylib.InitWindow(ScreenCanvas.ReferenceWidth,ScreenCanvas.ReferenceHeight,"Mini Hospital - TP Edition");
Raylib.SetTargetFPS(240);

TextureManager.LoadDefault();
Hospital hospital = new();
try
{
    using var renderer=new HospitalRenderer();
    while (!Raylib.WindowShouldClose())
    {
        
        if (Raylib.IsKeyPressed(KeyboardKey.Space)) hospital.SpeedMultiplier = 0f;
        if (Raylib.IsKeyPressed(KeyboardKey.One)) hospital.SpeedMultiplier = 1f;
        if (Raylib.IsKeyPressed(KeyboardKey.Two)) hospital.SpeedMultiplier = 2f;
        if (Raylib.IsKeyPressed(KeyboardKey.Four)) hospital.SpeedMultiplier = 4f;
        if (Raylib.IsKeyPressed(KeyboardKey.Zero)) hospital.SpeedMultiplier = 1000f;
            if (Raylib.IsKeyPressed(KeyboardKey.D)) HospitalRenderer.DrawDebug = !HospitalRenderer.DrawDebug;
        if (Raylib.IsKeyPressed(KeyboardKey.B)) Hospital.CallBrainOnUpdate = !Hospital.CallBrainOnUpdate;
        if (Raylib.IsKeyPressed(KeyboardKey.C))
        {
            Chaos.Enabled = !Chaos.Enabled;
            Raylib.SetWindowTitle(Chaos.Enabled ? "Mini Hospital - TP Edition [CHAOS]" : "Mini Hospital - TP Edition");
        }
        hospital.Update(Raylib.GetFrameTime());
        renderer.Draw(hospital);
    }
}
finally {
    TextureManager.Unload();
    Raylib.CloseWindow(); 
}
