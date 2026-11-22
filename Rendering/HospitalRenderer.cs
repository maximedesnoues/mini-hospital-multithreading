using MiniHospital.Config;
using MiniHospital.Simulation;
using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{
    public sealed class HospitalRenderer : IDisposable
    {
        public static bool DrawDebug { get; set; } = false;

        private ScreenCanvas canvas;
        private readonly Texture2D background;
        private Font font;

        public HospitalRenderer()
        {
            background = TextureManager.Get(TextureId.Background);

            //police
            int[] characters = Enumerable.Range(32, 95)
                .Concat("àâäçéèêëîïôöùûüÿÀÂÄÇÉÈÊËÎÏÔÖÙÛÜŸœŒ"
                .Select(c => (int)c))
                .Distinct()
                .ToArray();

            string path = Path.Combine(AppContext.BaseDirectory, "Assets/font.ttf");
            font = Raylib.LoadFontEx(
                path,
                48,
                characters,
                characters.Length
            );

            Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);
        }


        public void Draw(Hospital hospital)
        {
            //todo: pas besoin de full refresh à chaque fois, vois si on peu catch le changement de résolution
            canvas = new ScreenCanvas(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(10, 34, 59, 255));
            DrawBackground(hospital);
            DrawSprites(hospital);
            DrawRoomOverlays(hospital);
            DrawHud(hospital);

            if (DrawDebug)
            {
                DrawNavigationDebug();
            }

            Raylib.EndDrawing();
        }

        private void DrawBackground(Hospital hospital)
        {
            Raylib.DrawTexturePro(background,
                new Rectangle(0, 0, background.Width, background.Height),
                canvas.At(0, 0, HospitalLayout.Width, HospitalLayout.Height),
                Vector2.Zero, 0f, Color.White);
        }

        private void DrawSprites(Hospital hospital)
        {
            List<Sprite> sprites = hospital.GetRenderSprites();
            //ajout des overlay closed pour order de rendu
            foreach (var room in hospital.Rooms)
                if (!room.IsOpen)
                    sprites.Add(new RectangleSprite(room.Config.ClosedArea, new Color(15, 25, 40, 180)));

            sprites.Sort((a, b) =>
            {
                int layer = a.Layer.CompareTo(b.Layer);

                return layer != 0
                    ? layer
                    : a.Position.Y.CompareTo(b.Position.Y);
            });

            foreach (Sprite sprite in sprites)
                sprite.Draw(canvas);
        }

        private void DrawRoomOverlays(Hospital hospital)
        {
            foreach (var room in hospital.Rooms)
            {
                var config = room.Config;
                Rectangle area = canvas.At(
                    config.ClosedArea.X,
                    config.ClosedArea.Y,
                    config.ClosedArea.Width,
                    config.ClosedArea.Height
                );

                DrawTextCapsule(
                    room.Config.Name,
                    area.X + area.Width / 2f,
                    area.Y + 8f * canvas.Scale,
                    23f * canvas.Scale,
                    Color.White,
                    new Color(26, 47, 69, 235),
                    new Color(125, 173, 200, 220)
                );

                if (!room.IsOpen)
                {
                    DrawTextCapsule(
                        "FERMÉ",
                        area.X + area.Width / 2f,
                        area.Y + area.Height / 2f - 18f * canvas.Scale,
                        28f * canvas.Scale,
                        Color.White,
                        new Color(150, 55, 65, 235),
                        new Color(230, 150, 150, 255)
                    );
                }
            }
        }

        private void DrawHud(Hospital hospital)
        {
            float s = Math.Clamp(canvas.Width / 1440f, 0.73f, 1.15f);
            int h = canvas.HeaderHeight;

            float X(float fraction) => canvas.Width * fraction;
            Color navy = new(11, 37, 68, 255);
            Color pale = new(241, 249, 255, 255);
            Color blue = new(121, 201, 245, 255);
            Color green = new(110, 233, 153, 255);
            Color red = new(252, 121, 132, 255);
            Color gold = new(255, 205, 112, 255);

            void Text(string value, float x, float y, float size, Color color)
            {
                Raylib.DrawTextEx(font, value, new Vector2(x, y), size * s, 0.4f * s, color);
            }

            void Separator(float x)
            {
                Raylib.DrawRectangle((int)x, 11, 2, h - 24,
                    new Color(55, 138, 179, 255));
            }

            void Stat(float x, string label, int value, Color color)
            {
                Text(label, x, 32, 17, pale);
                Text(value.ToString(), x, 50, 30, color);
            }

            // Fond du HUD
            Raylib.DrawRectangle(0, 0, canvas.Width, h, navy);
            Raylib.DrawRectangle(0, h - 3, canvas.Width, 3,
                new Color(29, 152, 199, 255));

            // Titre et horloge
            Text("MINI HOSPITAL", X(0.018f), 11, 24, pale);
            Text($"Jour {hospital.DayNumber}   {hospital.ClockTime.ToString(@"hh\:mm")}",
                X(0.018f), 48, 23, pale);
            Separator(X(0.173f));

            // Résultats de la journée
            Text("AUJOURD'HUI", X(0.184f), 8, 17, pale);
            Stat(X(0.184f), "Arrivés", hospital.DayStats.Arrived, blue);
            Stat(X(0.250f), "Soignés", hospital.DayStats.Treated, green);
            Stat(X(0.316f), "Non soignés", hospital.DayStats.Untreated, red);
            Stat(X(0.398f), "Présents", hospital.DayStats.Active, blue);
            Separator(X(0.463f));

            // Résultats depuis le premier jour
            Text("TOTAL", X(0.474f), 8, 17, pale);
            Stat(X(0.474f), "Arrivés", hospital.TotalStats.Arrived, blue);
            Stat(X(0.542f), "Soignés", hospital.TotalStats.Treated, green);
            Stat(X(0.594f), "Non soignés", hospital.TotalStats.Untreated, red);
            Separator(X(0.672f));

            // Score cumulé en évidence, avec les points du jour et le combo
            Text("SCORE GLOBAL", X(0.688f), 8, 17, pale);
            Text(hospital.TotalStats.Score.ToString(), X(0.688f), 27, 40, gold);
            Text($"Jour : {hospital.DayStats.Score}   Combo : {hospital.DayStats.Combo}   Record : {hospital.TotalStats.BestCombo}",
                X(0.688f), 68, 15, green);
            Separator(X(0.908f));

            // Employés arrivés dans le bus
            Text("BUS", X(0.920f), 16, 17, pale);
            Text($"{hospital.EmployeesInBusCount} / {hospital.EmployeeCount}",
                X(0.920f), 41, 29, green);
        }

        private void DrawTextCapsule(string text, float centerX, float topY, float fontSize, Color textColor, Color bgColor, Color bgBordercolor)
        {
            float spacing = 1f * canvas.Scale;
            float paddingX = 12f * canvas.Scale;
            float paddingY = 5f * canvas.Scale;

            Vector2 textSize = Raylib.MeasureTextEx(
                font, text, fontSize, spacing);

            Rectangle frame = new(
                centerX - textSize.X / 2f - paddingX,
                topY,
                textSize.X + paddingX * 2,
                textSize.Y + paddingY * 2
            );

            Raylib.DrawRectangleRounded(
                frame, 0.3f, 6,
                bgColor
            );

            Raylib.DrawRectangleLinesEx(
                frame, Math.Max(1f, canvas.Scale),
                bgBordercolor
            );

            Raylib.DrawTextEx(
                font,
                text,
                new Vector2(centerX - textSize.X / 2f, topY + paddingY),
                fontSize,
                spacing,
                textColor
            );
        }

        private void DrawNavigationDebug()
        {
            // Connexions
            foreach (var (from, to) in HospitalLayout.NavLinks)
            {
                Vector2 a = canvas.At(HospitalLayout.NavPoints[from]);
                Vector2 b = canvas.At(HospitalLayout.NavPoints[to]);

                Raylib.DrawLineEx(
                    a, b, 2f,
                    new Color(0, 240, 220, 210)
                );
            }

            // Points et indices
            foreach (var (node, worldPosition) in HospitalLayout.NavPoints)
            {
                Vector2 p = canvas.At(worldPosition);

                Raylib.DrawCircleV(p, 5f, Color.Yellow);

                Raylib.DrawText(
                    ((int)node).ToString(),
                    (int)p.X + 7,
                    (int)p.Y - 15,
                    12,
                    Color.White
                );
            }
        }

        public void Dispose()
        {
            Raylib.UnloadFont(font);
        }
    }

}