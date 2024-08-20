using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirearmPreset : MonoBehaviour
{
    public GameObject bullet;
    public GameObject bulletSpawn;

    public AudioClip gunshot;

    public InputActionReference attack;
    public InputActionReference reloadWeapon;
    public InputActionReference toggleFire;
}
