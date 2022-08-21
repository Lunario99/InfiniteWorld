using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBuillder : MonoBehaviour
{
    public Transform WaypointsParent;

    public bool AlwaysUp = false;
    public bool CloseLoop = false;
    public float PostSeparation = 0.5f;
    public float FenceHeight = 1f;
    public float CrossbeamHeight = 0.6f;
    public float PostWidth = 0.1f;
    [Range(0f, 1f)] public float RandomizeRotationAmount = 0.1f;
    [Range(0f, 1f)] public float RandomizeHeightAmount = 0.1f;
    public Color color;

    [ContextMenu("Build Fence")]
    public void BuildFence()
    {
        ClearObjects();

        var lastPos = Vector3.zero;
        for (int i = 0; i < WaypointsParent.childCount - 1; i++)
        {
            lastPos = BuildFenceSegment(WaypointsParent.GetChild(i).position, WaypointsParent.GetChild(i + 1).position, lastPos);
        }

        ColorObjects();
    }

    public void Awake()
    {
        BuildFence();
    }

    public Vector3 BuildFenceSegment(Vector3 start, Vector3 end, Vector3 lastEnd)
    {
        //Getting how many segnments we need based on total distance and post spacing
        var distance = (end - start).magnitude;
        var requiredPosts = Mathf.RoundToInt(distance / PostSeparation);
        var lastPos = lastEnd;
        var offSet = (end - start);
        for (int i = 0; i < requiredPosts; i++)
        {
            //Setting up the right position
            var pos = Vector3.Lerp(start, end, Mathf.InverseLerp(0, requiredPosts, i));
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.transform.position = pos;
            var defaultRotation = Quaternion.LookRotation(AlwaysUp ? new Vector3(offSet.x, 0f, offSet.z) : offSet);
            post.transform.rotation = Quaternion.Lerp(defaultRotation, Random.rotation, RandomizeRotationAmount);
            var height = FenceHeight * (1f - (Random.value * RandomizeHeightAmount));
            post.transform.Translate(0f, height * 0.5f, 0f, Space.Self);
            post.transform.localScale = new Vector3(PostWidth, height, PostWidth);
            post.transform.parent = transform;

            var beamPos = pos + post.transform.up * height * CrossbeamHeight;
            if (i == 0 && lastPos == Vector3.zero)
            {
                lastPos = beamPos;
                continue;
            }

            var beam  = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.transform.position = Vector3.Lerp(lastPos, beamPos, 0.5f);
            beam.transform.rotation = Quaternion.LookRotation(beamPos - lastPos, Vector3.up);
            beam.transform.Translate(PostWidth * 0.5f, 0f, 0f, Space.Self);
            beam.transform.localScale = new Vector3(PostWidth * 0.3f, PostWidth, (lastPos - beamPos).magnitude);
            beam.transform.parent = transform;

            var beam2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam2.transform.rotation = Quaternion.LookRotation(beamPos - lastPos, Vector3.up);
            beam2.transform.position = Vector3.Lerp(lastPos, beamPos, 0.5f);
            beam2.transform.Translate(-PostWidth * 0.5f, 0f, 0f, Space.Self);
            beam2.transform.localScale = new Vector3(PostWidth * 0.3f, PostWidth, (lastPos - beamPos).magnitude);
            beam2.transform.parent = transform;
            lastPos = beamPos;
        }

        return lastPos;
    }

    [ContextMenu("Color Objects")]
    public void ColorObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var targetRender = transform.GetChild(i).GetComponent<Renderer>();
            SetColor(targetRender, color);
        }
    }

    private void SetColor(Renderer target, Color color)
    {
        var fenceColor = new MaterialPropertyBlock();
        fenceColor.SetColor("_Color", color);
        target.SetPropertyBlock(fenceColor);
    }

    [ContextMenu("Clear Objects")]
    public void ClearObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}
