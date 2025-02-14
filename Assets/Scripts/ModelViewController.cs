using System.Collections.Generic;
using UnityEngine;

public class ModelViewController : MonoBehaviour
{
    [SerializeField] private List<GameObject> modelsPrefabs;
    [SerializeField] private GameObject modelParent;
    [SerializeField] private ModelUIController modelUIController;
    public List<GameObject> ModelsPool { get; private set; } = new();

    private void Start()
    {
        foreach (var model in modelsPrefabs)
        {
            var modelInstance = Instantiate(model, modelParent.transform);
            modelInstance.name = model.name;
            modelInstance.layer = LayerMask.NameToLayer("Grabbable");
            modelInstance.SetActive(false);
            ModelsPool.Add(modelInstance);
        }

        modelUIController.UpdateUI();
    }
}