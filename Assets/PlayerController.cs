using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Vector3 mineColliderSize;
    [SerializeField] private float mineColliderOffsetForward;

    private StarterAssetsInputs _input;
    private ThirdPersonController _controller;
    private Animator _animator;
    private Coroutine _coroutine;
    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _controller = GetComponent<ThirdPersonController>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_input.mouseButton)
        {
            StartCoroutine(MineAnim());
            

        }
        
        
    }

    private IEnumerator MineAnim()
    {
        _controller.lockPlayer = true;
        _animator.SetBool("Mine", true);
        float time = 0;
        while (time < 2f)
        {
            time += Time.deltaTime;
            if (_input.move.magnitude>0)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _animator.SetBool("Mine", false);
        _controller.lockPlayer = false;
        
    }

    private void Mine_AE()
    {
        var transform1 = transform;
        Collider[] col =
            Physics.OverlapBox(
                transform1.position + (transform1.forward * mineColliderOffsetForward) + new Vector3(0, 0.5f, 0),
                mineColliderSize);
        for (int i = 0; i < col.Length; i++)
        {
            if (col[i].TryGetComponent<ResourceSource>(out ResourceSource source))
            {
                Debug.Log(source,source);
            }
        }
    }

    
    private void OnDrawGizmosSelected()
    {
        var transform1 = transform;
        Gizmos.DrawCube(transform1.position + (transform1.forward* mineColliderOffsetForward)+ new Vector3(0,0.5f,0), mineColliderSize);
    }
}
