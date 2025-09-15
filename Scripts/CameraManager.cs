using System;    
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject actionCameraGameObject;

    private void Start()
    {
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

        HideActionCamera();
    }
    private void ShowActionCamera()
    {
        actionCameraGameObject.SetActive(true);
    }
    private void HideActionCamera()
    {
        actionCameraGameObject?.SetActive(false);
    }

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:

                Unit shootUnit = shootAction.GetUnit();
                Unit targetUnit = shootAction.GetTargetUnit();
                Vector3 cameraCharaterHeight = Vector3.up * 1.7f;

                Vector3 shootDir = (targetUnit.GetWorldPosition() - shootUnit.GetWorldPosition()).normalized;

                float shoulderOffsetAmount = 0.5f;

                Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0)*shootDir*shoulderOffsetAmount;

                Vector3 actionCameraPosition = shootUnit.GetWorldPosition()+cameraCharaterHeight+shoulderOffset+(shootDir*-1);//

                actionCameraGameObject.transform.position = actionCameraPosition;
                actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition()+cameraCharaterHeight);
                ShowActionCamera();
                break;
        }
    }
    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                HideActionCamera();
                break;
        }
    }
}
