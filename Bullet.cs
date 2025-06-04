using System.Numerics;
using Raylib_cs;

namespace ck.qiekn;

public class Bullet : IBullet {
  public Vector2 Position;
  public Vector2 Velocity;
  public float Radius;
  public int Level; // 分裂等级,1级碰到判定点会变为0级，变成0级就会消失
  public bool IsActive;
  public static Texture2D BulletTexture;
  public static Sound HitSound;

  public event Action? OnHit;

  public Bullet() => Reset();

  public void Init(Vector2 position, Vector2 velocity, float radius, int level) {
    Position = position;
    Velocity = velocity;
    Radius = radius;
    Level = level;
    IsActive = true;
  }

  public void Reset() {
    Position = Vector2.Zero;
    Velocity = Vector2.Zero;
    Radius = 0f;
    Level = 0;
    IsActive = false;
  }

  public static void LoadTexture(string imagePath) {
    BulletTexture = Raylib.LoadTexture(imagePath);
    if (BulletTexture.Id == 0) {
      Console.WriteLine($"Failed to load texture: {imagePath}");
    }
  }

  public static void UnloadTexture() {
    Raylib.UnloadTexture(BulletTexture);
  }

  public static void LoadSound(string soundPath) {
    HitSound = Raylib.LoadSound(soundPath);
  }

  public static void UnloadSound() {
    Raylib.UnloadSound(HitSound);
  }

  public void Update() {
    if (!IsActive) return;
    Position += Velocity * Raylib.GetFrameTime();
    if (Position.X <= Conf.judgmentX) {
      IsActive = false;
      OnHit?.Invoke();
      Raylib.PlaySound(HitSound);
    }
  }

  public void Draw() {
    if (!IsActive) return;
    const float w = 50f;
    const float h = 50f;
    Rectangle sourceRect = new(0, 0, BulletTexture.Width, BulletTexture.Height);
    Rectangle destRect = new(Position.X, Position.Y, w, h);
    Vector2 origin = new(w / 2, h / 2); // center image

    Raylib.DrawTexturePro(BulletTexture, sourceRect, destRect, origin, 0f, Color.White);
  }

  public bool IsOffScreen() {
    return Position.X < -Radius || Position.X > Conf.ScreenWidth + Radius ||
           Position.Y < -Radius || Position.Y > Conf.ScreenHeight + Radius;
  }
}
