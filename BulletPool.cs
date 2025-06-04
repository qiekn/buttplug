using System.Numerics;
namespace ck.qiekn;

// Bullet 对象池
public class BulletPool {
  private readonly Queue<Bullet> pool_ = new Queue<Bullet>();
  private readonly List<Bullet> activeBullets_ = new List<Bullet>();
  private readonly int initialPoolSize_;
  private readonly int maxPoolSize_;

  public BulletPool(int initialPoolSize = 20, int maxPoolSize = 100) {
    initialPoolSize_ = initialPoolSize;
    maxPoolSize_ = maxPoolSize;

    for (int i = 0; i < initialPoolSize_; i++) {
      pool_.Enqueue(new Bullet());
    }
  }

  // public methods
  public void Update() {
    foreach (var bullet in activeBullets_) {
      bullet.Update();
    }
    ReturnInactiveBullets();
  }

  public void Draw() {
    foreach (var bullet in activeBullets_) {
      bullet.Draw();
    }
  }

  public Bullet Get(Vector2 position, Vector2 velocity, float radius, int level) {
    Bullet bullet = pool_.Count > 0 ? pool_.Dequeue() : new Bullet();
    bullet.Init(position, velocity, radius, level);
    activeBullets_.Add(bullet);
    return bullet;
  }

  public void Return(Bullet bullet) {
    if (bullet == null) return;
    activeBullets_.Remove(bullet);
    bullet.Reset();
    if (pool_.Count < maxPoolSize_) {
      pool_.Enqueue(bullet);
    }
  }

  public int ActiveBulletCount => activeBullets_.Count;
  public int PooledBulletCount => pool_.Count;
  public int TotalBulletCount => ActiveBulletCount + PooledBulletCount;

  public void Clear() {
    while (activeBullets_.Count > 0) {
      Return(activeBullets_[0]);
    }
  }

  public void Warmup(int count) {
    for (int i = 0; i < count && pool_.Count < maxPoolSize_; i++) {
      pool_.Enqueue(new Bullet());
    }
  }

  public void PrintStats() {
    Console.WriteLine($"active bullets: {ActiveBulletCount}");
    Console.WriteLine($"pool   bullets: {PooledBulletCount}");
    Console.WriteLine($"total  bullets: {TotalBulletCount}");
  }

  // private methods
  private void ReturnInactiveBullets() {
    for (int i = activeBullets_.Count - 1; i >= 0; i--) {
      var bullet = activeBullets_[i];
      if (!bullet.IsActive || bullet.IsOffScreen()) {
        Return(bullet);
      }
    }
  }
}
