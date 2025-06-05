using System.Numerics;

namespace ck.qiekn;

public interface IBullet {
  event Action? OnHit;

  static abstract void LoadTexture(string imagePath);
  static abstract void UnloadTexture();
  void Draw();
  void Init(Vector2 position, Vector2 velocity, float radius, int level);
  bool IsOffScreen();
  void Reset();
  void Update();
}
