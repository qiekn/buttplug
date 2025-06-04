using System.Numerics;
using Raylib_cs;

namespace ck.qiekn;

public enum GameState {
  Menu,
  Playing,
  Paused,
  GameOver
}

internal class Game {
  #region field

  GameState gameState;

  BulletPool bullets = new BulletPool(20, 100);
  int bulletSpeed = 300;

  int score;
  int totalHits;
  int targetHits = 1000;

  float timer = 0f;
  int bpm = 100;
  float interval;

  int judgmentX = Conf.judgmentX;
  int judgmentY = Conf.judgmentY;
  int judgmentRadius = Conf.judgmentRadius;

  // raylib fonts
  Font regular;
  Font italic;

  #endregion

  public Game() {
    gameState = GameState.Playing;
    score = 0;
    totalHits = 0;
    interval = 0.6f;
  }

  public void Init() {
    Bullet.LoadTexture("assets/images/top500.png");
    Raylib.InitAudioDevice();
    Bullet.LoadSound("assets/audio/fap.wav");
  }

  public void CleanUp() {
    Bullet.UnloadTexture();
    Bullet.UnloadSound();
    Raylib.CloseAudioDevice();
  }

  public Game(Font a, Font b) : this() {
    regular = a;
    italic = b;
  }


  public void HandleInput() {
    if (Raylib.IsKeyPressed(KeyboardKey.Space) && gameState == GameState.Menu) {
      gameState = GameState.Playing;
    }
  }

  /*─────────────────────────────────────┐
  │            Update Methods            │
  └──────────────────────────────────────*/

  public void Update() {
    switch (gameState) {
      case GameState.Playing:
        UpdateGame();
        break;
    }
  }

  private void UpdateGame() {
    float deltaTime = Raylib.GetFrameTime();
    timer += deltaTime;
    if (timer >= interval) {
      timer = 0;
      var bullet = bullets.Get(new Vector2(Conf.ScreenWidth - 50, Conf.judgmentY),
                  new Vector2(-1 * bulletSpeed, 0), 20, 1);
      bullet.OnHit += () => totalHits++;
    }
    bullets.Update();
  }

  /*─────────────────────────────────────┐
  │             Draw Methods             │
  └──────────────────────────────────────*/

  public void Draw() {
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    switch (gameState) {
      case GameState.Menu:
        DrawMenu();
        break;
      case GameState.Playing:
        DrawGame();
        break;
      case GameState.Paused:
        DrawPaused();
        break;
      case GameState.GameOver:
        DrawGameOver();
        break;
    }

    Raylib.EndDrawing();
  }

  private void DrawMenu() {
    Raylib.DrawTextEx(regular, "Fap Hero: Split Bullets", new Vector2(250, 150), 32, 1, Color.White);
    Raylib.DrawTextEx(regular, "Press Space to Start", new Vector2(250, 250), 20, 1, Color.Gray);
    Raylib.DrawTextEx(regular, "Press Esc to Quit", new Vector2(250, 300), 20, 1, Color.Gray);
  }

  private void DrawGame() {
    // 绘制判定线
    Raylib.DrawLine(0, judgmentY, judgmentX - judgmentRadius, judgmentY, Color.White);
    Raylib.DrawLine(judgmentX + judgmentRadius, judgmentY, Conf.ScreenWidth, judgmentY, Color.White);
    Raylib.DrawCircleLines(judgmentX, judgmentY, 15, Color.White);

    // 绘制所有弹幕
    bullets.Draw();

    // 绘制UI
    Raylib.DrawTextEx(regular, $"Score: {score}", new Vector2(10, 10), 20, 1, Color.White);
    Raylib.DrawTextEx(regular, $"Hits: {totalHits}/{targetHits}", new Vector2(10, 40), 20, 1, Color.White);
    Raylib.DrawTextEx(regular, $"Active Bullets: {bullets.ActiveBulletCount}", new Vector2(10, 70), 16, 1, Color.Gray);
    // Raylib.DrawTextEx(regular, "Press Spacebar to Pause", new Vector2(10, 450), 16, 1, Color.Gray);
  }

  private void DrawPaused() {
    return;
  }

  private void DrawGameOver() {
    return;
  }
}
