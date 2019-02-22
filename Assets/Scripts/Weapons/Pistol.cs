namespace Weapons
{
    public class Pistol : PlayerWeapon
    {
        private void Start()
        {
            _name = "Pistol";

            _damage = 20;

            _bulletsInMag = 7;
            _magSize = 7;

            _maxAmmo = 49;
            _ammo = 49;

            _range = 100;
            _fireRate = 0.8f;
            _reloadTime = 1.1f;
            _accuracyFacor = 1.0f;
        }
        
        
    }
}