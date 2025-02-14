using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ModelUIController : MonoBehaviour
{
    [Header("Sliders")] [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider alphaSlider;

    [Header("Containers")] [SerializeField]
    private ModelViewController modelViewController;

    [SerializeField] private Transform modelScrollViewContainer;
    [SerializeField] private Transform animationScrollViewContainer;
    [SerializeField] private GameObject buttonPrefab;

    private readonly List<Button> animationButtonPool = new();
    private List<GameObject> modelsPool;
    private GameObject activeModel;
    private readonly HashSet<Material> modelMaterials = new();

    private void Start()
    {
        redSlider.onValueChanged.AddListener(UpdateColor);
        greenSlider.onValueChanged.AddListener(UpdateColor);
        blueSlider.onValueChanged.AddListener(UpdateColor);
        alphaSlider.onValueChanged.AddListener(UpdateColor);
    }

    public void UpdateUI()
    {
        if (modelViewController == null)
        {
            Debug.LogError("ModelViewController is not assigned!");
            return;
        }

        modelsPool = modelViewController.ModelsPool;
        PopulateModelScrollView();
    }

    private void PopulateModelScrollView()
    {
        foreach (var model in modelsPool)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, modelScrollViewContainer);
            buttonObj.GetComponentInChildren<Text>().text = model.name;
            buttonObj.GetComponent<Button>().onClick.AddListener(() => ActivateModel(model));
        }
    }

    private void ActivateModel(GameObject modelPrefab)
    {
        activeModel?.SetActive(false);
        modelPrefab.SetActive(true);
        activeModel = modelPrefab;
        modelMaterials.Clear();

        if (modelPrefab.TryGetComponent(out Animator animatorComponent))
        {
            UpdateAnimationList(animatorComponent);
        }
        else
        {
            ClearInactiveButtons();
        }
    }

    private void UpdateAnimationList(Animator animatorComponent)
    {
        var clips = animatorComponent.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            Button buttonComponent;

            if (i < animationButtonPool.Count)
            {
                buttonComponent = animationButtonPool[i];
                buttonComponent.gameObject.SetActive(true);
            }
            else
            {
                GameObject buttonObj = Instantiate(buttonPrefab, animationScrollViewContainer);
                buttonComponent = buttonObj.GetComponent<Button>();
                animationButtonPool.Add(buttonComponent);
            }

            string animationName = clips[i].name;
            buttonComponent.GetComponentInChildren<Text>().text = animationName;
            buttonComponent.onClick.RemoveAllListeners();
            var currentClipIndexRoot = i;
            buttonComponent.onClick.AddListener(() => PlayAnimation(clips[currentClipIndexRoot], animatorComponent));
        }

        ClearInactiveButtons(clips.Length);
    }

    private void PlayAnimation(AnimationClip clip, Animator animatorComponent)
    {
        animatorComponent.Play(clip.name);
    }

    private void ClearInactiveButtons(int clipsLength = 0)
    {
        for (int i = clipsLength; i < animationButtonPool.Count; i++)
        {
            animationButtonPool[i].gameObject.SetActive(false);
        }
    }

    private void UpdateColor(float value)
    {
        if (activeModel == null) return;

        Color newColor = new(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);

        if (activeModel.TryGetComponent(out MeshRenderer modelMeshRenderer))
        {
            modelMaterials.Add(modelMeshRenderer.material);
        }

        foreach (var renderer in activeModel.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                modelMaterials.Add(material);
            }
        }

        foreach (var material in modelMaterials)
        {
            material.color = newColor;
        }
    }
}