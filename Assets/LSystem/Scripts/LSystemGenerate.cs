using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LSystem.Scripts;
using UnityEngine;

public class GenerateMeshData
{
    public List<Vector3> vector3s = new List<Vector3>();
    public List<int> triangents = new List<int>();
}

// https://www.bilibili.com/read/cv3006796
public class LSystemGenerate
{
    private IGenerateImp iGenerateImp; 
    public GenerateMeshData Generate(ShapeSetting shapeSetting)
    {
        iGenerateImp = CreateGenerate(shapeSetting.generateType);
        var generateMeshData = iGenerateImp.Generate(shapeSetting);
        iGenerateImp.SaveFile(shapeSetting);
        return generateMeshData;
    }

    private IGenerateImp CreateGenerate(GenerateType shapeSettingGenerateType)
    {
        switch (shapeSettingGenerateType)
        {
            case GenerateType.Demo:
                return new DemoGenerateImp();
            case GenerateType.FactalPlant:
                return new FactalPlantGenerateImp();
            case GenerateType.Template:
                return new TemplateGenerateImp();
            break;
            default:
                throw new NotImplementedException();
        }
    }
}


public class LsystemEnv
{
    public Vector3 up = Vector3.up;
    public  Vector3 right = Vector3.right;
    public  Vector3 pos = Vector3.zero;

    public LsystemEnv(LsystemEnv curEvn)
    {
        up = curEvn.up;
        right = curEvn.right;
        pos = curEvn.pos;
    }

    public LsystemEnv()
    {
        
    }

    public override string ToString()
    {
        return "up:" + up + ",righe:" + right + ",pos" + pos;
    }
}

public abstract class IGenerateImp
{
    private Vector3[] rect = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
    };
    private Vector3[] forwardRect = new Vector3[]
    {
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
    };


    private LsystemEnv curEvn = new LsystemEnv();
    private Stack<LsystemEnv> _stack = new Stack<LsystemEnv>();
    private StringBuilder _stringBuilder = new StringBuilder();
    public abstract GenerateMeshData  Generate(ShapeSetting shapeSetting);
    
    protected  void UpdatePos(Vector2 shapeSettingSize)
    {
        curEvn.pos += curEvn.up * shapeSettingSize.y;
        _stringBuilder.AppendLine("UpdatePos:"+curEvn.ToString());
    }

    protected void UpdateRect(ShapeSetting shapeSetting)
    {

        var scale = Mathf.Lerp(1, 0.25f, _stack.Count*1.0f / shapeSetting.maxIter);
        rect[0] = curEvn.pos + curEvn.right*shapeSetting.size.x*(-0.5f)*scale+curEvn.up*0;
        rect[1] = curEvn.pos + curEvn.right*shapeSetting.size.x*0.5f*scale+curEvn.up*0;
        rect[2] = curEvn.pos + curEvn.right*shapeSetting.size.x*0.5f*scale+curEvn.up*shapeSetting.size.y;
        rect[3] = curEvn.pos + curEvn.right*shapeSetting.size.x*(-0.5f)*scale+curEvn.up*shapeSetting.size.y;
        
        
        var forward = Vector3.Cross(curEvn.right, curEvn.up);
        for (int i = 0; i < 4; i++)
        {
            forwardRect[i] = rect[i] + forward * shapeSetting.size.x * scale;
        }
        
        
        _stringBuilder.AppendLine("UpdateRect:"+curEvn.ToString());

    }

    protected  void AddCell(ref GenerateMeshData generateMeshData)
    {
        int startIndex = generateMeshData.vector3s.Count;

        for (int i = 0; i < 4; i++)
        {
            generateMeshData.vector3s.Add(rect[i]);
        }
        for (int i = 0; i < 4; i++)
        {
            generateMeshData.vector3s.Add(forwardRect[i]);
        }
        
        //0,1,2,3
        AddCellFace(ref generateMeshData,startIndex, 0, 1, 2, 3);
        //1,5,6,2
        AddCellFace(ref generateMeshData,startIndex, 1,5,6,2);
        //5,4,7,6
        AddCellFace(ref generateMeshData,startIndex, 5,4,7,6);
        //4,0,3,7
        AddCellFace(ref generateMeshData,startIndex, 4,0,3,7);
        //3,2,6,7
        AddCellFace(ref generateMeshData,startIndex, 3,2,6,7);
        //5,1,0,4
        AddCellFace(ref generateMeshData,startIndex, 5,1,0,4);
        
        _stringBuilder.AppendLine("AddCell:"+curEvn.ToString());
    }

    private void AddCellFace(ref GenerateMeshData generateMeshData,int startIndex,int i0, int i1, int i2, int i3)
    {
        generateMeshData.triangents.Add(startIndex+i0);
        generateMeshData.triangents.Add(startIndex+i3);
        generateMeshData.triangents.Add(startIndex+i2);
        
        generateMeshData.triangents.Add(startIndex+i0);
        generateMeshData.triangents.Add(startIndex+i2);
        generateMeshData.triangents.Add(startIndex+i1);
    }

    protected  void Rotation(float angle)
    {
        var noise = Unity.Mathematics.noise.snoise(curEvn.pos);
        noise = (noise + 1) * 0.5f;
        var dir = Vector3.Lerp(Vector3.forward, Vector3.right, noise);
        var rotation = Quaternion.AngleAxis(angle, dir);
        curEvn.up =  rotation * curEvn.up;
        curEvn.right = rotation * curEvn.right;
        _stringBuilder.AppendLine("Rotation:"+curEvn.ToString());
    }

    protected void PushEnv()
    {
        LsystemEnv env = new LsystemEnv(curEvn);
        _stack.Push(env);
        _stringBuilder.AppendLine("PushEnv:"+curEvn.ToString());
    }

    protected void PopEvn()
    {
        curEvn = _stack.Pop();
        _stringBuilder.AppendLine("PopEvn:"+curEvn.ToString());
    }

    public void SaveFile(ShapeSetting shapeSetting)
    {
        File.WriteAllText(shapeSetting.generateType.ToString()+"_"+shapeSetting.maxIter.ToString()+".txt",_stringBuilder.ToString());
    }
}
