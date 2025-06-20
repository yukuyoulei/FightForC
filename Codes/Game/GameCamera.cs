using CloverExternal;
using System;
using UnityEngine;

internal class GameCamera : Entity
{
    Camera camera;
    public override void OnStart()
    {
        base.OnStart();

        camera = Camera.main;

        RegisterCall<Vector3>(Events.OnPlayerMove, OnPlayerMove);
    }

    Vector3? playerPos;
    Vector3 cameraPos;
    private void OnPlayerMove(Vector3 vector)
    {
        if (!playerPos.HasValue)
        {
            playerPos = vector;
            cameraPos = camera.transform.position;
        }
        else
        {
            var delta = vector - playerPos.Value;
            playerPos = vector;
            cameraPos += delta;
            camera.transform.position = cameraPos;
        }
    }
}
