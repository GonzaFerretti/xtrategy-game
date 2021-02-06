using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] float startingHeight;
    [SerializeField] float maxHeight;
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;

    [SerializeField] Transform itemModel;
    [SerializeField] MeshRenderer itemMeshRenderer;
    [SerializeField] MeshFilter itemMeshFilter;
    [SerializeField] TextMeshProUGUI nameLabel;
    public Vector3Int coordinates;
    public ItemData itemData;

    // TO DO MAKE AN EDITOR SCRIPT THAT BAKES A FINITE AMOUNT OF POINTS OF THIS CURVE INSTEAD OF CALCULATING IT!

    float GetHoverHeight(float time)
    {
        float normalizedFunction = Mathf.Cos(time * speed + Mathf.PI) / 2 + 0.5f;
        return startingHeight + normalizedFunction * (maxHeight - 1);
    }

    public void Update()
    {
        itemModel.localPosition = new Vector3(0, GetHoverHeight(Time.realtimeSinceStartup), 0);
        itemModel.localEulerAngles += new Vector3(0,rotationSpeed * Time.deltaTime,0);
    }

    public void UpdateItem(ItemData itemData, Vector3Int coordinates)
    {
        itemMeshFilter.mesh = itemData.itemVisualInfo.mesh;
        itemMeshRenderer.materials = itemData.itemVisualInfo.materials;
        nameLabel.text = itemData.displayName;
        this.itemData = itemData;
        this.coordinates = coordinates;
    }
}

[System.Serializable]
public struct ItemVisualInfo
{
    public Mesh mesh;
    public Material[] materials;
}