using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BulletPattern", menuName = "BulletPattern")]
public class BulletPattern : ScriptableObject
{
    public enum YesNoEnum { No, Yes }
    public enum BulletColor { Color1, Color2 }
    //public enum ProjectileType { Bullet, Laser, Missile, Sword }
    public enum RotationDirection { Clockwise, CounterClockwise}

    [BoxGroup("Set up")]
    public string projectileTag;
    //[BoxGroup("Set up"), EnumToggleButtons]
    //public ProjectileType projectileType;

    [BoxGroup("Color"), EnumToggleButtons, HideIf("@this.alternateBetweenShots == YesNoEnum.Yes || this.alternateBetweenBullets == YesNoEnum.Yes")]
    public YesNoEnum useShipColor;
    [BoxGroup("Color"), EnumToggleButtons, HideIf("@this.useShipColor == YesNoEnum.Yes || this.alternateBetweenShots == YesNoEnum.Yes")]
    public YesNoEnum alternateBetweenBullets;
    [BoxGroup("Color"), EnumToggleButtons, HideIf("@this.useShipColor == YesNoEnum.Yes || this.alternateBetweenBullets == YesNoEnum.Yes")]
    public YesNoEnum alternateBetweenShots;
    [BoxGroup("Color"), EnumToggleButtons, HideIf("@this.useShipColor == YesNoEnum.Yes || this.alternateBetweenShots == YesNoEnum.Yes || this.alternateBetweenBullets == YesNoEnum.Yes")]
    public BulletColor color;
    [BoxGroup("Color"), EnumToggleButtons, ShowIf("@this.alternateBetweenBullets == YesNoEnum.Yes || this.alternateBetweenShots == YesNoEnum.Yes")]
    public BulletColor startingColor;

    [BoxGroup("Angle"), EnumToggleButtons]
    public YesNoEnum useShipDirection;
    [BoxGroup("Angle"),Range(0, 360), ShowIf("useShipDirection", YesNoEnum.No)]
    public float direction;
    [BoxGroup("Angle"), Range(0, 360)]
    public float angle;
    [BoxGroup("Angle"), EnumToggleButtons]
    public YesNoEnum randomAngle;
    [BoxGroup("Angle"), EnumToggleButtons]
    public RotationDirection rotationDirection;

    [BoxGroup("Amount")]
    public int bulletCount;
    [BoxGroup("Amount"), EnumToggleButtons]
    public YesNoEnum SkipBulletNumber1;

    [BoxGroup("Time between bullets"), EnumToggleButtons, ShowIf("AlternateTimeBetweenBullets", YesNoEnum.No)]
    public YesNoEnum randomBulletTime;
    [BoxGroup("Time between bullets"), ShowIf("randomBulletTime", YesNoEnum.Yes), MinMaxSlider(0, 20, true)]
    public Vector2 bulletTimeRange = new Vector2(0, 0);
    [BoxGroup("Time between bullets"), EnumToggleButtons, ShowIf("randomBulletTime", YesNoEnum.No)]
    public YesNoEnum AlternateTimeBetweenBullets;
    [BoxGroup("Time between bullets"), ShowIf("@this.randomBulletTime == YesNoEnum.No && this.AlternateTimeBetweenBullets == YesNoEnum.No")]
    public float timeBetweenBullets;
    [BoxGroup("Time between bullets"), ShowIf("@this.randomBulletTime == YesNoEnum.No && this.AlternateTimeBetweenBullets == YesNoEnum.Yes")]
    public float[] timeBetweenBulletsArray;

    [BoxGroup("Time between shots"), EnumToggleButtons, ShowIf("AlternateTimeBetweenShots", YesNoEnum.No)]
    public YesNoEnum randomShootTime;
    [BoxGroup("Time between shots"), ShowIf("@this.randomShootTime == YesNoEnum.Yes && this.AlternateTimeBetweenShots == YesNoEnum.No"), MinMaxSlider(0, 20, true)]
    public Vector2 shotTimeRange = new Vector2(0, 0);
    [BoxGroup("Time between shots"), EnumToggleButtons, ShowIf("randomShootTime", YesNoEnum.No)]
    public YesNoEnum AlternateTimeBetweenShots;
    [BoxGroup("Time between shots"), ShowIf("@this.randomShootTime == YesNoEnum.No && this.AlternateTimeBetweenShots == YesNoEnum.No")]
    public float timeBetweenShots;
    [BoxGroup("Time between shots"), ShowIf("@this.randomShootTime == YesNoEnum.No && this.AlternateTimeBetweenShots == YesNoEnum.Yes")]
    public float[] timeBetweenShotsArray;

    [BoxGroup("Speed"), EnumToggleButtons]
    public YesNoEnum randomSpeed;
    [BoxGroup("Speed"), ShowIf("randomSpeed", YesNoEnum.Yes), MinMaxSlider(0, 100, true)]
    public Vector2 speedRange = new Vector2(0, 0);
    [BoxGroup("Speed"), ShowIf("randomSpeed", YesNoEnum.No), Range(0,200)]
    public float speed;

    [BoxGroup("Tracking"), EnumToggleButtons, HideIf("@this.curveProjectile == YesNoEnum.Yes || this.useHitScan == YesNoEnum.Yes")]
    public YesNoEnum tracking;
    [BoxGroup("Tracking"), ShowIf("tracking", YesNoEnum.Yes), Range(0,50)]
    public float TrackingSensitivity;
    [BoxGroup("Tracking"), ShowIf("tracking", YesNoEnum.Yes), Range(0, 30)]
    public float DistanceToStopTracking;

    [BoxGroup("Curve"), EnumToggleButtons, HideIf("@this.tracking == YesNoEnum.Yes || this.useHitScan == YesNoEnum.Yes")]
    public YesNoEnum curveProjectile;
    [BoxGroup("Curve"), ShowIf("curveProjectile", YesNoEnum.Yes)]
    public AnimationCurve curve;
    [BoxGroup("Curve"), ShowIf("curveProjectile", YesNoEnum.Yes)]
    public float curveTime;
    [BoxGroup("Curve"), ShowIf("curveProjectile", YesNoEnum.Yes), Range(0, 360)]
    public float curveAngle;
    [BoxGroup("Curve"), EnumToggleButtons, ShowIf("curveProjectile", YesNoEnum.Yes)]
    public RotationDirection CurveDirection;

    [BoxGroup("Emitter"), EnumToggleButtons]
    public YesNoEnum useEmitter;
    [BoxGroup("Emitter"), ShowIf("useEmitter", YesNoEnum.Yes)]
    public BulletPattern emitterPattern;

    [BoxGroup("Hitscan"), EnumToggleButtons, HideIf("@this.tracking == YesNoEnum.Yes || this.curveProjectile == YesNoEnum.Yes")]
    public YesNoEnum useHitScan;
    [BoxGroup("Hitscan"), ShowIf("useHitScan", YesNoEnum.Yes)]
    public float hitScanDuration;

    [BoxGroup("Lifetime"), EnumToggleButtons]
    public YesNoEnum useLifetime;
    [BoxGroup("Lifetime"), ShowIf("useLifetime", YesNoEnum.Yes)]
    public float lifetime;


    [BoxGroup("SFX")]
    public AK.Wwise.Event ShotSfx;
    [BoxGroup("SFX")]
    public AK.Wwise.Event BulletSfx;
}
