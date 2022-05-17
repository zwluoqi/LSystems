using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayIter : MonoBehaviour
{
    public int maxIter;
    public int curIter = 1;
    private float space = 0.5f;
    private float timer = 0;
    private void Start()
    {
        var item = GetComponent<LSystemMesh>();

        maxIter = item.shapeSetting.maxIter;
        curIter = 1;
        item.shapeSetting.maxIter = curIter;
        item.GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > space)
        {
            timer -= space;
            var item = GetComponent<LSystemMesh>();
            curIter++;
            if (curIter <= maxIter)
            {
                item.shapeSetting.maxIter = curIter;
                item.GenerateMesh();
            }
        }
        
    }
}
