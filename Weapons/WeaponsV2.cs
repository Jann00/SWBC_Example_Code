using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class WeaponsV2 : MonoBehaviour
{
    
    public enum EnemyOrPlayer { Enemy, Player}
    [Title("$bulletPattern")]
    [BoxGroup("Weapon Setup")]
    [Tooltip("Assign the ships 'Ship Color System' component here.\nNote: If the ship does not curently have the 'Ship Color System' Component then add it.")]
    public ShipColorSystem colorSystem;
    [BoxGroup("Weapon Setup")]
    [Tooltip("This enables or disables the Weapons.")]
    public bool enableWeapons = true;
    [BoxGroup("Weapon Setup")]
    [Tooltip("Assign the center object here")]
    public Transform center;
    [BoxGroup("Weapon Setup"), EnumToggleButtons]
    public EnemyOrPlayer enemyOrPlayer;

    


    [BoxGroup("Pattern setup")]
    public BulletPattern bulletPattern;
    [BoxGroup("Pattern setup")]
    public Transform[] bulletspawn;
    [BoxGroup("Pattern setup"), Range(0,10)]
    public float delay;
    [BoxGroup("Pattern setup")]
    public bool grounded;

    [BoxGroup("Third Person Shooter"), ShowIf("enemyOrPlayer", EnemyOrPlayer.Player)]
    public bool useAmmo = false;
    [BoxGroup("Third Person Shooter"), ShowIf("useAmmo", true)]
    public int bulletsPerMagazine = 30;
    [BoxGroup("Third Person Shooter"), ShowIf("useAmmo", true)]
    public Image progressBar;
    [BoxGroup("Third Person Shooter"), ShowIf("useAmmo", true)]
    public ThirdPerson thirdPersonScript;

    [BoxGroup("Emitter"), ReadOnly]
    public bool isEmmiterBullet = false;
    [BoxGroup("Emitter"), ReadOnly]
    public Bullet emitterParrent;

    [HideInInspector]
    public int bulletsLeft;
    [HideInInspector]
    public bool outOfAmmo = false;
    [HideInInspector]
    public bool doNotUpdateBar = false;



    //Current bullets and fire rate
    [HideInInspector]
    public float fireRateTimer;
    int bulletNum;
    Bullet currentBullet;
    GameObject latestBullet;
    Vector3 dir;
    float forwardDir;
    bool alternate = false;
    float targetDir;
    float startingDir;

    int bulletSpawnCounter = 0;

    int bulletTimeCounter = 0;
    int shotTimeCounter = 0;

    [HideInInspector]
    public Transform target;

    [BoxGroup("Disable Weapons")]
    public AK.Wwise.Event WeaponsDisabledSFX;
    [BoxGroup("Disable Weapons")]
    public GameObject disabledParticles;
    float DisableWeaponTimer;
    bool disableWeapons = false;

    public void SetUp()
    {
        //Set the fire rate to the assigned delay
        fireRateTimer = delay;

        //Assign the center transform
        if (GameStats.instance != null)
        {
            center = GameStats.instance.center;
        }

        //Set up ammo
        if (useAmmo)
        {
            bulletsLeft = bulletsPerMagazine;
            progressBar.fillAmount = bulletsLeft / bulletsPerMagazine;
        }


        if (bulletPattern != null)
        {
            //Set up alternating colors
            if (bulletPattern.alternateBetweenBullets == BulletPattern.YesNoEnum.Yes || bulletPattern.alternateBetweenShots == BulletPattern.YesNoEnum.Yes)
            {
                if (bulletPattern.startingColor == BulletPattern.BulletColor.Color1)
                {
                    alternate = false;
                }
                if (bulletPattern.startingColor == BulletPattern.BulletColor.Color2)
                {
                    alternate = true;
                }
            }
        }
    }

    public void Start()
    {
        SetUp();
    }

    private void OnEnable()
    {
        SetUp();
    }

    public void Update()
    {
        if (enableWeapons && !outOfAmmo && !disableWeapons)
        {
            fireRateTimer -= Time.deltaTime;
            if (fireRateTimer <= 0)
            {
                //Shoots a bullet
                ShootBullet();
            }
        }
        if (disableWeapons)
        {
            DisableWeaponTimer -= Time.deltaTime;
            if (DisableWeaponTimer <= 0)
            {
                EnableWeapons();
            }
        }
    }

    public void ShootBullet()
    {
        //Spawn the current bullet
        SpawnBullet();
        SetBulletDirection();
        SetEmitter();
        SetProjectileColor();
        SetSpeed();
        SetCurve();
        SetTracking();
        SetEnemyOrPlayer();
        SetHitScan();
        SetLifetime();
        UsingChargeUp();
        PlaySFX();
        currentBullet.SetUpProjectile();
        bulletSpawnCounter++;

        //Decrease the ammo
        if (useAmmo)
        {
            bulletsLeft -= 1;
            if (bulletsLeft <= 0)
            {
                outOfAmmo = true;
                thirdPersonScript.ReloadMagazine(false);
            }
            
            if (!doNotUpdateBar)
            {
                float progress = (float)bulletsLeft / (float)bulletsPerMagazine;
                progressBar.fillAmount = progress;
            }
        }

        //Reset the bullet spawn location counter
        if (bulletSpawnCounter == bulletspawn.Length)
        {
            bulletSpawnCounter = 0;
        }

        //Reset the shot
        if (bulletNum == bulletPattern.bulletCount - 1)
        {
            //Reset bulletNum
            if (bulletPattern.SkipBulletNumber1 == BulletPattern.YesNoEnum.Yes)
            {
                bulletNum = 1;
            }
            else
            {
                bulletNum = 0;
            }
            
            //Set the fire rate between shots
            if (bulletPattern.randomShootTime == BulletPattern.YesNoEnum.No && bulletPattern.AlternateTimeBetweenShots == BulletPattern.YesNoEnum.No)
            {
                fireRateTimer = bulletPattern.timeBetweenShots;
            }
            if (bulletPattern.randomShootTime == BulletPattern.YesNoEnum.Yes)
            {
                fireRateTimer = Random.Range(bulletPattern.shotTimeRange.x, bulletPattern.shotTimeRange.y);
            }
            if (bulletPattern.AlternateTimeBetweenShots == BulletPattern.YesNoEnum.Yes)
            {
                fireRateTimer = bulletPattern.timeBetweenShotsArray[shotTimeCounter];
                shotTimeCounter++;
                if (shotTimeCounter >= bulletPattern.timeBetweenShotsArray.Length)
                {
                    shotTimeCounter = 0;
                }
            }

            //Alternate colors if AlternateBetweenShots is set to Yes
            if (bulletPattern.alternateBetweenShots == BulletPattern.YesNoEnum.Yes)
            {
                alternate = !alternate;
            }
        }
        //Setup fire rate for the next bullet
        else
        {
            bulletNum++;

            //Set the fire rate of the next bullet 
            if (bulletPattern.randomBulletTime == BulletPattern.YesNoEnum.No && bulletPattern.AlternateTimeBetweenBullets == BulletPattern.YesNoEnum.No)
            {
                fireRateTimer = bulletPattern.timeBetweenBullets;
            }
            if (bulletPattern.randomBulletTime == BulletPattern.YesNoEnum.Yes)
            {
                fireRateTimer = Random.Range(bulletPattern.bulletTimeRange.x, bulletPattern.bulletTimeRange.y);
            }
            if (bulletPattern.AlternateTimeBetweenBullets == BulletPattern.YesNoEnum.Yes)
            {
                fireRateTimer = bulletPattern.timeBetweenBulletsArray[bulletTimeCounter];
                bulletTimeCounter++;
                if (bulletTimeCounter >= bulletPattern.timeBetweenBulletsArray.Length)
                {
                    bulletTimeCounter = 0;
                }
            }
            if (fireRateTimer <= 0)
            {
                ShootBullet();
            }
        }

        

    }

    //Spawns the current bullet from the object pooler
    public void SpawnBullet()
    {
        if (grounded)
        {
            
            Vector3 targetScreenLocation = Camera.main.WorldToScreenPoint(bulletspawn[bulletSpawnCounter].transform.position);
            Vector3 targetStartLocation = Camera.main.ScreenToWorldPoint(new Vector3(targetScreenLocation.x, targetScreenLocation.y, 55));
            latestBullet = ObjectPooler.instance.SpawnFromPool(bulletPattern.projectileTag, targetStartLocation);
        }
        else
        {
            latestBullet = ObjectPooler.instance.SpawnFromPool(bulletPattern.projectileTag, new Vector3(bulletspawn[bulletSpawnCounter].transform.position.x, bulletspawn[bulletSpawnCounter].transform.position.y, bulletspawn[bulletSpawnCounter].transform.position.z));
        }
        currentBullet = latestBullet.GetComponent<Bullet>();
        if (center != null)
        {
            
            currentBullet.transform.parent = center;
        }

        if (currentBullet.projectileType == Bullet.ProjectileType.Bullet)
        {
            currentBullet.ResetMuzzleFlash();
            currentBullet.AssignMuzzleFlashParrent(transform);
        }
    }

    public void SetBulletDirection()
    {
        //Set the forward dir angle to the ship angle
        if (bulletPattern.useShipDirection == BulletPattern.YesNoEnum.Yes)
        {
            forwardDir = bulletspawn[bulletSpawnCounter].transform.eulerAngles.y;
        }

        //Set the forward direction angle to the bullet pattern direction
        if (bulletPattern.useShipDirection == BulletPattern.YesNoEnum.No)
        {
            forwardDir = bulletPattern.direction;
        }
        
        //Calculate the direction of each bullet.
        if (bulletPattern.randomAngle == BulletPattern.YesNoEnum.No && bulletPattern.bulletCount != 1)
        {
            if (bulletPattern.rotationDirection == BulletPattern.RotationDirection.Clockwise)
            {
                startingDir = forwardDir - (bulletPattern.angle / 2);
                targetDir = startingDir + ((bulletPattern.angle / (bulletPattern.bulletCount - 1)) * bulletNum);
            }
            if (bulletPattern.rotationDirection == BulletPattern.RotationDirection.CounterClockwise)
            {
                startingDir = forwardDir + (bulletPattern.angle / 2);
                targetDir = startingDir - ((bulletPattern.angle / (bulletPattern.bulletCount - 1)) * bulletNum);
            }
            dir = Quaternion.Euler(0, targetDir, 0) * Vector3.forward;
        }
        
        //Calculate random angle if randomAngle is enabled
        if (bulletPattern.randomAngle == BulletPattern.YesNoEnum.Yes)
        {
            float randomAngle = forwardDir + Random.Range(-bulletPattern.angle/2, bulletPattern.angle/2);
            targetDir = randomAngle;
            dir = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        }

        if (bulletPattern.bulletCount == 1 && bulletPattern.randomAngle == BulletPattern.YesNoEnum.No)
        {
            targetDir = forwardDir;
            dir = Quaternion.Euler(0, targetDir, 0) * Vector3.forward;
        }

        currentBullet.dir = dir;
    }

    public void SetProjectileColor()
    {
        //If the use ship color option is disabled then the bullet pattern color is assigned as the color instead.
        if (bulletPattern.useShipColor == BulletPattern.YesNoEnum.No)
        {
            switch (bulletPattern.color)
            {
                case BulletPattern.BulletColor.Color1:
                    currentBullet.projectileColor = getWeaponColor(1);
                    currentBullet.colorID = 1;
                    break;
                case BulletPattern.BulletColor.Color2:
                    currentBullet.projectileColor = getWeaponColor(2);
                    currentBullet.colorID = 2;
                    break;
            }
        }
        //If the use ship color option is enabled then the ship color is assigned as the projectile color;
        if (bulletPattern.useShipColor == BulletPattern.YesNoEnum.Yes && bulletPattern.alternateBetweenBullets == BulletPattern.YesNoEnum.No && bulletPattern.alternateBetweenShots == BulletPattern.YesNoEnum.No)
        {
            if (isEmmiterBullet)
            {
                currentBullet.projectileColor = getWeaponColor(emitterParrent.colorID);
                currentBullet.colorID = emitterParrent.colorID;
            }
            else
            {
                switch (colorSystem.shipColor)
                {
                    case ShipColor.Color1:
                        currentBullet.projectileColor = getWeaponColor(1);
                        currentBullet.colorID = 1;
                        break;
                    case ShipColor.Color2:
                        currentBullet.projectileColor = getWeaponColor(2);
                        currentBullet.colorID = 2;
                        break;
                }
            }
            
        }
        //If AlternateBetweenBullets is enabled the pattern will alternate color every bullet
        if (bulletPattern.alternateBetweenBullets == BulletPattern.YesNoEnum.Yes)
        {
            if (alternate)
            {
                currentBullet.projectileColor = getWeaponColor(1);
                currentBullet.colorID = 1;
            }
            else
            {
                currentBullet.projectileColor = getWeaponColor(2);
                currentBullet.colorID = 2;
            }
            alternate = !alternate;
        }
        //If AlternateBetweenShots is enabled the pattern will alternate color every shot
        if (bulletPattern.alternateBetweenShots == BulletPattern.YesNoEnum.Yes)
        {
            if (alternate)
            {
                currentBullet.projectileColor = getWeaponColor(1);
                currentBullet.colorID = 1;
            }
            else
            {
                currentBullet.projectileColor = getWeaponColor(2);
                currentBullet.colorID = 2;
            }
        }
    }

    public void SetSpeed()
    {
        //Set projectile speed equal to the bullet pattern speed
        if (bulletPattern.randomSpeed == BulletPattern.YesNoEnum.No)
        {
            currentBullet.speed = bulletPattern.speed;
        }
        //Set the projectile speed based on the speed range from the bullet pattern
        if (bulletPattern.randomSpeed == BulletPattern.YesNoEnum.Yes)
        {
            float randomSpeed = Random.Range(bulletPattern.speedRange.x, bulletPattern.speedRange.y);
            currentBullet.speed = randomSpeed;
        }
    }

    public void SetCurve() 
    {
        //Set up the curve modifier
        if (bulletPattern.curveProjectile == BulletPattern.YesNoEnum.Yes)
        {
            currentBullet.useCurve = true;
            currentBullet.curve = bulletPattern.curve;
            currentBullet.curveTime = bulletPattern.curveTime;
            currentBullet.defaultAngle = targetDir;
            currentBullet.curveAngle = bulletPattern.curveAngle;
            currentBullet.curveTimer = 0;
            if (bulletPattern.CurveDirection == BulletPattern.RotationDirection.Clockwise)
            {
                currentBullet.counterClockwise = false;
            }
            else if (bulletPattern.CurveDirection == BulletPattern.RotationDirection.CounterClockwise)
            {
                currentBullet.counterClockwise = true;
            }
        }
        //Disable the curve modifier
        else
        {
            currentBullet.useCurve = false;
        }
    }

    public void SetTracking()
    {
        //Enable projectile tracking
        if (bulletPattern.tracking == BulletPattern.YesNoEnum.Yes)
        {
            currentBullet.tracking = true;
            currentBullet.TrackingSensitivity = bulletPattern.TrackingSensitivity;
            currentBullet.DistanceToStopTracking = bulletPattern.DistanceToStopTracking;
        }
        //Disable projectile tracking
        else
        {
            currentBullet.tracking = false;
        }
        
    }

    public void SetEnemyOrPlayer()
    {
        //Set the bullet to enemy or player bullet
        switch (enemyOrPlayer)
        {
            case EnemyOrPlayer.Enemy:
                currentBullet.playerBullets = false;
                break;
            case EnemyOrPlayer.Player:
                currentBullet.playerBullets = true;
                break;
        }
    }

    //Get the current weapon Color value
    public Color getWeaponColor(int ID)
    {
        if (ID == 1)
            return ColorManager.instance.color1;
        if (ID == 2)
            return ColorManager.instance.color2;
        else
            return Color.white;

    }

    public void PlaySFX()
    {
        //Play shot sound
        if (bulletNum == 0)
        {
            bulletPattern.ShotSfx.Post(gameObject);
        }
        //Play bullet sound
        bulletPattern.BulletSfx.Post(gameObject);
    }

    public void SetHitScan()
    {
        //Set the projectile to use Hit scan
        if (bulletPattern.useHitScan == BulletPattern.YesNoEnum.Yes)
        {
            currentBullet.hitScan = true;
            currentBullet.hitScanTimer = 0;
            currentBullet.hitScanDuration = bulletPattern.hitScanDuration;
            currentBullet.target = target;
        }
        //Disable hit scan
        else
        {
            currentBullet.hitScan = false;
        }
    }

    public void SetEmitter()
    {
        //Set up projectile emittion on the projectile
        if (bulletPattern.useEmitter == BulletPattern.YesNoEnum.Yes)
        {
            if (currentBullet.BulletParticle.GetComponent<WeaponsV2>())
            {
                WeaponsV2 weapon = currentBullet.BulletParticle.GetComponent<WeaponsV2>();
                weapon.bulletNum = 0;
                weapon.delay = bulletPattern.emitterPattern.timeBetweenShots;
                weapon.bulletPattern = bulletPattern.emitterPattern;
                weapon.fireRateTimer = bulletPattern.emitterPattern.timeBetweenShots;
                weapon.enableWeapons = true;
                weapon.emitterParrent = currentBullet;
                weapon.isEmmiterBullet = true;
                
            }
            else
            {
                Debug.LogError(name + " Pattern has emitter enabled but the bullet is missing a WeaponsV2 component");
            }
        }
        //Disable projectile emittion
        else
        {
            if (currentBullet.BulletParticle.GetComponent<WeaponsV2>())
            {
                WeaponsV2 weapon = currentBullet.BulletParticle.GetComponent<WeaponsV2>();
                weapon.enableWeapons = false;
                weapon.isEmmiterBullet = false;
            }
            
        }
    }

    public void UsingChargeUp()
    {
        //Set charge up value
        if (currentBullet.useChargeParticle == Bullet.UseChargeUpParticle.Yes)
        {
            latestBullet.transform.parent = bulletspawn[bulletSpawnCounter].transform;
            latestBullet.transform.rotation = bulletspawn[bulletSpawnCounter].transform.rotation;
        }
    }

    public void SetLifetime()
    {
        //Set projectile lifetime
        if (bulletPattern.useLifetime == BulletPattern.YesNoEnum.Yes)
        {
            currentBullet.useLifetime = true;
            currentBullet.lifetime = bulletPattern.lifetime;
        }
        //Disable lifetime
        else
        {
            currentBullet.useLifetime = false;
        }
    }

    //Reload the magazine
    public void ReloadMagazine()
    {
        bulletsLeft = bulletsPerMagazine;
        outOfAmmo = false;
        doNotUpdateBar = false;
        progressBar.color = Color.white;
    }

    private void OnDestroy()
    {
        //Make sure the projectile is not destroyed when this object is destroyed
        if (latestBullet != null && latestBullet.transform.parent != center)
        {
            latestBullet.transform.parent = null;
        }
        //Disable a charging bullet if this object is destroyed
        if (latestBullet != null)
        {
            Bullet bullet = latestBullet.GetComponent<Bullet>();
            if (bullet.useChargeParticle == Bullet.UseChargeUpParticle.Yes && bullet.chargeTimer > 0)
            {
                latestBullet.SetActive(false);
            }
        }
    }

    //Disable the weapons for an assigned time in seconds
    public void DisableWeapons(float time)
    {
        WeaponsDisabledSFX.Post(gameObject);
        DisableWeaponTimer = time;
        disableWeapons = true;
        disabledParticles.SetActive(true);
        disabledParticles.GetComponent<Renderer>().material.SetColor("_BaseColor", getWeaponColor(colorSystem.getShipColorID()));
        Renderer[] particleChildrenMaterials = disabledParticles.GetComponentsInChildren<Renderer>();
        foreach (Renderer m in particleChildrenMaterials)
        {
            m.material.SetColor("_BaseColor", getWeaponColor(colorSystem.getShipColorID()));
        }
    }

    //Enable the weapons
    public void EnableWeapons()
    {
        WeaponsDisabledSFX.Stop(gameObject);
        disableWeapons = false;
        disabledParticles.SetActive(false);
    }
    
}
