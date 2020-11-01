using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] controllerImages;
    public GameObject[] keyboardImages;
    private InputManager _inputManager;

    [Header("Don't change, read ONLY.")]
    [SerializeField] private bool _isUsingController = false;

    private void Awake()
    {
        _inputManager = this.GetComponent<InputManager>();
    }

    private void Start()
    {
        UpdateImages(_inputManager.GetIsUsingController());
    }

    private void Update()
    {
        if (_isUsingController == _inputManager.GetIsUsingController())
            return;
        else
            _isUsingController = _inputManager.GetIsUsingController();


        UpdateImages(_isUsingController);
    }

    private void UpdateImages(bool isUsingController)
    {
        if (isUsingController)
        {
            foreach (GameObject image in controllerImages)
                image.SetActive(true);

            foreach (GameObject image in keyboardImages)
                image.SetActive(false);
        }
        else
        {
            foreach (GameObject image in controllerImages)
                image.SetActive(false);

            foreach (GameObject image in keyboardImages)
                image.SetActive(true);
        }
    }
}
