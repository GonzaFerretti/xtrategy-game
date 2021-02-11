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

    [SerializeField] Transform itemModelPivot;
    [SerializeField] TextMeshProUGUI nameLabel;
    public Vector3Int coordinates;
    public ItemData itemData;

    // TO DO MAKE AN EDITOR SCRIPT THAT BAKES A FINITE AMOUNT OF POINTS OF THIS CURVE INSTEAD OF CALCULATING IT!

    void SetHoverHeight()
    {
        float time = Time.realtimeSinceStartup;
        float normalizedFunction = Mathf.Cos(time * speed + Mathf.PI) / 2 + 0.5f;
        
        float finalSpeed = startingHeight + normalizedFunction * (maxHeight - 1);

        itemModelPivot.localPosition = new Vector3(0, finalSpeed, 0);
        itemModelPivot.localEulerAngles += new Vector3(0, rotationSpeed * Time.deltaTime, 0);
    }

    public void Update()
    {
        SetHoverHeight();
    }

    public void UpdateItem(ItemData itemData, Vector3Int coordinates)
    {
        Instantiate(itemData.modelPrefab, itemModelPivot).transform.localPosition = Vector3.zero;
        nameLabel.text = itemData.displayName;
        this.itemData = itemData;
        this.coordinates = coordinates;
    }
}