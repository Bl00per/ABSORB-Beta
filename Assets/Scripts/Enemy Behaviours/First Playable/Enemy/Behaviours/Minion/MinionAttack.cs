using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
 
public class MinionAttack : AIBehaviour 
{ 
    [Header("General Movement")] 
    public bool lookAtPlayer = true; 
    public float turnSpeed = 0.20f; 
 
    [Header("References")] 
    public Trigger weaponTrigger; 
    public GameObject weaponToEnable; 
    private Animator _animator; 
    private CombatHandler _combatHandler; 
 
    [Header("Timers")] 
    public float transitionTime = 2.0f; 
    public float timeBeforeAttack = 0.5f; 
 
    [Header("Attack Properties")] 
    public float attackForce = 10.0f; 
 
    private Vector3 _initialPos; 
    private Quaternion _initialRot; 
 
    // private bool _hasAttacked = false; 
    // private bool _canAttack = false; 
 
    private void Awake() 
    { 
        _animator = this.GetComponent<Animator>(); 
    } 
 
    private void Start() 
    { 
        _initialPos = weaponToEnable.transform.localPosition; 
        _initialRot = weaponToEnable.transform.localRotation; 
        _combatHandler = this.brain.PlayerTransform.GetComponent<CombatHandler>(); 
    } 
 
    public override void OnStateEnter() 
    { 
        enemyHandler.SetJustAttacked(true); 
        StartCoroutine(enemyHandler.Coroutine_JustAttacked()); 
        _animator.SetBool("Attacking", true); 
    } 
 
    public override void OnStateExit() { } 
 
    public override void OnStateFixedUpdate() { } 
 
    public override void OnStateUpdate() 
    { 
        // Get direction to player 
        Vector3 dir = brain.GetDirectionToPlayer(); 
 
        // // Checking if we should be locked onto the player or not... 
        // if (this.destinationLockedToPlayer) 
        //     this.currentDestination = brain.PlayerTransform.position; 
 
        // // Updating the target destination every frame 
        // brain.SetDestinationOnCooldown(this.currentDestination, 1.0f); 
 
        // Rotate to face direction 
        if (lookAtPlayer) 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed); 
    } 
 
    // Deactivates the collision check on the enemy's weapon 
    public void Key_DeactivateMinionAttack() 
    { 
        weaponToEnable.SetActive(false); 
        _animator.SetBool("Attacking", false); 
        //_canAttack = false; 
        brain.SetBehaviour("Movement"); 
    } 
 
    public void Key_ActivateMinionAttack() 
    { 
        weaponToEnable.SetActive(true); 
        weaponToEnable.transform.SetParent(null); 
        StartCoroutine(ReparentWeapon()); 
    } 
 
    private IEnumerator ReparentWeapon() 
    { 
        yield return new WaitForSeconds(2.0f); 
        weaponToEnable.transform.SetParent(this.gameObject.transform); 
        weaponToEnable.transform.localPosition = _initialPos; 
        weaponToEnable.transform.localRotation = _initialRot; 
    } 
} 
