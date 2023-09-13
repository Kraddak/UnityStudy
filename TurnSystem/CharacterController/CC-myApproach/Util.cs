using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public static class Util{
    
    public enum _direction{
        left, right
    }

    
    public static Vector3 Direction(Vector3 source, Vector3 destination){
        return (destination - source).normalized;
    }


    public static Vector3 PerpendicularLeft(Vector3 direction){
        return Vector3.Cross(direction, Vector3.up).normalized;
    }

    public static Vector3 PerpendicularRight(Vector3 direction){
        return Vector3.Cross(direction, Vector3.down).normalized;
    }


    public static void DrawPoint(Vector3 point, Color color, float duration = 5f){
        Debug.DrawLine(point + Vector3.down * .1f, point + Vector3.up * .1f, color, duration);
        Debug.DrawLine(point + Vector3.right * .1f, point + Vector3.left * .1f, color, duration);
    }

    public static void DrawPoint(Vector3 source, Vector3 point, Color color, float duration = 5f){
        Debug.Log($"Point {point}");
        DrawPoint(point, color);
        Debug.DrawLine(source, point, color, duration);
    }


    public static float Distance2D(Vector3 a, Vector3 b){

        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);

        return Vector2.Distance(a2, b2);
    }


    public static _direction GetLeftOrRight(Vector3 a, Vector3 b){
        Vector3 leftVec = Vector3.Cross(a, Vector3.up).normalized; // Calculate the left vector relative to vector "a"
        float dot = Vector3.Dot(leftVec, b); // Calculate the dot product of the left vector and vector "b"
        return (dot > 0) ? _direction.left : _direction.right; // Return "left" if dot product is positive, otherwise "right"
    }



    public static void DoOnRange(int x, int y, Action<int,int> func){
        for (int i = 0; i < y; i++)
            for (int j = 0; j < x; j++)
                func(j,i);
    }


    public static (int x, int z) Index1DTo2D(int index1D, int sizeZ){
        int x = index1D / sizeZ;
        int z = index1D % sizeZ;
        return (x, z);
    }

    public static int Index2DTo1D(int x, int z, int sizeZ){
        return x * sizeZ + z;
    }




    public static async Task MoveTo(Transform movingObject, Vector3 destination, float speed, bool shouldRotate, AnimationCurve curve, Action action){
        Vector3 origin = movingObject.position;
        Vector3 direction = Direction(origin, destination);
        float distance = Vector3.Distance(origin, destination);
        float remainingDistance = distance;
        float completeness;

        while (remainingDistance > 0){

            if(shouldRotate)
                movingObject.forward = Vector3.Lerp(
                    movingObject.forward, 
                    new Vector3(direction.x,0,direction.z), 
                    Time.deltaTime * speed * 3);

            completeness = 1 - (remainingDistance / distance);
            movingObject.position = Vector3.Lerp(origin, destination, curve.Evaluate(completeness));
            remainingDistance -= speed * Time.deltaTime;
            action();
            await Task.Yield();
        }
    }
    public static async Task MoveTo(Transform movingObject, Vector3 destination, float speed, bool shouldRotate, Action action){
        Vector3 origin = movingObject.position;
        Vector3 direction = Direction(origin, destination);
        float distance = Vector3.Distance(origin, destination);
        float remainingDistance = distance;
        float completeness;

        while (remainingDistance > 0){

            // Rotation
            if(shouldRotate)
                movingObject.forward = Vector3.Lerp(
                    movingObject.forward, 
                    new Vector3(direction.x,0,direction.z), 
                    Time.deltaTime * speed * 3);

            completeness = 1 - (remainingDistance / distance);
            movingObject.position = Vector3.Lerp(origin, destination, completeness);
            remainingDistance -= speed * Time.deltaTime;
            action();
            await Task.Yield();
        }
    }
    public static async Task MoveTo(Transform movingObject, Vector3 destination, float speed, bool shouldRotate, AnimationCurve curve){
        await MoveTo(movingObject, destination, speed, shouldRotate, curve, () => {});
    }
    public static async Task MoveTo(Transform movingObject, Vector3 destination, float speed, Action action){
        await MoveTo(movingObject, destination, speed, true, action);
    }
    public static async Task MoveTo(Transform movingObject, Vector3 destination, float speed, bool shouldRotate){
        await MoveTo(movingObject, destination, speed, shouldRotate, () => {});
    }
    public static async Task MoveTo(Transform movingObject, Vector3 destination, float speed){
        await MoveTo(movingObject, destination, speed, true);
    }



    public static async Task MoveFixTime(Transform movingObject, Vector3 destination, float speed, Action action){
        Vector3 origin = movingObject.position;
        float time = 0;

        while (time < 1){
            movingObject.position = Vector3.Lerp(origin, destination, time);
            time += Time.deltaTime * speed;
            action();
            await Task.Yield();
        }
        //movingObject.position = destination;
    }



    public static async Task Rotate2D(Transform rotatingObject, Vector3 destination, float speed){
        Vector3 direction = Direction(rotatingObject.position, destination);
        while(Vector3.Distance(rotatingObject.forward, direction) > 0.001f){
            rotatingObject.forward = Vector3.Lerp(
                rotatingObject.forward, 
                new Vector3(direction.x,0,direction.z), 
                Time.deltaTime * speed);
            await Task.Yield();
        }
    }

    public static async Task RotateToTarget(Transform rotatingObject, Vector3 target){
        float rotateSpeed = 10f;
        float time = 0;
        Quaternion lookRotation = Quaternion.LookRotation(target - rotatingObject.position);
        Quaternion initialRotation = rotatingObject.rotation;

        Vector3 enemyDirection2D = (target - rotatingObject.position).normalized;

        // Rotate to look at enemy
        while (time < 1){
            rotatingObject.rotation = Quaternion.Slerp(initialRotation, lookRotation, time);
            time += Time.deltaTime * rotateSpeed;
            await Task.Yield();
        }
    }

    public static async Task RotateToTarget(Transform rotatingObject, Vector3 target, float yoffset){
        float rotateSpeed = 10f;
        float time = 0;
        //DrawPoint(rotatingObject.position, target, Color.blue);
        float softDeltaY = ((target.y - rotatingObject.transform.position.y) * yoffset);
        target = target - Vector3.up * softDeltaY;
        //DrawPoint(rotatingObject.position, target, Color.magenta);

        Quaternion lookRotation = Quaternion.LookRotation(target - rotatingObject.position);
        Quaternion initialRotation = rotatingObject.rotation;

        Vector3 enemyDirection2D = (target - rotatingObject.position).normalized;

        // Rotate to look at enemy
        while (time < 1){
            rotatingObject.rotation = Quaternion.Slerp(initialRotation, lookRotation, time);
            time += Time.deltaTime * rotateSpeed;
            await Task.Yield();
        }
    }




    public static float GetYDifference(Vector3 vec1, Vector3 vec2, Vector3 vec3, Vector3 vec4){
        float[] yValues = { vec1.y, vec2.y, vec3.y, vec4.y };
        float minY = Mathf.Min(yValues);
        float maxY = Mathf.Max(yValues);
        float yDifference = maxY - minY;
        return yDifference;
    }



}
